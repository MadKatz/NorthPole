using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthPole.Utils
{
    public static class BotUtils
    {
        public static void Wait(Random random)
        {
            int rndnum = random.Next(3, 6);
            Wait(rndnum * 1000);
        }

        public static void Wait(int value_in_ms)
        {
            try
            {
                System.Threading.Thread.Sleep(value_in_ms);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public static bool HasElement(IWebElement element, By by)
        {
            bool result = false;
            try
            {
                element.FindElement(by);
                result = true;
            }
            catch (NoSuchElementException ex)
            {
                result = false;
            }
            return result;
        }
    }
}
