using HackLinks_Server.Daemons;
using HackLinks_Server.Daemons.Types;
using HackLinks_Server.Daemons.Types.Bank;
using HackLinks_Server.Daemons.Types.Mission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.Computers.Processes
{
    class MissionClient : DaemonClient
    {
        public SortedDictionary<string, Tuple<string, Command>> commands = new SortedDictionary<string, Tuple<string, Command>>()
        {
            { "account", new Tuple<string, Command>("account [create/login/resetpass/delete]\n    Performs an account operation.", Account) },
        };

        public override SortedDictionary<string, Tuple<string, Command>> Commands => commands;
        private MissionAccount loggedInAccount = null;

        public MissionClient(Session session, Daemon daemon, int pid, Printer printer, Node computer, Credentials credentials) : base(session, daemon, pid, printer, computer, credentials)
        {
            
        }

        public override bool RunCommand(string command)
        {
            // We hide the old runCommand function to perform this check on startup
            if (!((MissionDaemon)Daemon).CheckFolders(this))
            {
                return true;
            }
            return base.RunCommand(command);
        }

        public static bool Account(CommandProcess process, string[] command)
        {
            MissionClient client = (MissionClient)process;
            MissionDaemon daemon = (MissionDaemon)client.Daemon;

            var bankFolder = process.computer.fileSystem.rootFile.GetFile("bank");
            var accountFile = bankFolder.GetFile("accounts.db");

            if (command[0] == "account")
            {
                if (command.Length < 2)
                {
                    process.Print("Usage : account [create/login/resetpass/delete]");
                    return true;
                }
                // TODO: Implement Transaction Log
                var cmdArgs = command[1].Split(' ');
                if (cmdArgs[0] == "create")
                {
                    // TODO: When mail daemon is implemented, require an email address for password reset
                    if (cmdArgs.Length < 3)
                    {
                        process.Print("Usage : account create [accountname] [password]");
                        return true;
                    }
                    List<string> accounts = new List<string>();
                    var accountsFile = accountFile.Content.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    if (accountsFile.Length != 0)
                    {
                        foreach (string line in accountFile.Content.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            var data = line.Split(',');
                            if (data.Length < 5)
                                continue;
                            accounts.Add(data[0]);
                        }
                    }
                    if (accounts.Contains(cmdArgs[1]))
                    {
                        process.Print("This account name is not available");
                        return true;
                    }
                    daemon.accounts.Add(new MissionAccount(cmdArgs[1], 0, 0, cmdArgs[2], client.Session.owner.username));
                    daemon.UpdateAccountDatabase();
                    process.Print("Your account has been opened. Use account login [accountname] [password] to login.");
                }
                if (cmdArgs[0] == "login")
                {
                    if (cmdArgs.Length < 3)
                    {
                        process.Print("Usage : account login [accountname] [password]");
                        return true;
                    }
                    foreach (var account in daemon.accounts)
                    {
                        if (account.accountName == cmdArgs[1] && account.password == cmdArgs[2])
                        {
                            client.loggedInAccount = account;
                            daemon.computer.Log(Log.LogEvents.Login, daemon.computer.logs.Count + 1 + " " + client.Session.owner.homeComputer.ip + " logged in as bank account " + account.accountName, client.Session.sessionId, client.Session.owner.homeComputer.ip);
                            process.Print($"Logged into bank account {account.accountName} successfully");
                            break;
                        }
                    }
                    process.Print("Invalid account name or password");
                }
                if (cmdArgs[0] == "resetpass")
                {
                    if (cmdArgs.Length < 3)
                    {
                        process.Print("Usage : account resetpass [accountname] [newpassword]");
                        return true;
                    }
                    // TODO: When mail daemon is implemented, change it to verify using email so players can hack by password reset
                    foreach (var account in daemon.accounts)
                    {
                        if (account.accountName == cmdArgs[1])
                        {
                            if (account.clientUsername == client.Session.owner.username)
                            {
                                account.password = cmdArgs[2];
                                daemon.UpdateAccountDatabase();
                                process.Print("Your password has been changed");
                            }
                            else
                                process.Print("You are not the owner of the account");
                            break;
                        }
                    }
                    if (cmdArgs[0] == "delete")
                    {
                        if (client.loggedInAccount == null)
                        {
                            process.Print("You are not logged in");
                            return true;
                        }
                        if (cmdArgs.Length >= 2)
                        {
                            if (cmdArgs[1] != "y")
                            {
                                process.Print("Are you sure you want to delete your account?\nRun account delete y if you are sure you want to delete your account");
                                return true;
                            }
                        }
                        else
                        {
                            process.Print("Are you sure you want to delete your account?\nRun account delete y if you are sure you want to delete your account");
                            return true;
                        }
                        daemon.accounts.Remove(client.loggedInAccount);
                        daemon.UpdateAccountDatabase();
                        client.loggedInAccount = null;
                        process.Print("Your account has been deleted");
                    }
                    return true;
                }
                return true;
            }
            return false;
        }
    }
}
