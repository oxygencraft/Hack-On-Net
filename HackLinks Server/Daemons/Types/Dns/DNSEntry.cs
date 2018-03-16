using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.Daemons.Types.Dns
{
    class DNSEntry
    {
        string IP;
        string URL;

        public DNSEntry(string ip, string url)
        {
            IP = ip;
            URL = url;
        }

        public string Ip
        {
            get
            {
                return IP;
            }
        }
        public string Url
        {
            get
            {
                return URL;
            }
        }

    }
}
