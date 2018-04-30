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
        public string username;

        public Account(string accountName, int balance, string username)
        {
            this.accountName = accountName;
            this.balance = balance;
            this.username = username;
        }
    }
}
