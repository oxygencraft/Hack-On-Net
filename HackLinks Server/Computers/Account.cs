using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HackLinks_Server.Computers;
using HackLinks_Server.Computers.Permissions;
using File = HackLinks_Server.Files.File;

namespace HackLinks_Server.Computers.Permissions
{
    public class Account
    {
        public string Username {get; set; }
        public string Password {get; set; }
        public int UserId { get; private set; }
        public int GroupId {get; set; }
        public File HomeDirectory { get; set; }
        public File DefaultProcess {get; set; }

        public string HomeString;

        public string DPString;
        
        public Node Computer { get; }

        public Account(string line,Node node)
        {
            string[] accountData = line.Split(':');

            Username = accountData[0];
            Password = accountData[1];
            UserId = Convert.ToInt32(accountData[2]);
            GroupId = Convert.ToInt32(accountData[3]);
            HomeString = accountData[4];
            DPString = accountData[5];
            Computer = node;
            HomeDirectory = Computer.fileSystem.rootFile.GetFileAtPath(accountData[4]);
            DefaultProcess = Computer.fileSystem.rootFile.GetFileAtPath(accountData[5]);

        }

        public Account( string username, string password, int groupId,string homeString, string dpString, Node computer)
        {
            HomeString = homeString;
            DPString = dpString;
            Username = username;
            Password = password;
            GroupId = groupId;
            Computer = computer;
            HomeDirectory = Computer.fileSystem.rootFile.GetFileAtPath(homeString);
            DefaultProcess = Computer.fileSystem.rootFile.GetFileAtPath(dpString);
        }

        public Account(string username, string password,Group group,string homeString, string dpString, Node computer)
        {
            HomeString = homeString;
            DPString = dpString;
            Username = username;
            GroupId = (int) group;
            Password = password;
            HomeDirectory = computer.fileSystem.rootFile.GetFileAtPath(homeString);
            DefaultProcess = computer.fileSystem.rootFile.GetFileAtPath(dpString);
            Computer = computer;
        }

        public Account(string username, string password, int groupId, File homeDirectory, File defaultProcess, Node computer)
        {
            Username = username;
            Password = password;
            GroupId = groupId;
            HomeDirectory = homeDirectory;
            DefaultProcess = defaultProcess;
            HomeString = homeDirectory.Name;
            DPString = defaultProcess.Name;
            Computer = computer;
        }

        public Account(string username, string password,Group group, File homeDirectory, File defaultProcess, Node computer)
        {
            Username = username;
            Password = password;
            GroupId = (int)group;
            HomeDirectory = homeDirectory;
            DefaultProcess = defaultProcess;
            HomeString = homeDirectory.Name;
            DPString = defaultProcess.Name;
            Computer = computer;
        }

        public Group GetGroup()
        {
            switch (GroupId)
            {
                case 0:
                    return Group.ROOT;
                case 1:
                    return Group.ADMIN;
                case 2:
                    return Group.USER;
                case 3:
                    return Group.GUEST;
                default:
                    return Group.INVALID;
            }
        }

        public void ApplyChanges()
        {
            File passwd = Computer.fileSystem.rootFile.GetFileAtPath("etc/passwd");
            string[] accounts = passwd.Content.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (UserId == -2)
            {
                UserId = new Account(accounts.Last(),Computer).UserId + 1;
            }
            string line = Username + ":" + Password + ":" + UserId + ":" + GroupId + ":" + HomeString + ":" + DPString;
            string acc = "";
            foreach (string account in accounts)
            {
                if (account.StartsWith(Username))
                {
                    if(UserId == -1) continue;
                    acc += "\r\n" + line;
                }
                else acc += "\r\n" + account;
            }
            if (accounts.All(ac => !ac.StartsWith(Username)))
            {
                acc += "\r\n" + line;
            }

            passwd.Content = acc;
        }

        public static List<Account> FromFile(File passwd,Node computer)
        {
            List<Account> tmp = new List<Account>();
            if(passwd.Name != "passwd") return tmp;
            string[] accounts = passwd.Content.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string account in accounts)
            {
                tmp.Add(new Account(account,computer));
            }

            return tmp;
        }
    }
}