using HackLinks_Server.Computers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.Daemons
{
    class Daemon
    {
        protected Node node;
        protected List<Session> connectedSessions = new List<Session>();

        public Daemon(Node node)
        {
            this.node = node;
        }

        public enum DaemonType
        {
            DEFAULT,
            IRC
        }

        public virtual DaemonType GetDaemonType()
        {
            return DaemonType.DEFAULT;
        }

        public virtual bool IsOfType(string strType)
        {
            return strType.ToLower() == "default";
        }

        public virtual bool HandleDaemonCommand(Session session, string[] command)
        {
            return false;
        }

        public virtual void OnConnect(Session connectSession)
        {
            connectedSessions.Add(connectSession);
        }

        public virtual void OnDisconnect(Session disconnectSession)
        {
            connectedSessions.Remove(disconnectSession);
        }

        
    }
}
