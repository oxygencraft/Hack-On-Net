using HackLinks_Server.Daemons.Types.Http.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.Daemons.Types.Http
{
    class WebPage
    {
        string content;

        Dictionary<string, WebInterface> interfaces = new Dictionary<string, WebInterface>();

        public void SendWebPage(Session session)
        {

        }
    }
}
