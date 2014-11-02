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
        public override void StartBot()
        {
            Console.WriteLine("BingBot is starting.");
            IWebDriver driver = new FirefoxDriver();

            Console.WriteLine("BingBot is going to " + "http://www.bing.com/");
            driver.Navigate().GoToUrl("http://www.bing.com/");

            SignIn(driver);
            SetDailyMaxPoints(driver);
            //TODO: remove readline()
            Console.ReadLine();
        }
    }
}
