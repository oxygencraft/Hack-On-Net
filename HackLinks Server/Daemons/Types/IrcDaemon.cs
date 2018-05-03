using HackLinks_Server.Computers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HackLinks_Server.Daemons.Types.Irc;
using static HackLinksCommon.NetUtil;
using HackLinks_Server.Computers.Permissions;
using HackLinks_Server.Computers.Processes;

namespace HackLinks_Server.Daemons.Types
{
    internal class IrcDaemon : Daemon
    {
        public List<IrcMessage> messages = new List<IrcMessage>();

        public override string StrType => "irc";

        protected override Type ClientType => typeof(IRCClient);

        public IrcDaemon(int pid, Printer printer, Node computer, Credentials credentials) : base(pid,  printer, computer, credentials)
        {
            this.accessLevel = Group.GUEST;
        }

        public override DaemonType GetDaemonType()
        {
            return DaemonType.IRC;
        }

        public override void OnConnect(Session connectSession, DaemonClient client)
        {
            base.OnConnect(connectSession, client);
            connectSession.owner.Send(PacketType.MESSG, "Connected to IRC Service");
            connectSession.owner.Send(PacketType.KERNL, "state", "irc", "join");
            var commandData = new List<string>() { "state", "irc", "messg" };
            foreach (IrcMessage message in messages)
            {
                commandData.AddRange(new string[] { message.author, message.content });
            }
            connectSession.owner.Send(PacketType.KERNL, commandData.ToArray());
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
                session.owner.Send(PacketType.KERNL, "state", "irc", "messg", message.author, message.content);
            }
        }

        public override string GetSSHDisplayName()
        {
            return "Open IRC";
        }
    }
}
