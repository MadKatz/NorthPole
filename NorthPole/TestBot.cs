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
        public override void StartBot(bool domobile)
        {
            IWebDriver driver = null;
            mobile = domobile;
            if (mobile)
            {
                agentString = mobileAgent;
                contextString = "Mobile";
            }
            else
            {
                agentString = desktopAgent;
                contextString = "PC";
            }

            // TODO:
            // Code should be move to BotManager
            Console.WriteLine("BingBot is starting... Mobile searching set to " + mobile.ToString());
            Console.WriteLine("Loading wordlist...");
            if (!LoadFile(@"wordlist.txt", out searchList))
            {
                Console.Write("Failed to load wordlist, aborting.");
                return;
            }
            Console.WriteLine("wordlist loaded. Word count: " + searchList.Count());
            // END TODO

            Console.WriteLine("BingBot is launching Firefox...");
            FirefoxProfile profile = new FirefoxProfile();
            profile.SetPreference("general.useragent.override", agentString);
            driver = new FirefoxDriver(profile);

            Console.WriteLine("BingBot is going to " + signinURL);
            driver.Navigate().GoToUrl(signinURL);
            //hack for mobile related searching
            if (true)
            {
                driver.Manage().Window.Maximize();
            }
            //endhack
            Console.WriteLine("BingBot is signing in as...");
            Console.WriteLine("username: " + username);
            Console.WriteLine("password: " + password);
            if (!SignIn(driver))
            {
                driver.Quit();
                return;
            }
            DoOffers(driver);
        }
    }
}
