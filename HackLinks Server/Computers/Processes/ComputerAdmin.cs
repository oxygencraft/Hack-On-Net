using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Proxies;
using System.Security;
using HackLinks_Server.Computers.Permissions;
using HackLinks_Server.Files;

namespace HackLinks_Server.Computers.Processes
{
    public class ComputerAdmin : CommandProcess
    {
        public ComputerAdmin(int pid, Printer printer, Node computer, Credentials credentials) : base(pid, printer, computer, credentials)
        {
        }

        private static SortedDictionary<string, Tuple<string, Command>> commands = new SortedDictionary<string, Tuple<string, Command>>()
        {
            { "cadmin",new Tuple<String,Command>("cadmin [command]",CommandExec)},
            { "setgroup", new Tuple<string, Command>("setgroup [username] [group]", SetPrimaryGroup) },
            { "adduser", new Tuple<string, Command>("adduser [username] [password] [group] <info> <system (t|f)> <HomeDirectory> <StartupProcess>", AddUser) },
            { "userinfo", new Tuple<string, Command>("userinfo [username]", UserInfo) },
            { "deluser",new Tuple<string, Command>("deluser [username]",DeleteUser)},
            { "edituser",new Tuple<string,Command>("edituser [username] [toedit] [value]",EditUser) },
            { "passwd",new Tuple<string,Command>("passwd [oldpass] [newpass]",Passwd)}
        };

        private static bool Passwd(CommandProcess process, string[] command)
        {
            if (command.Length == 1) return false;
            var cmdArgs = command[1].Split(' ');
            if (cmdArgs.Length != 2) return false;
            Account account = Account.FromId(process.Credentials.UserId, process.computer);
            if (account == null) return true;
            string oldpass = cmdArgs[0];
            string newpass = cmdArgs[1];

            if (oldpass != account.Password)
            {
                process.Print("The old password is invalid");
                return true;
            }

            account.Password = newpass;
            account.ApplyChanges();
            process.Print("Changes successfully applied");
            return true;

        }

        private static bool EditUser(CommandProcess process, string[] command)
        {
            if (command.Length == 1) return false;
            File groupsFile = process.computer.fileSystem.rootFile.GetFile("etc").GetFile("group");
            File usersFile = process.computer.fileSystem.rootFile.GetFile("etc").GetFile("passwd");
            if (!groupsFile.HasWritePermission(process.Credentials) && !usersFile.HasWritePermission(process.Credentials))
            {
                process.Print("You do not have the required permissions");
                return true;
            }
            var cmdArgs = command[1].Split(' ');
            if (cmdArgs.Length != 3) return false;
            string username = cmdArgs[0];
            List<Account> accounts = process.computer.Kernel.GetAccounts();
            if (accounts.Find(acc => acc.Username == username) == null)
            {
                process.Print("The user \"" + username + "\" does not exists");
                return true;
            }

            Account account = accounts.Find(acc => acc.Username == username);

            string toedit = cmdArgs[1];
            string value = cmdArgs[2];
            switch (toedit)
            {
                case "username":
                    if (accounts.Find(acc => acc.Username == value) != null)
                    {
                        process.Print("The user \"" + value + "\" already exists");
                        return true;
                    }
                    account.Username = value;
                    account.UserId = -2;
                    break;
                case "homedir":
                    account.HomeString = value;
                    break;
                case "defaultpr":
                    account.DPString = value;
                    break;
                case "password":
                    account.Password = value;
                    break;
                default:
                    return false;
            }
            account.ApplyChanges();
            process.Print("Successfully modified the account");
            return true;
        }

        public override SortedDictionary<string, Tuple<string, Command>> Commands => commands;
        
        public static bool CommandExec(CommandProcess process, string[] command)
        {
            if(command.Length > 1)
            {
                return process.RunCommand(command[1]);
            }
            process.Print(commands[command[0]].Item1);
            return true;
        }
        
        public static bool SetPrimaryGroup(CommandProcess process, string[] command)
        {
            if (command.Length == 1) return false;
            var cmdArgs = command[1].Split(' ');
            if (cmdArgs.Length != 2) return false;
            string username = cmdArgs[0];
            string groupname = cmdArgs[1];
            File groupsFile = process.computer.fileSystem.rootFile.GetFile("etc").GetFile("group");
            File usersFile = process.computer.fileSystem.rootFile.GetFile("etc").GetFile("passwd");
            if (!groupsFile.HasWritePermission(process.Credentials) && !usersFile.HasWritePermission(process.Credentials))
            {
                process.Print("You do not have the required permissions");
                return true;
            }
            string[] groups = groupsFile.Content.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            List<Account> accounts = process.computer.Kernel.GetAccounts();
            if (accounts.All(acc => acc.Username.ToLower() != username.ToLower()))
            {
                process.Print("The user \"" + username + "\" does not exists");
                return true;
            }
            if (groups.All( g => !g.StartsWith(groupname.ToLower() + ":")))
            {
                process.Print("The group \"" + groupname + "\" does not exists");
                return true;
            }

            Account user = accounts.Find(acc => acc.Username.ToLower() == username.ToLower());
            Enum.TryParse(groupname, true, out Group theGroup);
            user.GroupId = (int)theGroup;
            user.ApplyChanges();
            process.Print("done");
            return true;
        }
        
        public static bool AddUser(CommandProcess process, string[] command)
        {
            if (command.Length == 1) return false;
            var cmdArgs = command[1].Split(' ');
            if (cmdArgs.Length < 3 || cmdArgs.Length > 7) return false;
            
            File groupsFile = process.computer.fileSystem.rootFile.GetFile("etc").GetFile("group");
            File usersFile = process.computer.fileSystem.rootFile.GetFile("etc").GetFile("passwd");
            if (!groupsFile.HasWritePermission(process.Credentials) && !usersFile.HasWritePermission(process.Credentials))
            {
                process.Print("You do not have the required permissions");
                return true;
            }
            string username = cmdArgs[0];
            string password = cmdArgs[1];
            if (!Enum.TryParse(cmdArgs[2], true, out Group group))
            {
                process.Print("The group doesn't exists !");
                return true;
            }

            string info = "";
            if (cmdArgs.Length > 3) info = cmdArgs[4];
            
            string homepath = "/root";
            string defaultprocessPath = "/bin/hash";
            string system = "f";
            if (cmdArgs.Length > 4) system = cmdArgs[4];
            if (cmdArgs.Length > 5) homepath = cmdArgs[5];
            if (cmdArgs.Length == 7) defaultprocessPath = cmdArgs[6];
            Node computer = process.computer;

            if(computer.Kernel.GetAccounts().Any( acc => acc.Username == username))
            {
                process.Print("This user already exists");
                return true;
            }

            if (computer.fileSystem.rootFile.GetFileAtPath(homepath) == null)
            {
                //TODO create home directory
            }
            if (computer.fileSystem.rootFile.GetFileAtPath(defaultprocessPath) == null)
            {
                process.Print("The default process's file doesn't exists !");
                return true;
            }
            
            Account account = new Account(username,password,group,info,homepath,defaultprocessPath,computer);
            if (system == "t") account.UserId = -3;
            account.ApplyChanges();
            process.Print("The user " + username + " has succesfully been created");
            return true;
        }

        public static bool UserInfo(CommandProcess process, string[] command)
        {
            if (command.Length == 1) return false;
            var cmdArgs = command[1].Split(' ');
            if (cmdArgs.Length != 1) return false;
            File usersFile = process.computer.fileSystem.rootFile.GetFile("etc").GetFile("passwd");
            if (!usersFile.HasReadPermission(process.Credentials))
            {
                process.Print("You do not have the required permissions");
                return true;
            }
            string username = cmdArgs[0];
            List<Account> accounts = process.computer.Kernel.GetAccounts();
            if (accounts.Find(acc => acc.Username == username) == null)
            {
                process.Print("The user \"" + username + "\" does not exists");
                return true;
            }

            Account account = accounts.Find(acc => acc.Username == username);
            process.Print("--------------------------------");
            process.Print("Username : " + account.Username);
            process.Print("UserId : " + account.UserId);
            process.Print("Primary group : " + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(account.GetGroup().ToString()) + " (" + account.GroupId + ")");
            process.Print("Info : " + account.Info);
            process.Print("System : " + (account.UserId < 100 ? "True" : "False"));
            process.Print("Home directory : " + account.HomeString);
            process.Print("Default process : " + account.DPString);
            process.Print("--------------------------------");
            return true;
        }

        public static bool DeleteUser(CommandProcess process, string[] command)
        {
            if (command.Length == 1) return false;
            var cmdArgs = command[1].Split(' ');
            if (cmdArgs.Length != 1) return false;
            File groupsFile = process.computer.fileSystem.rootFile.GetFile("etc").GetFile("group");
            File usersFile = process.computer.fileSystem.rootFile.GetFile("etc").GetFile("passwd");
            if (!groupsFile.HasWritePermission(process.Credentials) && !usersFile.HasWritePermission(process.Credentials))
            {
                process.Print("You do not have the required permissions");
                return true;
            }
            string username = cmdArgs[0];
            List<Account> accounts = process.computer.Kernel.GetAccounts();
            if (accounts.Find(acc => acc.Username == username) == null)
            {
                process.Print("The user \"" + username + "\" does not exists");
                return true;
            }
            
            Account account = accounts.Find(acc => acc.Username == username);
            account.Delete();
            account.ApplyChanges();
            process.Print("The account \"" + account.Username + "\" has been successfully deleted");
            return true;
        }
    }
}