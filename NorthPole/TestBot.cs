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
            Console.WriteLine("Mobile searching set to " + mobile.ToString());
            if (mobile)
            {
                agentString = mobileAgent;
            }
            else
            {
                agentString = desktopAgent;
            }
            driver = null;
            Console.WriteLine("TestBot is starting.");

            Console.WriteLine("Loading wordlist...");
            if (!LoadFile(@"wordlist.txt", out searchList))
            {
                return;
            }
            Console.WriteLine("wordlist loaded. Word count: " + searchList.Count());

            Console.WriteLine("TestBot is launching Firefox...");
            if (!LoadFireFox(agentString, out driver))
            {
                return;
            }
            if (!GoToURL(driver, homepage + signinURL))
            {
                return;
            }
            Console.WriteLine("TestBot is going to " + homepage + signinURL);

            Console.WriteLine("TestBot is signing in as...");
            Console.WriteLine("username: " + username);
            Console.WriteLine("password: " + password);

            SignIn(driver);
            //SetCurrentPoints(driver);
            driver.Navigate().GoToUrl(homepage);
            DoSearch(driver, searchList);
            doRelatedSearch(driver);
            //DoSearch(driver, searchList);
            //DoSearch(driver, searchList);
            //DoSearch(driver, searchList);
            //DoSearch(driver, searchList);
        }
    }
}
