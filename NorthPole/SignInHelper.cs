using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace NorthPole
{
    class SignInHelper
    {

        public bool SignIn(IWebDriver driver, bool mobile, string username, string password, Random random)
        {
            Console.WriteLine("BingBot: Logging in...");
            BingBotUtils.Wait(random);
            if (mobile)
            {
                GoToLoginPageMobile(driver);
            }
            else
            {
                GoToLoginPageDesktop(driver);
            }
            BingBotUtils.Wait(random);
            Login(driver, username, password);
            BingBotUtils.Wait(random);
            if (driver.Url.Equals("https://www.bing.com/rewards/dashboard"))
            {
                Console.WriteLine("BingBot: login successfull.");
                return true;
            }
            else if (driver.Title.Equals("Sign in to your Microsoft account"))
            {
                Console.WriteLine("BingBot: login failed.");
                return false;
            }
            else
            {
                Console.WriteLine("BingBot: Something unknown happened during signin.");
                return false;
            }
        }

        public void GoToLoginPageDesktop(IWebDriver driver)
        {
            var elements = driver.FindElements(By.ClassName("identityOption"));
            foreach (var item in elements)
            {
                if (item.Text.Contains("Microsoft account"))
                {
                    item.FindElement(By.TagName("a")).Click();
                    break;
                }
            }
        }

        public void GoToLoginPageMobile(IWebDriver driver)
        {
            driver.FindElement(By.Id("WLSignin")).FindElement(By.ClassName("idText")).Click();
        }

        public void Login(IWebDriver driver, string username, string password)
        {
            //WebDriverWait _wait = new WebDriverWait(driver, new TimeSpan(0, 1, 0));
            //_wait.Until(d => d.FindElement(By.Name("login")));
            //
            //Hack for waiting till login elements are available.
            var tempList = driver.FindElements(By.Name("login"));
            while (tempList.Count() == 0)
            {
                tempList = driver.FindElements(By.Name("login"));
            }
            //EndHack
            driver.FindElement(By.Name("login")).SendKeys(username);
            driver.FindElement(By.Name("passwd")).SendKeys(password);
            driver.FindElement(By.Id("idSIButton9")).Click();
        }
    }
}
