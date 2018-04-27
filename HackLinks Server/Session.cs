using HackLinks_Server.Computers;
using HackLinks_Server.Computers.Permissions;
using HackLinks_Server.Computers.Processes;
using HackLinks_Server.Daemons;
using HackLinks_Server.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using static HackLinks_Server.GameClient;
using static HackLinksCommon.NetUtil;

namespace HackLinks_Server
{
    public class Session
    {
        public float trace = 100;

        public float traceSpd = 0;

        public float traceUpdtCooldown = 0;

        public GameClient owner;
        public Node connectedNode;
        public Daemon activeDaemon;
        private Process attachedProcess;

        // TODO implement process for this
        /*
        private SortedDictionary<string, Tuple<string, CommandHandler.Command>> sessionCommands = new SortedDictionary<string, Tuple<string, CommandHandler.Command>>()
        {
            { "daemon", new Tuple<string, CommandHandler.Command>("daemon [daemon name]\n    If it's available we'll launch the given daemon.", Daemon) },
        };

        public SortedDictionary<string, Tuple<string, CommandHandler.Command>> Commands
        {
            get
            {
                //If we've not got a active daemon then we don't need to do a merge
                if (activeDaemon == null)
                {
                    return sessionCommands;
                }
                else
                {
                    //Commands from the daemon will override commands from the session if there's a conflict.
                    Dictionary<string, Tuple<string, CommandHandler.Command>> newCommands = sessionCommands
                        .Union(activeDaemon.Commands)
                        .GroupBy(k => k.Key, v => v.Value)
                        .ToDictionary(k => k.Key, v => v.Last());

                    //finally create sorted dictionary from this
                    return new SortedDictionary<string, Tuple<string, CommandHandler.Command>>(newCommands);
                }
            }
        }
        */

        public bool HasProcessId(long pid)
        {
            return attachedProcess.ProcessId == pid;
        }

        public int sessionId;

        public Session(GameClient client, Node node, Process process)
        {
            this.connectedNode = node;
            this.owner = client;
            this.sessionId = GenerateSessionId(node);
            this.attachedProcess = process;
            node.sessions.Add(this);
            SendNodeInfo();
        }

        public void WriteInput(string inputData)
        {
            attachedProcess.WriteInput(inputData);
        }

        public void SendNodeInfo()
        {
            List<string> daemonTx = new List<string>(new string[] { "node", connectedNode.GetDisplayName() });
            foreach(Daemon daemon in connectedNode.daemons)
            {
                var daemonDisplay = daemon.GetSSHDisplayName();
                if (daemonDisplay == null)
                    continue;
                daemonTx.AddRange(new string[] { $"daemon { daemon.StrType }", daemonDisplay });
            }
            owner.Send(PacketType.KERNL, daemonTx.ToArray());
        }

        // TODO relocate to process
        private static bool Daemon(GameClient client, string[] command)
        {
            Session activeSession = client.activeSession;

            if (command.Length != 2)
            {
                activeSession.owner.Send(PacketType.MESSG, "Usage : daemon [name of daemon]");
                return true;
            }
            var target = command[1];
            if(target == "exit")
            {
                activeSession.activeDaemon.OnDisconnect(activeSession);
                activeSession.activeDaemon = null;
                return true;
            }
            foreach (Daemon daemon in activeSession.connectedNode.daemons)
            {
                if (daemon.IsOfType(target))
                {
                    if(activeSession.activeDaemon != null)
                        activeSession.activeDaemon.OnDisconnect(activeSession);
                    activeSession.activeDaemon = daemon;
                    daemon.OnConnect(activeSession);
                    return true;
                }
            }

            return true;
        }

        private int GenerateSessionId(Node node)
        {
            List<int> sessionIds = new List<int>();
            foreach (var session in node.sessions)
            {
                sessionIds.Add(session.sessionId);
            }
            foreach (var log in node.logs)
            {
                sessionIds.Add(log.sessionId);
            }
            Random rand = new Random();
            while (true)
            {
                int sessionId = rand.Next();
                if (!sessionIds.Contains(sessionId))
                    return sessionId;
            }
        }

        public void DisconnectSession()
        {
            ResetTrace();
            if (this.connectedNode != null)
                this.connectedNode.sessions.Remove(this);
            if(activeDaemon != null)
                activeDaemon.OnDisconnect(this);
            activeDaemon = null;
            connectedNode = null;
        }

        public void ResetTrace()
        {
            this.trace = 100;
            this.traceSpd = 0;
            owner.Send(PacketType.FX, "traceEnd");
        }

        public void SetTraceLevel(float spd)
        {
            this.traceSpd = spd;
            this.traceUpdtCooldown = 2f;
            owner.Send(PacketType.FX, "trace", this.trace.ToString(), this.traceSpd.ToString());
        }

        public void UpdateTrace(double dT)
        {

            if (this.traceSpd == 0)
                return;
            this.trace -= this.traceSpd * (float)dT;
            if(this.trace < 0)
            {
                this.owner.TraceTermination();
                owner.Send(PacketType.FX, "traceOver");
            }
            else if(this.trace > 100)
            {
                ResetTrace();
            }
            else if(this.trace < 100)
            {
                if (this.traceUpdtCooldown > 0)
                    traceUpdtCooldown -= (float)dT;
                else
                {
                    traceUpdtCooldown = 2f;
                    owner.Send(PacketType.FX, "trace", this.trace.ToString(), this.traceSpd.ToString());
                }
            }
        }
    }
}
