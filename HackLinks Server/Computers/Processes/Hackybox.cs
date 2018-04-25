using HackLinks_Server.Daemons.Types;
using HackLinks_Server.Files;
using HackLinksCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.Computers.Processes
{
    public class Hackybox : CommandProcess
    {
        private static SortedDictionary<string, Tuple<string, Command>> commands = new SortedDictionary<string, Tuple<string, Command>>()
        {
            { "hackybox", new Tuple<string, Command>("hackybox COMMAND [args]\n    Executes the given HackyBox command with args.", CommandExec) },
            { "ping", new Tuple<string, Command>("ping [ip]\n    Outputs success if there is system online at the given IP.", Ping) },
            { "connect", new Tuple<string, Command>("connect [ip]\n    Connect to the system at the given IP.", Connect) },
            { "disconnect", new Tuple<string, Command>("disconnect \n    Terminate the current connection.", Disconnect) },
            { "dc", new Tuple<string, Command>("dc \n    Alias for disconnect.", Disconnect) },
            { "ls", new Tuple<string, Command>("ls \n    Lists all files in current directory.", Ls) },
            { "cd", new Tuple<string, Command>("cd [dir]\n    Moves current working directory to the specified directory.", ChangeDirectory) },
            { "touch", new Tuple<string, Command>("touch [file]\n    Create the given file if it doesn't already exist.", Touch) },
            { "view", new Tuple<string, Command>("view [file]\n    Displays the given file on the Display Module.", View)},
            { "mkdir", new Tuple<string, Command>("mkdir [dir]\n    Create the given directory if it doesn't already exist.", MkDir) },
            { "rm", new Tuple<string, Command>("rm [file]\n    Remove the given file.", Remove) },
            { "login", new Tuple<string, Command>("login [username] [password]\n    Login to the current connected system using the given username and password.", Login) },
            { "chown", new Tuple<string, Command>("chown [file] [username]\n    Change the required user level for read and write operations on the given file.", ChOwn) },
            { "chmod", new Tuple<string, Command>("chmod [mode] [file]\n    Change the required user level for read and write operations on the given file.\n", ChMod) },
            { "fedit", new Tuple<string, Command>("fedit [append/line/remove/insert/help]\n     Edits the given file according to the mode used.", Fedit) },
            { "help", new Tuple<string, Command>("help [page]\n    Displays the specified page of commands.", Help) },
            { "trace", new Tuple<string, Command>("trace [over/start]\n    DEBUG COMMAND", TraceDebug) },
            { "giveperms", new Tuple<string, Command>("giveperms [username] [admin/kick/ban/giveperms]\n    Gives user permissions", GivePermissions) },
            { "kick", new Tuple<string, Command>("kick [username]\n    Kicks User", Kick) },
            { "ban", new Tuple<string, Command>("ban [username] [unban (t/f)] [permban (t/f)] [days] [hr] [mins]\n    Bans user for a specified amount of time", Ban) },
            { "unban", new Tuple<string, Command>("unban\n    Unbans a user", Unban) },
            { "netmap", new Tuple<string, Command>("netmap [ip] [x] [y]\n    Adds a node to the network map", AddToNetMap) },
            { "music", new Tuple<string, Command>("music [file ((DLC\\)Music\\NameOfFile)] [playimmediately (0/1)] (DEBUG COMMAND)", PlayMusic) },
        };

        public override SortedDictionary<string, Tuple<string, Command>> Commands => commands;

        public Hackybox(long pid, long ppid, Printer printer, Node computer, Credentials credentials) : base(pid, ppid, printer, computer, credentials)
        {
        }

        public static bool CommandExec(CommandProcess process, string[] command)
        {
            if(command.Length > 1)
            {
                return process.RunCommand(command[1]);
            }
            process.Print(commands[command[0]].Item1);
            return true;
        }

        public static bool Fedit(CommandProcess process, string[] command)
        {
            if (command.Length < 2)
            {
                process.Print("Usage : fedit [append/line/remove/insert/help]");
                return true;
            }
            var cmdArgs = command[1].Split(' ');
            if (cmdArgs[0] == "help")
            {
                process.Print("fedit [append] [file] [text] - Appends 'text' in a new line, at the bottom of the file.\n" +
                    "fedit [line] [file] [n] [text] - Changes content of line 'n' to 'text'.\n" +
                    "fedit [remove] [file] [n] - Removes line 'n' of the file.\n" +
                    "fedit [insert] [file] [n] [text] - Insert a new line containing 'text' in the 'n' line number.");
                return true;
            }
            if (cmdArgs[0] == "append")
            {
                if (cmdArgs.Length < 3)
                {
                    process.Print("Missing arguments");
                    return true;
                }
                var file = process.ActiveDirectory.GetFile(cmdArgs[1]);
                if (file == null)
                {
                    process.Print("File " + cmdArgs[1] + " not found.");
                    return true;
                }
                if (!file.HasWritePermission(process.Credentials))
                {
                    process.Print("Permission denied.");
                    return true;
                }

                file.Content += '\n' + cmdArgs.JoinWords(" ", 2);
                process.Print("Content appended.");
                return true;
            }
            if (cmdArgs[0] == "line")
            {
                if (cmdArgs.Length < 3)
                {
                    process.Print("Missing arguments");
                    return true;
                }
                var file = process.ActiveDirectory.GetFile(cmdArgs[1]);
                if (file == null)
                {
                    process.Print("File " + cmdArgs[1] + " not found.");
                    return true;
                }
                if (!file.HasWritePermission(process.Credentials))
                {
                    process.Print("Permission denied.");
                    return true;
                }
                int n;
                if (!int.TryParse(cmdArgs[2], out n))
                {
                    process.Print("Wrong line number.");
                    return true;
                }
                var nth = file.Content.GetNthOccurence(n, '\n');
                file.Content = file.Content.Remove(nth, file.Content.GetNthOccurence(n + 1, '\n') - nth);
                file.Content = file.Content.Insert(nth, '\n' + cmdArgs.JoinWords(" ", 3));
                process.Print("Line edited.");
                return true;
            }
            if (cmdArgs[0] == "remove")
            {
                if (cmdArgs.Length < 3)
                {
                    process.Print("Missing arguments");
                    return true;
                }
                var file = process.ActiveDirectory.GetFile(cmdArgs[1]);
                if (file == null)
                {
                    process.Print("File " + cmdArgs[1] + " not found.");
                    return true;
                }
                if (!file.HasWritePermission(process.Credentials))
                {
                    process.Print("Permission denied.");
                    return true;
                }
                int n;
                if (!int.TryParse(cmdArgs[2], out n))
                {
                    process.Print("Wrong line number.");
                    return true;
                }
                var nth = file.Content.GetNthOccurence(n, '\n');
                file.Content = file.Content.Remove(nth, file.Content.GetNthOccurence(n + 1, '\n') - nth);
                process.Print("Line removed");
                return true;
            }
            if (cmdArgs[0] == "insert")
            {
                if (cmdArgs.Length < 3)
                {
                    process.Print("Missing arguments");
                    return true;
                }
                var file = process.ActiveDirectory.GetFile(cmdArgs[1]);
                if (file == null)
                {
                    process.Print("File " + cmdArgs[1] + " not found.");
                    return true;
                }
                if (!file.HasWritePermission(process.Credentials))
                {
                    process.Print("Permission denied.");
                    return true;
                }
                int n;
                if (!int.TryParse(cmdArgs[2], out n))
                {
                    process.Print("Wrong line number.");
                    return true;
                }
                file.Content = file.Content.Insert(file.Content.GetNthOccurence(n, '\n'), '\n' + cmdArgs.JoinWords(" ", 3));
                process.Print("Content inserted");
                return true;
            }
            process.Print("Usage : fedit [append/line/remove/insert/help]");
            return true;
        }

        public static bool View(CommandProcess process, string[] command)
        {
            if (command.Length < 2)
            {
                process.Print("Usage : view [file]");
                return true;
            }
            var cmdArgs = command[1].Split(' ');
            if (cmdArgs.Length != 1)
            {
                process.Print("Usage : view [file]");
                return true;
            }
            var activeDirectory = process.ActiveDirectory;
            var file = activeDirectory.GetFile(cmdArgs[0]);
            if (file == null)
            {
                process.Print("File " + cmdArgs[0] + " not found.");
                return true;
            }
            if (file.IsFolder())
            {
                process.Print("You cannot display a directory.");
                return true;
            }
            if (!file.HasReadPermission(process.Credentials))
            {
                process.Print("Permission denied.");
                return true;
            }
            // TODO view
            //client.Send(NetUtil.PacketType.KERNL, "state", "view", file.Name, file.Content);
            return true;
        }

        public static bool Help(CommandProcess process, string[] command)
        {
            const int ITEMS_PER_PAGE = 10;
            int totalPages = commands.Count / ITEMS_PER_PAGE + 1;

            int pageNum = 0;

            bool inputValid = command.Length == 1 || int.TryParse(command[1], out pageNum) && pageNum <= totalPages;

            if (pageNum == 0 || !inputValid)
                pageNum = 1;

            string header = $"---------------------------------\nCommand List - Page {pageNum} of {totalPages}:\n";
            string footer = "\n---------------------------------\n";

            StringBuilder builder = new StringBuilder();

            if (!inputValid)
                builder.AppendLine("Invalid Page Number");

            builder.AppendLine(header);

            if (pageNum <= totalPages)
            {
                builder.AppendLine("------- Commands -------\n");
                foreach (string key in commands.Keys.Skip((pageNum - 1) * 10).Take(10))
                {
                    builder.AppendLine(commands[key].Item1);
                    builder.AppendLine();
                }
            }

            builder.Append(commands["help"].Item1);

            builder.Append(footer);

            process.Print(builder.ToString());

            return true;
        }

        public static bool ChOwn(CommandProcess process, string[] command)
        {
            if (command.Length < 2)
            {
                process.Print(commands[command[0]].Item1);
                return true;
            }

            var cmdArgs = command[1].Split(' ');

            if (cmdArgs.Length < 2)
            {
                process.Print(commands[command[0]].Item1);
                return true;
            }
            int pos = cmdArgs[1].IndexOf(':');

            string username;
            Permissions.Group? group;

            if (pos != -1)
            {
                username = cmdArgs[1].Substring(0, pos);
                string groupString = cmdArgs[1].Substring(pos + 1);
                group = Permissions.PermissionHelper.GetGroupFromString(groupString);
                if (group == Permissions.Group.INVALID)
                {
                    process.Print($"Invalid group '{groupString}' specified");
                    return true;
                }
            }
            else
            {
                username = cmdArgs[1];
                group = null;
            }

            if (!process.computer.HasUser(username))
            {
                process.Print($"User {username} does not exist!");
                return true;
            }

            var activeDirectory = process.ActiveDirectory;
            foreach (var file in activeDirectory.children)
            {
                if (file.Name == cmdArgs[0])
                {
                    if (file.OwnerId != process.Credentials.UserId)
                    {
                        process.Print("Permission denied. Only the current file owner may change file permissions.");
                        return true;
                    }
                    //TODO get userid
                    file.OwnerId = process.computer.GetUserId(username);
                    string message;
                    if (group.HasValue)
                    {
                        message = $"File {file.Name} owner changed to {username} and group set to {group}";
                    }
                    else
                    {
                        message = $"File {file.Name} owner changed to {username}";
                    }
                    process.Print(message);
                    return true;
                }
            }

            return true;
        }

        public static bool ChMod(CommandProcess process, string[] command)
        {
            if (command.Length < 2)
            {
                process.Print(commands[command[0]].Item1);
                return true;
            }
            var cmdArgs = command[1].Split(' ');
            if (cmdArgs.Length != 2)
            {
                process.Print(commands[command[0]].Item1);
                return true;
            }
            var activePrivs = process.Credentials.Groups;

            var activeDirectory = process.ActiveDirectory;
            foreach (var fileC in activeDirectory.children)
            {
                if (fileC.Name == cmdArgs[1])
                {
                    if (process.Credentials.UserId != fileC.OwnerId)
                    {
                        process.Print("Permission denied.");
                        return true;
                    }

                    if (!Permissions.PermissionHelper.ApplyModifiers(cmdArgs[0], fileC.Permissions))
                    {
                        process.Print($"Invalid mode '{cmdArgs[0]}'\r\nUsage : chmod [permissions] [file]");
                        return true;
                    }

                    process.Print($"File {fileC.Name} permissions changed. to {fileC.Permissions.PermissionValue}");

                    return true;
                }
            }
            process.Print("File " + cmdArgs[1] + " was not found.");
            return true;
        }

        public static bool Login(CommandProcess process, string[] command)
        {
            if (command.Length < 2)
            {
                process.Print("Usage : login [username] [password]");
                return true;
            }
            var args = command[1].Split(' ');
            if (args.Length < 2)
            {
                process.Print("Usage : login [username] [password]");
                return true;
            }
            //TODO login
            //process.computer.Login(client, args[0], args[1]);
            return true;
        }

        public static bool Ping(CommandProcess process, string[] command)
        {
            //TODO ping
            //var compManager = client.server.GetComputerManager();
            //if (command.Length < 2)
            //{
            //    process.Print("Usage : ping [ip]");
            //    return true;
            //}
            //var connectingToNode = compManager.GetNodeByIp(command[1]);
            //if (connectingToNode == null)
            //{
            //    process.Print("Ping on " + command[1] + " timeout.");
            //    return true;
            //}
            //process.Print("Ping on " + command[1] + " success.");
            return true;
        }

        public static bool Connect(CommandProcess process, string[] command)
        {
            if (command.Length < 2)
            {
                process.Print("Usage : command [ip]");
                return true;
            }
            //TODO connect
            //if (client.activeSession != null)
            //    client.activeSession.DisconnectSession();
            //var compManager = client.server.GetComputerManager();
            //string resultIP = null;

            //if (client.homeComputer != null)
            //{
            //    if (command[1] == "localhost" || command[1] == "127.0.0.1")
            //        resultIP = client.homeComputer.ip;
            //    else
            //    {
            //        var DNSConfigFile = client.homeComputer.fileSystem.rootFile.GetFileAtPath("/cfg/dns.cfg");
            //        if (DNSConfigFile != null)
            //        {
            //            foreach (string ip in DNSConfigFile.Content.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
            //            {
            //                var DNSNode = compManager.GetNodeByIp(ip);
            //                if (DNSNode == null)
            //                    continue;
            //                var daemon = (DNSDaemon)DNSNode.GetDaemon("dns");
            //                if (daemon == null)
            //                    continue;
            //                resultIP = daemon.LookUp(command[1]);
            //                if (resultIP != null)
            //                    break;
            //            }
            //        }
            //    }
            //}
            //var connectingToNode = compManager.GetNodeByIp(resultIP ?? command[1]);
            //if (connectingToNode != null)
            //    client.ConnectTo(connectingToNode);
            //else
            //    client.Send(NetUtil.PacketType.KERNL, "connect", "fail", "0");
            return true;
        }

        public static bool Disconnect(CommandProcess process, string[] command)
        {
            //TODO disconnect
            //client.Disconnect();

            return true;
        }

        public static bool Ls(CommandProcess process, string[] command)
        {
            var root = process.computer.fileSystem.rootFile;
            if (command.Length == 2)
            {
                foreach (var file in process.ActiveDirectory.children)
                {
                    if (command[1] == file.Name)
                    {
                        process.Print($"File {file.Name} > Owner '{process.computer.GetUsername(file.OwnerId)}' Group '{file.Group}' Permissions '{Permissions.PermissionHelper.PermissionToDisplayString(file.Permissions)}'");
                        return true;
                    }
                }
                process.Print("File " + command[1] + " not found.");
                return true;
            }
            else
            {
                List<string> fileList = new List<string>(new string[] { "ls", process.ActiveDirectory.Name });
                foreach (File file in process.ActiveDirectory.children)
                {
                    if (file.HasReadPermission(process.Credentials))
                    {
                        fileList.AddRange(new string[] {
                                file.Name, (file.IsFolder() ? "d" : "f"), (file.HasWritePermission(process.Credentials) ? "w" : "-")
                            });
                    }
                    else
                    {
                        Console.WriteLine($"Group {process.Credentials.Groups} doesn't have permission for {file.Name} {file.Group} {file.Permissions.PermissionValue}");
                    }
                }
                //TODO file list
                //client.Send(NetUtil.PacketType.KERNL, fileList.ToArray());
                return true;
            }
        }

        public static bool ChangeDirectory(CommandProcess process, string[] command)
        {
            if (command.Length < 2)
            {
                process.Print("Usage : cd [folder]");
                return true;
            }
            if (command[1] == "..")
            {
                if (process.ActiveDirectory.Parent != null)
                {
                    process.ActiveDirectory = process.ActiveDirectory.Parent;
                    return true;
                }
                else
                {
                    process.Print("Invalid operation.");
                    return true;
                }
            }
            foreach (var file in process.ActiveDirectory.children)
            {
                if (file.Name == command[1])
                {
                    if (!file.IsFolder())
                    {
                        process.Print("You cannot change active directory to a file.");
                        return true;
                    }
                    if (!file.HasExecutePermission(process.Credentials))
                    {
                        process.Print("You do not have permission to do this. You must have execute permission to access a directory.");
                        return true;
                    }
                    process.ActiveDirectory = file;
                    //TODO send CD
                    //client.Send(NetUtil.PacketType.KERNL, "cd", file.Name);
                    return true;
                }
            }
            process.Print("No such folder.");
            return true;
        }

        public static bool Touch(CommandProcess process, string[] command)
        {
            if (command.Length < 2)
            {
                process.Print("Usage : touch [fileName]");
            }

            var activeDirectory = process.ActiveDirectory;
            foreach (var fileC in activeDirectory.children)
            {
                if (fileC.Name == command[1])
                {
                    process.Print("File " + command[1] + " touched.");
                    fileC.Dirty = true;
                    return true;
                }
            }
            if (!activeDirectory.HasWritePermission(process.Credentials))
            {
                process.Print("Permission denied.");
                return true;
            }

            File file = process.computer.fileSystem.CreateFile(process.computer, activeDirectory, command[1]);
            file.OwnerId = process.Credentials.UserId;
            file.Permissions.SetPermission(FilePermissions.PermissionType.User, true, true, false);
            file.Permissions.SetPermission(FilePermissions.PermissionType.Group, true, true, false);
            file.Group = file.Parent.Group;

            process.Print("File " + command[1]);
            return true;
        }

        public static bool Remove(CommandProcess process, string[] command)
        {
            if (command.Length < 2)
            {
                process.Print("Usage : rm [fileName]");
            }
            var activeDirectory = process.ActiveDirectory;
            foreach (var fileC in activeDirectory.children)
            {
                if (fileC.Name == command[1])
                {
                    if (!fileC.HasWritePermission(process.Credentials))
                    {
                        process.Print("Permission denied.");
                        return true;
                    }
                    process.Print("File " + command[1] + " removed.");
                    fileC.RemoveFile();
                
                    //TODO add to delete
                    //client.server.GetComputerManager().AddToDelete(fileC);
                    return true;
                }
            }



            process.Print("File does not exist.");
            return true;
        }

        public static bool MkDir(CommandProcess process, string[] command)
        {
            if (command.Length < 2)
            {
                process.Print("Usage : mkdir [folderName]");
                return true;
            }

            var activeDirectory = process.ActiveDirectory;
            foreach (var fileC in activeDirectory.children)
            {
                if (fileC.Name == command[1])
                {
                    process.Print("Folder " + command[1] + " already exists.");
                    return true;
                }
            }

            bool passed = activeDirectory.HasWritePermission(process.Credentials);

            if (!passed)
            {
                process.Print("Permission denied.");
                return true;
            }

            File file = process.computer.fileSystem.CreateFile(process.computer, activeDirectory, command[1]);
            file.OwnerId = process.Credentials.UserId;
            file.Permissions.SetPermission(FilePermissions.PermissionType.User, true, true, true);
            file.Permissions.SetPermission(FilePermissions.PermissionType.Group, true, true, true);
            file.Group = file.Parent.Group;
            return true;
        }

        public static bool TraceDebug(CommandProcess process, string[] command)
        {
            //if (command.Length != 2)
            //    return true;
            //if (command[1] == "over")
            //{
            //    client.TraceTermination();
            //    return true;
            //}
            //if (command[1] == "beep")
            //{
            //    client.Send(NetUtil.PacketType.FX, "warnBlink");
            //}
            //if (command[1] == "trace")
            //{
            //    client.Send(NetUtil.PacketType.FX, "trace", "100", "5");
            //}
            //if (command[1] == "traceEnd")
            //{
            //    client.Send(NetUtil.PacketType.FX, "traceEnd");
            //}
            //if (command[1] == "realTrace")
            //{
            //    if (client.activeSession != null)
            //        client.activeSession.SetTraceLevel(5);
            //}
            //if (command[1] == "realMagic")
            //{
            //    if (client.activeSession != null)
            //        client.activeSession.SetTraceLevel(-5);
            //}
            return true;
        }

        public static bool GivePermissions(CommandProcess process, string[] commandUnsplit)
        {
            //TODO kernel
            //if (client.permissions.Contains(Permissions.Admin) == false && client.permissions.Contains(Permissions.GivePerms) == false)
            //{
            //    client.Send(NetUtil.PacketType.MESSG, "Insufficent Privileges");
            //    return true;
            //}

            //List<string> command = new List<string>();
            //command.Add("giveperms");
            //command.AddRange(commandUnsplit[1].Split(' '));
            //if (command.Count < 3)
            //{
            //    client.Send(NetUtil.PacketType.MESSG, "Usage: giveperms [username] [admin/kick/ban/giveperms]");
            //    return true;
            //}
            //if (!Server.Instance.DatabaseLink.GetUsersInDatabase().ContainsValue(command[1]))
            //{
            //    client.Send(NetUtil.PacketType.MESSG, "User does not exist in the user database");
            //    return true;
            //}

            //List<Permissions> permissions = Server.Instance.DatabaseLink.GetUserPermissions()[command[1]];


            //if (command[2] == "admin")
            //{
            //    permissions.Add(Permissions.Admin);
            //}
            //if (command[2] == "kick")
            //{
            //    permissions.Add(Permissions.Kick);
            //}
            //if (command[2] == "ban")
            //{
            //    permissions.Add(Permissions.Ban);
            //}
            //if (command[2] == "giveperms")
            //{
            //    permissions.Add(Permissions.GivePerms);
            //}
            //Server.Instance.DatabaseLink.SetUserPermissions(command[1], permissions);
            //foreach (var client2 in Server.Instance.clients)
            //{
            //    if (client2.username == command[1])
            //    {
            //        client = client2;
            //    }
            //}
            //client.permissions = permissions;
            return true;
        }

        public static bool Kick(Process process, string[] command)
        {
            //TODO kernel
            //if (client.permissions.Contains(Permissions.Admin) == false && client.permissions.Contains(Permissions.Kick) == false)
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
            List<string> command = new List<string>();
            command.Add("ban");
            command.AddRange(commandUnsplit[1].Split());

            //TODO kernel
            //if (client.permissions.Contains(Permissions.Admin) == false && client.permissions.Contains(Permissions.Ban) == false)
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
            if (command.Length < 2)
            {
                //TODO kernel
                //client.Send(NetUtil.PacketType.MESSG, "Usage: unban [username]");
                return true;
            }
            Server.Instance.DatabaseLink.SetUserBanStatus(command[1], 0, true, false);
            return true;
        }

        public static bool AddToNetMap(CommandProcess process, string[] commandUnsplit)
        {
            List<string> command = new List<string>();
            command.Add("netmap");
            command.AddRange(commandUnsplit[1].Split());
            if (command.Count < 4)
            {
                //TODO kernel
                //client.Send(NetUtil.PacketType.MESSG, "Usage: netmap [ip] [x] [y]");
                return true;
            }
            //TODO kernel
            //Server.Instance.DatabaseLink.AddUserNode(client.username, command[1], command[2] + ":" + command[3]);
            return true;
        }

        public static bool PlayMusic(CommandProcess process, string[] commandUnsplit)
        {
            List<string> command = new List<string>();
            command.Add("music");
            command.AddRange(commandUnsplit[1].Split());
            if (command.Count < 3)
            {
                process.Print("Usage: music [file ((DLC\\)Music\\NameOfFile)] [playimmediately (0/1)]");
                return true;
            }
            //TODO kernel
            //client.Send(NetUtil.PacketType.MUSIC, command[1], command[2]);
            return true;
        }
    }
}
