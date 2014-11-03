using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;

namespace NorthPole
{
    class TestBot : BingBot
    {
        private IWebDriver driver;
        public override void StartBot()
        {

            Console.WriteLine("TestBot is starting.");

            Console.WriteLine("Loading wordlist...");
            if (!CreateSearchList())
            {
                return;
            }
            Console.WriteLine("wordlist loaded. Word count: " + searchList.Count());

            Console.WriteLine("TestBot is launching Firefox...");
            try
            {
                driver = new FirefoxDriver();
                driver.Navigate().GoToUrl(startpage);
            }
            catch (Exception e)
            {
                string msg = "Failed to load firefox";
                Console.WriteLine(msg);
                Console.WriteLine(e.Message);
                return;
            }
            Console.WriteLine("TestBot is going to " + startpage);

            Console.WriteLine("TestBot is signing in as...");
            Console.WriteLine("username: " + username);
            Console.WriteLine("password: " + password);

            SignIn(driver);
            SetCurrentPoints(driver);
        }
    }
}
