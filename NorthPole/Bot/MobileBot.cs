using NorthPole.Account;
using NorthPole.Helpers;
using NorthPole.Utils;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthPole.Bot
{
    public class MobileBot : BotBase
    {
        public MobileBot(AccountContext accountContext, List<string> searchWordList, Random random)
            : base(accountContext, searchWordList, random)
        {
            mobile = true;
        }

        public override void Setup()
        {
            FirefoxProfile profile = new FirefoxProfile();
            profile.SetPreference("general.useragent.override", Constants.MOBILEAGENT);
            driver = new FirefoxDriver(profile);
            driver.Navigate().GoToUrl(Constants.MOBILE_SIGNIN_URL);
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
            if (AccountContext.AccountCredits.MobileSearchCredits == -1 || AccountContext.AccountCredits.MobileSearchMaxCredits == -1)
            {
                DoDashboard_Workflow();
            }
            DoSearch_Workflow();
            while (!VerifyWorkDone())
            {
                DoSearch_Workflow();
            }
            TearDown();
        }

        public void DoDashboard_Workflow()
        {
            MobileDashboardHelper dbHelper = new MobileDashboardHelper(driver, AccountContext.AccountCredits);
            dbHelper.SetCreditsForToday();
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

        public bool VerifyWorkDone()
        {
            driver.Navigate().GoToUrl(Constants.HOMEPAGE + Constants.DASHBOARDURL);
            BotUtils.Wait(random);
            MobileDashboardHelper dbHelper = new MobileDashboardHelper(driver, AccountContext.AccountCredits);
            dbHelper.SetCurrentCredits();
            if (dbHelper.IsMobileComplete())
                return true;
            else
                return false;
        }

        private int CheckCreditsLeftToEarn()
        {
            int creditsLeftToEarn = AccountContext.AccountCredits.MobileSearchMaxCredits - AccountContext.AccountCredits.MobileSearchCredits;
            if (creditsLeftToEarn == 0)
            {
                Debug.WriteLine("accountContext.AccountCredits.MoibleSearchMaxCredits - accountContext.AccountCredits.MobileSearchCredits = 0");
                TearDown();
                throw new Exception("No credits left to earn. \n");
            }
            return creditsLeftToEarn;
        }
    }
}
