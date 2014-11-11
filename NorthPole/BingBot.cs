using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Firefox;

namespace NorthPole
{
    class BingBot : Bot
    {
        public string username = "";
        protected string password = "";
        protected string homepage = "http://www.bing.com/";
        //TODO: CHANGE BACK!!??!!
        //protected string signinURL = "rewards/signin";
        protected string signinURL = "https://login.live.com/login.srf?wa=wsignin1.0&rpsnv=12&ct=1415674836&rver=6.0.5286.0&wp=MBI&wreply=https:%2F%2Fwww.bing.com%2Fsecure%2FPassport.aspx%3Frequrl%3Dhttp%253a%252f%252fwww.bing.com%252f&lc=1033&id=264960";
        protected string dashboardURL = "rewards/dashboard";
        protected string desktopAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko";
        protected string mobileAgent = "Mozilla/5.0 (Linux; U; Android 2.2; en-us; LG-P500 Build/FRF91) AppleWebKit/533.0 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1";
        protected string agentString;

        protected List<string> searchList;

        private Random random;
        private bool STOPBOT;
        protected bool mobile;

        private int TOTAL_SESSION_POINTS_EARNED_STATS;
        private int TOTAL_SESSION_SEARCHES_STATS;
        private int TOTAL_RP_LTE;

        private int STARTING_RP_COUNT_ACTUAL; // starting reward points counter (how many points did we start at)
        private int STARTING_SEARCH_RP_COUNT;
        private int CURRENT_RP_COUNT_ACTUAL; // current reward points counter
        private int DAILY_MAX_SEARCH_RP; // max daily PC search reward points
        private int checkRPcount_counter; // counter for checking our rewards points so far
        private const int MOBILESEARCHTOPOINTRATIO = 3;
        private const int DESKTOPSEARCHTOPOINTRATIO = 2;


        public BingBot()
        {
            random = new Random();
            searchList = null;
            mobile = false;
            TOTAL_SESSION_POINTS_EARNED_STATS = 0;
            TOTAL_SESSION_SEARCHES_STATS = 0;
        }
        public virtual void StartBot(bool domobile)
        {
            IWebDriver driver = null;
            string tempstr;
            mobile = domobile;
            if (mobile)
            {
                agentString = mobileAgent;
                tempstr = "Mobile";
            }
            else
            {
                agentString = desktopAgent;
                tempstr = "PC";
            }
            TOTAL_RP_LTE = -1;
            STOPBOT = false;
            STARTING_RP_COUNT_ACTUAL = -1;
            CURRENT_RP_COUNT_ACTUAL = -1;
            STARTING_SEARCH_RP_COUNT = -1;
            DAILY_MAX_SEARCH_RP = -1;
            checkRPcount_counter = 0;

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
            //TODO: CHANGE BACK!!??!!
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

            driver.Navigate().GoToUrl(homepage + dashboardURL);
            Wait();
            Console.WriteLine("Setting current & daily point count.");
            if (!SetCurrentPoints(driver))
            {
                string msg = "Could not set current points, Aborting.";
                LogError(msg);
                driver.Quit();
                return;
            }

            STARTING_RP_COUNT_ACTUAL = CURRENT_RP_COUNT_ACTUAL;

            if (!SetDailyMaxPoints(driver))
            {
                string msg = "Could not set daily max points, Aborting.";
                LogError(msg);
                driver.Quit();
                return;
            }

            Console.WriteLine("Todays daily " + tempstr + " search points: " + STARTING_SEARCH_RP_COUNT + " out of " + DAILY_MAX_SEARCH_RP);
            Console.WriteLine("BingBot is returning to " + homepage);

            driver.Navigate().GoToUrl(homepage);
            Wait();

            //start botting!
            Console.WriteLine("Starting search loop.");
            doSearchLoop(driver);

            //TODO: add check
            Console.WriteLine("searching complete.");
            CalcRP_LTE(driver);
            //Console.WriteLine("Todays daily " + tempstr + " search points: " + (STARTING_SEARCH_RP_COUNT + (CURRENT_RP_COUNT_ACTUAL - STARTING_RP_COUNT_ACTUAL)) + " out of " + DAILY_MAX_SEARCH_RP);
            Console.WriteLine("Total points earned: " + TOTAL_SESSION_POINTS_EARNED_STATS);
            Console.WriteLine("Total searched performed: " + TOTAL_SESSION_SEARCHES_STATS);
            driver.Quit();
            return;
        }

        public bool SignIn(IWebDriver driver)
        {
            //temp hack below
            if (false)
            //end hack
            {
                GoToLoginPageMobile(driver);
            }
            else
            {
                GoToLoginPageDesktop(driver);
            }
            Wait();
            Login(driver);
            Wait();
            if (driver.Url.Equals(homepage))
            {
                string msg = "login successfull.";
                LogError(msg);
                return true;
            }
            else if (driver.Title.Equals("Sign in to your Microsoft account"))
            {
                string msg = "login failed. Aborting.";
                LogError(msg);
                return false;
            }
            else
            {
                string msg = "Something unknown happened, Aborting.";
                LogError(msg);
                return false;
            }
        }

        public bool SetCurrentPoints(IWebDriver driver)
        {
            //bug when doing mobile searching, id_rc may not be present
            int cpoints;
            IWebElement rewardCount = null;
            if (!SetElementByID(driver, "id_rc", out rewardCount))
            {
                string emsg1 = "Unable to set current points.";
                LogError(emsg1);
                Console.WriteLine("Retrying to set current points...");
                if (!SetElementByID(driver, "id_rc", out rewardCount))
                {
                    string emsg2 = "Unable to set current points.";
                    LogError(emsg2);
                    return false;
                }
            }
            Wait();
            //TODO:
            //So TryParse fails for some unknown reason.
            //PossibleSolution: adding Wait() to start of method
            //Other possible solution, do a forearch on the string for Char.IsDigit()
            string str = rewardCount.Text.ToString();
            if (int.TryParse(str, out cpoints))
            {
                CURRENT_RP_COUNT_ACTUAL = cpoints;
                string msg = "Current Reward Points: ";
                Console.WriteLine(msg + CURRENT_RP_COUNT_ACTUAL);
                return true;
            }
            else
            {
                string emsg = "Failed to parse reward points string.";
                LogError(emsg);
                return false;
            }
        }

        public bool SetDailyMaxPoints(IWebDriver driver)
        {
            string searchString = "";
            string maxSearchString = "";
            if (mobile)
            {
                searchString = "Mobile search";
            }
            else
            {
                searchString = "PC search";
            }
            var elements = driver.FindElements(By.ClassName("row"));
            foreach (var item in elements)
            {
                if (item.Text.Contains(searchString))
                {
                    var searchElements = item.FindElements(By.TagName("li"));
                    foreach (var searchelement in searchElements)
                    {
                        if (searchelement.Text.Contains(searchString))
                        {
                            /* code some how pulls the element from the other row?
                            var temp = searchelement.FindElements(By.XPath("//div[contains(@class, 'check close-check dashboard-sprite')]"));
                            if (temp.Count() > 0)
                            {
                                LogError("No daily points left to earn.");
                                return false;
                            }
                             * */
                            maxSearchString = searchelement.FindElement(By.ClassName("progress")).Text.ToString();
                            return ParseMaxSearchString(maxSearchString);
                        }
                    }
                }
            }
            return false;
        }

        public void DoSearch(IWebDriver driver, List<string> searchList)
        {
            IWebElement searchBar = driver.FindElement(By.Id("sb_form_q"));
            // update randomneess
            string randomSearchString = searchList[random.Next(0, searchList.Count())];
            Console.WriteLine("Performing search on: : " + randomSearchString);
            searchBar.Clear();
            searchBar.SendKeys(randomSearchString);
            searchBar.SendKeys(Keys.Enter);
            TOTAL_SESSION_SEARCHES_STATS++;
            Wait();
        }

        public bool doRelatedSearch(IWebDriver driver)
        {
            //put in logic for webelement timeout ( WebDriverExpection )
            if (mobile)
            {
                //for mobile
                //try getting classname=rl_srch
                //if empty try getting classname=b_rs
                //
                var RelatedSearchElements = driver.FindElements(By.ClassName("rl_srch"));
                if (RelatedSearchElements.Count() < 1)
                {
                    RelatedSearchElements = driver.FindElements(By.ClassName("b_rs"));
                }
                if (!FindRelatedSearch(RelatedSearchElements))
                {
                    return false;
                }
            }
            else
            {
                var RelatedSearchElements = driver.FindElements(By.ClassName("b_ans"));
                if (!FindRelatedSearch(RelatedSearchElements))
                {
                    return false;
                }
            }
            return true;
        }
        public void doSearchLoop(IWebDriver driver)
        {
            bool relatedsearchresult = true;
            while (!STOPBOT)
            {
                DoSearch(driver, searchList);
                checkRPcount_counter++;
                Debug.WriteLine(checkRPcount_counter);
                CheckRPCount(driver);
                while (relatedsearchresult && !STOPBOT)
                {
                    if (relatedsearchresult = doRelatedSearch(driver))
                    {
                        checkRPcount_counter++;
                        Debug.WriteLine(checkRPcount_counter);
                        CheckRPCount(driver);
                    }
                }
            }
        }

        private bool FindRelatedSearch(System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> elements)
        {
            bool result = false;
            foreach (var item in elements)
            {
                if (item.Text.Contains("Related searches") || item.Text.Contains("RELATED SEARCHES"))
                {
                    //bug need to check collection first
                    var resultList = item.FindElements(By.TagName("a"));
                    Console.WriteLine("found a related search");
                    //Bug with click element off screen when in mobile
                    int rndLink = random.Next(0, resultList.Count());
                    resultList[rndLink].SendKeys("");
                    if (!ClickElement(resultList[rndLink]))
                    {
                        return false;
                    }
                    TOTAL_SESSION_SEARCHES_STATS++;
                    result = true;
                    break;
                }
            }
            if (!result)
            {
                Console.WriteLine("failed to find any related searches");
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
                STARTING_SEARCH_RP_COUNT = startNum;
                DAILY_MAX_SEARCH_RP = endNum;
                TOTAL_RP_LTE = DAILY_MAX_SEARCH_RP - STARTING_SEARCH_RP_COUNT;
                return true;
            }
            else
            {
                return false;
            }
        }

        // edge case with if some how earning a offer while searching will cause us to be off by 1 or more
        private void CalcRP_LTE(IWebDriver driver)
        {
            TOTAL_SESSION_POINTS_EARNED_STATS = CURRENT_RP_COUNT_ACTUAL - STARTING_RP_COUNT_ACTUAL;
            Console.WriteLine("Total Rewards Points Earned: " + TOTAL_SESSION_POINTS_EARNED_STATS);

            int total_dailypoints_so_far = STARTING_SEARCH_RP_COUNT + (CURRENT_RP_COUNT_ACTUAL - STARTING_RP_COUNT_ACTUAL);
            TOTAL_RP_LTE = DAILY_MAX_SEARCH_RP - total_dailypoints_so_far;

            Console.WriteLine("points so far: " + total_dailypoints_so_far + " out of " + DAILY_MAX_SEARCH_RP + ". " + TOTAL_RP_LTE + " rewards points left.");
            if (total_dailypoints_so_far >= DAILY_MAX_SEARCH_RP || TOTAL_RP_LTE <= 0)
            {
                STOPBOT = true;
            }
        }

        private void CheckRPCount(IWebDriver driver)
        {
            if (checkRPcount_counter > TOTAL_RP_LTE * 3)
            {
                Console.WriteLine("Checking how many points left to earn...");
                if (SetCurrentPoints(driver))
                {
                    CalcRP_LTE(driver);
                    checkRPcount_counter = 0;
                }
                else
                {
                    driver.Navigate().GoToUrl(homepage + dashboardURL);
                    if (SetCurrentPoints(driver))
                    {
                        CalcRP_LTE(driver);
                        checkRPcount_counter = 0;
                        driver.Navigate().GoToUrl(homepage);
                        DoSearch(driver, searchList);
                    }
                    else
                    {
                        throw new Exception("failed to set current points from dashboard, aborting.");
                    }
                }
            }
        }

        protected void GoToLoginPageDesktop(IWebDriver driver)
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

        protected void GoToLoginPageMobile(IWebDriver driver)
        {
            //TODO:
            //Due to login page changing, for mobile use GoToDeskTopLogin()
            driver.FindElement(By.Id("WLSignin")).FindElement(By.ClassName("idText")).Click();
        }

        protected void Login(IWebDriver driver)
        {
            driver.FindElement(By.Name("login")).SendKeys(username);
            driver.FindElement(By.Name("passwd")).SendKeys(password); ;
            driver.FindElement(By.Id("idSIButton9")).Click();
        }
    }
}
