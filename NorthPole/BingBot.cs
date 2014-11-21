using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Firefox;

namespace NorthPole
{
    class BingBot : Bot
    {
        public string username;
        protected string password;

        //protected string oldsigninURL = "https://login.live.com/login.srf?wa=wsignin1.0&rpsnv=12&ct=1415674836&rver=6.0.5286.0&wp=MBI&wreply=https:%2F%2Fwww.bing.com%2Fsecure%2FPassport.aspx%3Frequrl%3Dhttp%253a%252f%252fwww.bing.com%252f&lc=1033&id=264960";

        protected string agentString;
        protected string contextString;

        protected List<string> searchList;

        protected Random random;
        protected bool STOPBOT;
        protected bool mobile;

        public int TOTAL_SESSION_POINTS_EARNED_STATS;
        private int TOTAL_SESSION_SEARCHES_STATS;
        private int TOTAL_RP_LTE;

        private int STARTING_RP_COUNT_ACTUAL; // starting reward points counter (how many points did we start at)
        private int STARTING_SEARCH_RP_COUNT; // PC/Mobile starting search counter
        private int CURRENT_RP_COUNT_ACTUAL; // current reward points counter
        private int DAILY_MAX_SEARCH_RP; // max daily PC search reward points
        private int checkRPcount_counter; // counter for checking our rewards points so far
        private int RP_COUNT_AFTER_OFFERS;
        private const int MOBILESEARCHTOPOINTRATIO = 3;
        private const int DESKTOPSEARCHTOPOINTRATIO = 2;


        public BingBot()
        {
            searchList = null;
            mobile = false;
            agentString = "";
            contextString = "";
            TOTAL_SESSION_POINTS_EARNED_STATS = 0;
            TOTAL_SESSION_SEARCHES_STATS = 0;
        }

        public virtual void StartBot(string username, string password, bool domobile, Random random, List<string> searchWordList)
        {
            this.username = username;
            this.password = password;
            this.random = random;
            searchList = searchWordList;

            IWebDriver driver = null;
            mobile = domobile;
            if (mobile)
            {
                agentString = Constants.MOBILEAGENT;
                contextString = "Mobile";
            }
            else
            {
                agentString = Constants.DESKTOPAGENT;
                contextString = "PC";
            }
            TOTAL_RP_LTE = -1;
            STOPBOT = false;
            STARTING_RP_COUNT_ACTUAL = -1;
            CURRENT_RP_COUNT_ACTUAL = -1;
            STARTING_SEARCH_RP_COUNT = -1;
            DAILY_MAX_SEARCH_RP = -1;
            checkRPcount_counter = 0;
            RP_COUNT_AFTER_OFFERS = -1;

            Console.WriteLine("############################################");
            Console.WriteLine("BingBot: Starting up...");

            Console.WriteLine("BingBot: Launching Firefox as " + contextString);
            FirefoxProfile profile = new FirefoxProfile();
            profile.SetPreference("general.useragent.override", agentString);
            driver = new FirefoxDriver(profile);

            Console.WriteLine("BingBot: Going to " + Constants.SIGNINURL);
            driver.Navigate().GoToUrl(Constants.SIGNINURL);

            driver.Manage().Window.Maximize();

            if (!SignIn(driver))
            {
                ShutDown(driver);
                return;
            }

            Console.WriteLine("BingBot: Setting current & daily point count.");
            if (!SetCurrentPoints(driver))
            {
                Console.WriteLine("BingBot: Could not set current points, Aborting.");
                ShutDown(driver);
                return;
            }

            STARTING_RP_COUNT_ACTUAL = CURRENT_RP_COUNT_ACTUAL;

            if (!mobile)
            {
                Console.WriteLine("BingBot: Doing Daily Offers...");
                DoOffers(driver);
                SetCurrentPoints(driver);
            }

            RP_COUNT_AFTER_OFFERS = CURRENT_RP_COUNT_ACTUAL;

            if (!SetDailyMaxPoints(driver))
            {
                Console.WriteLine("BingBot: Could not set daily max points, Aborting.");
                ShutDown(driver);
                return;
            }

            Console.WriteLine("BingBot: Todays daily " + contextString + " search points: " + STARTING_SEARCH_RP_COUNT + " out of " + DAILY_MAX_SEARCH_RP);

            Console.WriteLine("BingBot: Returning to " + Constants.HOMEPAGE);

            driver.Navigate().GoToUrl(Constants.HOMEPAGE);
            Wait(random);

            Console.WriteLine("BingBot: Starting search loop.");
            DoSearchLoop(driver);

            Console.WriteLine("BingBot: Verifying rewards points left.");
            driver.Navigate().GoToUrl(Constants.HOMEPAGE + Constants.DASHBOARDURL);
            Wait(random);
            if (SetDailyMaxPoints(driver))
            {
                Console.WriteLine("BingBot: " + TOTAL_RP_LTE + " Points left to earn still.");
                driver.Navigate().GoToUrl(Constants.HOMEPAGE);
                Wait(random);
                DoSearchLoop(driver);
            }

            Console.WriteLine("BingBot: Total Rewards Points Earned: " + TOTAL_SESSION_POINTS_EARNED_STATS);
            Console.WriteLine("BingBot: searching complete.");
            ShutDown(driver);
            return;
        }

        public bool SignIn(IWebDriver driver)
        {
            Console.WriteLine("BingBot: Logging in...");
            Wait(random);
            if (mobile)
            {
                GoToLoginPageMobile(driver);
            }
            else
            {
                GoToLoginPageDesktop(driver);
            }
            Wait(random);
            Login(driver);
            Wait(random);
            if (driver.Url.Equals("https://www.bing.com/rewards/dashboard"))
            {
                Console.WriteLine("BingBot: login successfull.");
                return true;
            }
            else if (driver.Title.Equals("Sign in to your Microsoft account"))
            {
                Console.WriteLine("BingBot: login failed. Aborting.");
                return false;
            }
            else
            {
                ShutDown(driver);
                Console.WriteLine("BingBot: Something unknown happened during signin, Aborting.");
                return false;
            }
        }

        public bool SetCurrentPoints(IWebDriver driver)
        {
            //bug when doing mobile searching, id_rc may not be present
            int cpoints;
            IWebElement rewardCount = null;
            if (!SetElement(driver, By.Id("id_rc"), out rewardCount))
            {
                Console.WriteLine("BingBot: Unable to set current points.");
                Console.WriteLine("BingBot: Retrying to set current points...");
                if (!SetElement(driver, By.Id("id_rc"), out rewardCount))
                {
                    Console.WriteLine("BingBot: Unable to set current points.");
                    return false;
                }
            }
            Wait(random);
            //TODO:
            //So TryParse fails for some unknown reason.
            //PossibleSolution: adding Wait() to start of method
            //Other possible solution, do a forearch on the string for Char.IsDigit()
            string str = rewardCount.Text.ToString();
            if (int.TryParse(str, out cpoints))
            {
                CURRENT_RP_COUNT_ACTUAL = cpoints;
                Console.WriteLine("BingBot: Current Reward Points: " + CURRENT_RP_COUNT_ACTUAL);
                return true;
            }
            else
            {
                Console.WriteLine("BingBot: Failed to parse reward points string.");
                return false;
            }
        }

        public bool SetDailyMaxPoints(IWebDriver driver)
        {
            string searchString = contextString + " search";
            string maxSearchString = "";

            if (VerifyIfMaxReached(driver, searchString))
            {
                return false;
            }

            // Update below to use reverse xpath of verify?
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
                            maxSearchString = searchelement.FindElement(By.ClassName("progress")).Text.ToString();
                            return ParseMaxSearchString(maxSearchString);
                        }
                    }
                }
            }
            return false;
        }

        public bool VerifyIfMaxReached(IWebDriver driver, string searchString)
        {
            var temp = driver.FindElements(By.XPath("//div[contains(@class, 'check close-check dashboard-sprite')]"));
            foreach (var i in temp)
            {
                var te = i.FindElement(By.XPath("../.."));
                if (te.Text.Contains(searchString))
                {
                    Console.WriteLine("Max Points reached for " + searchString);
                    return true;
                }
            }
            return false;
        }

        public void DoSearch(IWebDriver driver, List<string> searchList)
        {
            IWebElement searchBar = driver.FindElement(By.Id("sb_form_q"));
            string randomSearchString = searchList[random.Next(0, searchList.Count())];
            Console.WriteLine("BingBot: Performing search on: : " + randomSearchString);
            searchBar.Clear();
            searchBar.SendKeys(randomSearchString);
            searchBar.SendKeys(Keys.Enter);
            TOTAL_SESSION_SEARCHES_STATS++;
            Wait(random);
        }

        public bool DoRelatedSearch(IWebDriver driver)
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

        public void DoSearchLoop(IWebDriver driver)
        {
            bool relatedsearchresult = true;
            while (!STOPBOT)
            {
                DoSearch(driver, searchList);
                checkRPcount_counter++;
                Debug.WriteLine(checkRPcount_counter);
                CheckRPCount(driver);

                relatedsearchresult = CheckRelatedSearch(driver);

                while (relatedsearchresult && !STOPBOT)
                {
                    relatedsearchresult = CheckRelatedSearch(driver);
                }
            }
        }

        public void DoOffers(IWebDriver driver)
        {
            string mainWinHandle = driver.CurrentWindowHandle.ToString();
            List<IWebElement> eList = new List<IWebElement>();
            // While there are active offers
            // Do a offer
            // refresh page
            // refresh offer list
            GetAvailableOffers(driver, eList);
            if (eList.Count == 0)
            {
                Console.WriteLine("BingBot: Failed to find any offers.");
            }
            while (eList.Count() > 0)
            {
                eList[random.Next(0, eList.Count())].Click();
                Wait(random);
                var WinHandles = driver.WindowHandles;
                foreach (var win in WinHandles)
                {
                    if (win.ToString() != mainWinHandle)
                    {
                        driver.SwitchTo().Window(win);
                        Wait(random);
                        driver.Close();
                    }
                }
                driver.SwitchTo().Window(mainWinHandle);
                Wait(random);

                eList = new List<IWebElement>();
                driver.Navigate().Refresh();
                GetAvailableOffers(driver, eList);
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

        public void Login(IWebDriver driver)
        {
            WebDriverWait _wait = new WebDriverWait(driver, new TimeSpan(0, 1, 0));
            _wait.Until(d => d.FindElement(By.Name("login")));

            driver.FindElement(By.Name("login")).SendKeys(username);
            driver.FindElement(By.Name("passwd")).SendKeys(password); ;
            driver.FindElement(By.Id("idSIButton9")).Click();
        }

        private bool CheckRelatedSearch(IWebDriver driver)
        {
            if (DoRelatedSearch(driver))
            {
                checkRPcount_counter++;
                Debug.WriteLine("search counter: " + checkRPcount_counter);
                CheckRPCount(driver);
                return true;
            }
            else
            {
                return false;
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
                    Debug.WriteLine("found a related search");
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
                Console.WriteLine("BingBot: failed to find any related searches");
            }
            Wait(random);
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

        private void CalcRP_LTE(IWebDriver driver)
        {
            TOTAL_SESSION_POINTS_EARNED_STATS = CURRENT_RP_COUNT_ACTUAL - STARTING_RP_COUNT_ACTUAL;

            int total_dailypoints_so_far = STARTING_SEARCH_RP_COUNT + (CURRENT_RP_COUNT_ACTUAL - RP_COUNT_AFTER_OFFERS);
            TOTAL_RP_LTE = DAILY_MAX_SEARCH_RP - total_dailypoints_so_far;

            Console.WriteLine("BingBot: points so far: " + total_dailypoints_so_far + " out of " + DAILY_MAX_SEARCH_RP + ". " + TOTAL_RP_LTE + " rewards points left.");
            if (total_dailypoints_so_far >= DAILY_MAX_SEARCH_RP || TOTAL_RP_LTE <= 0)
            {
                STOPBOT = true;
            }
        }

        private void CheckRPCount(IWebDriver driver)
        {
            if (checkRPcount_counter > TOTAL_RP_LTE * 3)
            {
                Console.WriteLine("BingBot: Checking how many points left to earn...");
                if (SetCurrentPoints(driver))
                {
                    CalcRP_LTE(driver);
                    checkRPcount_counter = 0;
                }
                else
                {
                    driver.Navigate().GoToUrl(Constants.HOMEPAGE + Constants.DASHBOARDURL);
                    if (SetCurrentPoints(driver))
                    {
                        CalcRP_LTE(driver);
                        checkRPcount_counter = 0;
                        driver.Navigate().GoToUrl(Constants.HOMEPAGE);
                        DoSearch(driver, searchList);
                    }
                    else
                    {
                        throw new Exception("failed to set current points from dashboard, aborting.");
                    }
                }
            }
        }

        private void ShutDown(IWebDriver driver)
        {
            Console.WriteLine("BingBot: Shutting Down.");
            Console.WriteLine("############################################");
            driver.Quit();
        }

        private void GetAvailableOffers(IWebDriver driver, List<IWebElement> eList)
        {
            var temp = driver.FindElements(By.XPath("//div[contains(@class, 'check open-check dashboard-sprite')]"));
            foreach (var e in temp)
            {
                var te = e.FindElement(By.XPath("../../../../.."));
                if (te.Text.Contains("Earn and explore"))
                {
                    eList.Add(e);
                }
            }
        }
    }
}
