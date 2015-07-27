using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthPole.Account
{
    public class AccountContext
    {
        private AccountCredits accountCredits;

        public AccountCredits AccountCredits
        {
            get { return accountCredits; }
            set { accountCredits = value; }
        }
        private String email;

        public String Email
        {
            get { return email; }
            set { email = value; }
        }
        private String password;

        public String Password
        {
            get { return password; }
            set { password = value; }
        }

        public AccountContext(String email, String password)
        {
            Email = email;
            Password = password;
            accountCredits = new AccountCredits();
        }
    }
}
