using HackLinks_Server.Computers;
using HackLinks_Server.Computers.Processes;
using HackLinks_Server.Daemons.Types.Mail;
using HackLinks_Server.Files;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

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

        public static readonly JObject defaultConfig = new JObject(
            new JProperty("DNS", "8.8.8.8"));

        public List<MailAccount> accounts = new List<MailAccount>();

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

                accounts.Add(new Mail.MailAccount(data[1], data[2]));
            }
        }

        #endregion

        #region UpdateAccountDatabase

        public void UpdateAccountDatabase() {
            File accountFile = node.fileSystem.rootFile.GetFileAtPath("/mail/accounts.db");
            if (accountFile == null)
                return;

            string newAccountFile = "";

            foreach (Mail.MailAccount account in accounts) {
                newAccountFile += "MAILACCOUNT," + account.accountName + "," + account.password + "\r\n";
            }

            accountFile.Content = newAccountFile;
        }

        #endregion

        #region Add Account

        public void AddAccount(MailAccount newAccount) {
            accounts.Add(newAccount);
            File mailDir = node.fileSystem.rootFile.GetFile("mail");
            File usersDir = mailDir.GetFile($"users");
            if (usersDir == null || !usersDir.IsFolder()) {
                if (usersDir != null)
                    usersDir.RemoveFile();
                usersDir = File.CreateNewFolder(node.fileSystem.fileSystemManager, node, mailDir, "users");
                SetFileAsRoot(usersDir);
            }
            File userDir = usersDir.GetFile(newAccount.accountName);
            if (userDir == null || !userDir.IsFolder()) {
                if (userDir != null)
                    userDir.RemoveFile();
                userDir = File.CreateNewFolder(node.fileSystem.fileSystemManager, node, usersDir, newAccount.accountName);
                SetFileAsRoot(userDir);
            }
            File inboxDir = userDir.GetFile("Inbox");
            File sentDir = userDir.GetFile("Sent");
            if (inboxDir == null || !inboxDir.IsFolder()) {
                if (inboxDir != null)
                    inboxDir.RemoveFile();
                inboxDir = File.CreateNewFolder(node.fileSystem.fileSystemManager, node, userDir, "Inbox");
                SetFileAsRoot(inboxDir);
            }
            if (sentDir == null || !sentDir.IsFolder()) {
                if (sentDir != null)
                    sentDir.RemoveFile();
                sentDir = File.CreateNewFolder(node.fileSystem.fileSystemManager, node, userDir, "Sent");
                SetFileAsRoot(sentDir);
            }
            UpdateAccountDatabase();
        }

        #endregion

        #region Receive Mail

        public bool ReceiveMail(MailMessage message) {
            bool DoesReceiveAccountExist = false;
            foreach (MailAccount account in accounts)
                if (account.accountName == message.To)
                    DoesReceiveAccountExist = true;
            if (!DoesReceiveAccountExist)
                return false;
            File userInboxDir = node.fileSystem.rootFile.GetFileAtPath($"mail/users/{message.To}/Inbox");
            if (userInboxDir == null)
                return false;
            File messageFile = File.CreateNewFile(node.fileSystem.fileSystemManager, node, userInboxDir, $"{userInboxDir.children.Count + 1}.json");
            messageFile.Content = message.ToJObject().ToString();
            SetFileAsRoot(messageFile);
            return true;
        }

        #endregion

        #region Check Folders

        public bool CheckFolders() {
            File mailFolder = node.fileSystem.rootFile.GetFile("mail");
            if (mailFolder == null || !mailFolder.IsFolder()) {
                if (mailFolder != null)
                    mailFolder.RemoveFile();
                mailFolder = File.CreateNewFolder(node.fileSystem.fileSystemManager, node, node.fileSystem.rootFile, "mail");
                SetFileAsRoot(mailFolder);
            }

            File accountFile = mailFolder.GetFile("accounts.db");
            if (accountFile == null) {
                accountFile = File.CreateNewFile(node.fileSystem.fileSystemManager, node, mailFolder, "accounts.db");
                SetFileAsRoot(accountFile);
            }

            File configFile = mailFolder.GetFile("config.json");
            if (configFile == null) {
                configFile = File.CreateNewFile(node.fileSystem.fileSystemManager, node, mailFolder, "config.json");
                configFile.Content = defaultConfig.ToString();
                SetFileAsRoot(configFile);
            }

            return true;
        }

        #endregion

        #region Helpers

        private void SetFileAsRoot(File file) {
            if (file == null)
                throw new ArgumentNullException(nameof(file));
            file.Group = Computers.Permissions.Group.ROOT;
            file.OwnerId = 0;
            file.Permissions.SetPermission(FilePermissions.PermissionType.User, true, true, true);
        }

        #endregion
    }
}
