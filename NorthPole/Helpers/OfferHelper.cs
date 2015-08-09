using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NorthPole.Utils;
using NorthPole.Account;

namespace NorthPole.Helpers
{
    public class OfferHelper
    {
        protected IWebDriver driver;
        protected Random random;
        protected String checkmarkXPATH = "//div[contains(@class, 'check open-check dashboard-sprite')]";
        protected List<String> blackList;

        public OfferHelper(IWebDriver driver, Random random)
        {
            this.driver = driver;
            this.random = random;
            blackList = new List<string>() { "trivia", "Trivia" };
        }


        public void DoOffers(AccountCredits accontCredits)
        {
            string mainWinHandle = driver.CurrentWindowHandle.ToString();
            List<IWebElement> offerList = GetAvailableOffers();
            int offer_count = 0;
            // While there are active offers
            // Do a offer
            // refresh page
            // refresh offer list
            accontCredits.OfferMaxCredits = offerList.Count;
            if (offerList.Count == 0)
            {
                Debug.WriteLine("Failed to find any offers.");
            }
            while (offerList.Count() > 0)
            {
                offer_count++;
                int randomInt = random.Next(0, offerList.Count());
                offerList[randomInt].Click();
                var WinHandles = driver.WindowHandles;
                foreach (var win in WinHandles)
                {
                    if (win.ToString() != mainWinHandle)
                    {
                        driver.SwitchTo().Window(win);
                        BotUtils.Wait(random);
                        driver.Close();
                    }
                }
                driver.SwitchTo().Window(mainWinHandle);
                driver.Navigate().Refresh();
                offerList = GetAvailableOffers();
            }
            accontCredits.OfferCredits = offer_count;
        }

        private List<IWebElement> GetAvailableOffers()
        {
            List<IWebElement> result = new List<IWebElement>();
            var temp = driver.FindElements(By.XPath(checkmarkXPATH));
            foreach (var e in temp)
            {
                var te = e.FindElement(By.XPath("../../../../.."));
                if (te.Text.Contains("Earn and explore"))
                {
                    var offerElement = e.FindElement(By.XPath("../.."));
                    string offerElementText = offerElement.Text;
                    bool offerNotTrivia = true;
                    foreach (String blackword in blackList)
                    {
                        if (offerElementText.Contains(blackword))
                        {
                            offerNotTrivia = false;
                        }
                    }
                    if (offerNotTrivia)
                    {
                        result.Add(offerElement);
                    }
                }
            }
            return result;
        }
    }
}
