using HackLinks_Server.Computers;
using HackLinks_Server.Computers.Permissions;
using HackLinks_Server.Daemons.Types.Bank;
using HackLinks_Server.Files;
using HackLinksCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HackLinksCommon.NetUtil;

namespace HackLinks_Server.Daemons.Types
{
    class BankDaemon : Daemon
    {

        public SortedDictionary<string, Tuple<string, CommandHandler.Command>> daemonCommands = new SortedDictionary<string, Tuple<string, CommandHandler.Command>>()
        {
            { "account", new Tuple<string, CommandHandler.Command>("account [create/resetpass/balance/transfer/loan/transactions/close]\n    Performs an account operation.", Account) },
        };

        public override SortedDictionary<string, Tuple<string, CommandHandler.Command>> Commands
        {
            get => daemonCommands;
        }

        public override string StrType => "bank";

        public override DaemonType GetDaemonType()
        {
            return DaemonType.BANK;
        }

        public BankDaemon(Node node) : base(node)
        {
            
            this.accessLevel = Group.GUEST;
        }

        public List<Account> accounts = new List<Account>();

        public static bool Account(GameClient client, string[] command)
        {
            Session session = client.activeSession;

            BankDaemon daemon = (BankDaemon)client.activeSession.activeDaemon;

            if (command[0] == "account")
            {
                if (command.Length < 2)
                {
                    session.owner.Send(PacketType.MESSG, "Usage : account [create/resetpass/balance/transfer/loan/transactions/close]");
                    return true;
                }
                var cmdArgs = command[1].Split(' ');
                if (command[0] == "create")
                {
                    if (cmdArgs.Length < 3)
                    {
                        session.owner.Send(PacketType.MESSG, "Usage : account create [username] [password]");
                        return true;
                    }
                    var configFolder = session.connectedNode.fileSystem.rootFile.GetFile("cfg");
                    if (configFolder == null || !configFolder.IsFolder())
                    {
                        client.Send(NetUtil.PacketType.MESSG, "No config folder was found ! (Contact the admin of this node to create one as the bank is useless without one)");
                        return true;
                    }
                    var usersFile = configFolder.GetFile("users.cfg");
                    if (usersFile == null)
                    {
                        client.Send(NetUtil.PacketType.MESSG, "No config file was found ! (Contact the admin of this node to create one as the bank is useless without one)");
                        return true;
                    }
                    if (daemon.accounts.Count != 0)
                    {
                        foreach (var account in daemon.accounts)
                        {
                            if (account.username == client.username)
                            {
                                client.Send(PacketType.MESSG, "You already have an account with this bank.\nTo reset your password, use account resetpass");
                                return true;
                            }
                        }
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
                    if (command[1] == "Guest" || !accounts.Contains(command[1]))
                    {
                        if (command[1] == "Guest")
                            client.Send(PacketType.MESSG, "The account name Guest is not allowed to prevent errors");
                        else
                            client.Send(PacketType.MESSG, "This account name is not available");
                        return true;
                    }
                    usersFile.Content = usersFile.Content + "\r\nguest," + command[1] + "=" + command[2];
                    int accountNumber = new Random().Next(9999999);
                    daemon.accounts.Add(new Account(command[1], 0, client.username));
                    client.Send(PacketType.MESSG, "An account has been opened with account number: " + accountNumber + ". Use your account name to login.");
                }
                if (command[0] == "resetpass")
                {
                    client.Send(PacketType.MESSG, "To be implemented.\nPlease contact the admin of this node to reset your password.");
                    return true;
                }
                if (command[0] == "balance")
                {
                    if (session.currentUsername == "Guest")
                    {
                        client.Send(PacketType.MESSG, "You are not logged in");
                        return true;
                    }
                    Account account = null;
                    foreach (var account2 in daemon.accounts)
                    {
                        if (account2.username == session.currentUsername)
                        {
                            account = account2;
                            break;
                        }
                    }
                    if (account == null)
                    {
                        client.Send(PacketType.MESSG, "Account data for this account does not exist in the database");
                        return true;
                    }
                    client.Send(PacketType.MESSG, $"Account balance for {account.accountName} is {account.balance}");
                }
                return true;
            }
            return false;
        }

        public void LoadAccounts()
        {
            accounts.Clear();
            File accountFile = node.fileSystem.rootFile.GetFileAtPath("/bank/accounts.db");
            if (accountFile == null)
                return;
            foreach (string line in accountFile.Content.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                var data = line.Split(',');
                if (data.Length < 3)
                    continue;
                accounts.Add(new Account(data[0], Convert.ToInt32(data[1]), data[2]));
            }
        }

        public override void OnStartUp()
        {
            
        }

        public override bool HandleDaemonCommand(GameClient client, string[] command)
        {
            if (Commands.ContainsKey(command[0]))
                return Commands[command[0]].Item2(client, command);

            return false;
        }

        public override string GetSSHDisplayName()
        {
            return null;
        }
    }
}
