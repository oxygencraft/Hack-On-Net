using HackLinks_Server.Computers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.Daemons.Types
{
    class DNSDaemon : Daemon
    {
        public DNSDaemon(Node node) : base(node)
        {

        }

        public SortedDictionary<string, Tuple<string, CommandHandler.Command>> daemonCommands = new SortedDictionary<string, Tuple<string, CommandHandler.Command>>()
        {
            { "dns", new Tuple<string, CommandHandler.Command>("dns [lookup/rlookup] [URL/IP]\n    Get the lookup of the specified URL/IP.", Dns) },
        };

        public override SortedDictionary<string, Tuple<string, CommandHandler.Command>> Commands
        {
            get => daemonCommands;
        }

        public override DaemonType GetDaemonType()
        {
            return DaemonType.DNS;
        }
        public override bool IsOfType(string strType)
        {
            return strType.ToLower() == "dns";
        }

        public static bool Dns(GameClient client, string[] command)
        {
            return true;
        }

        public override bool HandleDaemonCommand(GameClient client, string[] command)
        {
            if (Commands.ContainsKey(command[0]))
                return Commands[command[0]].Item2(client, command);

            return false;
        }
    }
}
