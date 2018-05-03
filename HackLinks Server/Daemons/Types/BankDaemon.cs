using HackLinks_Server.Computers;
using HackLinks_Server.Computers.Permissions;
using HackLinks_Server.Computers.Processes;
using HackLinks_Server.Daemons.Types.Bank;
using HackLinks_Server.Files;
using HackLinksCommon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HackLinksCommon.NetUtil;

namespace HackLinks_Server.Daemons.Types
{
    class BankDaemon : Daemon
    {
        public override string StrType => "bank";

        protected override Type ClientType => typeof(BankClient);

        public override DaemonType GetDaemonType()
        {
            return DaemonType.BANK;
        }

        public BankDaemon(int pid, Printer printer, Node computer, Credentials credentials) : base(pid, printer, computer, credentials)
        {

        }

        public List<Account> accounts = new List<Account>();

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
                accounts.Add(new Account(data[0], Convert.ToInt32(data[1]), int.Parse(data[2])));
            }
        }

        public void UpdateAccountDatabase()
        {
            File accountFile = node.fileSystem.rootFile.GetFileAtPath("/bank/accounts.db");
            if (accountFile == null)
                return;
            string newAccountsFile = "";
            foreach (var account in accounts)
            {
                newAccountsFile += account.accountName + "," + 0 + "," + account.userId + "\r\n";
            }
            accountFile.Content = newAccountsFile;
        }

        public bool CheckFolders(CommandProcess process)
        {
            var configFolder = process.computer.fileSystem.rootFile.GetFile("cfg");
            if (configFolder == null || !configFolder.IsFolder())
            {
                process.Print("No config folder was found ! (Contact the admin of this node to create one as the bank is useless without one)");
                return false;
            }
            var usersFile = configFolder.GetFile("users.cfg");
            if (usersFile == null)
            {
                process.Print("No config file was found ! (Contact the admin of this node to create one as the bank is useless without one)");
                return false;
            }
            var bankFolder = process.computer.fileSystem.rootFile.GetFile("bank");
            if (configFolder == null || !configFolder.IsFolder())
            {
                process.Print("No bank daemon folder was found ! (Contact the admin of this node to create one as the bank is useless without one)");
                return false;
            }
            var accountFile = bankFolder.GetFile("accounts.db");
            if (accountFile == null)
            {
                process.Print("No accounts file was found ! (Contact the admin of this node to create one as the bank is useless without one)");
                return false;
            }

            return true;
        }

        public void ProcessBankTransfer(Account from, Account to, string ip, int amount)
        {
            if (ip != node.ip)
                return; // TODO handle remote bank transfer
            Account account = null;

            account.balance += amount;
        }

        public override void OnStartUp()
        {
            LoadAccounts();
        }

        public override string GetSSHDisplayName()
        {
            return null;
        }
    }
}
