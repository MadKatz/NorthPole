using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NorthPole.Account;
using OpenQA.Selenium.Firefox;
using NorthPole.Helpers;
using System.Diagnostics;
using NorthPole.Utils;

namespace NorthPole.Bot
{
    public class DesktopBot : BotBase
    {
        protected Random random;
        protected List<string> searchWordList;
        private AccountContext accountContext;

        public AccountContext AccountContext
        {
            get { return accountContext; }
            set { accountContext = value; }
        }

        public DesktopBot(AccountContext accountContext, List<string> searchWordList, Random random)
            : base()
        {
            this.accountContext = accountContext;
            this.searchWordList = searchWordList;
            this.random = random;
        }

        public override void Setup()
        {
            driver = new FirefoxDriver();
            driver.Navigate().GoToUrl(Constants.SIGNINURL);
        }

        public override void TearDown()
        {
            driver.Quit();
        }

        public override void StartBot()
        {
            Setup();
            SignIn();
            BotUtils.Wait(random);
            DoDashboard_Workflow();
            DoSearch_Workflow();
            while (!VerifyWorkDone())
            {
                DoSearch_Workflow();
            }
            TearDown();
        }

        public void SignIn()
        {
            SignInHelper signInHelper = new SignInHelper(driver, accountContext.Email, accountContext.Password, false);
            signInHelper.SignIn();
        }

        public void DoOffers()
        {
            //TODO: move ref of accountContext.AccountCredits to helper instead of ref ref
            OfferHelper offerHelper = new OfferHelper(driver, random);
            int offerMaxCredits = -1;
            int offerCredits = -1;
            offerHelper.DoOffers(ref offerMaxCredits, ref offerCredits);
            AccountContext.AccountCredits.OfferCredits = offerCredits;
            AccountContext.AccountCredits.OfferMaxCredits = offerMaxCredits;
        }

        public void DoDashboard_Workflow()
        {
            DesktopDashboardHelper dbHelper = new DesktopDashboardHelper(driver, AccountContext.AccountCredits);
            dbHelper.SetCurrentCredits();
            dbHelper.IsMobileComplete();
            DoOffers();
            if (dbHelper.IsDesktopComplete())
                throw new Exception("Max PC search credits reached.");
            dbHelper.SetCreditsForToday(false);
        }

        public bool VerifyWorkDone()
        {
            driver.Navigate().GoToUrl(Constants.HOMEPAGE + Constants.DASHBOARDURL);
            BotUtils.Wait(random);
            DesktopDashboardHelper dbHelper = new DesktopDashboardHelper(driver, AccountContext.AccountCredits);
            dbHelper.SetCurrentCredits();
            if (dbHelper.IsDesktopComplete())
                return true;
            dbHelper.SetCreditsForToday(false);
            return false;
        }

        public void DoSearch_Workflow()
        {
            driver.Navigate().GoToUrl(Constants.HOMEPAGE);
            SearchHelper searchHelper = new SearchHelper(driver, searchWordList, random);
            int creditsLeftToEarn = accountContext.AccountCredits.PCSearchMaxCredits - accountContext.AccountCredits.PCSearchCredits;
            if (creditsLeftToEarn == 0)
            {
                Debug.WriteLine("accountContext.AccountCredits.PCSearchMaxCredits - accountContext.AccountCredits.PCSearchCredits = 0");
                throw new Exception("No credits left to earn.");
            }
            bool needNewSearch = true;
            while (searchHelper.SearchCounter < Constants.SEARCHTOPOINTRATIO * creditsLeftToEarn)
            {
                if (needNewSearch)
                {
                    searchHelper.DoSearch();
                    needNewSearch = false;
                }
                if (!searchHelper.DoRelatedSearch(false))
                {
                    needNewSearch = true;
                }                
            }
        }
    }
}
