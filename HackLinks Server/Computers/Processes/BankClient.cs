using HackLinks_Server.Daemons;
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
            { "account", new Tuple<string, Command>("account [create/login/resetpass/balance/transfer/transactions/close]\n    Performs an account operation.", Account) },
            { "balance", new Tuple<string, Command>("balance set [accountname] [value]/get [accountname]\n    Sets or gets balance (DEBUG COMMAND)", Balance) }
        };

        public override SortedDictionary<string, Tuple<string, Command>> Commands => commands;
        private Account loggedInAccount = null;

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

        public static bool Account(CommandProcess process, string[] command)
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
                            if (data.Length < 4)
                                continue;
                            accounts.Add(data[0]);
                        }
                    }
                    if (accounts.Contains(cmdArgs[1]))
                    {
                        process.Print("This account name is not available");
                        return true;
                    }
                    daemon.accounts.Add(new Account(cmdArgs[1], 0, cmdArgs[2], client.Session.owner.username));
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
                    Account accountTo = null;
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
                    string[] transactions = transactionLog.Content.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    if (transactions.Length == 0)
                    {
                        process.Print("The transaction log database is empty");
                        return true;
                    }
                    string transactionLogForClient = "";
                    foreach (var transactionUnsplit in transactions)
                    {
                        string[] transaction = transactionUnsplit.Split(',');
                        if (transaction[0] == client.loggedInAccount.accountName)
                            transactionLogForClient += transaction[1] + "\n";
                    }
                    if (transactionLogForClient == "")
                        transactionLogForClient += "Your transaction log is empty";
                    File transactionFileForClient = client.Session.owner.homeComputer.fileSystem.rootFile.GetFile("Bank_Transaction_Log_For_" + client.loggedInAccount.accountName);
                    if (transactionFileForClient == null)
                    {
                        transactionFileForClient = client.Session.owner.homeComputer.fileSystem.CreateFile(client.Session.owner.homeComputer, client.Session.owner.homeComputer.fileSystem.rootFile, "Bank_Transaction_Log_For_" + client.loggedInAccount.accountName);
                        transactionFileForClient.Content = transactionLogForClient;
                        transactionFileForClient.OwnerId = 0;
                        transactionFileForClient.Permissions.SetPermission(FilePermissions.PermissionType.User, true, true, true);
                        transactionFileForClient.Permissions.SetPermission(FilePermissions.PermissionType.Group, true, true, true);
                        transactionFileForClient.Group = transactionFileForClient.Parent.Group;
                        process.Print("A file containing your transaction log has been uploaded to your computer");
                        return true;
                    }
                    transactionFileForClient.Content = transactionLogForClient;
                    process.Print("A file containing your transaction log has been uploaded to your computer");
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
                Account account = null;
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
