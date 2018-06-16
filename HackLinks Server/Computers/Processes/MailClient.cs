using HackLinks_Server.Daemons;
using HackLinks_Server.Daemons.Types;
using System;
using System.Collections.Generic;
using HackLinks_Server.Daemons.Types.Mail;
using HackLinks_Server.Files;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace HackLinks_Server.Computers.Processes {
    class MailClient : DaemonClient {
        private const string help = "mail account\nShows account infomation.";
        public SortedDictionary<string, Tuple<string, Command>> commands = new SortedDictionary<string, Tuple<string, Command>>() {
            { "mail", new Tuple<string, Command>(help, MailServerCommands)}
        };
        public override SortedDictionary<string, Tuple<string, Command>> Commands => commands;

        Account loggedInAccount = null;

        public MailClient(Session session, Daemon daemon, int pid, Printer printer, Node computer, Credentials credentials) : base(session, daemon, pid, printer, computer, credentials) {}

        public override bool RunCommand(string command) {
            // We hide the old runCommand function to perform this check on startup
            if (!((MailDaemon)Daemon).CheckFolders())
                return true;
            return base.RunCommand(command);
        }

        public static bool MailServerCommands(CommandProcess process, string[] command) {
            MailClient client = (MailClient)process;
            MailDaemon daemon = (MailDaemon)client.Daemon;

            File mailFolder = process.computer.fileSystem.rootFile.GetFile("mail");
            File accountFile = mailFolder.GetFile("accounts.db");

            if (command.Length < 2) {
                process.Print(help);
                return true;
            }
            string[] cmdArgs = command[1].Split(' ');
            if (cmdArgs[0] == "account") {
                if (cmdArgs[1] == "create") {
                    if (cmdArgs.Length != 4) {
                        process.Print("Usage : mail account create [username] [password]");
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
                    foreach (Account account in accounts)
                        if (account != null)
                            if (account.accountName == cmdArgs[2]) {
                                process.Print("This username already exists!");
                                return true;
                            }
                    daemon.AddAccount(new Account(cmdArgs[2], cmdArgs[3]));
                    process.Print($"Created an account with the name {cmdArgs[2]}");
                    return true;
                } else if (cmdArgs[1] == "login") {
                    if (cmdArgs.Length != 4) {
                        process.Print("Usage : mail account login [username] [password]");
                        return true;
                    }
                    Account accountToLogin = new Account(cmdArgs[2], cmdArgs[3]);
                    if (daemon.accounts.Count == 0) {
                        process.Print("This server has no accounts.");
                        return true;
                    }
                    foreach (Account account in daemon.accounts) {
                        if (account.accountName != accountToLogin.accountName || account.password != accountToLogin.password) {
                            process.Print("Either this account does not exist or the password is incorrect!");
                            return true;
                        }
                    }
                    client.loggedInAccount = accountToLogin;
                    process.Print($"Logged in as {accountToLogin.accountName}");
                } else if (cmdArgs[1] == "resetpass") {
                    if (cmdArgs.Length != 3) {
                        process.Print("Usage : mail account resetpass [new password]");
                        return true;
                    }
                    if (client.loggedInAccount == null) {
                        process.Print("You are not logged in!");
                        return true;
                    }
                    daemon.accounts.Remove(client.loggedInAccount);
                    client.loggedInAccount.password = cmdArgs[1];
                    daemon.AddAccount(client.loggedInAccount);
                    process.Print($"Your new password is \"{cmdArgs[1]}\"!");
					return true;
                }
            } else if (cmdArgs[0] == "config") {
                if (cmdArgs[1] == "dns") {
                    if (client.Credentials.Group != Permissions.Group.ROOT) {
                        process.Print("You must be logged in as root to use this command!");
                        return true;
                    }
                    if (cmdArgs.Length != 3) {
                        process.Print("mail config dns [IP of DNS Server]");
                        return true;
                    }
                    Node dnsServer = Server.Instance.GetComputerManager().GetNodeByIp(cmdArgs[1]);
                    if (dnsServer == null) {
                        process.Print($"{cmdArgs[2]} does not exist!");
                        return true;
                    }
                    File configFile = client.computer.fileSystem.rootFile.GetFileAtPath("mail/config.json");
                    JObject configObject = JObject.Parse(configFile.Content);
                    configObject["DNS"] = cmdArgs[2];
                    configFile.Content = configObject.ToString();
                }
            /*} else if (cmdArgs[0] == "send") {
                var email = cmdArgs[1].Split('@');
                int i = 0;
                string message = "";
                foreach (string word in cmdArgs) {
                    if (i <= 1)
                        message += word + " ";
                    i++;
                }
                if (client.loggedInAccount == null) {
                    process.Print("");
                }*/
            }
            return true;
        }
    }
}
