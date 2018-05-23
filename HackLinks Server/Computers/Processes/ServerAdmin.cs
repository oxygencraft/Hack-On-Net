using HackLinks_Server.Files;
using HackLinksCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.Computers.Processes
{
    public class ServerAdmin : CommandProcess
    {
        private static SortedDictionary<string, Tuple<string, Command>> commands = new SortedDictionary<string, Tuple<string, Command>>()
        {
            { "admin", new Tuple<string, Command>("admin COMMAND [args]\n    Executes the given Server Admin command with args.", CommandExec) },
            { "trace", new Tuple<string, Command>("trace [over/start]\n    DEBUG COMMAND", TraceDebug) },
            { "giveperms", new Tuple<string, Command>("giveperms [username] [admin/kick/ban/giveperms]\n    Gives user permissions", GivePermissions) },
            { "kick", new Tuple<string, Command>("kick [username]\n    Kicks User", Kick) },
            { "ban", new Tuple<string, Command>("ban [username] [unban (t/f)] [permban (t/f)] [days] [hr] [mins]\n    Bans user for a specified amount of time", Ban) },
            { "unban", new Tuple<string, Command>("unban\n    Unbans a user", Unban) },
            { "changetheme", new Tuple<string, Command>("changetheme [filepathtotheme] (DEBUG COMMAND)", ChangeTheme) },
            { "compile", new Tuple<string, Command>("compile FILENAME TYPE", Compile) },
        };

        public override SortedDictionary<string, Tuple<string, Command>> Commands => commands;

        private GameClient client;

        public ServerAdmin(int pid, Printer printer, Node computer, Credentials credentials, GameClient client) : base(pid, printer, computer, credentials)
        {
            this.client = client;
        }

        public override void Run(string command)
        {
            if (client.permissions.Contains(HackLinks_Server.Permissions.Admin))
            {
                base.Run(command);
            } else
            {
                Print("Unauthorized access to privilaged resources detected, halting execution");
            }
        }

        public static bool CommandExec(CommandProcess process, string[] command)
        {
            if (command.Length > 1)
            {
                return process.RunCommand(command[1]);
            }
            process.Print(commands[command[0]].Item1);
            return true;
        }

        public static bool Compile(CommandProcess process, string[] command)
        {
            ServerAdmin serverAdmin = (ServerAdmin)process;
            if (command.Length != 2)
            {
                process.Print("Usage: compile FILENAME TYPE");
                return true;
            }
            string[] args = command[1].Split(' ');
            if(args.Length != 2)
            {
                process.Print("Usage: compile FILENAME TYPE");
                return true;
            }
            File file = process.ActiveDirectory.GetFile(args[0]);
            if (file == null)
            {
                process.Print("File not found");
                return true;
            }
            if (file.IsFolder())
            {
                process.Print("Invalid file, cannot be folder");
                return true;
            }
            serverAdmin.client.server.GetCompileManager().AddType(file.Checksum, args[1]);
            return true;
        }

        public static bool TraceDebug(CommandProcess process, string[] command)
        {
            GameClient client = ((ServerAdmin)process).client;

            if (command.Length != 2)
                return true;
            if (command[1] == "over")
            {
                client.TraceTermination();
                return true;
            }
            if (command[1] == "beep")
            {
                client.Send(NetUtil.PacketType.FX, "warnBlink");
            }
            if (command[1] == "trace")
            {
                client.Send(NetUtil.PacketType.FX, "trace", "100", "5");
            }
            if (command[1] == "traceEnd")
            {
                client.Send(NetUtil.PacketType.FX, "traceEnd");
            }
            if (command[1] == "realTrace")
            {
                if (client.activeSession != null)
                    client.activeSession.SetTraceLevel(5);
            }
            if (command[1] == "realMagic")
            {
                if (client.activeSession != null)
                    client.activeSession.SetTraceLevel(-5);
            }
            return true;
        }

        public static bool GivePermissions(CommandProcess process, string[] commandUnsplit)
        {
            GameClient client = ((ServerAdmin)process).client;

            if (client.permissions.Contains(HackLinks_Server.Permissions.Admin) == false && client.permissions.Contains(HackLinks_Server.Permissions.GivePerms) == false)
            {
                client.Send(NetUtil.PacketType.MESSG, "Insufficent Privileges");
                return true;
            }

            List<string> command = new List<string>();
            command.Add("giveperms");
            command.AddRange(commandUnsplit[1].Split(' '));
            if (command.Count < 3)
            {
                client.Send(NetUtil.PacketType.MESSG, "Usage: giveperms [username] [admin/kick/ban/giveperms]");
                return true;
            }
            if (!Server.Instance.DatabaseLink.GetUsersInDatabase().ContainsValue(command[1]))
            {
                client.Send(NetUtil.PacketType.MESSG, "User does not exist in the user database");
                return true;
            }

            List<HackLinks_Server.Permissions> permissions = Server.Instance.DatabaseLink.GetUserPermissions()[command[1]];


            if (command[2] == "admin")
            {
                permissions.Add(HackLinks_Server.Permissions.Admin);
            }
            if (command[2] == "kick")
            {
                permissions.Add(HackLinks_Server.Permissions.Kick);
            }
            if (command[2] == "ban")
            {
                permissions.Add(HackLinks_Server.Permissions.Ban);
            }
            if (command[2] == "giveperms")
            {
                permissions.Add(HackLinks_Server.Permissions.GivePerms);
            }
            Server.Instance.DatabaseLink.SetUserPermissions(command[1], permissions);
            foreach (var client2 in Server.Instance.clients)
            {
                if (client2.username == command[1])
                {
                    client = client2;
                }
            }
            client.permissions = permissions;
            return true;
        }

        public static bool Kick(Process process, string[] command)
        {
            GameClient client = ((ServerAdmin)process).client;

            if (client.permissions.Contains(HackLinks_Server.Permissions.Admin) == false && client.permissions.Contains(HackLinks_Server.Permissions.Kick) == false)
            {
                process.Print("Insufficent Privileges");
                return true;
            }
            if (command.Length < 2)
            {
                process.Print("Usage: kick [username]");
                return true;
            }
            GameClient targetClient = null;
            foreach (var client2 in Server.Instance.clients)
            {
                if (client2.username == command[1])
                {
                    targetClient = client2;
                    break;
                }
            }

            if (targetClient == null)
            {
                process.Print("The player isn't in the server");
                return true;
            }

            targetClient.Send(NetUtil.PacketType.DSCON, "You have been kicked from the server");
            targetClient.netDisconnect();

            return true;
        }

        public static bool Ban(CommandProcess process, string[] commandUnsplit)
        {
            GameClient client = ((ServerAdmin)process).client;

            List<string> command = new List<string>();
            command.Add("ban");
            command.AddRange(commandUnsplit[1].Split());

            if (client.permissions.Contains(HackLinks_Server.Permissions.Admin) == false && client.permissions.Contains(HackLinks_Server.Permissions.Ban) == false)
            {
                process.Print("Insufficent Privileges");
                return true;
            }
            if (command.Count < 3)
            {
                process.Print("Usage: ban [username] [unban (t/f)] [permban (t/f)] [days] [hr] [mins]");
                return true;
            }
            if (command.Count < 4)
            {
                Server.Instance.DatabaseLink.SetUserBanStatus(command[1], 0, false, command[3] == "t" ? true : false);
                return true;
            }
            int days = Convert.ToInt32(command[4]);
            int hours = command.Count <= 6 ? Convert.ToInt32(command[5]) : 0;
            int minutes = command.Count <= 7 ? Convert.ToInt32(command[6]) : 0;
            days = days * 86400;
            hours = hours * 3600;
            minutes = minutes * 60;
            int banExpiry = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() + days + hours + minutes;

            if (!Server.Instance.DatabaseLink.SetUserBanStatus(command[1], banExpiry, false, false))
                process.Print("The user does not exist in the user database");
            return true;
        }

        public static bool Unban(CommandProcess process, string[] command)
        {
            GameClient client = ((ServerAdmin)process).client;

            if (command.Length < 2)
            {
                client.Send(NetUtil.PacketType.MESSG, "Usage: unban [username]");
                return true;
            }
            Server.Instance.DatabaseLink.SetUserBanStatus(command[1], 0, true, false);
            return true;
        }

        public static bool ChangeTheme(CommandProcess process, string[] command)
        {
            GameClient client = ((ServerAdmin)process).client;

            if (command.Length < 2)
            {
                client.Send(NetUtil.PacketType.MESSG, "Usage: changetheme [filepathtotheme]");
                return true;
            }
            var file = process.ActiveDirectory.GetFile(command[1]);
            if (file == null)
            {
                client.Send(NetUtil.PacketType.MESSG, "File " + command[1] + " not found.");
                return true;
            }
            client.Send(NetUtil.PacketType.KERNL, "changetheme", file.Content);
            return true;
        }
    }
}
