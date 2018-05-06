using Hacknet;
using HackOnNet.Sessions.States;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackOnNet.Sessions
{
    class Session
    {
        public string ip = "";

        public string accountName = "Guest";
        public int privilege = 3;

        public string workingPath = "/";

        public struct DaemonButton
        {
            public string Command;
            public string Display;

            public DaemonButton(string c, string d)
            {
                Command = c;
                Display = d;
            }
        }

        public List<DaemonButton> daemonButtons = new List<DaemonButton>();
        public string serverName = null;

        public SessionState currentState;

        public Session(string ip, int privilege)
        {
            this.ip = ip;
            this.privilege = privilege;
        }

        public SessionState GetState()
        {
            if(currentState == null)
            {
                currentState = new DefaultState(this);
            }
            return currentState;
        }

        public void SetState(SessionState newSessionState)
        {
            currentState = newSessionState;
        }

        public string GetRankName()
        {
            return privilege == 3 ? "Guest" : privilege == 2 ? "User" : privilege == 1 ? "Administrator" : "root";
        }

        public void SetNodeInfo(string[] command)
        {
            daemonButtons.Clear();
            if (command[1] != "none")
                serverName = command[1];
            for(int i = 2; i < command.Length; i += 2)
            {
                daemonButtons.Add(new DaemonButton(command[i], command[i + 1]));
            }
        }
    }
}
