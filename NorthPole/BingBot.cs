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
        private string username = "";
        private string password = "";
        private string startpage = "http://www.bing.com/";

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
        }
        public virtual void StartBot()
        {
            TOTAL_SESSION_POINTS_EARNED_STATS = 0;
            TOTAL_SESSION_SEARCHES_STATS = 0;
            TOTAL_RP_LTE = -1;
            STOPBOT = false;
            STARTING_RP_COUNT_ACTUAL = -1;
            CURRENT_RP_COUNT_ACTUAL = -1;
            STARTING_PC_RP_COUNT = -1;
            DAILY_MAX_PC_RP = -1;
            checkRPcount_counter = 0;
            Console.WriteLine("BingBot is starting.");
            Console.WriteLine("Loading wordlist...");
            List<string> searchList = CreateSearchList();
            Console.WriteLine("wordlist loaded. Word count: " + searchList.Count());
            Console.WriteLine("BingBot is launching Firefox...");
            IWebDriver driver = new FirefoxDriver();

            Console.WriteLine("BingBot is going to " + startpage);
            driver.Navigate().GoToUrl(startpage);

            Console.WriteLine("BingBot is signing in...");
            SignIn(driver);

            Console.WriteLine("Setting current & daily point count.");
            if (!SetCurrentPoints(driver))
            {
                string msg = "Could not set current points, Aborting.";
                Console.WriteLine(msg);
                Debug.WriteLine(msg);
                Abort(404);
            }
            STARTING_RP_COUNT_ACTUAL = CURRENT_RP_COUNT_ACTUAL;
            if (!SetDailyMaxPoints(driver))
            {
                string msg = "Could not set daily max points, Aborting.";
                Console.WriteLine(msg);
                Debug.WriteLine(msg);
                Abort(404);
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
            Console.WriteLine("Todays daily PC search points: " + STARTING_PC_RP_COUNT + " out of " + DAILY_MAX_PC_RP);
            CalRP_LTE(driver);
            Console.WriteLine("Total points earned: " + TOTAL_SESSION_POINTS_EARNED_STATS);
            Console.WriteLine("Total searched performed: " + TOTAL_SESSION_SEARCHES_STATS);
            //TODO: remove readline()
            Console.ReadLine();
        }

        public void SignIn(IWebDriver driver)
        {
            driver.FindElement(By.Id("id_l")).Click();
            Wait();
            IWebElement loginLinks = driver.FindElement(By.ClassName("b_idProvidersBottom"));
            var loginLinkList = loginLinks.FindElements(By.ClassName("id_name"));

            foreach (IWebElement item in loginLinkList)
            {
                if (item.Text.ToString() == "Microsoft account")
                {
                    item.Click();
                    break;
                }
                else
                {
                    string msg = "Could not find 'Microsoft account' link, Aborting.";
                    Console.WriteLine(msg);
                    Debug.WriteLine(msg);
                    Abort(404, driver);
                }
            }
            loginLinks = null;
            loginLinkList = null;
            Wait();

            IWebElement loginBox = driver.FindElement(By.Name("login"));
            loginBox.SendKeys(username);
            IWebElement passwordBox = driver.FindElement(By.Name("passwd"));
            passwordBox.SendKeys(password);
            driver.FindElement(By.Name("SI")).Click();
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
            //TODO: add try/catch logic
            int cpoints;
            IWebElement rewardCount = driver.FindElement(By.Id("id_rc"));
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
                //TODO: add retry and abort logic here
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

        protected List<string> CreateSearchList()
        {
            List<string> result = new List<string>();
            try
            {
                result = new List<string>(LoadFile(@"wordlist.txt"));
            }
            catch (Exception e)
            {
                Console.WriteLine("failed to load worlist, Aborting.");
                Console.WriteLine(e.Message);
                Abort(404);
            }
            return result;
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
            //driver.Quit();
            Environment.Exit(exitcode);
        }

        protected void Abort(int exitcode)
        {
            //driver.Quit();
            Environment.Exit(exitcode);
        }

        protected void Wait()
        {
            string msg = "Sleeping for 3 seconds...";
            Console.WriteLine(msg);
            Debug.WriteLine(msg);
            System.Threading.Thread.Sleep(3000);
        }
    }
}
