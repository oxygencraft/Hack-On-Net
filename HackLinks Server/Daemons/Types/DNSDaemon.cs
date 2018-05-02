using HackLinks_Server.Computers;
using HackLinks_Server.Computers.Permissions;
using HackLinks_Server.Computers.Processes;
using HackLinks_Server.Daemons.Types.Dns;
using HackLinks_Server.Files;
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
        public static string DEFAULT_CONFIG_PATH = "/dns/entries.db";

        public DNSDaemon(int pid, Printer printer, Node computer, Credentials credentials) : base(pid,  printer, computer, credentials)
        {
            this.accessLevel = Group.GUEST;
        }

        public List<DNSEntry> entries = new List<DNSEntry>();

        public SortedDictionary<string, Tuple<string, Command>> daemonCommands = new SortedDictionary<string, Tuple<string, Command>>()
        {
            { "dns", new Tuple<string, Command>("dns [lookup/rlookup] [URL/IP]\n    Get the lookup of the specified URL/IP.", Dns) },
        };

        public override SortedDictionary<string, Tuple<string, Command>> Commands
        {
            get => daemonCommands;
        }

        public override string StrType => "dns";

        public override DaemonType GetDaemonType()
        {
            return DaemonType.DNS;
        }

        public static bool Dns(CommandProcess process, string[] command)
        {
            DNSDaemon daemon = (DNSDaemon) process;

            if (command[0] == "dns")
            {
                if (command.Length < 2)
                {
                    daemon.Print("Usage : dns [lookup/rlookup] [URL/IP]");
                    return true;
                }
                var cmdArgs = command[1].Split(' ');
                if (cmdArgs[0] == "update")
                {
                    if(PermissionHelper.CheckCredentials(process.Credentials, Group.ADMIN))
                    {
                        daemon.Print("Permission denied");
                        return true;
                    }
                    daemon.LoadEntries();
                    daemon.Print("Successfully updated the DNS.");
                    return true;
                }
                if (cmdArgs[0] == "lookup")
                {
                    var url = cmdArgs[1];
                    var ip = daemon.LookUp(url);
                    daemon.Print("Result IP : " + (ip ?? "unknown"));
                    return true;
                }
                if (cmdArgs[0] == "rlookup")
                {
                    var ip = cmdArgs[1];
                    var url = daemon.RLookUp(ip);
                    daemon.Print("Result URL : " + (url ?? "unknown"));
                    return true;
                }
                if (cmdArgs[0] == "assign")
                {
                    if(PermissionHelper.CheckCredentials(process.Credentials, Group.ADMIN))
                    {
                        daemon.Print("Insufficient permission.");
                        return true;
                    }
                    if (cmdArgs.Length <= 2)
                    {
                        daemon.Print("Missing arguments.\nProper usage: dns assign [IP] [URL]");
                        return true;
                    }
                    File dnsFolder = daemon.node.fileSystem.rootFile.GetFile("dns");
                    if (dnsFolder == null)
                    {
                        dnsFolder = daemon.node.fileSystem.CreateFile(daemon.computer, daemon.node.fileSystem.rootFile, "dns");
                        dnsFolder.OwnerId = 0; // root
                        dnsFolder.isFolder = true;
                    }
                    else
                    {
                        if (!dnsFolder.IsFolder())
                            return true;
                    }
                    File dnsEntries = dnsFolder.GetFile("entries.db");
                    if (dnsEntries == null)
                    {
                        dnsEntries = daemon.node.fileSystem.CreateFile(process.computer, dnsFolder, "entries.db");
                        dnsEntries.OwnerId = 0; // root
                        dnsEntries.Permissions.SetPermission(FilePermissions.PermissionType.Group, true, true, true);
                        dnsEntries.Group = Group.ADMIN;
                    }
                    else if (dnsEntries.IsFolder())
                    {
                        dnsEntries.RemoveFile();
                        dnsEntries = daemon.node.fileSystem.CreateFile(process.computer, dnsFolder, "entries.db");
                        dnsEntries.OwnerId = 0; // root
                        dnsEntries.Permissions.SetPermission(FilePermissions.PermissionType.Group, true, true, true);
                        dnsEntries.Group = Group.ADMIN;
                    }
                    foreach (DNSEntry entry in daemon.entries)
                    {
                        if (entry.Url == cmdArgs[2])
                        {
                            daemon.Print("The provided URL is already assigned an IP address.");
                            return true;
                        }
                    }
                    dnsEntries.Content += '\n' + cmdArgs[1] + '=' + cmdArgs[2];
                    daemon.LoadEntries();
                    daemon.Print("Content appended.");
                    return true;
                }
                daemon.Print("Usage : dns [lookup/rlookup] [URL/IP]");
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

        public override void OnConnect(Session connectSession)
        {
            base.OnConnect(connectSession);
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
