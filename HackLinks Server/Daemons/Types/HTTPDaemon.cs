using HackLinks_Server.Computers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.Daemons.Types
{
    class HTTPDaemon : Daemon
    {
        public string indexFile = "/http/index.html";

        public Dictionary<Session, HTTPSession> httpSessions = new Dictionary<Session, HTTPSession>();

        public HTTPDaemon(Node node) : base(node)
        {

        }

        public override SortedDictionary<string, Tuple<string, CommandHandler.Command>> Commands => throw new NotImplementedException();

        public override string StrType => "http";

        public override void OnStartUp()
        {
            base.OnStartUp();
        }

        public override void OnConnect(Session connectSession)
        {
            base.OnConnect(connectSession);

        }

        public override void OnDisconnect(Session disconnectSession)
        {
            base.OnDisconnect(disconnectSession);
        }

        public override string GetSSHDisplayName()
        {
            return "Open Website";
        }
    }
}
