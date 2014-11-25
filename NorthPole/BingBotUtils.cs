using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace NorthPole
{
    public static class BingBotUtils
    {
        public static void Wait(Random random)
        {
            int rndnum = random.Next(3, 6);
            Wait(rndnum * 1000);
        }

        public static void Wait(int value_in_ms)
        {
            System.Threading.Thread.Sleep(value_in_ms);
        }

        public static bool SetElement(IWebDriver driver, By by, out IWebElement element)
        {
            try
            {
                element = driver.FindElement(by);
                return true;
            }
            catch (Exception e)
            {
                element = null;
                Console.WriteLine("Unable to find element by: " + by.ToString());
                Console.WriteLine(e.Message);
                return false;
            }
        }
    }
}
