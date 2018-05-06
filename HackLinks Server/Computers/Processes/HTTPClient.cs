using HackLinks_Server.Computers;
using HackLinks_Server.Computers.Processes;
using HackLinks_Server.Daemons;
using HackLinks_Server.Daemons.Types;
using HackLinks_Server.Daemons.Types.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.Computers.Processes
{
    class HTTPClient : DaemonClient
    {
        public SortedDictionary<string, Tuple<string, Command>> daemonCommands = new SortedDictionary<string, Tuple<string, Command>>()
        {
            { "web", new Tuple<string, Command>("web [interface name] [arguments]\n    Use an interface on your current webpage.", Web) }
        };

        public override SortedDictionary<string, Tuple<string, Command>> Commands => daemonCommands;

        WebPage currentWebPage;
        public WebPage ActivePage { get { return currentWebPage; } }

        public new HTTPDaemon Daemon => (HTTPDaemon) base.Daemon;

        public HTTPClient(Session session, Daemon daemon, int pid, Printer printer, Node computer, Credentials credentials) : base(session, daemon, pid, printer, computer, credentials)
        {

        }

        public static bool Web(CommandProcess process, string[] arguments)
        {
            HTTPClient client = (HTTPClient) process;

            if (arguments.Length < 2)
            {
                process.Print("Usage : web [interface name] [arguments]");
                return true;
            }

            client.ActivePage.UseInterfaces(client, arguments[1].Split(' '));

            return true;
        }

        public void SetActivePage(WebPage page)
        {
            if (page == null)
                return;
            currentWebPage = page;
            page.SendWebPage(Session);
        }
    }
}
