using HackLinks_Server.Daemons;
using HackLinks_Server.Daemons.Types;
using System;
using System.Collections.Generic;
using HackLinks_Server.Daemons.Types.Mail;
using HackLinks_Server.Files;
using System.Linq;

namespace HackLinks_Server.Computers.Processes {
    class MailClient : DaemonClient {
        private const string help = "account\nShows account infomation.";
        public SortedDictionary<string, Tuple<string, Command>> commands = new SortedDictionary<string, Tuple<string, Command>>() {
            { "account", new Tuple<string, Command>(help, MailAccount)}
        };
        public override SortedDictionary<string, Tuple<string, Command>> Commands => commands;

        Account loggedInAccount = null;

        public MailClient(Session session, Daemon daemon, int pid, Printer printer, Node computer, Credentials credentials) : base(session, daemon, pid, printer, computer, credentials) {}

        public override bool RunCommand(string command) {
            // We hide the old runCommand function to perform this check on startup
            if (!((MailDaemon)Daemon).CheckFolders(this))
                return true;
            return base.RunCommand(command);
        }

        public static bool MailAccount(CommandProcess process, string[] command) {
            MailClient client = (MailClient)process;
            MailDaemon daemon = (MailDaemon)client.Daemon;

            File mailFolder = process.computer.fileSystem.rootFile.GetFile("mail");
            File accountFile = mailFolder.GetFile("accounts.db");

            if (command[0] == "account") {
                if (command.Length < 2)
                    process.Print(help);
                string[] cmdArgs = command[1].Split(' ');
                if (cmdArgs[0] == "create") {
                    if (cmdArgs.Length != 3) {
                        process.Print("Usage : account create [username] [password]");
                        return true;
                    }
                    List<Account> accounts = new List<Account>();
                    accounts.AddRange(accountFile.Content.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Select(x => {
                        // The mail account format is MAILACCOUNT,(username),(password)
                        var data = x.Split(',');
                        if (data[0] != "MAILACCOUNT" || data.Length < 3)
                            return null;
                        return new Account(data[1], data[2]);
                    }));
                    accounts.RemoveAll(null);
                    foreach (Account account in accounts)
                        if (account.accountName == cmdArgs[1]) {
                            process.Print("This username already exists!");
                            return true;
                        }
                    daemon.AddAccount(new Account(cmdArgs[1], cmdArgs[2]));
                    return true;
                } else if (cmdArgs[0] == "login") {
                    if (cmdArgs.Length != 3) {
                        process.Print("Usage : account login [username] [password]");
                        return true;
                    }
                    Account accountToLogin = new Account(cmdArgs[1], cmdArgs[2]);
                    if (!daemon.accounts.Contains(accountToLogin)) {
                        process.Print("This account either does not exist or the password is incorrect.");
                        return true;
                    }
                    client.loggedInAccount = accountToLogin;
                } else if (cmdArgs[0] == "resetpass") {
                    if (cmdArgs.Length != 2) {
                        process.Print("You are not logged in!");
                        return true;
                    }
                    daemon.accounts.Remove(client.loggedInAccount);
                    client.loggedInAccount.password = cmdArgs[1];
                    daemon.AddAccount(client.loggedInAccount);
                    process.Print($"Your new password is \"{cmdArgs[1]}\"!");
					return true;
                }
            }
            return true;
        }
    }
}
