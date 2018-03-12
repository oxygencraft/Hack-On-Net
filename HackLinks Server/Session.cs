using HackLinks_Server.Computers;
using HackLinks_Server.Daemons;
using HackLinks_Server.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server
{
    class Session
    {

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

        public GameClient owner;
        public Node connectedNode;
        public Daemon activeDaemon;

        public Folder activeDirectory;

        public int privilege = 3;
        public string currentUsername = "Guest";

        public Session(GameClient client, Node node)
        {
            this.connectedNode = node;
            this.activeDirectory = node.rootFolder;
            this.owner = client;
            node.sessions.Add(this);
        }

        public void Login(string level, string username)
        {
            if (level == "root")
                privilege = 0;
            else if (level == "admin")
                privilege = 1;
            else if (level == "user")
                privilege = 2;
            else if (level == "guest")
                privilege = 3;
            currentUsername = username;

            owner.Send("KERNL:login;" + privilege + ";" + username);
        }

        public bool HandleSessionCommand(GameClient client, string[] command)
        {
            if (Commands.ContainsKey(command[0]))
                return Commands[command[0]].Item2(client, command);

            return false;
        }

        private static bool Daemon(GameClient client, string[] command)
        {
            Session activeSession = client.activeSession;

            if (command.Length != 2)
            {
                activeSession.owner.Send("MESSG:Usage : daemon [name of daemon]");
                return true;
            }
            var target = command[1];
            foreach (Daemon daemon in activeSession.connectedNode.daemons)
            {
                if (daemon.IsOfType(target))
                {
                    activeSession.activeDaemon = daemon;
                    daemon.OnConnect(activeSession);
                    return true;
                }
            }

            return true;
        }

        public void DisconnectSession()
        {
            if (this.connectedNode != null)
                this.connectedNode.sessions.Remove(this);
            if(activeDaemon != null)
                activeDaemon.OnDisconnect(this);
            activeDaemon = null;
            connectedNode = null;
        }
    }
}
