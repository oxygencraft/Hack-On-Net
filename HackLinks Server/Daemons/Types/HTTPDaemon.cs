using HackLinks_Server.Computers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.Daemons.Types
{
    class HTTPDaemon : Daemon
    {
        public HTTPDaemon(Node node) : base(node)
        {

        }

        public override SortedDictionary<string, Tuple<string, CommandHandler.Command>> Commands => throw new NotImplementedException();
    }
}
