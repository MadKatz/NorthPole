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
            driver = null;
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
                FirefoxProfile profile = new FirefoxProfile();
                profile.SetPreference("general.useragent.override", "User-Agent: Mozilla/5.0 (Linux; U; Android 2.2; en-us; LG-P500 Build/FRF91) AppleWebKit/533.0 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1");
                driver = new FirefoxDriver(profile);
                driver.Navigate().GoToUrl(homepage);
            }
            catch (Exception e)
            {
                string msg = "Failed to load firefox";
                Console.WriteLine(msg);
                Console.WriteLine(e.Message);
                return;
            }
            Console.WriteLine("TestBot is going to " + homepage);

            Console.WriteLine("TestBot is signing in as...");
            Console.WriteLine("username: " + username);
            Console.WriteLine("password: " + password);

            SignIn(driver);
            SetCurrentPoints(driver);
            DoSearch(driver, searchList);
            DoSearch(driver, searchList);
            DoSearch(driver, searchList);
            DoSearch(driver, searchList);
            DoSearch(driver, searchList);
        }
    }
}
