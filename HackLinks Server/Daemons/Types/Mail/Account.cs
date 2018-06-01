namespace HackLinks_Server.Daemons.Types.Mail {
    class Account {
        public string accountName;
        public string password;

        public Account(string name, string pass) {
            this.accountName = name;
            this.password = pass;
        }
    }
}
