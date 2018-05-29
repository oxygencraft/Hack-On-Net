using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Proxies;
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
            {"cadmin",new Tuple<String,Command>("cadmin [command]",CommandExec)},
            { "group", new Tuple<string, Command>("group [username] [group]", SetPrimaryGroup) },
            { "adduser", new Tuple<string, Command>("adduser [username] [password] [group] [info] <HomeDirectory> <StartupProcess>", AddUser) },
            { "userinfo", new Tuple<string, Command>("userinfo [username]", UserInfo) }
        };
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
            var cmdArgs = command[1].Split(' ');
            if (cmdArgs.Length != 2) return false;
            string groupname = cmdArgs[0];
            string username = cmdArgs[1];
            File groupsFile = process.computer.fileSystem.rootFile.GetFile("etc").GetFile("groups");
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
            if (!groups.Contains(groupname.ToLower()))
            {
                process.Print("The group \"" + username + "\" does not exists");
                return true;
            }

            Account user = accounts.Find(acc => acc.Username.ToLower() != username.ToLower());
            Enum.TryParse(groupname, true, out Group theGroup);
            user.GroupId = (int)theGroup;
            user.ApplyChanges();
            process.Print("done");
            return true;
        }
        
        public static bool AddUser(CommandProcess process, string[] command)
        {
            var cmdArgs = command[1].Split(' ');
            if (cmdArgs.Length < 4 || cmdArgs.Length > 6) return false;
            string username = cmdArgs[0];
            string password = cmdArgs[1];
            if (!Enum.TryParse(cmdArgs[2], true, out Group group))
            {
                process.Print("The group doesn't exists !");
                return true;
            }

            string info = cmdArgs[3];
            
            string homepath = "/root";
            string defaultprocessPath = "/bin/hash";
            if (cmdArgs.Length > 4) homepath = cmdArgs[4];
            if (cmdArgs.Length == 6) defaultprocessPath = cmdArgs[5];
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
            account.ApplyChanges();
            process.Print("The user " + username + " has succesfully been created");
            return true;
        }

        public static bool UserInfo(CommandProcess process, string[] command)
        {
            var cmdArgs = command[1].Split(' ');
            if (cmdArgs.Length != 1) return false;
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
            process.Print("Home directory : " + account.HomeString);
            process.Print("Default process : " + account.DPString);
            process.Print("--------------------------------");
            return true;
        }
    }
}