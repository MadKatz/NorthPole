using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;


namespace NorthPole
{
    class OfferHelper
    {

        public void DoOffers(IWebDriver driver, Random random, ref int max_offers, ref int offer_count)
        {
            string mainWinHandle = driver.CurrentWindowHandle.ToString();
            List<IWebElement> offerList = GetAvailableOffers(driver);
            // While there are active offers
            // Do a offer
            // refresh page
            // refresh offer list
            max_offers = offerList.Count;
            if (offerList.Count == 0)
            {
                Console.WriteLine("BingBot: Failed to find any offers.");
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
                        BingBotUtils.Wait(random);
                        driver.Close();
                    }
                }
                driver.SwitchTo().Window(mainWinHandle);
                driver.Navigate().Refresh();
                offerList = GetAvailableOffers(driver);
            }
        }

        private List<IWebElement> GetAvailableOffers(IWebDriver driver)
        {
            List<IWebElement> result = new List<IWebElement>();
            var temp = driver.FindElements(By.XPath("//div[contains(@class, 'check open-check dashboard-sprite')]"));
            foreach (var e in temp)
            {
                var te = e.FindElement(By.XPath("../../../../.."));
                if (te.Text.Contains("Earn and explore"))
                {
                    var offerElement = e.FindElement(By.XPath("../.."));
                    string s = offerElement.Text;
                    string blacklist1 = "trivia";
                    string blacklist2 = "Trivia";
                    if (!s.Contains(blacklist2))
                    {
                        result.Add(offerElement);
                    }
                }
            }
            return result;
        }

        private void RemoveOffers(List<IWebElement> offerList)
        {
            foreach (var offer in offerList)
            {
                if (offer.Text.Contains("trivia"))
                {
                    offerList.Remove(offer);
                }
            }
        }
    }
}
