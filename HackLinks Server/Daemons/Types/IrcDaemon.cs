using HackLinks_Server.Computers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HackLinks_Server.Daemons.Types.Irc;

namespace HackLinks_Server.Daemons.Types
{
    internal class IrcDaemon : Daemon
    {
        public List<IrcMessage> messages = new List<IrcMessage>();


        public IrcDaemon(Node node) : base(node)
        {

        }

        public override DaemonType GetDaemonType()
        {
            return DaemonType.IRC;
        }

        public override bool IsOfType(string strType)
        {
            return strType.ToLower() == "irc";
        }

        public override void OnConnect(Session connectSession)
        {
            base.OnConnect(connectSession);
            connectSession.owner.Send("MESSG:Connected to IRC Service");
            connectSession.owner.Send("KERNL:state;irc;join");
            var messageText = "";
            foreach (var message in messages)
                messageText += message.author + "`" + message.content + ";";
            connectSession.owner.Send("KERNL:state;irc;messg;" + messageText);
            SendMessage(new IrcMessage("ChanBot", connectSession.owner.username + " just logged in !"));
        }

        public override void OnDisconnect(Session disconnectSession)
        {
            base.OnDisconnect(disconnectSession);
        }

        public void SendMessage(IrcMessage message)
        {
            messages.Add(message);
            if (messages.Count > 60)
                messages.RemoveAt(0);
            foreach (Session session in this.connectedSessions)
            {
                if (session == null)
                    continue;
                session.owner.Send("KERNL:state;irc;messg;" + message.author + "`" + message.content);
            }
        }

        public override bool HandleDaemonCommand(Session session, string[] command)
        {
            if (command[0] == "irc")
            {
                if(command.Length < 2)
                {
                    session.owner.Send("MESSG:Usage : irc [send]");
                    return true;
                }
                var cmdArgs = command[1].Split(' ');
                if(cmdArgs.Length < 2)
                {
                    session.owner.Send("MESSG:Usage : irc [send]");
                    return true;
                }
                if (cmdArgs[0] == "send")
                {
                    var text = "";
                    for (int i = 1; i < cmdArgs.Length; i++)
                        text += cmdArgs[i] + (i != cmdArgs.Length ? " " : "");
                    SendMessage(new IrcMessage(session.owner.username, text));
                    return true;
                }
                session.owner.Send("MESSG:Usage : irc [send/logout]");
                return true;
            }
            return false;
        }
    }
}
