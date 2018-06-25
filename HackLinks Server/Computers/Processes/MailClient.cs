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
        private const string help = "mail account\nOperations on your account\nmail list (page number)\nLists your received mail\nmail send [email address] [message]\nSends an email";
        public SortedDictionary<string, Tuple<string, Command>> commands = new SortedDictionary<string, Tuple<string, Command>>() {
            { "mail", new Tuple<string, Command>(help, MailServerCommands)}
        };
        public override SortedDictionary<string, Tuple<string, Command>> Commands => commands;

        MailAccount loggedInAccount = null;

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
                if (cmdArgs.Length < 2) {
                    process.Print(help);
                    return true;
                }
                if (cmdArgs[1] == "create") {
                    if (cmdArgs.Length != 4) {
                        process.Print("Usage : mail account create [username] [password]");
                        return true;
                    }
                    List<MailAccount> accounts = new List<MailAccount>();
                    accounts.AddRange(accountFile.Content.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Select(x => {
                        // The mail account format is MAILACCOUNT,(username),(password)
                        var data = x.Split(',');
                        if (data[0] != "MAILACCOUNT" || data.Length < 3)
                            return null;
                        return new MailAccount(data[1], data[2]);
                    }));
                    foreach (MailAccount account in accounts)
                        if (account != null)
                            if (account.accountName == cmdArgs[2]) {
                                process.Print("This username already exists!");
                                return true;
                            }
                    daemon.AddAccount(new MailAccount(cmdArgs[2], cmdArgs[3]));
                    process.Print($"Created an account with the name {cmdArgs[2]}");
                    return true;
                } else if (cmdArgs[1] == "login") {
                    if (cmdArgs.Length != 4) {
                        process.Print("Usage : mail account login [username] [password]");
                        return true;
                    }
                    MailAccount accountToLogin = new MailAccount(cmdArgs[2], cmdArgs[3]);
                    if (daemon.accounts.Count == 0) {
                        process.Print("This server has no accounts.");
                        return true;
                    }
                    bool accountExists = false;
                    foreach (MailAccount account in daemon.accounts)
                        if (account.accountName == accountToLogin.accountName && account.password == accountToLogin.password)
                            accountExists = true;
                    if (!accountExists) {
                        process.Print("This account either doesn't exist or the password is incorrect!");
                        return true;
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
                if (cmdArgs.Length < 2) {
                    process.Print(help);
                    return true;
                }
                if (cmdArgs[1] == "dns") {
                    if (!Permissions.PermissionHelper.CheckCredentials(process.Credentials, Permissions.Group.ROOT)) {
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
                    File dnsDaemon = dnsServer.fileSystem.rootFile.GetFileAtPath("daemons/dns");
                    if (dnsDaemon == null) {
                        process.Print($"{dnsServer.ip} doesn't have a DNS daemon");
                        return true;
                    }
                    File configFile = client.computer.fileSystem.rootFile.GetFileAtPath("mail/config.json");
                    JObject configObject = JObject.Parse(configFile.Content);
                    configObject["DNS"] = cmdArgs[2];
                    configFile.Content = configObject.ToString();
                }
            } else if (cmdArgs[0] == "send") {
                if (cmdArgs.Length < 2) {
                    process.Print(help);
                    return true;
                }
                var email = cmdArgs[1].Split('@');
                int i = 0;
                string message = "";
                foreach (string word in cmdArgs) {
                    if (i > 1)
                        message += word + " ";
                    i++;
                }
                if (client.loggedInAccount == null) {
                    process.Print("You aren't logged in!");
                    return true;
                }
                JObject config = JObject.Parse(process.computer.fileSystem.rootFile.GetFileAtPath("mail/config.json").Content);
                Node dnsServer = Server.Instance.GetComputerManager().GetNodeByIp(config.Properties()
                    .Where(x => x.Name == "DNS")
                    .Select(y => { return (string)y.Value; })
                    .Single());
                if (dnsServer == null) {
                    process.Print($"Error! The specified DNS server ({dnsServer.ip}) does not exist! Please notify the network admin!");
                    return true;
                }
                File dnsFile = dnsServer.fileSystem.rootFile.GetFileAtPath("daemons/dns");
                if (dnsFile == null) {
                    process.Print($"Error! {dnsServer.ip} does not have a DNS server installed! Please notify the network admin!");
                    return true;
                }
                string ipOfMailServer = new DNSDaemon(dnsServer.NextPID, null, dnsServer, new Credentials(dnsServer.GetUserId("guest"), Permissions.Group.GUEST)).LookUp(email[1], true);
                Node mailServer = Server.Instance.GetComputerManager().GetNodeByIp(ipOfMailServer);
                if (mailServer == null) {
                    process.Print("The receiving server does not exist!");
                    return true;
                }
                File mailDaemonFile = mailServer.fileSystem.rootFile.GetFileAtPath("mail/config.json");
                if (mailDaemonFile == null) {
                    process.Print("The receiving end does not have a mail server set up!");
                    return true;
                }
                MailMessage messageObject = new MailMessage(email[0], client.loggedInAccount.accountName + "@" + process.computer.ip, message);
                if (!(new MailDaemon(mailServer.NextPID, null, mailServer, new Credentials(mailServer.GetUserId("guest"), Permissions.Group.GUEST)).ReceiveMail(messageObject))) {
                    process.Print("The receiving account does not exist!");
                    return true;
                }
                process.Print("The email has been sent!");
                return true;
            } else if (cmdArgs[0] == "list") {
                if (client.loggedInAccount == null) {
                    process.Print("You aren't logged in!");
                    return true;
                }
                List<File> children = process.computer.fileSystem.rootFile.GetFileAtPath($"mail/users/{client.loggedInAccount.accountName}/Inbox").children;
                if (cmdArgs.Length != 2) {
                    string outputString = "Here are your ten most recent messages :\n";
                    int i = 0;
                    foreach (File message in process.computer.fileSystem.rootFile.GetFileAtPath($"mail/users/{client.loggedInAccount.accountName}/Inbox").children) {
                        MailMessage messageObject = new MailMessage(message);
                        outputString += $"[{message.Name.Replace(".json", "")}] From {messageObject.From} At {messageObject.TimeSent}\n";
                        i++;
                        if (i >= 10)
                            break;
                    }
                    process.Print(outputString);
                    return true;
                }
                if (!int.TryParse(cmdArgs[1], out _)) {
                    process.Print("Please provide a valid page number!");
                    return true;
                }
                int page = int.Parse(cmdArgs[1]);
                if (!(page > 0) || (double)page - 1 > (double)children.Count / 10) {
                    process.Print("Please provide a valid page number!");
                    return true;
                }
                int startInt = page == 1 ? 1 : page * 10 - 10;
                int endInt = startInt + 9;
                int index = 1;
                List<File> messages = new List<File>();
                foreach (File message in children) {
                    if (index >= startInt && index <= endInt) {
                        messages.Add(message);
                    }
                    index++;
                }
                string printString = $"Page {page} :\n";
                foreach (File message in messages) {
                    MailMessage messageObject = new MailMessage(message);
                    printString += $"[{message.Name.Replace(".json", "")}] From {messageObject.From} At {messageObject.TimeSent}\n";
                }
                process.Print(printString);
                return true;
            } else if (cmdArgs[0] == "show") {
                if (client.loggedInAccount == null) {
                    process.Print("You aren't logged in!");
                    return true;
                }
                if (cmdArgs.Length != 2) {
                    process.Print(help);
                    return true;
                }
                if (!int.TryParse(cmdArgs[1], out int number)) {
                    process.Print("Please provide a proper message number");
                    return true;
                }
                File message = process.computer.fileSystem.rootFile.GetFileAtPath($"mail/users/{client.loggedInAccount.accountName}/Inbox/{number}.json");
                if (message == null) {
                    process.Print("That message doesn't exist!");
                    return true;
                }
                MailMessage messageObject = new MailMessage(message);
                process.Print(messageObject.Body);
                return true;
            }
            return true;
        }
    }
}
