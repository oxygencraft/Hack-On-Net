using Hacknet;
using HackOnNet.Sessions.States;
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

        public List<Daemon> daemons = new List<Daemon>();

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
    }
}
