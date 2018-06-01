using HackLinks_Server.Daemons;
using HackLinks_Server.Daemons.Types;
using HackLinks_Server.Daemons.Types.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.Computers.Processes
{
    class MailClient : DaemonClient {
        public SortedDictionary<string, Tuple<string, Command>> commands = new SortedDictionary<string, Tuple<string, Command>>() {
            { "account", new Tuple<string, Command>("account\nShows account infomation.", Account)}
        };
        public override SortedDictionary<string, Tuple<string, Command>> Commands => commands;

        public MailClient(Session session, Daemon daemon, int pid, Printer printer, Node computer, Credentials credentials) : base(session, daemon, pid, printer, computer, credentials) {}

        public override bool RunCommand(string command) {
            // We hide the old runCommand function to perform this check on startup
            if (!((MailDaemon)Daemon).CheckFolders(this)) {
                return true;
            }
            return base.RunCommand(command);
        }

        public static bool Account(CommandProcess process, string[] command) {
            return true;
        }
    }
}
