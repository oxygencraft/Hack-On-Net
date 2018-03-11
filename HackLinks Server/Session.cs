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

        public bool HandleSessionCommand(string[] command)
        {
            if(command[0] == "daemon")
            {
                if(command.Length != 2)
                {
                    owner.Send("MESSG:Usage : daemon [name of daemon]");
                    return true;
                }
                var target = command[1];
                foreach(Daemon daemon in connectedNode.daemons)
                {
                    if(daemon.IsOfType(target))
                    {
                        activeDaemon = daemon;
                        daemon.OnConnect(this);
                        return true;
                    }
                }
            }

            if (activeDaemon != null && activeDaemon.HandleDaemonCommand(this, command))
                return true;

            return false;
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
