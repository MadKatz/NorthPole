using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Firefox;
using NorthPole.Helpers;
using NorthPole.Utils;

namespace NorthPole
{
    class BingBot
    {

        /*protected IWebDriver driver;
        protected string username;
        protected string password;

        public string Username
        {
            get { return username; }
        }

        protected string agentString;
        protected string contextString;

        protected List<string> searchWordList;

        protected Random random;
        protected bool STOPBOT;
        protected bool mobile;
        protected bool complete;
        public bool Complete { get { return complete; } }

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
            searchWordList = null;
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
            complete = true;
            driver.Quit();
        }

        public void StartBot(string username, string password, bool domobile, Random random, List<string> searchWordList)
        {
            this.username = username;
            this.password = password;
            this.random = random;
            this.searchWordList = searchWordList;

            mobile = domobile;

            Init();
            SetUp();
            try
            {
                driver.Navigate().GoToUrl(Constants.SIGNINURL);
                //driver.Manage().Window.Maximize();
                SignInHelper signInHelper = new SignInHelper(driver, username, password, mobile);
                signInHelper.SignIn();

                DesktopDashboardHelper dbHelper = new DesktopDashboardHelper(driver);
                current_rp_count_actual = Int32.Parse(dbHelper.GetCurrentPoints());

                STARTING_RP_COUNT_ACTUAL = current_rp_count_actual;
                if (!mobile)
                {
                    OfferHelper offerHelper = new OfferHelper(driver, random);
                    offerHelper.DoOffers(ref offer_max, ref offer_count);
                    current_rp_count_actual = Int32.Parse(dbHelper.GetCurrentPoints());
                }
                RP_COUNT_AFTER_OFFERS = current_rp_count_actual;
                if (VerifyIfMaxReached(contextString + " search"))
                {
                    throw new Exception("Max points reached for " + username);
                }
                else if (!SetDailyMaxPoints())
                {
                    throw new Exception("Could not set daily max points for " + username);
                }

                Debug.WriteLine(username + ": Todays daily " + contextString + " search points: " + starting_search_rp_count + " out of " + daily_max_search_rp);

                driver.Navigate().GoToUrl(Constants.HOMEPAGE);
                BotUtils.Wait(random);
                Debug.WriteLine(username + ": Starting search loop.");
                DoSearchLoop();

                Debug.WriteLine(username + ": Verifying rewards points left.");
                driver.Navigate().GoToUrl(Constants.HOMEPAGE + Constants.DASHBOARDURL);
                BotUtils.Wait(random);

                if (!VerifyIfMaxReached(contextString + " search"))
                {
                    SetDailyMaxPoints();
                    if (TOTAL_RP_LTE > 0)
                    {
                        Debug.WriteLine(username + ": " + TOTAL_RP_LTE + " Points left to earn still.");
                        driver.Navigate().GoToUrl(Constants.HOMEPAGE);
                        BotUtils.Wait(random);
                        DoSearchLoop();
                    }
                }

                Debug.WriteLine(username + ": Total Rewards Points Earned: " + TOTAL_SESSION_POINTS_EARNED_STATS);
                Debug.WriteLine(username + ": searching complete.");
                ShutDown();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                ShutDown();
            }
        }

        public bool SetDailyMaxPoints()
        {
            string searchString = contextString + " search";
            string maxSearchString = "";

            // Update below to use reverse xpath of verify?
            if (mobile)
            {
                IWebElement temp = driver.FindElement(By.ClassName("row"));
                var eList = temp.FindElements(By.TagName("li"));
                foreach (var element in eList)
                {
                    if (element.FindElement(By.ClassName("offerTitle")).Text.Contains(searchString))
                    {
                        maxSearchString = element.FindElement(By.ClassName("progress")).Text.ToString();
                        return ParseMaxSearchString(maxSearchString);
                    }
                }
            }
            else
            {
                var eList = driver.FindElements(By.TagName("li"));
                foreach (var element in eList)
                {
                    if (element.FindElement(By.ClassName("title")).Text.Contains(searchString))
                    {
                        maxSearchString = element.FindElement(By.ClassName("progress")).Text.ToString();
                        return ParseMaxSearchString(maxSearchString);
                    }
                }
            }
            Debug.WriteLine(username + ": Failed to find element with " + searchString + " while setting max points.");
            return false;
        }

        public bool VerifyIfMaxReached(string searchString)
        {
            var eList = driver.FindElements(By.XPath("//div[contains(@class, 'check close-check dashboard-sprite')]"));
            if (mobile)
            {
                foreach (var element in eList)
                {
                    IWebElement targetElement = element.FindElement(By.XPath("../../.."));
                    if (targetElement.FindElement(By.ClassName("offerTitle")).Text.Contains(searchString))
                    {
                        Debug.WriteLine(username + ": Max Points reached for " + searchString);
                        ParseCompletedSearchString(targetElement.FindElement(By.ClassName("progress")).Text.ToString());
                        return true;
                    }
                }
            }
            else
            {
                foreach (var element in eList)
                {
                    IWebElement targetElement = element.FindElement(By.XPath("../.."));
                    if (targetElement.FindElement(By.ClassName("title")).Text.Contains(searchString))
                    {
                        Debug.WriteLine(username + ": Max Points reached for " + searchString);
                        ParseCompletedSearchString(targetElement.FindElement(By.ClassName("progress")).Text.ToString());
                        return true;
                    }
                }
            }
            return false;
        }

        public void DoSearchLoop()
        {
            SearchHelper searchHelper = new SearchHelper(driver, searchWordList, random);
            bool relatedsearchresult = true;
            STOPBOT = false;
            while (!STOPBOT)
            {
                searchHelper.DoSearch();
                checkRPcount_counter++;
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
            complete = false;
        }

        private bool CheckRelatedSearch(SearchHelper searchHelper)
        {
            if (searchHelper.DoRelatedSearch(mobile))
            {
                checkRPcount_counter++;
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

            Debug.WriteLine(username + ": points so far: " + total_dailypoints_so_far + " out of " + daily_max_search_rp + ". " + TOTAL_RP_LTE + " rewards points left.");
            if (total_dailypoints_so_far >= daily_max_search_rp || TOTAL_RP_LTE <= 0)
            {
                STOPBOT = true;
            }
        }

        private void CheckRPCount()
        {
            DesktopDashboardHelper dbHelper = new DesktopDashboardHelper(driver);
            if (checkRPcount_counter > TOTAL_RP_LTE * Constants.SEARCHTOPOINTRATIO)
            {
                Debug.WriteLine(username + ": Checking how many points left to earn...");
                try
                {
                    current_rp_count_actual = Int32.Parse(dbHelper.GetCurrentPoints());
                    CalcRP_LTE();
                    checkRPcount_counter = 0;
                }
                catch (Exception e)
                {
                    driver.Navigate().GoToUrl(Constants.HOMEPAGE + Constants.DASHBOARDURL);
                    BotUtils.Wait(random);
                    current_rp_count_actual = Int32.Parse(dbHelper.GetCurrentPoints());
                    CalcRP_LTE();
                    checkRPcount_counter = 0;
                    driver.Navigate().GoToUrl(Constants.HOMEPAGE);
                    BotUtils.Wait(random);
                    SearchHelper searchHelper = new SearchHelper(driver, searchWordList, random);
                    searchHelper.DoSearch();
                }
            }
        }
         *     */
    }
}
