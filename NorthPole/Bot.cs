using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium;

namespace NorthPole
{
    class Bot
    {
        protected void Wait()
        {
            System.Threading.Thread.Sleep(3000);
        }

        protected void Wait(Random random)
        {
            int rndnum = random.Next(3, 6);
            Wait(rndnum * 1000);
        }

        protected void Wait(int value_in_ms)
        {
            System.Threading.Thread.Sleep(value_in_ms);
        }

        protected void LogError(string str, Exception e)
        {
            LogError(str);
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
        }

        protected void LogError(string str)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("LOG: ");
            sb.Append(str);
            Console.WriteLine(sb);
        }

        protected bool LoadFile(string path, out List<string> file)
        {
            try
            {
                string[] temp = File.ReadAllLines(path);
                file = new List<string>(temp);
                return true;
            }
            catch (Exception e)
            {
                file = null;
                string msg = "Failed to load the word list";
                LogError(msg, e);
                return false;
            }
        }

        protected bool ClickElement(IWebElement element)
        {
            try
            {
                element.Click();
                return true;
            }
            catch (Exception e)
            {
                string msg = "Failed to click the element";
                LogError(msg, e);
                return false;
            }
        }

        protected bool ClearElement(IWebElement element)
        {
            try
            {
                element.Clear();
                return true;
            }
            catch (Exception e)
            {
                string msg = "Failed to clear the element";
                LogError(msg, e);
                return false;
            }
        }

        protected bool SetElement(IWebDriver driver, By by, out IWebElement element)
        {
            try
            {
                element = driver.FindElement(by);
                return true;
            }
            catch (Exception e)
            {
                element = null;
                string msg = "Unable to find element by: " + by.ToString();
                LogError(msg, e);
                return false;
            }
        }
    }
}



       