using NorthPole.Account;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthPole.Helpers
{
    public abstract class DashboardHelperBase
    {
        protected IWebDriver driver;
        private AccountCredits accountCredits;

        public AccountCredits AccountCredits
        {
            get { return accountCredits; }
            set { accountCredits = value; }
        }

        public DashboardHelperBase(IWebDriver driver, AccountCredits accountCredits)
        {
            this.driver = driver;
            AccountCredits = accountCredits;
        }
    }
}
