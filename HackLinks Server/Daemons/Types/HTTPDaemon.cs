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

        public Dictionary<Session, HTTPSession> httpSessions = new Dictionary<Session, HTTPSession>();

        public HTTPDaemon(int pid, Printer printer, Node computer, Credentials credentials) : base(pid,  printer, computer, credentials)
        {

        }

        public SortedDictionary<string, Tuple<string, Command>> daemonCommands = new SortedDictionary<string, Tuple<string, Command>>()
        {
            { "web", new Tuple<string, Command>("web [interface name] [arguments]\n    Use an interface on your current webpage.", Web) }
        };

        public override SortedDictionary<string, Tuple<string, Command>> Commands
        {
            get => daemonCommands;
        }

        public static bool Web(CommandProcess process, string[] arguments)
        {
            return true;
            if(arguments.Length < 2)
            {
                process.Print("Usage : web [interface name] [arguments]");
                return true;
            }

            HTTPDaemon daemon = (HTTPDaemon) process;

            // TODO this
            //var httpSession = daemon.httpSessions[session];

            //httpSession.ActivePage.UseInterfaces(httpSession, arguments[1].Split(' '));

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
