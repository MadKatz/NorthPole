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

        public DesktopBot(AccountContext accountContext, List<string> searchWordList, Random random)
            : base(accountContext, searchWordList, random)
        {
            mobile = false;
        }

        public override void Setup()
        {
            driver = new FirefoxDriver();
            driver.Navigate().GoToUrl(Constants.DESKTOP_SIGNIN_URL);
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

        public void DoOffers()
        {
            OfferHelper offerHelper = new OfferHelper(driver, random);
            offerHelper.DoOffers(AccountContext.AccountCredits);
        }

        public void DoDashboard_Workflow()
        {
            DesktopDashboardHelper dbHelper = new DesktopDashboardHelper(driver, AccountContext.AccountCredits);
            dbHelper.SetCurrentCredits();
            dbHelper.IsMobileComplete();
            DoOffers();
            if (dbHelper.IsDesktopComplete())
            {
                TearDown();
                throw new Exception("Max PC search credits reached. \n");
            }
            dbHelper.SetCreditsForToday(mobile);
        }

        public bool VerifyWorkDone()
        {
            driver.Navigate().GoToUrl(Constants.HOMEPAGE + Constants.DASHBOARDURL);
            BotUtils.Wait(random);
            DesktopDashboardHelper dbHelper = new DesktopDashboardHelper(driver, AccountContext.AccountCredits);
            dbHelper.SetCurrentCredits();
            if (dbHelper.IsDesktopComplete())
                return true;
            dbHelper.SetCreditsForToday(mobile);
            return false;
        }

        public void DoSearch_Workflow()
        {
            int creditsLeftToEarn = CheckCreditsLeftToEarn();
            driver.Navigate().GoToUrl(Constants.HOMEPAGE);
            BotUtils.Wait(random);
            SearchHelper searchHelper = new SearchHelper(driver, searchWordList, random);
            bool needNewSearch = true;
            while (searchHelper.SearchCounter < Constants.SEARCHTOPOINTRATIO * creditsLeftToEarn)
            {
                if (needNewSearch)
                {
                    searchHelper.DoSearch();
                    needNewSearch = false;
                }
                if (!searchHelper.DoRelatedSearch(mobile))
                {
                    needNewSearch = true;
                }                
            }
        }

        private int CheckCreditsLeftToEarn()
        {
            int creditsLeftToEarn = AccountContext.AccountCredits.PCSearchMaxCredits - AccountContext.AccountCredits.PCSearchCredits;
            if (creditsLeftToEarn == 0)
            {
                TearDown();
                Debug.WriteLine("accountContext.AccountCredits.PCSearchMaxCredits - accountContext.AccountCredits.PCSearchCredits = 0");
                throw new Exception("No credits left to earn. \n");
            }
            return creditsLeftToEarn;
        }
    }
}
