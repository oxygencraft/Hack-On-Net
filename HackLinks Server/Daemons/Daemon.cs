using HackLinks_Server.Computers;
using HackLinks_Server.Computers.Permissions;
using HackLinks_Server.Computers.Processes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.Daemons
{
    public abstract class Daemon : CommandProcess
    {
        protected Node node;
        protected List<Session> connectedSessions = new List<Session>();
        protected Group accessLevel = Group.ROOT;

        public abstract string StrType
        {
            get;
        }

        public Daemon(int pid, Printer printer, Node computer, Credentials credentials) : base(pid,  printer, computer, credentials)
        {
            node = computer;
            OnStartUp();
        }

        public enum DaemonType
        {
            DEFAULT,
            IRC,
            DNS,
            BANK
        }

        public virtual DaemonType GetDaemonType()
        {
            return DaemonType.DEFAULT;
        }

        public bool IsOfType(string strType)
        {
            return StrType == strType.ToLower();
        }

        public virtual void OnStartUp() { }

        public virtual void OnConnect(Session connectSession)
        {
            connectedSessions.Add(connectSession);
        }

        public virtual void OnDisconnect(Session disconnectSession)
        {
            connectedSessions.Remove(disconnectSession);
        }

        public bool CanBeAccessedBy(Session session)
        {
            foreach(Group group in Credentials.Groups)
            {
                if(group <= accessLevel)
                {
                    return true;
                }
            }
            return false;
        }

        public virtual string GetSSHDisplayName()
        {
            return null;
        }
    }
}
