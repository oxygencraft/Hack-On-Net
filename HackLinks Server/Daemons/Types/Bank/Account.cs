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
        public int userId;

        public Account(string accountName, int balance, int userId)
        {
            this.accountName = accountName;
            this.balance = balance;
            this.userId = userId;
        }
    }
}
