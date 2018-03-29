using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HackLinks_Server.FileSystem;

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

        public void SetActivePage(WebPage page)
        {
            if (page == null)
                return;
            currentWebPage = page;
            page.SendWebPage(session);
        }
    }
}
