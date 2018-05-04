using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackLinks_Server.Daemons.Types.Bank
{
    class Account
    {
        public string accountName;
        public int balance;
        public string password;
        public string clientUsername;

        public Account(string accountName, int balance, string password, string clientUsername)
        {
            this.accountName = accountName;
            this.balance = balance;
            this.password = password;
            this.clientUsername = clientUsername;
        }
    }
}
