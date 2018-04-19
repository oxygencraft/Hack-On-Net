using HackLinks_Server.Computers;
using HackLinks_Server.Computers.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.Daemons
{
    public abstract class Daemon
    {
        protected Node node;
        protected List<Session> connectedSessions = new List<Session>();
        protected Group accessLevel = Group.ROOT;

        public abstract string StrType
        {
            get;
        }
        //This should be created and populated by the implementing class
        public abstract SortedDictionary<string, Tuple<string, CommandHandler.Command>> Commands { get; }

        public Daemon(Node node)
        {
            this.node = node;
            OnStartUp();
        }

        public enum DaemonType
        {
            DEFAULT,
            IRC,
            DNS
        }

        public virtual DaemonType GetDaemonType()
        {
            return DaemonType.DEFAULT;
        }

        public bool IsOfType(string strType)
        {
            return StrType == strType.ToLower();
        }

        public virtual bool HandleDaemonCommand(GameClient client, string[] command)
        {
            return false;
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
            foreach(Group group in session.Groups)
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
