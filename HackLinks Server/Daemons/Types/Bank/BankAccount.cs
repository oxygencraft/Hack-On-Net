namespace HackLinks_Server.Daemons.Types.Bank {
    class BankAccount
    {
        public string accountName;
        public int balance;
        public string password;
        public string clientUsername;
        public string email;

        public BankAccount(string accountName, int balance, string password, string clientUsername, string email)
        {
            this.accountName = accountName;
            this.balance = balance;
            this.password = password;
            this.clientUsername = clientUsername;
            this.email = email;
        }
    }
}
