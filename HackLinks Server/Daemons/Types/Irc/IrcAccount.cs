using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.Daemons.Types.Irc
{
    class IrcAccount
    {
        public string username;
        public string rank;
        public string color;

        public IrcAccount(string username, string rank, string color)
        {
            this.username = username;
            this.rank = rank;
            this.color = color;
        }

        public string Username
        {
            get
            {
                return username;
            }
        }

        public string Rank
        {
            get
            {
                return rank;
            }
        }

        public string Color
        {
            get
            {
                return Color;
            }
        }
    }
}