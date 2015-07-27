using NorthPole.Account;
using NorthPole.Utils;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NorthPole.Helpers
{
    public class DesktopDashboardHelper : DashboardHelperBase
    {
        private AccountCredits accountCredits;

        public AccountCredits AccountCredits
        {
            get { return accountCredits; }
            set { accountCredits = value; }
        }
        protected String checkmarkXPATH = "//div[contains(@class, 'check close-check dashboard-sprite')]";
        protected String userStatusID = "user-status";
        protected String creditsLeftClass = "credits";
        protected String pcSearch = "PC search";
        protected String mobileSearch = "Moible search";


        public DesktopDashboardHelper(IWebDriver driver, AccountCredits accountCredits) : base(driver)
        {
            AccountCredits = accountCredits;
        }

        public AccountCredits GetAccountCredits()
        {
            return AccountCredits;
        }

        public void SetCurrentCredits()
        {
            IWebElement userStatusContainer = driver.FindElement(By.Id(userStatusID));
            IWebElement creditsLeftElement = userStatusContainer.FindElement(By.ClassName(creditsLeftClass));
            String creditsLeft = creditsLeftElement.Text;
            accountCredits.CurrentCredits = Int32.Parse(creditsLeft);
        }

        public bool IsDesktopComplete()
        {
            var eList = driver.FindElements(By.XPath(checkmarkXPATH));
            foreach (var element in eList)
            {
                IWebElement targetElement = element.FindElement(By.XPath("../.."));
                if (targetElement.FindElement(By.ClassName("title")).Text.Contains(pcSearch))
                {
                    String credits = targetElement.FindElement(By.ClassName("progress")).Text.ToString();
                    SetCompletedSearchCredits(credits, false);
                    return true;
                }
            }
            return false;
        }

        public bool IsMobileComplete()
        {
            var eList = driver.FindElements(By.XPath(checkmarkXPATH));
            foreach (var element in eList)
            {
                IWebElement targetElement = element.FindElement(By.XPath("../../.."));
                if (targetElement.FindElement(By.ClassName("title")).Text.Contains(mobileSearch))
                {
                    String credits = targetElement.FindElement(By.ClassName("progress")).Text.ToString();
                    SetCompletedSearchCredits(credits, true);
                    return true;
                }
            }
            return false;
        }

        public void SetCreditsForToday(bool mobile)
        {
            var eList = driver.FindElements(By.TagName("li"));
            if (mobile)
            {
                foreach (var element in eList)
                {
                    if (element.FindElement(By.ClassName("title")).Text.Contains(mobileSearch))
                    {
                        String maxCreditString = element.FindElement(By.ClassName("progress")).Text.ToString();
                        String[] creditsString = Regex.Split(maxCreditString, "of");
                        AccountCredits.MobileSearchCredits = int.Parse(creditsString[0]);
                        AccountCredits.MobileSearchMaxCredits = ParseCreditsString(creditsString[1]);
                    }
                }
            }
            else
            {
                foreach (var element in eList)
                {
                    bool hasPCSearchTitle = false;
                    By by = By.ClassName("title");
                    if (BotUtils.HasElement(element, by))
                    {
                        if ((element.FindElement(by).Text.Contains(pcSearch)))
                        {
                            hasPCSearchTitle = true;
                        }
                    }
                    if (hasPCSearchTitle)
                    {
                        String maxCreditString = element.FindElement(By.ClassName("progress")).Text.ToString();
                        String[] creditsString = Regex.Split(maxCreditString, "of");
                        AccountCredits.PCSearchCredits = int.Parse(creditsString[0]);
                        AccountCredits.PCSearchMaxCredits = ParseCreditsString(creditsString[1]);
                        return;
                    }
                }
            }
            throw new Exception("Failed to get current Credits from dashboard.");
        }

        public void SetCompletedSearchCredits(String credits, bool mobile)
        {
            int parsedCredits = ParseCreditsString(credits);
            if (mobile)
            {
                accountCredits.MobileSearchCredits = parsedCredits;
                accountCredits.MobileSearchMaxCredits = parsedCredits;
            }
            else
            {
                accountCredits.PCSearchCredits = parsedCredits;
                accountCredits.PCSearchMaxCredits = parsedCredits;
            }
        }
        private int ParseCreditsString(String csstr)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < csstr.Count(); i++)
            {
                if (char.IsDigit(csstr[i]))
                {
                    sb.Append(csstr[i]);
                }
            }
            int temp = -1;
            int.TryParse(sb.ToString(), out temp);
            return temp;
        }
    }
}
