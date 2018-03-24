using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.Daemons.Types.Http
{
    class HTTPSession
    {
        HTTPDaemon daemon;
        Session session;

        WebPage currentWebPage;

        public HTTPSession(HTTPDaemon daemon, Session session)
        {
            this.session = session;
            this.daemon = daemon;
        }
    }
}
