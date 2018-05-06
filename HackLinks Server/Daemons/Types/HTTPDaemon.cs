using HackLinks_Server.Computers;
using HackLinks_Server.Computers.Processes;
using HackLinks_Server.Daemons.Types.Http;
using HackLinks_Server.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.Daemons.Types
{
    class HTTPDaemon : Daemon
    {
        public WebPage defaultPage;

        public string websiteName;

        public List<WebPage> webPages = new List<WebPage>();

        public Dictionary<Session, HTTPClient> httpSessions = new Dictionary<Session, HTTPClient>();

        protected override Type ClientType => typeof(HTTPClient);


        public HTTPDaemon(int pid, Printer printer, Node computer, Credentials credentials) : base(pid,  printer, computer, credentials)
        {

        }

        public WebPage GetPage(string v)
        {
            foreach (WebPage page in webPages)
                if (page.title == v)
                    return page;
            return null;
        }

        public override string StrType => "http";

        public override void OnStartUp()
        {
            base.OnStartUp();
            LoadWebPages();
        }

        public override void OnConnect(Session connectSession, DaemonClient client)
        {
            base.OnConnect(connectSession, client);
            httpSessions.Add(connectSession, (HTTPClient)client);
            ((HTTPClient)client).SetActivePage(defaultPage);
        }

        public override void OnDisconnect(Session disconnectSession)
        {
            base.OnDisconnect(disconnectSession);

            httpSessions.Remove(disconnectSession);
        }

        public override string GetSSHDisplayName()
        {
            return "Open Website";
        }

        public void LoadWebPages()
        {
            File www = node.fileSystem.rootFile.GetFile("www");
            if (www == null || !www.IsFolder())
                return;
            foreach(File file in www.children)
            {
                WebPage newPage = WebPage.ParseFromFile(file);
                if (newPage == null)
                    return;
                webPages.Add(newPage);
                if(newPage.title == "index")
                {
                    this.defaultPage = newPage;
                }
            }
        }
    }
}
