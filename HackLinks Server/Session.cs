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
        private Process attachedProcess;

        public bool HasProcessId(int pid)
        {
            return attachedProcess.ProcessId == pid;
        }

        public void AttachProcess(Process process)
        {
            attachedProcess = process;
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
            //TODO kill process
            connectedNode = null;
        }

        public void ResetTrace()
        {
            this.trace = 100;
            this.traceSpd = 0;
            if(owner.status != PlayerStatus.DISCONNECTING)
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
