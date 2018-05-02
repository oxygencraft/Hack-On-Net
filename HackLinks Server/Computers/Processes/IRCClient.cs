using HackLinks_Server.Computers;
using HackLinks_Server.Computers.Processes;
using HackLinks_Server.Daemons;
using HackLinks_Server.Daemons.Types;
using HackLinks_Server.Daemons.Types.Irc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.Computers.Processes
{
    class IRCClient : DaemonClient
    {
        public SortedDictionary<string, Tuple<string, Command>> daemonCommands = new SortedDictionary<string, Tuple<string, Command>>()
        {
            { "irc", new Tuple<string, Command>("irc send [text to send]\n    Send a message via the connected IRC daemon.", Irc) },
        };

        public override SortedDictionary<string, Tuple<string, Command>> Commands
        {
            get => daemonCommands;
        }

        public IRCClient(Session session, Daemon daemon, int pid, Printer printer, Node computer, Credentials credentials) : base(session, daemon, pid, printer, computer, credentials)
        {

        }

        private static bool Irc(CommandProcess process, string[] command)
        {
            IRCClient client = (IRCClient)process;
            IrcDaemon daemon = (IrcDaemon)client.Daemon;

            if (command[0] == "irc")
            {
                if (command.Length < 2)
                {
                    process.Print("Usage : irc [send]");
                    return true;
                }
                var cmdArgs = command[1].Split(' ');
                if (cmdArgs.Length < 2)
                {
                    process.Print("Usage : irc [send]");
                    return true;
                }
                if (cmdArgs[0] == "send")
                {
                    var text = "";
                    for (int i = 1; i < cmdArgs.Length; i++)
                        text += cmdArgs[i] + (i != cmdArgs.Length ? " " : "");
                    daemon.SendMessage(new IrcMessage(process.computer.GetUsername(process.Credentials.UserId), text));
                    return true;
                }
                process.Print("Usage : irc [send]");
                return true;
            }
            return false;
        }
    }
}
