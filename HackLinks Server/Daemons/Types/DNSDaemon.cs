using HackLinks_Server.Computers;
using HackLinks_Server.Daemons.Types.Dns;
using HackLinks_Server.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HackLinksCommon.NetUtil;

namespace HackLinks_Server.Daemons.Types
{
    class DNSDaemon : Daemon
    {
        public DNSDaemon(Node node) : base(node)
        {

        }

        public List<DNSEntry> entries = new List<DNSEntry>();

        public SortedDictionary<string, Tuple<string, CommandHandler.Command>> daemonCommands = new SortedDictionary<string, Tuple<string, CommandHandler.Command>>()
        {
            { "dns", new Tuple<string, CommandHandler.Command>("dns [lookup/rlookup] [URL/IP]\n    Get the lookup of the specified URL/IP.", Dns) },
        };

        public override SortedDictionary<string, Tuple<string, CommandHandler.Command>> Commands
        {
            get => daemonCommands;
        }

        public override DaemonType GetDaemonType()
        {
            return DaemonType.DNS;
        }
        public override bool IsOfType(string strType)
        {
            return strType.ToLower() == "dns";
        }

        public static bool Dns(GameClient client, string[] command)
        {
            Session session = client.activeSession;

            DNSDaemon daemon = (DNSDaemon)client.activeSession.activeDaemon;

            if (command[0] == "dns")
            {
                if (command.Length < 2)
                {
                    session.owner.Send(PacketType.MESSG, "Usage : dns [lookup/rlookup] [URL/IP]");
                    return true;
                }
                var cmdArgs = command[1].Split(' ');
                if (cmdArgs[0] == "update")
                {
                    if(session.privilege > 1)
                    {
                        session.owner.Send(PacketType.MESSG, "Permission denied");
                        return true;
                    }
                    daemon.LoadEntries();
                    session.owner.Send(PacketType.MESSG, "Successfully updated the DNS.");
                    return true;
                }
                if (cmdArgs.Length != 2)
                {
                    session.owner.Send(PacketType.MESSG, "Usage : dns [lookup/rlookup] [URL/IP]");
                    return true;
                }
                if (cmdArgs[0] == "lookup")
                {
                    var url = cmdArgs[1];
                    var ip = daemon.LookUp(url);
                    session.owner.Send(PacketType.MESSG, "Result IP : " + (ip ?? "unknown"));
                    return true;
                }
                if (cmdArgs[0] == "rlookup")
                {
                    var ip = cmdArgs[1];
                    var url = daemon.RLookUp(ip);
                    session.owner.Send(PacketType.MESSG, "Result URL : " + (url ?? "unknown"));
                    return true;
                }
                
                session.owner.Send(PacketType.MESSG, "Usage : dns [lookup/rlookup] [URL/IP]");
                return true;
            }
            return false;
        }

        public string LookUp(string url)
        {
            foreach(DNSEntry entry in entries)
                if (entry.Url == url)
                    return entry.Ip;
            return null;
        }

        public string RLookUp(string ip)
        {
            foreach (DNSEntry entry in entries)
                if (entry.Ip == ip)
                    return entry.Url;
            return null;
        }

        public override void OnStartUp()
        {
            LoadEntries();
        }

        public void LoadEntries()
        {
            this.entries.Clear();
            File entryFile = node.rootFolder.GetFileAtPath("/daemons/dns/entries.db");
            if (entryFile == null)
                return;
            foreach (string line in entryFile.content.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                var data = line.Split(':');
                entries.Add(new DNSEntry(data[1], data[0]));
            }
        }

        public override void OnConnect(Session connectSession)
        {
            base.OnConnect(connectSession);
            connectSession.owner.Send(PacketType.MESSG, "Opening DNS service");
            connectSession.owner.Send(PacketType.KERNL, "state;dns;open");
        }

        public override void OnDisconnect(Session disconnectSession)
        {
            base.OnDisconnect(disconnectSession);
        }

        public override bool HandleDaemonCommand(GameClient client, string[] command)
        {
            if (Commands.ContainsKey(command[0]))
                return Commands[command[0]].Item2(client, command);

            return false;
        }
    }
}
