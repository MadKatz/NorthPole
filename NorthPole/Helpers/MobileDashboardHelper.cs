using NorthPole.Account;
using NorthPole.Utils;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NorthPole.Helpers
{
    class MobileDashboardHelper : DashboardHelperBase
    {
        private String creditProgressID = "credit-progress";
        //Mobile Search
        private String mobileProgressXPath = "//div[contains(@class,'column right member')]";
        private String primaryClass = "primary";
        private String secondaryClass = "secondary";

        //Current Credits
        private String statusBarID = "status-bar";
        private String currentCreditXPath = "//div[contains(@class,'text member')]";
        private String suggestionID = "suggestion";
        private String progressClass = "progress";

        public MobileDashboardHelper(IWebDriver driver, AccountCredits accountCredits)
            : base(driver, accountCredits)
        {
        }

        public void SetCreditsForToday()
        {
            IWebElement mobileProgress = driver.FindElement(By.XPath(mobileProgressXPath));
            IWebElement primary = mobileProgress.FindElement(By.ClassName(primaryClass));
            IWebElement secondary = mobileProgress.FindElement(By.ClassName(secondaryClass));
            AccountCredits.MobileSearchCredits = BotUtils.GetIntegerFromString(primary.Text.ToString());
            AccountCredits.MobileSearchMaxCredits = BotUtils.GetIntegerFromString(secondary.Text.ToString());
        }

        public void SetCurrentCredits()
        {
            IWebElement suggestion = driver.FindElement(By.Id(suggestionID));
            IWebElement progress = suggestion.FindElement(By.ClassName(progressClass));
            String[] creditsString = Regex.Split(progress.Text, "/");
            int currentCredits = BotUtils.GetIntegerFromString(creditsString[0]);
            AccountCredits.CurrentCredits = currentCredits;
        }

        public bool IsMobileComplete()
        {
            SetCreditsForToday();
            if (AccountCredits.MobileSearchCredits < AccountCredits.MobileSearchMaxCredits)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
