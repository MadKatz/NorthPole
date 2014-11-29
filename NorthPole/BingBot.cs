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
    class BingBot
    {

        protected IWebDriver driver;
        protected string username;
        protected string password;

        public string Username
        {
            get { return username; }
        }

        protected string agentString;
        protected string contextString;

        protected List<string> searchList;

        protected Random random;
        protected bool STOPBOT;
        protected bool mobile;

        public int TOTAL_SESSION_POINTS_EARNED_STATS; // TODO: Check to see if we can remove
        private int TOTAL_RP_LTE;

        private int STARTING_RP_COUNT_ACTUAL; // starting reward points counter (how many points did we start at) TODO: change to lowercase, and audit
        private int starting_search_rp_count; // search point count when we started

        private int current_rp_count_actual; // current reward points counter
        public int Current_RP_Count_Actual
        {
            get { return current_rp_count_actual; }
        }

        private int total_dailypoints_so_far;
        public int Total_DailyPoints_So_Far
        {
            get { return total_dailypoints_so_far; }
        }

        private int daily_max_search_rp; // max daily PC search reward points
        public int Daily_Max_Search_RP
        {
            get { return daily_max_search_rp; }
        }

        private int checkRPcount_counter; // counter for checking our rewards points so far
        private int RP_COUNT_AFTER_OFFERS; // TODO: check to see if we can remove
        private int offer_count;
        public int Offer_Count
        {
            get { return offer_count; }
        }
        private int offer_max;
        public int Offer_Max
        {
            get { return offer_max; }
        }

        public BingBot()
        {
            searchList = null;
            mobile = false;
            STOPBOT = false;
            agentString = Constants.DESKTOPAGENT;
            contextString = "PC";
            TOTAL_SESSION_POINTS_EARNED_STATS = 0;
        }

        public void SetUp()
        {
            Console.WriteLine("BingBot: Starting up...");

            Console.WriteLine("BingBot: Launching Firefox as " + contextString);
            FirefoxProfile profile = new FirefoxProfile();
            profile.SetPreference("general.useragent.override", agentString);
            driver = new FirefoxDriver(profile);
        }

        public void ShutDown()
        {
            Console.WriteLine("BingBot: Shutting Down.");
            Console.WriteLine("############################################");
            driver.Quit();
        }

        public void StartBot(string username, string password, bool domobile, Random random, List<string> searchWordList)
        {
            this.username = username;
            this.password = password;
            this.random = random;
            searchList = searchWordList;

            mobile = domobile;

            Init();
            SetUp();

            Console.WriteLine("BingBot: Going to " + Constants.SIGNINURL);
            driver.Navigate().GoToUrl(Constants.SIGNINURL);

            driver.Manage().Window.Maximize();

            SignInHelper signInHelper = new SignInHelper();
            if (!signInHelper.SignIn(driver, mobile, username, password, random))
            {
                ShutDown();
                return;
            }

            Console.WriteLine("BingBot: Setting current points.");
            if (!SetCurrentPoints())
            {
                Console.WriteLine("BingBot: Could not set current points, Aborting.");
                ShutDown();
                return;
            }

            STARTING_RP_COUNT_ACTUAL = current_rp_count_actual;

            OfferHelper offerHelper = new OfferHelper();
            if (!mobile)
            {
                Console.WriteLine("BingBot: Doing Daily Offers...");
                offerHelper.DoOffers(driver, random, ref offer_max, ref offer_count);
                Console.WriteLine("BingBot: Setting current points.");
                if (!SetCurrentPoints())
                {
                    Console.WriteLine("BingBot: Could not set current points, Aborting.");
                    ShutDown();
                    return;
                }
            }
            Console.WriteLine("BingBot: Setting daily point count.");
            RP_COUNT_AFTER_OFFERS = current_rp_count_actual;


            if (VerifyIfMaxReached(contextString + " search"))
            {
                ShutDown();
                return;
            }
            else if (!SetDailyMaxPoints())
            {
                Console.WriteLine("BingBot: Could not set daily max points, Aborting.");
                ShutDown();
                return;
            }

            Console.WriteLine("BingBot: Todays daily " + contextString + " search points: " + starting_search_rp_count + " out of " + daily_max_search_rp);

            Console.WriteLine("BingBot: Returning to " + Constants.HOMEPAGE);

            driver.Navigate().GoToUrl(Constants.HOMEPAGE);
            BingBotUtils.Wait(random);

            Console.WriteLine("BingBot: Starting search loop.");
            DoSearchLoop();

            Console.WriteLine("BingBot: Verifying rewards points left.");
            driver.Navigate().GoToUrl(Constants.HOMEPAGE + Constants.DASHBOARDURL);
            BingBotUtils.Wait(random);
            if (!VerifyIfMaxReached(contextString + " search"))
            {
                SetDailyMaxPoints();
                if (TOTAL_RP_LTE > 0)
                {
                    Console.WriteLine("BingBot: " + TOTAL_RP_LTE + " Points left to earn still.");
                    driver.Navigate().GoToUrl(Constants.HOMEPAGE);
                    BingBotUtils.Wait(random);
                    DoSearchLoop();
                }
            }

            Console.WriteLine("BingBot: Total Rewards Points Earned: " + TOTAL_SESSION_POINTS_EARNED_STATS);
            Console.WriteLine("BingBot: searching complete.");

            ShutDown();
            return;
        }

        public bool SetCurrentPoints()
        {
            //bug when doing mobile searching, id_rc may not be present
            int cpoints;
            IWebElement rewardCount = null;
            if (!BingBotUtils.SetElement(driver, By.Id("id_rc"), out rewardCount))
            {
                Console.WriteLine("BingBot: Unable to set current points.");
                Console.WriteLine("BingBot: Retrying to set current points...");
                if (!BingBotUtils.SetElement(driver, By.Id("id_rc"), out rewardCount))
                {
                    Console.WriteLine("BingBot: Unable to set current points.");
                    return false;
                }
            }
            BingBotUtils.Wait(random);
            //TODO:
            //So TryParse fails for some unknown reason.
            //PossibleSolution: adding Wait() to start of method
            //Other possible solution, do a forearch on the string for Char.IsDigit()
            string str = rewardCount.Text.ToString();
            if (int.TryParse(str, out cpoints))
            {
                current_rp_count_actual = cpoints;
                Console.WriteLine("BingBot: Current Reward Points: " + current_rp_count_actual);
                return true;
            }
            else
            {
                Console.WriteLine("BingBot: Failed to parse reward points string.");
                return false;
            }
        }

        public bool SetDailyMaxPoints()
        {
            string searchString = contextString + " search";
            string maxSearchString = "";
            //TODO: remove commented code below
            //if (VerifyIfMaxReached(searchString))
            //{
            //    return false;
            //}

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

        public bool VerifyIfMaxReached(string searchString)
        {
            var temp = driver.FindElements(By.XPath("//div[contains(@class, 'check close-check dashboard-sprite')]"));
            foreach (var i in temp)
            {
                var te = i.FindElement(By.XPath("../.."));
                if (te.Text.Contains(searchString))
                {
                    //TODO: parse string to set PC/Mobile current and max stat strings
                    Console.WriteLine("BingBot: Max Points reached for " + searchString);
                    ParseCompletedSearchString(te.FindElement(By.ClassName("progress")).Text.ToString());
                    return true;
                }
            }
            return false;
        }

        public void DoSearchLoop()
        {
            SearchHelper searchHelper = new SearchHelper();
            bool relatedsearchresult = true;
            STOPBOT = false;
            while (!STOPBOT)
            {
                searchHelper.DoSearch(driver, searchList, random);
                checkRPcount_counter++;
                Debug.WriteLine(checkRPcount_counter);
                CheckRPCount();

                relatedsearchresult = CheckRelatedSearch(searchHelper);

                while (relatedsearchresult && !STOPBOT)
                {
                    relatedsearchresult = CheckRelatedSearch(searchHelper);
                }
            }
        }

        private void Init()
        {
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
            STARTING_RP_COUNT_ACTUAL = -1;
            current_rp_count_actual = -1;
            starting_search_rp_count = -1;
            daily_max_search_rp = -1;
            total_dailypoints_so_far = -1;
            checkRPcount_counter = 0;
            RP_COUNT_AFTER_OFFERS = -1;
            offer_count = 0;
            offer_max = 0;
        }

        private bool CheckRelatedSearch(SearchHelper searchHelper)
        {
            if (searchHelper.DoRelatedSearch(driver, mobile, random))
            {
                checkRPcount_counter++;
                Debug.WriteLine("search counter: " + checkRPcount_counter);
                CheckRPCount();
                return true;
            }
            else
            {
                return false;
            }
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
                starting_search_rp_count = startNum;
                daily_max_search_rp = endNum;
                TOTAL_RP_LTE = daily_max_search_rp - starting_search_rp_count;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void ParseCompletedSearchString(string csstr)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < csstr.Count(); i++)
            {
                if (char.IsDigit(csstr[i]))
                {
                    sb.Append(csstr[i]);
                }
            }
            int temp;
            if (int.TryParse(sb.ToString(), out temp))
            {
                starting_search_rp_count = temp;
                daily_max_search_rp = temp;
                TOTAL_RP_LTE = daily_max_search_rp - starting_search_rp_count;
                total_dailypoints_so_far = temp;
            }
        }

        private void CalcRP_LTE()
        {
            TOTAL_SESSION_POINTS_EARNED_STATS = current_rp_count_actual - STARTING_RP_COUNT_ACTUAL;

            total_dailypoints_so_far = starting_search_rp_count + (current_rp_count_actual - RP_COUNT_AFTER_OFFERS);
            TOTAL_RP_LTE = daily_max_search_rp - total_dailypoints_so_far;

            Console.WriteLine("BingBot: points so far: " + total_dailypoints_so_far + " out of " + daily_max_search_rp + ". " + TOTAL_RP_LTE + " rewards points left.");
            if (total_dailypoints_so_far >= daily_max_search_rp || TOTAL_RP_LTE <= 0)
            {
                STOPBOT = true;
            }
        }

        private void CheckRPCount()
        {
            if (checkRPcount_counter > TOTAL_RP_LTE * Constants.SEARCHTOPOINTRATIO)
            {
                Console.WriteLine("BingBot: Checking how many points left to earn...");
                if (SetCurrentPoints())
                {
                    CalcRP_LTE();
                    checkRPcount_counter = 0;
                }
                else
                {
                    driver.Navigate().GoToUrl(Constants.HOMEPAGE + Constants.DASHBOARDURL);
                    BingBotUtils.Wait(random);
                    if (SetCurrentPoints())
                    {
                        CalcRP_LTE();
                        checkRPcount_counter = 0;
                        driver.Navigate().GoToUrl(Constants.HOMEPAGE);
                        BingBotUtils.Wait(random);
                        SearchHelper searchHelper = new SearchHelper();
                        searchHelper.DoSearch(driver, searchList, random);
                    }
                    else
                    {
                        throw new Exception("failed to set current points from dashboard, aborting.");
                    }
                }
            }
        }
    }
}
