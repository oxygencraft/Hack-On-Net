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
            { "group", new Tuple<string, Command>("setprimarygroup [username] [group]", SetPrimaryGroup) },
            { "adduser", new Tuple<string, Command>("adduser [username] [password] [group] <HomeDirectory> <StartupProcess>", AddUser) },
            { "userinfo", new Tuple<string, Command>("userinfo [username]", UserInfo) }
        };
        public override SortedDictionary<string, Tuple<string, Command>> Commands => commands;
        public static bool SetPrimaryGroup(CommandProcess process, string[] command)
        {
            if (command.Length != 3) return false;
            if (command[1] != "join" && command[1] != "leave") return false;
            string groupname = command[2];
            string username = command[3];
            File groupsFile = process.computer.fileSystem.rootFile.GetFile("etc").GetFile("groups");
            File usersFile = process.computer.fileSystem.rootFile.GetFile("etc").GetFile("passwd");
            if (!groupsFile.HasWritePermission(process.Credentials) && !usersFile.HasWritePermission(process.Credentials))
            {
                process.Print("You do not have the required privileges");
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
            return true;
        }
        
        public static bool AddUser(CommandProcess process, string[] command)
        {
            if (command.Length < 2 && command.Length > 5) return false;
            string username = command[0];
            string password = command[1];
            if (!Enum.TryParse(command[2], true, out Group group))
            {
                process.Print("The group doesn't exists !");
                return true;
            }
            
            string homepath = "/root";
            string defaultprocessPath = "/bin/hash";
            if (command.Length > 3) homepath = command[3];
            if (command.Length == 5) defaultprocessPath = command[4];
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
            
            Account account = new Account(username,password,group,homepath,defaultprocessPath,computer);
            account.ApplyChanges();
            process.Print("The user " + username + " has succesfully been created");
            return true;
        }

        public static bool UserInfo(CommandProcess process, string[] command)
        {
            if (command.Length != 1) return false;
            string username = command[0];
            List<Account> accounts = process.computer.Kernel.GetAccounts();
            if (accounts.Any(acc => acc.Username.ToLower() == username.ToLower()))
            {
                process.Print("The user \"" + username + "\" does not exists");
                return true;
            }

            Account account = accounts.Find(acc => acc.Username.ToLower() == username.ToLower());
            process.Print("--------------------------------");
            process.Print("Username : " + account.Username);
            process.Print("Primary group : " + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(account.GetGroup().ToString()));
            process.Print("Home directory : " + account.HomeString);
            process.Print("Default process : " + account.DPString);
            process.Print("--------------------------------");
            return true;
        }
    }
}