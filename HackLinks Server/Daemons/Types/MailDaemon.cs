using HackLinks_Server.Computers;
using HackLinks_Server.Computers.Permissions;
using HackLinks_Server.Computers.Processes;
using HackLinks_Server.Daemons.Types.Mail;
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

namespace HackLinks_Server.Daemons.Types {
    class MailDaemon : Daemon {
        #region Overrides

        public override string StrType => "mail";

        protected override Type ClientType => typeof(MailClient);

        public override DaemonType GetDaemonType() {
            return DaemonType.MAIL;
        }

        public override void OnStartUp() {
            LoadAccounts();
        }

        public override string GetSSHDisplayName() {
            return "Mail";
        }

        #endregion

        public MailDaemon(int pid, Printer printer, Node computer, Credentials credentials) : base(pid, printer, computer, credentials) { }

        public List<Account> accounts = new List<Account>();

        #region Load Acoounts

        public void LoadAccounts() {
            accounts.Clear();

            File accountFile = node.fileSystem.rootFile.GetFileAtPath("/mail/accounts.db");

            if (accountFile == null)
                return;

            foreach (string line in accountFile.Content.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries)) {
                string[] data = line.Split(',');

                if (data[0] != "MAILACCOUNT" || data.Length != 3)
                    return;

                accounts.Add(new Account(data[1], data[2]));
            }
        }

        #endregion

        #region UpdateAccountDatabase

        public void UpdateAccountDatabase() {
            File accountFile = node.fileSystem.rootFile.GetFileAtPath("/mail/accounts.db");
            if (accountFile == null)
                return;

            string newAccountFile = "";

            foreach (Account account in accounts) {
                newAccountFile += "MAILACCOUNT" + account.accountName + "," + account.password + "\r\n";
            }

            accountFile.Content = newAccountFile;
        }

        #endregion

        #region Check Folders

        public bool CheckFolders(CommandProcess process) {
            File mailFolder = node.fileSystem.rootFile.GetFile("mail");
            if (mailFolder == null || !mailFolder.IsFolder()) {
                process.Print("No mail daemon folder was found! (Contact the admin of this node to create one as the mail daemon is useless without one)");
                return false;
            }

            File accountFile = mailFolder.GetFile("accounts.db");
            if (accountFile == null) {
                process.Print("No mail daemon file was found! (Contact the admin of this node to create one as the mail daemon is useless without one)");
                return false;
            }

            return true;
        }

        #endregion


    }
}
