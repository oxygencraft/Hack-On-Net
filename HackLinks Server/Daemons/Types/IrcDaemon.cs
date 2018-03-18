using HackLinks_Server.Computers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HackLinks_Server.Daemons.Types.Irc;
using static HackLinksCommon.NetUtil;
using HackLinks_Server.FileSystem;

namespace HackLinks_Server.Daemons.Types
{
    internal class IrcDaemon : Daemon
    {
        public static string DEFAULT_CONFIG_PATH = "/daemons/irc/config.conf";
        public List<IrcMessage> messages = new List<IrcMessage>();
        public List<string> config = new List<string>();

        public SortedDictionary<string, Tuple<string, CommandHandler.Command>> daemonCommands = new SortedDictionary<string, Tuple<string, CommandHandler.Command>>()
        {
            { "irc", new Tuple<string, CommandHandler.Command>("irc send [text to send]\n    Send a message via the connected IRC daemon.", Irc) },
        };

        public override SortedDictionary<string, Tuple<string, CommandHandler.Command>> Commands
        {
            get => daemonCommands;
        }

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

        public override void OnStartUp()
        {
            Console.WriteLine("IRC Daemon started");
            LoadConfig();
        }

        public override void OnConnect(Session connectSession)
        {
            base.OnConnect(connectSession);
            connectSession.owner.Send(PacketType.MESSG, "Connected to IRC Channel: #"+ IrcConfig.configData[0].Split(new string[] { "\"" }, StringSplitOptions.RemoveEmptyEntries)[1].Replace(" ", "_"));
            connectSession.owner.Send(PacketType.KERNL, "state;irc;join");
            var messageText = "";
            foreach (var message in messages)
                messageText += message.author + "`" + message.content + ";";
            connectSession.owner.Send(PacketType.KERNL, "state;irc;messg;" + messageText);
            Console.WriteLine("IRC:" + connectSession.owner.activeSession.connectedNode.ip + ": " + connectSession.owner.username + " Has Joined the IRC Channel");
            SendMessage(new IrcMessage(IrcConfig.configData[1].Split(new string[] { "\"" }, StringSplitOptions.RemoveEmptyEntries)[1].Replace(" ", "_"), connectSession.owner.username + " just logged in !"));
        }

        public override void OnDisconnect(Session disconnectSession)
        {
            Console.WriteLine("IRC:"+ disconnectSession.owner.activeSession.connectedNode.ip+": "+ disconnectSession.owner.username+" Has left the IRC Channel");
            SendMessage(new IrcMessage(IrcConfig.configData[1].Split(new string[] { "\"" }, StringSplitOptions.RemoveEmptyEntries)[1].Replace(" ", "_"), disconnectSession.owner.username + "Just left :("));
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
                session.owner.Send(PacketType.KERNL, "state;irc;messg;" + message.author + "`" + message.content);
                session.owner.Send(PacketType.KERNL, "state;irc;admin;" + IrcConfig.configData[2].Split(new string[] { "\"" }, StringSplitOptions.RemoveEmptyEntries)[1]);
            }
        }

        private static bool Irc(GameClient client, string[] command)
        {

            Session session = client.activeSession;

            IrcDaemon daemon = (IrcDaemon) client.activeSession.activeDaemon;

            if (command[0] == "irc")
            {
                if (command.Length < 2)
                {
                    session.owner.Send(PacketType.MESSG, "Usage : irc [send]");
                    return true;
                }
                var cmdArgs = command[1].Split(' ');
                if (cmdArgs.Length < 2)
                {
                    session.owner.Send(PacketType.MESSG, "Usage : irc [send]");
                    return true;
                }
                if (cmdArgs[0] == "send")
                {
                    var text = "";
                    for (int i = 1; i < cmdArgs.Length; i++)
                        text += cmdArgs[i] + (i != cmdArgs.Length ? " " : "");
                    daemon.SendMessage(new IrcMessage(session.owner.username, text));
                    return true;
                }
                session.owner.Send(PacketType.MESSG, "Usage : irc [send/logout]");
                return true;
            }
            return false;
        }

        public void LoadConfig()
        {
            File entryFile = node.rootFolder.GetFileAtPath(DEFAULT_CONFIG_PATH);
            if (entryFile == null)
                return;
            string[] a = entryFile.content.Split(("\r\n").ToCharArray());
            string admin = a[4].Replace(',', ' ');

            IrcConfig.configData[0] = a[0]; // irc channel name
            IrcConfig.configData[1] = a[2]; // bot name
            IrcConfig.configData[2] = admin; // admins
            
        }

        public static class IrcConfig
        {
            public static string[] configData = new string[999];
        }

        public override bool HandleDaemonCommand(GameClient client, string[] command)
        {
            if (Commands.ContainsKey(command[0]))
                return Commands[command[0]].Item2(client, command);

            return false;
        }
    }
}
