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

        public void SetNodeInfo(string command)
        {
            var args = command.Split(';');
            if (args[1] != "none")
                serverName = args[1];
            var daemonDisplays = args[2].Split('`');
            foreach(string button in daemonDisplays)
            {
                var buttonArgs = button.Split(',');
                if (buttonArgs.Length != 2)
                    continue;
                daemonButtons.Add(new DaemonButton(buttonArgs[0], buttonArgs[1]));
            }
        }
    }
}
