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
using static HackLinks_Server.Computers.Processes.CommandProcess;
using static HackLinksCommon.NetUtil;

namespace HackLinks_Server.Daemons.Types
{
    class BankDaemon : Daemon
    {



        public override string StrType => "bank";

        protected override Type ClientType => typeof(IRCClient);

        public override DaemonType GetDaemonType()
        {
            return DaemonType.BANK;
        }

        public BankDaemon(int pid, Printer printer, Node computer, Credentials credentials) : base(pid, printer, computer, credentials)
        {
            this.accessLevel = Group.GUEST;
        }

        public List<Account> accounts = new List<Account>();
        internal static ObservableCollection<string> bankTransfers = new ObservableCollection<string>();

        // TODO: Fix this daemon for Jaber's pull when it is merged (preferrablely merging when that daemon bug is fixed would be better)

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

        public void UpdateAccountDatabase()
        {
            File accountFile = node.fileSystem.rootFile.GetFileAtPath("/bank/accounts.db");
            if (accountFile == null)
                return;
            string newAccountsFile = "";
            foreach (var account in accounts)
            {
                newAccountsFile += account.accountName + "," + 0 + "," + account.username + "\r\n";
            }
            accountFile.Content = newAccountsFile;
        }

        public void ProcessBankTransfer(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                string transferUnsplit = (string)e.NewItems[0];
                string[] transfernameip = transferUnsplit.Split('@');
                string[] transferipamount = transfernameip[1].Split(':');
                if (transferipamount[0] != node.ip)
                    return;
                Account account = null;
                foreach (var account2 in accounts)
                {
                    if (account2.accountName == transfernameip[0])
                    {
                        account = account2;
                        break;
                    }
                }
                if (account == null)
                    return;
                account.balance += Convert.ToInt32(transferipamount[1]);
            }
        }

        public override void OnStartUp()
        {
            LoadAccounts();
            bankTransfers.CollectionChanged += ProcessBankTransfer;
        }

        public override string GetSSHDisplayName()
        {
            return null;
        }
    }
}
