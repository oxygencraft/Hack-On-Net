using HackLinks_Server.Computers;
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

        public Dictionary<Session, HTTPSession> httpSessions = new Dictionary<Session, HTTPSession>();

        public HTTPDaemon(Node node) : base(node)
        {

        }

        public SortedDictionary<string, Tuple<string, CommandHandler.Command>> daemonCommands = new SortedDictionary<string, Tuple<string, CommandHandler.Command>>()
        {
            { "web", new Tuple<string, CommandHandler.Command>("web [interface name] [arguments]\n    Use an interface on your current webpage.", Web) }
        };

        public override SortedDictionary<string, Tuple<string, CommandHandler.Command>> Commands
        {
            get => daemonCommands;
        }

        public static bool Web(GameClient client, string[] arguments)
        {
            if(arguments.Length < 2)
            {
                client.Send(HackLinksCommon.NetUtil.PacketType.MESSG, "Usage : web [interface name] [arguments]");
                return true;
            }
            Session session = client.activeSession;

            HTTPDaemon daemon = (HTTPDaemon)client.activeSession.activeDaemon;

            var httpSession = daemon.httpSessions[session];

            httpSession.ActivePage.UseInterfaces(httpSession, arguments[1].Split(' '));

            return true;
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

        public override void OnConnect(Session connectSession)
        {
            base.OnConnect(connectSession);
            var newHTTPSession = new HTTPSession(this, connectSession);
            httpSessions.Add(connectSession, newHTTPSession);
            newHTTPSession.SetActivePage(defaultPage);
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
            File wwwF = node.rootFolder.GetFile("www");
            if (wwwF == null || !wwwF.IsFolder())
                return;
            Folder www = (Folder)wwwF;
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
