using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.Daemons.Types.Irc
{
    class IrcMessage
    {
        public string author;
        public string content;

        public IrcMessage(string author, string content)
        {
            this.author = author;
            this.content = content;
        }
    }
}
