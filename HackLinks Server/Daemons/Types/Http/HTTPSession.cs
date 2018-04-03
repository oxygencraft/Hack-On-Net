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
        public HTTPDaemon Daemon { get { return daemon; } }
        Session session;
        public Session Session { get { return session; } }

        WebPage currentWebPage;
        public WebPage ActivePage { get { return currentWebPage; } }

        public HTTPSession(HTTPDaemon daemon, Session session)
        {
            this.session = session;
            this.daemon = daemon;
        }

        public void SetActivePage(WebPage page)
        {
            if (page == null)
                return;
            currentWebPage = page;
            page.SendWebPage(session);
        }
    }
}
