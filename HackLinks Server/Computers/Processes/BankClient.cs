using HackLinks_Server.Daemons;
using HackLinks_Server.Daemons.Types;
using HackLinks_Server.Daemons.Types.Bank;
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
            { "account", new Tuple<string, Command>("account [create/resetpass/balance/transfer/loan/transactions/close]\n    Performs an account operation.", Account) },
            { "balance", new Tuple<string, Command>("balance [add/subtract]\n    Adds or subtracts balance (DEBUG COMMAND)", Balance) }
        };

        public override SortedDictionary<string, Tuple<string, Command>> Commands => commands;

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

            var configFolder = process.computer.fileSystem.rootFile.GetFile("cfg");
            var usersFile = configFolder.GetFile("users.cfg");
            var bankFolder = process.computer.fileSystem.rootFile.GetFile("bank");
            var accountFile = bankFolder.GetFile("accounts.db");

            if (command[0] == "account")
            {
                if (command.Length < 2)
                {
                    process.Print("Usage : account [create/resetpass/balance/transfer/loan/transactions/close]");
                    return true;
                }
                // TODO: Implement Loans
                // TODO: Implement Transaction Log
                var cmdArgs = command[1].Split(' ');
                if (cmdArgs[0] == "create")
                {
                    if (cmdArgs.Length < 3)
                    {
                        process.Print("Usage : account create [accountname] [password]");
                        return true;
                    }
                    List<string> accounts = new List<string>();
                    var accountsFile = usersFile.Content.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    if (accountsFile.Length != 0)
                    {
                        foreach (var account in accountsFile)
                        {
                            string[] accountData = account.Split('=');
                            string accountPassword = accountData[1];
                            // Update temporary holding array
                            accountData = accountData[0].Split(',');
                            accounts.Add(accountData[accountData.Length - 1]);
                        }
                    }
                    if (accounts.Contains(cmdArgs[1]))
                    {
                        process.Print("This account name is not available");
                        return true;
                    }
                    usersFile.Content = usersFile.Content + "\r\nguest," + cmdArgs[1] + "=" + cmdArgs[2];
                    int accountNumber = new Random().Next(9999999);
                    daemon.accounts.Add(new Account(cmdArgs[1], 0, client.Credentials.UserId));
                    accountFile.Content += cmdArgs[1] + "," + 0 + "," + client.Credentials.UserId + "\r\n";
                    process.Print("An account has been opened with account number: " + accountNumber + ". Use your account name to login.");
                }
                if (cmdArgs[0] == "resetpass")
                {
                    // TODO: Implement this when Jaber's pull is merged
                    process.Print("To be implemented.\nPlease contact the admin of this node to reset your password.");
                    return true;
                }
                if (cmdArgs[0] == "balance")
                {
                    if (cmdArgs.Length < 3)
                    {
                        process.Print("Usage : account balance [accountname]");
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
                    process.Print($"Account balance for {account.accountName} is {account.balance}");
                }
                if (cmdArgs[0] == "transfer")
                {
                    if (cmdArgs.Length < 4)
                    {
                        process.Print("Usage : account transfer [receivingaccountname] [bankip] [amount]");
                        return true;
                    }
                    Account accountFrom = null;
                    foreach (var account in daemon.accounts)
                    {
                        if (account.userId == client.Credentials.UserId)
                        {
                            accountFrom = account;
                            break;
                        }
                    }
                    if (accountFrom == null)
                    {
                        process.Print("Account data for this account does not exist in the database");
                        return true;
                    }
                    if (accountFrom.balance < Convert.ToInt32(cmdArgs[3]))
                    {
                        process.Print("Account does not have enough balance");
                        return true;
                    }
                    Account accountTo = null;
                    foreach (var account in daemon.accounts)
                    {
                        if (account.accountName == cmdArgs[1])
                        {
                            accountTo = account;
                            break;
                        }
                    }

                    daemon.ProcessBankTransfer(accountFrom, accountTo, cmdArgs[2], int.Parse(cmdArgs[3]));
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
                if (command.Length < 3)
                {
                    process.Print("Usage : balance [[set [account] [value]]/[get [account]]]");
                    return true;
                }
                if (command.Length < 3)
                {
                    process.Print("Usage : balance [[set [account] [value]]/[get [account]]]");
                    return true;
                }
                Account account = null;
                foreach (var account2 in daemon.accounts)
                {
                    if (account2.accountName == command[2])
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
                account.balance = Convert.ToInt32(command[3]);
                return true;
            }
            return false;
        }
    }
}
