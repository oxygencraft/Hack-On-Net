namespace HackLinks_Server.Daemons.Types.Mail {
    class MailAccount {
        public string accountName;
        public string password;

        public MailAccount(string name, string pass) {
            this.accountName = name;
            this.password = pass;
        }
    }
}
