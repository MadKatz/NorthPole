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
        protected void LogError(string str, Exception e)
        {
            Console.WriteLine(str);
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
        }
        protected void LogError(string str)
        {
            Console.WriteLine(str);
        }
        protected bool LoadFireFox(out IWebDriver driver)
        {
            try
            {
                driver = new FirefoxDriver();
                return true;
            }
            catch (Exception e)
            {
                driver = null;
                string msg = "Failed to load firefox";
                LogError(msg, e);
                return false;
            }
        }
        protected bool LoadFireFox(string agent, out IWebDriver driver)
        {
            FirefoxProfile profile = new FirefoxProfile();
            profile.SetPreference("general.useragent.override", agent);
            try
            {
                driver = new FirefoxDriver(profile);
                return true;
            }
            catch (Exception e)
            {
                driver = null;
                string msg = "Failed to load firefox";
                LogError(msg, e);
                return false;
            }
        }
        protected bool MaximizeWindow(IWebDriver driver)
        {
            try
            {
                driver.Manage().Window.Maximize();
                return true;
            }
            catch (Exception e)
            {
                string msg = "Failed to fetch the webdriver. Did firefox close?";
                LogError(msg, e);
                return false;
            }
        }
        protected bool GoToURL(IWebDriver driver, string url)
        {
            try
            {
                driver.Navigate().GoToUrl(url);
                return true;
            }
            catch (Exception e)
            {
                driver = null;
                string msg = "Failed to fetch the webdriver. Did firefox close?";
                LogError(msg, e);
                return false;
            }
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
        protected bool SetElementByID(IWebDriver driver, string id, out IWebElement element)
        {
            try
            {
                element = driver.FindElement(By.Id(id));
                return true;
            }
            catch (Exception e)
            {
                element = null;
                string msg = "Unable to find element by id " + id;
                LogError(msg, e);
                return false;
            }
        }
        protected bool SetElementByName(IWebDriver driver, string name, out IWebElement element)
        {
            try
            {
                element = driver.FindElement(By.Name(name));
                return true;
            }
            catch (Exception e)
            {
                element = null;
                string msg = "Unable to find element by Name " + name;
                LogError(msg, e);
                return false;
            }
        }
        protected bool SetElementByTagName(IWebDriver driver, string tname, out IWebElement element)
        {
            try
            {
                element = driver.FindElement(By.TagName(tname));
                return true;
            }
            catch (Exception e)
            {
                element = null;
                string msg = "Unable to find element by Name " + tname;
                LogError(msg, e);
                return false;
            }
        }
        protected bool SetElementByClassName(IWebDriver driver, string cname, out IWebElement element)
        {
            try
            {
                element = driver.FindElement(By.ClassName(cname));
                return true;
            }
            catch (Exception e)
            {
                element = null;
                string msg = "Unable to find element by ClassName " + cname;
                LogError(msg, e);
                return false;
            }
        }
        protected bool SetElementsByClassName(IWebDriver driver, string cname, out System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> elements)
        {
            try
            {
                elements = driver.FindElements(By.ClassName(cname));
                return true;
            }
            catch (Exception e)
            {
                elements = null;
                string msg = "Unable to find elements by lassName " + cname;
                LogError(msg, e);
                return false;
            }
        }
    }
}
