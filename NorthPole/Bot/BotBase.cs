using NorthPole.Account;
using NorthPole.Helpers;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthPole.Bot
{
    public abstract class BotBase
    {
        protected IWebDriver driver;
        protected Random random;
        protected List<string> searchWordList;
        protected bool mobile;
        private AccountContext accountContext;

        public AccountContext AccountContext
        {
            get { return accountContext; }
            set { accountContext = value; }
        }

        public BotBase(AccountContext accountContext, List<string> searchWordList, Random random)
        {
            this.accountContext = accountContext;
            this.searchWordList = searchWordList;
            this.random = random;
        }

        public abstract void Setup();
        public abstract void TearDown();
        public abstract void StartBot();

        public void SignIn()
        {
            SignInHelper signInHelper = new SignInHelper(driver, AccountContext.Email, AccountContext.Password, mobile);
            signInHelper.SignIn();
        }
    }
}
