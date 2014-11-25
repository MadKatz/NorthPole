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
            List<IWebElement> eList = new List<IWebElement>();
            // While there are active offers
            // Do a offer
            // refresh page
            // refresh offer list
            GetAvailableOffers(driver, eList);
            max_offers = eList.Count;
            if (eList.Count == 0)
            {
                Console.WriteLine("BingBot: Failed to find any offers.");
            }
            while (eList.Count() > 0)
            {
                offer_count++;
                eList[random.Next(0, eList.Count())].Click();
                BingBotUtils.Wait(random);
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
                BingBotUtils.Wait(random);

                eList = new List<IWebElement>();
                driver.Navigate().Refresh();
                GetAvailableOffers(driver, eList);
            }
        }

        private void GetAvailableOffers(IWebDriver driver, List<IWebElement> eList)
        {
            var temp = driver.FindElements(By.XPath("//div[contains(@class, 'check open-check dashboard-sprite')]"));
            foreach (var e in temp)
            {
                var te = e.FindElement(By.XPath("../../../../.."));
                if (te.Text.Contains("Earn and explore"))
                {
                    eList.Add(e);
                }
            }
        }
    }
}
