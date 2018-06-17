using HackLinks_Server.Computers;
using HackLinks_Server.Computers.Permissions;
using HackLinks_Server.Computers.Processes;
using HackLinks_Server.Daemons.Types.Dns;
using HackLinks_Server.Files;
using System;
using System.Collections.Generic;
using static HackLinksCommon.NetUtil;

namespace HackLinks_Server.Daemons.Types {
    class DNSDaemon : Daemon
    {
        public static string DEFAULT_CONFIG_PATH = "/dns/entries.db";

        public DNSDaemon(int pid, Printer printer, Node computer, Credentials credentials) : base(pid,  printer, computer, credentials)
        {
            this.accessLevel = Group.GUEST;
        }

        protected override Type ClientType => typeof(DNSClient);

        public List<DNSEntry> entries = new List<DNSEntry>();

        public override string StrType => "dns";

        public override DaemonType GetDaemonType()
        {
            return DaemonType.DNS;
        }

        public string LookUp(string url, bool returnUrl = false)
        {
            foreach(DNSEntry entry in entries)
                if (entry.Url == url)
                    return entry.Ip;
            if (returnUrl)
                return url;
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
            File entryFile = node.fileSystem.rootFile.GetFileAtPath(DEFAULT_CONFIG_PATH);
            if (entryFile == null)
                return;
            foreach (string line in entryFile.Content.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                var data = line.Split(new char[] { ':', '=' });
                if (data.Length < 2)
                    continue;
                entries.Add(new DNSEntry(data[0], data[1]));
            }
        }

        public override void OnConnect(Session connectSession, DaemonClient client)
        {
            base.OnConnect(connectSession, client);
            connectSession.owner.Send(PacketType.MESSG, "Opening DNS service");
            connectSession.owner.Send(PacketType.KERNL, "state", "dns", "open");
        }

        public override void OnDisconnect(Session disconnectSession)
        {
            base.OnDisconnect(disconnectSession);
        }

        public override string GetSSHDisplayName()
        {
            return null;
        }
    }
}
