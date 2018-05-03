using HackLinks_Server.Daemons;
using HackLinksCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HackLinksCommon.NetUtil;
using HackLinks_Server.Daemons.Types.Bank;
using HackLinks_Server.Daemons.Types;

namespace HackLinks_Server.Computers.Processes
{
    class BankClient : DaemonClient
    {
        public SortedDictionary<string, Tuple<string, Command>> daemonCommands = new SortedDictionary<string, Tuple<string, Command>>()
        {
            { "account", new Tuple<string, Command>("account [create/resetpass/balance/transfer/loan/transactions/close]\n    Performs an account operation.", Account) },
            { "balance", new Tuple<string, Command>("balance [add/subtract]\n    Adds or subtracts balance (DEBUG COMMAND)", Balance) }
        };

        public override SortedDictionary<string, Tuple<string, Command>> Commands
        {
            get => daemonCommands;
        }

        public BankClient(Session session, Daemon daemon, int pid, Printer printer, Node computer, Credentials credentials) : base(session, daemon, pid, printer, computer, credentials)
        {
            
        }

        public static bool Account(CommandProcess process, string[] command)
        {
            BankClient client = (BankClient)process;
            BankDaemon daemon = (BankDaemon)client.Daemon;
            Session session = client.Session;

            if (command[0] == "account")
            {
                if (command.Length < 2)
                {
                    process.Print("Usage : account [create/resetpass/balance/transfer/loan/transactions/close]");
                    return true;
                }
                // TODO: Implement Loans
                // TODO: Implement Transaction Log
                // TODO: Update and implement anything user account management related when Jaber's pull has been merged
                var cmdArgs = command[1].Split(' ');
                if (cmdArgs[0] == "create")
                {
                    if (cmdArgs.Length < 3)
                    {
                        session.owner.Send(PacketType.MESSG, "Usage : account create [accountname] [password]");
                        return true;
                    }
                    var configFolder = session.connectedNode.fileSystem.rootFile.GetFile("cfg");
                    if (configFolder == null || !configFolder.IsFolder())
                    {
                        process.Print("No config folder was found ! (Contact the admin of this node to create one as the bank is useless without one)");
                        return true;
                    }
                    var usersFile = configFolder.GetFile("users.cfg");
                    if (usersFile == null)
                    {
                        process.Print("No config file was found ! (Contact the admin of this node to create one as the bank is useless without one)");
                        return true;
                    }
                    var bankFolder = session.connectedNode.fileSystem.rootFile.GetFile("bank");
                    if (configFolder == null || !configFolder.IsFolder())
                    {
                        process.Print("No bank daemon folder was found ! (Contact the admin of this node to create one as the bank is useless without one)");
                        return true;
                    }
                    var accountFile = bankFolder.GetFile("accounts.db");
                    if (accountFile == null)
                    {
                        process.Print("No accounts file was found ! (Contact the admin of this node to create one as the bank is useless without one)");
                        return true;
                    }
                    List<string> accounts = new List<string>();
                    var accountsFile = usersFile.Content.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
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
                    if (cmdArgs[1] == "Guest" || accounts.Contains(cmdArgs[1]))
                    {
                        if (cmdArgs[1] == "Guest")
                            process.Print("The account name Guest is not allowed to prevent errors");
                        else
                            process.Print("This account name is not available");
                        return true;
                    }
                    int nodeAccountsCount = usersFile.Content.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).Length + 1;
                    usersFile.Content = usersFile.Content + "\r\n" + cmdArgs[1] + ":" + cmdArgs[2] + ":" + nodeAccountsCount + "";
                    daemon.accounts.Add(new Account(cmdArgs[1], 0, session.owner.username));
                    accountFile.Content += cmdArgs[1] + "," + 0 + "," + session.owner.username + "\r\n";
                    process.Print("Your account has been opened. Use your account name to login.");
                }
                if (cmdArgs[0] == "resetpass")
                {
                    // TODO: Implement this when Jaber's pull is merged
                    process.Print("To be implemented.\nPlease contact the admin of this node to reset your password.");
                    return true;
                }
                if (cmdArgs[0] == "balance")
                {
                    if (process.computer.GetUsername(process.Credentials.UserId) == "Guest")
                    {
                        process.Print("You are not logged in");
                        return true;
                    }
                    Account account = null;
                    foreach (var account2 in daemon.accounts)
                    {
                        if (account2.accountName == process.computer.GetUsername(process.Credentials.UserId))
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
                        session.owner.Send(PacketType.MESSG, "Usage : account transfer [receivingaccountname] [bankip] [amount]");
                        return true;
                    }
                    if (process.computer.GetUsername(process.Credentials.UserId) == "Guest")
                    {
                        process.Print("You are not logged in");
                        return true;
                    }
                    Account account = null;
                    foreach (var account2 in daemon.accounts)
                    {
                        if (account2.accountName == process.computer.GetUsername(process.Credentials.UserId))
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
                    if (account.balance < Convert.ToInt32(cmdArgs[3]))
                    {
                        process.Print("Account does not have enough balance");
                        return true;
                    }
                    if (BankDaemon.bankTransfers.Count != 0)
                        BankDaemon.bankTransfers.Clear();
                    account.balance -= Convert.ToInt32(cmdArgs[3]);
                    BankDaemon.bankTransfers.Add(cmdArgs[1] + "@" + cmdArgs[2] + ":" + cmdArgs[3]);
                }
                return true;
            }
            return false;
        }

        public static bool Balance(CommandProcess process, string[] command)
        {
            BankClient client = (BankClient)process;
            BankDaemon daemon = (BankDaemon)client.Daemon;
            Session session = client.Session;

            if (command[0] == "balance")
            {
                if (command.Length < 2)
                {
                    process.Print("Usage : balance [balance]");
                    return true;
                }
                if (process.computer.GetUsername(process.Credentials.UserId) == "Guest")
                {
                    process.Print("You are not logged in");
                    return true;
                }
                Account account = null;
                foreach (var account2 in daemon.accounts)
                {
                    if (account2.accountName == process.computer.GetUsername(process.Credentials.UserId))
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
                account.balance = Convert.ToInt32(command[1]);
                return true;
            }
            return false;
        }
    }
}
