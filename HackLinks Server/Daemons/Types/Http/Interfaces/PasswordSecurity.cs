using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.Daemons.Types.Http.Interfaces
{
    class PasswordSecurity : WebInterface
    {

        public string password;
        public string action;

        public new static WebInterface Instanciate(Dictionary<string, string> attributes)
        {
            var newInterface = new PasswordSecurity();
            if (!attributes.ContainsKey("password"))
                return null;
            if (!attributes.ContainsKey("action"))
                return null;

            newInterface.password = attributes["password"];
            newInterface.action = attributes["action"];

            return newInterface;
        }

        public override void Use(HTTPSession session, string[] args)
        {
            if(args.Length < 2)
            {
                session.Session.owner.Send(HackLinksCommon.NetUtil.PacketType.MESSG, "Password required");
                return;
            }
            if(args[1] == password)
            {
                RunAction(session, args);
            }
        }

        public override string GetClientDisplay()
        {
            return "PASSWORD FIELD : " + this.ID;
        }

        private void RunAction(HTTPSession session, string[] args)
        {
            var actionData = action.Split(new char[] { ':' }, 2);
            if(actionData[0] == "goto")
            {
                if (actionData.Length < 2)
                    return;
                session.SetActivePage(session.Daemon.GetPage(actionData[1]));
            }
        }
    }
}
