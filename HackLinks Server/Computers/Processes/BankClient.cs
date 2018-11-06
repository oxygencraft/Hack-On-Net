﻿using HackLinks_Server.Daemons;
using HackLinks_Server.Daemons.Types;
using HackLinks_Server.Daemons.Types.Bank;
using HackLinks_Server.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.Computers.Processes
{
    class BankClient : DaemonClient
    {
        public SortedDictionary<string, Tuple<string, Command>> commands = new SortedDictionary<string, Tuple<string, Command>>()
        {
            { "account", new Tuple<string, Command>("account [create/login/resetpass/balance/transfer/transactions/close]\n    Performs an account operation.", BankAccount) },
            { "balance", new Tuple<string, Command>("balance set [accountname] [value]/get [accountname]\n    Sets or gets balance (DEBUG COMMAND)", Balance) }
        };

        public override SortedDictionary<string, Tuple<string, Command>> Commands => commands;
        private BankAccount loggedInAccount = null;

        public BankClient(Session session, Daemon daemon, int pid, Printer printer, Node computer, Credentials credentials) : base(session, daemon, pid, printer, computer, credentials)
        {
            
        }

        public override bool RunCommand(string command)
        {
            // We hide the old runCommand function to perform this check on startup
            if (!((BankDaemon)Daemon).CheckFolders(this))
            {
                return true;
            }
            return base.RunCommand(command);
        }

        public static bool BankAccount(CommandProcess process, string[] command)
        {
            BankClient client = (BankClient)process;
            BankDaemon daemon = (BankDaemon)client.Daemon;

            var bankFolder = process.computer.fileSystem.rootFile.GetFile("bank");
            var accountFile = bankFolder.GetFile("accounts.db");

            if (command[0] == "account")
            {
                if (command.Length < 2)
                {
                    process.Print("Usage : account [create/login/resetpass/balance/transfer/transactions/close]");
                    return true;
                }
                var cmdArgs = command[1].Split(' ');
                if (cmdArgs[0] == "create")
                {
                    if (cmdArgs.Length < 4)
                    {
                        process.Print("Usage : account create [accountname] [email] [password]");
                        return true;
                    }
                    string[] emailArgs = cmdArgs[2].Split('@');
                    if (emailArgs.Length != 2) {
                        process.Print("Not a valid email address");
                        return true;
                    }
                    Node emailServer = Server.Instance.GetComputerManager().GetNodeByIp(emailArgs[1]);
                    if (emailServer == null) {
                        process.Print("The email server does not exist!\nMake sure you use the IP of the mail server, not the domain.");
                    }
                    MailDaemon mailDaemon = (MailDaemon)emailServer.GetDaemon("mail");
                    if (mailDaemon == null) {
                        process.Print("IP of the email address does not have an email server installed!");
                        return true;
                    }
                    if (!mailDaemon.accounts.Any(x => x.accountName == emailArgs[0])) {
                        process.Print("The email account on that email server does not exist!");
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
                    daemon.accounts.Add(new BankAccount(cmdArgs[1], 0, cmdArgs[3], client.Session.owner.username, cmdArgs[2]));
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
                            return true;
                        }
                    }
                    process.Print("Invalid account name or password");
                }
                if (cmdArgs[0] == "resetpass")
                {
                    if (cmdArgs.Length == 2) {
                        BankAccount account = daemon.accounts.Where(x => x.accountName == cmdArgs[1]).DefaultIfEmpty(null).First();
                        if (account == null) {
                            process.Print("That account does not exist!");
                            return true;
                        }
                        if (!MailDaemon.SendPasswordResetEmail(process.computer, account.email, account.accountName)) {
                            process.Print("The email failed to send! Either the account no longer exists, or an authentication code has already been sent to this email less than an hour ago!");
                            return true;
                        }
                        process.Print("Password reset email sent to the email associated with this account!");
                        return true;
                    }
                    if (cmdArgs.Length < 4)
                    {
                        process.Print("Usage : account resetpass [accountname] [authentication code] [newpassword]");
                        return true;
                    }
                    if (!int.TryParse(cmdArgs[2], out int authCode)) {
                        process.Print("Please use a valid authentication code!");
                        return true;
                    }
                    if (!MailDaemon.CheckIfAuthRequestIsValid(process.computer, cmdArgs[1], authCode) || !daemon.accounts.Any(x => x.accountName == cmdArgs[1])) {
                        process.Print("Something went wrong when trying to authenticate!\nEither the username does not exist, or the authentication code is invalid!");
                        return true;
                    }
                    
                    foreach (BankAccount account in daemon.accounts) {
                        if (account.accountName == cmdArgs[1]) {
                            account.password = cmdArgs[3];
                            daemon.UpdateAccountDatabase();
                        }
                    }
                    process.Print("Password reset successful!");
                    return true;
                }
                if (cmdArgs[0] == "balance")
                {
                    if (client.loggedInAccount == null)
                    {
                        process.Print("You are not logged in");
                        return true;
                    }
                    process.Print($"Account balance for {client.loggedInAccount.accountName} is {client.loggedInAccount.balance}");
                }
                if (cmdArgs[0] == "transfer")
                {
                    if (cmdArgs.Length < 4)
                    {
                        process.Print("Usage : account transfer [receivingaccountname] [receivingbankip] [amount]");
                        return true;
                    }
                    if (client.loggedInAccount == null)
                    {
                        process.Print("You are not logged in");
                        return true;
                    }
                    if (client.loggedInAccount.balance < Convert.ToInt32(cmdArgs[3]))
                    {
                        process.Print("Account does not have enough balance");
                        return true;
                    }
                    if (Server.Instance.GetComputerManager().GetNodeByIp(cmdArgs[2]) == null)
                    {
                        process.Print("The receiving computer does not exist");
                        return true;
                    }
                    BankDaemon targetBank = null;
                    foreach (var computer in Server.Instance.GetComputerManager().NodeList)
                    {
                        if (computer.ip == cmdArgs[2])
                        {
                            Daemon targetDaemon = computer.GetDaemon("Bank");
                            if (targetDaemon == null)
                            {
                                process.Print("The receiving computer does not have a bank daemon");
                                return true;
                            }
                            targetBank = (BankDaemon)targetDaemon;
                            break;
                        }
                    }
                    BankAccount accountTo = null;
                    foreach (var account in targetBank.accounts)
                    {
                        if (account.accountName == cmdArgs[1])
                        {
                            accountTo = account;
                            break;
                        }
                    }
                    if (accountTo == null)
                    {
                        process.Print("The receiving account does not exist");
                        return true;
                    }                    
                    targetBank.ProcessBankTransfer(client.loggedInAccount, accountTo, cmdArgs[2], int.Parse(cmdArgs[3]), client.Session);
                    daemon.LogTransaction($"{client.loggedInAccount.accountName},{client.Session.owner.homeComputer.ip} transferred {cmdArgs[3]} from {client.loggedInAccount.accountName} to {accountTo.accountName}@{targetBank.computer.ip}", client.Session.sessionId, client.Session.owner.homeComputer.ip);
                }
                if (cmdArgs[0] == "transactions")
                {
                    if (client.loggedInAccount == null)
                    {
                        process.Print("You are not logged in");
                        return true;
                    }
                    File transactionLog = bankFolder.GetFile("transactionlog.db");
                    if (transactionLog == null)
                    {
                        process.Print("This bank does not keep transaction logs");
                        return true;
                    }
                    List<string> transactions = transactionLog.Content.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (transactions.Count == 0)
                    {
                        process.Print("The transaction log database is empty");
                        return true;
                    }
                    int pageNumber = 1;
                    int totalPages = transactions.Count / 10 == 0 ? 1 : transactions.Count / 10 + transactions.Count % 10 >= 1 ? 1 : 0;
                    if (cmdArgs.Length > 1)
                    {
                        if (int.TryParse(cmdArgs[1], out pageNumber))
                        {
                            if (pageNumber != 1)
                            {
                                if (pageNumber <= 0 || pageNumber > totalPages)
                                {
                                    process.Print("Invalid page number");
                                    return true;
                                }
                            }
                        }
                    }
                    string transactionLogForClient = "------ Transaction Log for " + client.loggedInAccount.accountName + " ------\n";
                    if (pageNumber == 1)
                        transactions = (List<string>)transactions.Where(transaction => transaction.Split(',')[0] == client.loggedInAccount.accountName).Take(10);
                    else
                        transactions = (List<string>)transactions.Where(transaction => transaction.Split(',')[0] == client.loggedInAccount.accountName).Skip(pageNumber * 10).Take(10);
                    //for (int i = transactions.Length - pageModifier; transactions.Length - pageModifier < i; i--)
                    //{
                    //    string[] transaction = transactions[currentTransaction].Split(',');
                    //    if (transaction[0] == client.loggedInAccount.accountName)
                    //    {
                    //        transactionLogForClient += transaction[1] + "\n";
                    //    }
                    //    else
                    //        i++;
                    //    currentTransaction--;
                    //}
                    foreach (var transaction in transactions)
                    {
                        transactionLogForClient += transaction[1] + "\n";
                    }
                    if (transactionLogForClient == "------ Transaction Log for " + client.loggedInAccount.accountName + " ------\n")
                        transactionLogForClient += "\nThis account's transaction log is empty\n";
                    transactionLogForClient += "------ Page " + pageNumber + "/" + totalPages + " ------";
                    process.Print(transactionLogForClient);
                }
                if (cmdArgs[0] == "close")
                {
                    if (client.loggedInAccount == null)
                    {
                        process.Print("You are not logged in");
                        return true;
                    }
                    if (client.loggedInAccount.balance != 0)
                    {
                        process.Print("Your account balance must be zero before you can close your account.\nUse account transfer to transfer your money out of your account");
                        return true;
                    }
                    daemon.accounts.Remove(client.loggedInAccount);
                    daemon.UpdateAccountDatabase();
                    client.loggedInAccount = null;
                    process.Print("Your account has been closed");
                }
                return true;
            }
            return false;
        }

        public static bool Balance(CommandProcess process, string[] command)
        {
            BankClient client = (BankClient)process;
            BankDaemon daemon = (BankDaemon)client.Daemon;

            if (command[0] == "balance")
            {
                if (command.Length < 2)
                {
                    process.Print("Usage : balance set [accountname] [value]/get [accountname]");
                    return true;
                }
                var cmdArgs = command[1].Split(' ');
                if (cmdArgs.Length < 2)
                {
                    process.Print("Usage : balance set [accountname] [value]/get [accountname]");
                    return true;
                }
                if (cmdArgs[0] == "set" && cmdArgs.Length < 3)
                {
                    process.Print("Usage : balance set [accountname] [value]/get [accountname]");
                    return true;
                }
                BankAccount account = null;
                foreach (var account2 in daemon.accounts)
                {
                    if (account2.accountName == cmdArgs[1])
                    {
                        account = account2;
                        break;
                    }
                }
                if (account == null)
                {
                    process.Print("Account data for this account does not exist in the database");
                    return true;
                }
                if (cmdArgs[0] == "set")
                {
                    if(int.TryParse(cmdArgs[2], out int val))
                    {
                        account.balance = val;
                        daemon.UpdateAccountDatabase();
                        var bankFolder = process.computer.fileSystem.rootFile.GetFile("bank");
                        daemon.LogTransaction($"{account.accountName},CHEATED Balance set to {val}", client.Session.sessionId, client.Session.owner.homeComputer.ip);
                    }
                    else
                    {
                        process.Print("Error: non-integer value specified");
                        return true;
                    }
                }
                process.Print($"Account balance for {account.accountName} is {account.balance}");
                return true;
            }
            return false;
        }
    }
}
