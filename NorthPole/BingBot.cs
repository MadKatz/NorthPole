using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;

namespace NorthPole
{
    class BingBot
    {
        protected string username = "";
        protected string password = "";
        protected string startpage = "http://www.bing.com/";

        protected IWebDriver driver;
        protected List<string> searchList;

        private Random random;
        private bool STOPBOT;

        private int TOTAL_SESSION_POINTS_EARNED_STATS;
        private int TOTAL_SESSION_SEARCHES_STATS;
        private int TOTAL_RP_LTE;

        private int STARTING_RP_COUNT_ACTUAL; // starting reward points counter (how many points did we start at)
        private int STARTING_PC_RP_COUNT;
        private int CURRENT_RP_COUNT_ACTUAL; // current reward points counter
        private int DAILY_MAX_PC_RP; // max daily PC search reward points
        private int checkRPcount_counter; // counter for checking our rewards points so far
        private const int SEARCHTOPOINTRATIO = 3; // bing says 3 searchs = 1 point


        public BingBot()
        {
            random = new Random();
            searchList = null;
            TOTAL_SESSION_POINTS_EARNED_STATS = 0;
            TOTAL_SESSION_SEARCHES_STATS = 0;
        }
        public virtual void StartBot()
        {
            TOTAL_RP_LTE = -1;
            STOPBOT = false;
            STARTING_RP_COUNT_ACTUAL = -1;
            CURRENT_RP_COUNT_ACTUAL = -1;
            STARTING_PC_RP_COUNT = -1;
            DAILY_MAX_PC_RP = -1;
            checkRPcount_counter = 0;

            Console.WriteLine("BingBot is starting.");

            Console.WriteLine("Loading wordlist...");
            if (!CreateSearchList())
            {
                return;
            }
            Console.WriteLine("wordlist loaded. Word count: " + searchList.Count());
            
            Console.WriteLine("BingBot is launching Firefox...");
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
            Console.WriteLine("BingBot is going to " + startpage);

            Console.WriteLine("BingBot is signing in as...");
            Console.WriteLine("username: " + username);
            Console.WriteLine("password: " + password);
            SignIn(driver);

            Console.WriteLine("Setting current & daily point count.");
            if (!SetCurrentPoints(driver))
            {
                string msg = "Could not set current points, Aborting.";
                Console.WriteLine(msg);
                Debug.WriteLine(msg);
                return;
            }
            STARTING_RP_COUNT_ACTUAL = CURRENT_RP_COUNT_ACTUAL;
            if (!SetDailyMaxPoints(driver))
            {
                string msg = "Could not set daily max points, Aborting.";
                Console.WriteLine(msg);
                Debug.WriteLine(msg);
                return;
            }
            Console.WriteLine("Todays daily PC search points: " + STARTING_PC_RP_COUNT + " out of " + DAILY_MAX_PC_RP);
            Console.WriteLine("BingBot is returning to " + startpage);
            driver.Navigate().GoToUrl(startpage);
            Wait();

            //start botting!
            Console.WriteLine("Starting search loop.");
            bool relatedsearchresult = true;

            while (!STOPBOT)
            {
                DoSearch(driver, searchList);
                checkRPcount_counter++;
                if (relatedsearchresult = doRelatedSearch(driver))
                {
                    checkRPcount_counter++;
                }                         
                
                while (relatedsearchresult && !STOPBOT)
                {
                    // do a check
                    if (checkRPcount_counter > TOTAL_RP_LTE * 2)
                    {
                        CalRP_LTE(driver);
                        checkRPcount_counter = 0;
                    }

                    if (relatedsearchresult = doRelatedSearch(driver))
                    {
                        checkRPcount_counter++;
                    }  
                }               
            }


            //TODO: add check
            Console.WriteLine("searching complete.");
            CalRP_LTE(driver);
            Console.WriteLine("Todays daily PC search points: " + (STARTING_PC_RP_COUNT + (CURRENT_RP_COUNT_ACTUAL - STARTING_RP_COUNT_ACTUAL)) + " out of " + DAILY_MAX_PC_RP);
            Console.WriteLine("Total points earned: " + TOTAL_SESSION_POINTS_EARNED_STATS);
            Console.WriteLine("Total searched performed: " + TOTAL_SESSION_SEARCHES_STATS);
            return;
        }

        public void SignIn(IWebDriver driver)
        {
            GetToSignInPage(driver);
            Wait();
            Login(driver);
            Wait();

            if (driver.Url.Equals(startpage))
            {
                string msg = "login successfull.";
                Console.WriteLine(msg);
                Debug.WriteLine(msg);
            }
            else if (driver.Title.Equals("Sign in to your Microsoft account"))
            {
                //Need to test this path
                string msg = "login failed. Aborting.";
                Console.WriteLine(msg);
                Debug.WriteLine(msg);
                Abort(500, driver);
            }
            else
            {
                string msg = "Something unknown happened, Aborting.";
                Console.WriteLine(msg);
                Debug.WriteLine(msg);
                Abort(404, driver);
            }
        }

        public bool SetCurrentPoints(IWebDriver driver)
        {
            int cpoints;
            IWebElement rewardCount = null;
            if (!SetElementByID(driver, "id_rc", out rewardCount))
            {
                string emsg1 = "Unable to set current points.";
                Console.WriteLine(emsg1);
                Debug.WriteLine(emsg1);
                Console.WriteLine("Retrying to set current points...");
                if (!SetElementByID(driver, "id_rc", out rewardCount))
                {
                    string emsg2 = "Unable to set current points.";
                    Console.WriteLine(emsg2);
                    Debug.WriteLine(emsg2);
                    return false;
                }
            }
            if (int.TryParse(rewardCount.Text.ToString(), out cpoints))
            {
                CURRENT_RP_COUNT_ACTUAL = cpoints;
                string msg = "Current Reward Points: ";
                Console.WriteLine(msg + CURRENT_RP_COUNT_ACTUAL);
                Debug.WriteLine(msg + CURRENT_RP_COUNT_ACTUAL);
                return true;
            }
            else
            {
                string emsg = "Failed to parse reward points string.";
                Console.WriteLine(emsg);
                Debug.WriteLine(emsg);
                return false;
            }
        }

        public bool SetDailyMaxPoints(IWebDriver driver)
        {
            string maxSearchString = "";
            driver.Navigate().GoToUrl("http://www.bing.com/rewards/dashboard");
            Wait();
            //TODO: add check for if max is already reached
            // check for check mark icon
            var dashElements = driver.FindElements(By.ClassName("row"));
            foreach (var item in dashElements)
            {
                if (item.Text.Contains("PC search"))
                {
                    var rowElements = item.FindElements(By.TagName("li"));
                    foreach (var row in rowElements)
                    {
                        if (row.Text.Contains("PC search"))
                        {
                            maxSearchString = row.FindElement(By.ClassName("progress")).Text.ToString();
                            break;
                        }
                    }
                }
            }
            return ParseMaxSearchString(maxSearchString);
        }

        public void DoSearch(IWebDriver driver, List<string> searchList)
        {
            IWebElement searchBar = driver.FindElement(By.Id("sb_form_q"));
            // update randomneess
            string randomSearchString = searchList[random.Next(0, searchList.Count())];
            Debug.WriteLine("Performing search on: : " + randomSearchString);
            searchBar.Clear();
            searchBar.SendKeys(randomSearchString);
            searchBar.SendKeys(Keys.Enter);
            TOTAL_SESSION_SEARCHES_STATS++;
            Wait();
        }

        public bool doRelatedSearch(IWebDriver driver)
        {
            bool result = false;
            IWebElement context = driver.FindElement(By.Id("b_context"));
            var contextList = context.FindElements(By.ClassName("b_ans"));
            foreach (var item in contextList)
            {
                if (item.Text.Contains("Related searches"))
                {
                    var resultList = item.FindElements(By.TagName("a"));
                    Debug.WriteLine("found a related search");
                    resultList[random.Next(0, resultList.Count())].Click();
                    TOTAL_SESSION_SEARCHES_STATS++;
                    result = true;
                    break;                    
                }
            }
            if (!result)
            {
                Debug.WriteLine("failed to find any related searches");
            }
            Wait();
            return result;
        }

        private bool ParseMaxSearchString(string maxss)
        {
            bool onFirstNum = true;
            string startStr = "";
            int startNum = -1;
            bool onMid = false;
            bool onEnd = false;
            string endStr = "";
            int endNum = -1;
            for (int i = 0; i < maxss.Count(); i++)
            {
                if (char.IsDigit(maxss[i]) && onFirstNum)
                {
                    startStr = startStr + maxss[i];
                }
                else if (!onMid)
                {
                    onFirstNum = false;
                    onMid = true;
                    i++;
                }

                if (onMid && char.IsDigit(maxss[i]))
                {
                    endStr = endStr + maxss[i];
                    onMid = false;
                    onEnd = true;
                    i++;
                }


                if (onEnd)
                {
                    if (char.IsDigit(maxss[i]))
                    {
                        endStr = endStr + maxss[i];
                    }
                    else
                    {
                        break;
                    }
                }
            }
            if (int.TryParse(startStr, out startNum) && int.TryParse(endStr, out endNum))
            {
                //TODO: break this into another function
                STARTING_PC_RP_COUNT = startNum;
                DAILY_MAX_PC_RP = endNum;
                TOTAL_RP_LTE = DAILY_MAX_PC_RP - STARTING_PC_RP_COUNT;
                return true;
            }
            else
            {
                return false;
            }
        }

        protected bool CreateSearchList()
        {
            try
            {
                searchList = new List<string>(LoadFile(@"wordlist.txt"));
                return true;
            }
            catch (Exception e)
            {
                string emsg = "failed to load worlist, Aborting.";
                Console.WriteLine(emsg);
                Console.WriteLine(e.Message);
                Debug.WriteLine(emsg);
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
                return false;
            }
        }

        protected void CalRP_LTE(IWebDriver driver)
        {

            if (SetCurrentPoints(driver))
            {
                CalcRPEarned();
                int total_dailypoints_so_far = STARTING_PC_RP_COUNT + (CURRENT_RP_COUNT_ACTUAL - STARTING_RP_COUNT_ACTUAL);
                TOTAL_RP_LTE = DAILY_MAX_PC_RP - total_dailypoints_so_far;
                Debug.WriteLine("points so far: " + total_dailypoints_so_far + " out of " + DAILY_MAX_PC_RP + ". " + TOTAL_RP_LTE +" rewards points left.");
                if (total_dailypoints_so_far > DAILY_MAX_PC_RP || TOTAL_RP_LTE <= 0)
                {
                    STOPBOT = true;
                }
            }
        }

        protected void CalcRPEarned()
        {
            TOTAL_SESSION_POINTS_EARNED_STATS = CURRENT_RP_COUNT_ACTUAL - STARTING_RP_COUNT_ACTUAL;
            Debug.WriteLine("Total Rewards Points Earned: " + TOTAL_SESSION_POINTS_EARNED_STATS);
        }

        protected string[] LoadFile(string path)
        {
            var file = File.ReadAllLines(path);
            return file;
        }

        protected void Abort(int exitcode, IWebDriver driver)
        {
            Console.WriteLine("Abort entered! exit " + exitcode + " *TESTCODE*");
            Console.ReadLine();
            //driver.Quit();
            //Environment.Exit(exitcode);
        }

        protected void Abort(int exitcode)
        {
            Console.WriteLine("Abort entered! exit " + exitcode + " *TESTCODE*");
            Console.ReadLine();
            //Environment.Exit(exitcode);
        }

        protected void Wait()
        {
            string msg = "Sleeping for 3 seconds...";
            Console.WriteLine(msg);
            Debug.WriteLine(msg);
            System.Threading.Thread.Sleep(3000);
        }

        protected bool GetToSignInPage(IWebDriver driver)
        {
            try
            {
                driver.FindElement(By.Id("id_l")).Click();
            }
            catch (OpenQA.Selenium.NoSuchElementException e)
            {
                Console.WriteLine(e.Message);
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
                return false;
            }
            catch (Exception e)
            {
                string emsg = "Something horrible happened in getting to the login page";
                Console.WriteLine(emsg);
                Debug.WriteLine(emsg);
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
                return false;
            }
            Wait();
            IWebElement loginLinks = null; // this bp?
            try
            {
                loginLinks = driver.FindElement(By.ClassName("b_idProvidersBottom"));
            }
            catch (OpenQA.Selenium.NoSuchElementException e)
            {
                Console.WriteLine(e.Message);
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
                return false;
            }
            catch (Exception e)
            {
                string emsg = "Something horrible happened in getting to the login page";
                Console.WriteLine(emsg);
                Debug.WriteLine(emsg);
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
                return false;
            }
            var loginLinkList = loginLinks.FindElements(By.ClassName("id_name"));

            foreach (IWebElement item in loginLinkList)
            {
                if (item.Text.ToString() == "Microsoft account")
                {
                    item.Click();
                    return true;
                }
            }
            string msg = "Could not find 'Microsoft account' link, Aborting.";
            Console.WriteLine(msg);
            Debug.WriteLine(msg);
            return false;
        }

        protected bool Login(IWebDriver driver)
        {
            try
            {
                IWebElement loginBox = driver.FindElement(By.Name("login"));
                loginBox.SendKeys(username);
            }
            catch (OpenQA.Selenium.NoSuchElementException e)
            {
                Console.WriteLine(e.Message);
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
                return false;
            }
            catch (Exception e)
            {
                string emsg = "Unable send keys for login";
                Console.WriteLine(emsg);
                Debug.WriteLine(emsg);
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
                return false;
            }

            try
            {
                IWebElement passwordBox = driver.FindElement(By.Name("passwd"));
                passwordBox.SendKeys(password);
                driver.FindElement(By.Name("SI")).Click();
            }
            catch (OpenQA.Selenium.NoSuchElementException e)
            {
                Console.WriteLine(e.Message);
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
                return false;
            }
            catch (Exception e)
            {
                string emsg = "Unable send keys for password";
                Console.WriteLine(emsg);
                Debug.WriteLine(emsg);
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
                return false;
            }
            return true;
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
                string emsg = "Unable to find element id " + id;
                Console.WriteLine(emsg);
                Debug.WriteLine(emsg);
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
                return false;
            }
        }
    }
}
