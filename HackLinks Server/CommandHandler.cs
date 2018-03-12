using HackLinks_Server.Computers;
using HackLinks_Server.FileSystem;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server
{
    static class CommandHandler
    {
        public delegate bool Command(GameClient client, string[] command);

        public static SortedDictionary<string, Tuple<string, Command>> commands = new SortedDictionary<string, Tuple<string, Command>>()
        {
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
            { "chmod", new Tuple<string, Command>("chmod [file] [readLevel] [writeLevel]\n    Change the required user level for read and write operations on the given file.", ChMod) },
            { "help", new Tuple<string, Command>("help [page]\n    Displays the specified page of commands.", Help) },
        };

        public static bool TreatCommand(string command, GameClient client)
        {
            if (TreatKernelCommands(client, command.Split(new char[] { ' ' }, 2)))
                return true;
            if (TreatSessionCommands(client, command.Split(new char[] { ' ' }, 2)))
                return true;

            client.Send("MESSG:Unknown command.");
            return false;
        }

         public static bool TreatKernelCommands(GameClient client, string[] command)
        {
            bool result = false;
            if (commands.ContainsKey(command[0]))
                result = commands[command[0]].Item2(client, command);
            return result;
        }

        public static bool TreatSessionCommands(GameClient client, string[] command)
        {
            if (client.activeSession == null)
                return false;
            return client.activeSession.HandleSessionCommand(client, command);
        }

        public static bool View(GameClient client, string[] command)
        {
            if (client.activeSession == null || client.activeSession.connectedNode == null)
            {
                client.Send("MESSG:You are not connected to a node.");
                return true;
            }
            if (command.Length < 2)
            {
                client.Send("MESSG:Usage : view [file]");
                return true;
            }
            var cmdArgs = command[1].Split(' ');
            if (cmdArgs.Length != 1)
            {
                client.Send("MESSG:Usage : view [file]");
                return true;
            }
            var activeDirectory = client.activeSession.activeDirectory;
            var file = activeDirectory.GetFile(cmdArgs[0]);
            if(file == null)
            {
                client.Send("MESSG:File " + cmdArgs[0] + " not found.");
                return true;
            }
            if(file.IsFolder())
            {
                client.Send("MESSG:You cannot display a directory.");
                return true;
            }
            if(!file.HasReadPermission(client.activeSession.privilege))
            {
                client.Send("MESSG:Permission denied.");
                return true;
            }
            client.Send("KERNL:state;view;"+file.name+";"+file.content);
            return true;
        }

        public static bool Help(GameClient client, string[] command)
        {
            const int ITEMS_PER_PAGE = 10;
            int totalKernelPages = commands.Count / ITEMS_PER_PAGE + 1;
            int totalSessionPages = 0;

            if(client.activeSession != null)
                totalSessionPages = client.activeSession.Commands.Count / ITEMS_PER_PAGE + 1;

            int totalPages = totalKernelPages + totalSessionPages;

            int pageNum = 0;

            bool inputValid = command.Length == 1 || int.TryParse(command[1], out pageNum) && pageNum <= totalPages;

            if (pageNum == 0 || !inputValid)
                pageNum = 1;

            string header = $"---------------------------------\nCommand List - Page {pageNum} of {totalPages}:\n";
            string footer = "\n---------------------------------\n";

            StringBuilder builder = new StringBuilder();

            if(!inputValid)
                builder.AppendLine("Invalid Page Number");

            builder.AppendLine(header);

            if (pageNum <= totalKernelPages)
            {
                builder.AppendLine("------- Kernel Commands -------\n");
                foreach (string key in commands.Keys.Skip((pageNum - 1) * 10).Take(10))
                {
                    builder.AppendLine(commands[key].Item1);
                    builder.AppendLine();
                }
            }
            else
            {
                builder.AppendLine("------- Session Commands -------\n");
                foreach (string key in client.activeSession.Commands.Keys.Skip((pageNum - totalKernelPages - 1) * 10).Take(10))
                {
                    builder.AppendLine(client.activeSession.Commands[key].Item1);
                    builder.AppendLine();
                }
            }

            builder.Append(commands["help"].Item1);

            builder.Append(footer);

            client.Send("MESSG:" + builder.ToString());

            return true;
        }

        public static bool ChMod(GameClient client, string[] command)
        {
            if (client.activeSession == null || client.activeSession.connectedNode == null)
            {
                client.Send("MESSG:You are not connected to a node.");
                return true;
            }
            if (command.Length < 2)
            {
                client.Send("MESSG:Usage : chmod [file] [readLevel] [writeLevel]");
                return true;
            }
            var cmdArgs = command[1].Split(' ');
            if(cmdArgs.Length != 3)
            {
                client.Send("MESSG:Usage : chmod [file] [readLevel] [writeLevel]");
                return true;
            }
            var writeLevel = PermissionHelper.ParsePermissionLevel(cmdArgs[2]);
            var readLevel = PermissionHelper.ParsePermissionLevel(cmdArgs[1]);
            var activePriv = client.activeSession.privilege;
            if(writeLevel == -1 || readLevel == -1)
            {
                client.Send("MESSG:Input valid writeLevel or readLevel");
                return true;
            }
            if(writeLevel < activePriv || readLevel < activePriv)
            {
                client.Send("MESSG:You cannot change to a higher permission than your current level.");
                return true;
            }
            var activeDirectory = client.activeSession.activeDirectory;
            foreach (var fileC in activeDirectory.children)
            {
                if (fileC.name == cmdArgs[0])
                {
                    if (!fileC.HasWritePermission(client.activeSession.privilege))
                    {
                        client.Send("MESSG:Permission denied.");
                        return true;
                    }
                    client.Send("MESSG:File " + cmdArgs[0] + " permissions changed.");
                    fileC.writePriv = writeLevel;
                    fileC.readPriv = readLevel;
                    return true;
                }
            }
            client.Send("MESSG:File " + cmdArgs[0] + " was not found.");
            return true;
        }

        public static bool Login(GameClient client, string[] command)
        {
            // login [username] [password]
            if(client.activeSession == null || client.activeSession.connectedNode == null)
            {
                client.Send("MESSG:You are not connected to a node.");
                return true;
            }
            if(command.Length < 2)
            {
                client.Send("MESSG:Usage : login [username] [password]");
                return true;
            }
            var args = command[1].Split(' ');
            if(args.Length < 2)
            {
                client.Send("MESSG:Usage : login [username] [password]");
                return true;
            }
            client.activeSession.connectedNode.Login(client, args[0], args[1]);
            return true; 
        }

        public static bool Ping(GameClient client, string[] command)
        {
            var compManager = client.server.GetComputerManager();
            if(command.Length < 2)
            {
                client.Send("MESSG:Usage : ping [ip]");
                return true;
            }
            var connectingToNode = compManager.GetNodeByIp(command[1]);
            if (connectingToNode == null)
            {
                client.Send("MESSG:Ping on " + command[1] + " timeout.");
                return true;
            }
            client.Send("MESSG:Ping on " + command[1] + " success.");
            return true;       
        }

        public static bool Connect(GameClient client, string[] command)
        {
            if (command.Length < 2)
            {
                client.Send("MESSG:Usage : command [ip]");
                return true;
            }
            if(client.activeSession != null)
                client.activeSession.DisconnectSession();
            var compManager = client.server.GetComputerManager();
            var connectingToNode = compManager.GetNodeByIp(command[1]);
            if(connectingToNode != null)
                client.ConnectTo(connectingToNode);                    
            else
                client.Send("KERNL:connect;fail;0");
            return true;
        }

/*        public static bool SendMessage(GameClient client, string[] command)
        {
            string message = "Sent from {{blue}}"+client.username+"{{white}}>";
            for(int i = 1; i < command.Length; i++)
            {
                message += command[i] + " ";
            }
            foreach(var Cclient in client.server.clients)
            {
                try
                {
                    Console.WriteLine(Cclient.username);
                    Cclient.Send("MESSG:" + message);
                }
                catch(Exception ex)
                {
                    
                }
                
            }
            return true;
        }*/

        public static bool Disconnect(GameClient client, string[] command)
        {
            client.Disconnect();

            return true;
        }

        public static bool Ls(GameClient client, string[] command)
        {
            var session = client.activeSession;
            if(session == null)
            {
                client.Send("MESSG:You are not connected to a node.");
                return true;
            }
            var root = session.connectedNode.rootFolder;
            if(command.Length == 2)
            {
                foreach (var file in session.activeDirectory.children)
                {
                    if (command[1] == file.name)
                    {
                        client.Send("MESSG:File " + file.name + " > Permissions " + file.readPriv + "" + file.writePriv);
                        return true;
                    }
                }
                client.Send("MESSG:File " + command[1] + " not found.");
                return true;
            }
            else
            {
                string fileList = "";
                foreach (var file in session.activeDirectory.children)
                {
                    if (file.HasReadPermission(client.activeSession.privilege))
                    {
                        fileList += file.name + "," + (file.IsFolder() ? "d" : "f") + ',' +
                            (file.HasWritePermission(client.activeSession.privilege) ? "w" : "-") + ";";
                    }
                }
                client.Send("KERNL:ls;" + session.activeDirectory.name + ";" + fileList); // ls;[working path];[listoffiles]
                return true;
            }
        }

        public static bool ChangeDirectory(GameClient client, string[] command)
        {
            var session = client.activeSession;
            if (session == null)
            {
                client.Send("MESSG:You are not connected to a node.");
                return true;
            }

            if (command.Length < 2)
            {
                client.Send("MESSG:Usage : cd [folder]");
                return true;
            }
            if(command[1] == "..")
            {
                if(session.activeDirectory.parent != null)
                {
                    session.activeDirectory = session.activeDirectory.parent;
                    return true;
                }
                else
                {
                    client.Send("MESSG:Invalid operation.");
                    return true;
                }
            }
            foreach(var file in session.activeDirectory.children)
            {
                if(file.name == command[1])
                {
                    if(!file.IsFolder())
                    {
                        client.Send("MESSG:You cannot change active directory to a file.");
                        return true;
                    }
                    session.activeDirectory = (Folder)file;
                    client.Send("KERNL:cd;"+file.name);
                    return true;
                }
            }
            client.Send("MESSG:No such folder.");
            return true;
        }

        public static bool Touch(GameClient client, string[] command)
        {
            var session = client.activeSession;
            if (session == null)
            {
                client.Send("MESSG:You are not connected to a node.");
                return true;
            }
            if(command.Length < 2)
            {
                client.Send("MESSG:Usage : touch [fileName]");
            }

            var activeDirectory = session.activeDirectory;
            foreach(var fileC in activeDirectory.children)
            {
                if(fileC.name == command[1])
                {
                    client.Send("MESSG:File " + command[1] + " touched.");
                    return true;
                }
            }
            if(!activeDirectory.HasWritePermission(client.activeSession.privilege))
            {
                client.Send("MESSG:Permission denied.");
                return true;
            }

            var file = new File(activeDirectory, command[1]);
            file.writePriv = client.activeSession.privilege;
            file.readPriv = client.activeSession.privilege;

            client.Send("MESSG:File " + command[1] + " was added.");
            return true;
        }

        public static bool Remove(GameClient client, string[] command)
        {
            var session = client.activeSession;
            if (session == null)
            {
                client.Send("MESSG:You are not connected to a node.");
                return true;
            }
            if (command.Length < 2)
            {
                client.Send("MESSG:Usage : rm [fileName]");
            }
            var activeDirectory = session.activeDirectory;
            foreach (var fileC in activeDirectory.children)
            {
                if (fileC.name == command[1])
                {
                    if (!fileC.HasWritePermission(client.activeSession.privilege))
                    {
                        client.Send("MESSG:Permission denied.");
                        return true;
                    }
                    client.Send("MESSG:File " + command[1] + " removed.");
                    fileC.RemoveFile();
                    return true;
                }
            }

            

            client.Send("MESSG:File does not exist.");
            return true;
        }

        public static bool MkDir(GameClient client, string[] command)
        {
            var session = client.activeSession;
            if (session == null)
            {
                client.Send("MESSG:You are not connected to a node.");
                return true;
            }
            if (command.Length < 2)
            {
                client.Send("MESSG:Usage : mkdir [folderName]");
                return true;
            }

            var activeDirectory = session.activeDirectory;
            foreach (var fileC in activeDirectory.children)
            {
                if (fileC.name == command[1])
                {
                    client.Send("MESSG:Folder " + command[1] + " already exists.");
                    return true;
                }
            }

            if (!activeDirectory.HasWritePermission(client.activeSession.privilege))
            {
                client.Send("MESSG:Permission denied.");
                return true;
            }

            var file = new Folder(activeDirectory, command[1]);
            file.writePriv = client.activeSession.privilege;
            file.readPriv = client.activeSession.privilege;
            return true;
        }

        public static bool Cat(GameClient client, string[] command)
        {


            return true;
        }
    }
}
