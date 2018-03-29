using HackLinks_Server.Daemons.Types.Http.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HackLinks_Server.FileSystem;
using static HackLinksCommon.NetUtil;

namespace HackLinks_Server.Daemons.Types.Http
{
    class WebPage
    {
        public string content;
        public string title;

        Dictionary<string, WebInterface> interfaces = new Dictionary<string, WebInterface>();

        public void SendWebPage(Session session)
        {
            var commandData = new List<string>() { "state", "http", "page", title, content};

            session.owner.Send(PacketType.KERNL, commandData.ToArray());
        }

        public static WebPage ParseFromFile(File file)
        {
            WebPage page = new WebPage();
            page.title = file.name;
            page.content = file.content;
            return page;
        }
    }
}
