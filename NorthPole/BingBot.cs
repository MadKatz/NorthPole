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
        protected string signinURL = "rewards/signin";
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
            Console.WriteLine("Mobile searching set to " + mobile.ToString());
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
            Console.WriteLine("BingBot is starting.");
            Console.WriteLine("Loading wordlist...");
            if (!LoadFile(@"wordlist.txt", out searchList))
            {
                Console.Write("Failed to load wordlist, aborting.");
                return;
            }
            Console.WriteLine("wordlist loaded. Word count: " + searchList.Count());
            // END TODO

            Console.WriteLine("BingBot is launching Firefox...");
            if (!LoadFireFox(agentString, out driver))
            {
                return;
            }
            if (!GoToURL(driver, homepage + signinURL))
            {
                driver.Quit();
                return;
            }
            //hack for mobile related searching
            if (true)
            {
                if (!MaximizeWindow(driver))
                {
                    driver.Quit();
                    return;
                }
            }
            //endhack
            Console.WriteLine("BingBot is going to " + homepage + signinURL);
            Console.WriteLine("BingBot is signing in as...");
            Console.WriteLine("username: " + username);
            Console.WriteLine("password: " + password);
            if (!SignIn(driver))
            {
                driver.Quit();
                return;
            }

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
            if (!GoToURL(driver, homepage))
            {
                return;
            }
            Wait();

            //start botting!
            Console.WriteLine("Starting search loop.");
            doSearchLoop(driver);

            //TODO: add check
            Console.WriteLine("searching complete.");
            CalcRP_LTE(driver);
            Console.WriteLine("Todays daily " + tempstr + " search points: " + (STARTING_SEARCH_RP_COUNT + (CURRENT_RP_COUNT_ACTUAL - STARTING_RP_COUNT_ACTUAL)) + " out of " + DAILY_MAX_SEARCH_RP);
            Console.WriteLine("Total points earned: " + TOTAL_SESSION_POINTS_EARNED_STATS);
            Console.WriteLine("Total searched performed: " + TOTAL_SESSION_SEARCHES_STATS);
            driver.Quit();
            return;
        }

        public bool SignIn(IWebDriver driver)
        {
            if (mobile)
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
            try
            {
                if (driver.Url.Equals("https://www.bing.com/" + dashboardURL))
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
            catch (Exception e)
            {
                LogError("Failed to grab driver.", e);
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
            if (int.TryParse(rewardCount.Text.ToString(), out cpoints))
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
            if (mobile)
            {
                searchString = "Mobile search";
            }
            else
            {
                searchString = "PC search";
            }
            string maxSearchString = "";
            //TODO: add check for if max is already reached
            // check for check mark icon
            System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> elements;
            SetElementsByClassName(driver, "row", out elements);
            if (elements == null || elements.Count <= 0)
            {
                LogError("Failed to fetch rows from dashboard.");
                return false;
            }
            foreach (var item in elements)
            {
                if (item.Text.Contains(searchString))
                {
                    var rowElements = item.FindElements(By.TagName("li"));
                    foreach (var row in rowElements)
                    {
                        if (row.Text.Contains(searchString))
                        {
                            maxSearchString = row.FindElement(By.ClassName("progress")).Text.ToString();
                            break;
                        }
                    }
                }
            }
            return ParseMaxSearchString(maxSearchString);
        }

        public bool DoSearch(IWebDriver driver, List<string> searchList)
        {
            IWebElement searchBar;
            if (!SetElementByID(driver, "sb_form_q", out searchBar))
            {
                return false;
            }
            // update randomneess
            string randomSearchString = searchList[random.Next(0, searchList.Count())];
            Console.WriteLine("Performing search on: : " + randomSearchString);
            try
            {
                searchBar.Clear();
                searchBar.SendKeys(randomSearchString);
                searchBar.SendKeys(Keys.Enter);
            }
            catch (Exception e)
            {
                LogError("Failed to clear or send keys to searchbar element.", e);
                return false;
            }
            TOTAL_SESSION_SEARCHES_STATS++;
            Wait();
            return true;
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
                if (relatedsearchresult = doRelatedSearch(driver))
                {
                    checkRPcount_counter++;
                    Debug.WriteLine(checkRPcount_counter);
                }
                CheckRPCount(driver);
                while (relatedsearchresult && !STOPBOT)
                {
                    if (relatedsearchresult = doRelatedSearch(driver))
                    {
                        checkRPcount_counter++;
                        Debug.WriteLine(checkRPcount_counter);
                    }
                    CheckRPCount(driver);
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
        private bool CalcRP_LTE(IWebDriver driver)
        {

            if (SetCurrentPoints(driver))
            {
                TOTAL_SESSION_POINTS_EARNED_STATS = CURRENT_RP_COUNT_ACTUAL - STARTING_RP_COUNT_ACTUAL;
                Console.WriteLine("Total Rewards Points Earned: " + TOTAL_SESSION_POINTS_EARNED_STATS);

                int total_dailypoints_so_far = STARTING_SEARCH_RP_COUNT + (CURRENT_RP_COUNT_ACTUAL - STARTING_RP_COUNT_ACTUAL);
                TOTAL_RP_LTE = DAILY_MAX_SEARCH_RP - total_dailypoints_so_far;

                Console.WriteLine("points so far: " + total_dailypoints_so_far + " out of " + DAILY_MAX_SEARCH_RP + ". " + TOTAL_RP_LTE + " rewards points left.");
                if (total_dailypoints_so_far > DAILY_MAX_SEARCH_RP || TOTAL_RP_LTE <= 0)
                {
                    STOPBOT = true;
                }
                return true;
            }
            else
            {
                //TODO:
                //Add verify step by going to dashboard and calling SetDailyMaxPoints
                return false;
            }
        }

        private void CheckRPCount(IWebDriver driver)
        {
            if (checkRPcount_counter > TOTAL_RP_LTE * (mobile ? MOBILESEARCHTOPOINTRATIO : DESKTOPSEARCHTOPOINTRATIO))
            {
                //bug in where if we fail to update our current reward points in CalcRP_LTE()->SetCurrentPoints() while mobile searching, counter gets reset when it shouldnt
                //Solution? if mobile goto homepage before calling CalcRP_LTE
                if (CalcRP_LTE(driver))
                {
                    checkRPcount_counter = 0;
                }
            }
        }

        protected bool GoToLoginPageDesktop(IWebDriver driver)
        {
            System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> elements;
            SetElementsByClassName(driver, "identityOption", out elements);
            foreach (var item in elements)
            {
                if (item.Text.Contains("Microsoft account"))
                {
                    IWebElement element = item.FindElement(By.TagName("a"));
                    if (ClickElement(element))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        protected bool GoToLoginPageMobile(IWebDriver driver)
        {
            // something wrong here
            IWebElement MSAccountElement;
            if (!SetElementByID(driver, "WLSignin", out MSAccountElement))
            {
                return false;
            }
            IWebElement SignInElement;
            if (!SetElementByClassName(driver, "idText", out SignInElement))
            {
                return false;
            }
            if (ClickElement(SignInElement))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected bool Login(IWebDriver driver)
        {
            IWebElement LoginBox;
            if (!SetElementByName(driver, "login", out LoginBox))
            {
                return false;
            }
            try
            {
                LoginBox.SendKeys(username);
            }
            catch (Exception e)
            {
                LogError("Failed to send keys to login element.", e);
                return false;
            }
            IWebElement PasswordBox;
            if (!SetElementByName(driver, "passwd", out PasswordBox))
            {
                return false;
            }

            try
            {
                PasswordBox.SendKeys(password);
            }
            catch (Exception e)
            {
                LogError("Unable send keys for password", e);
                return false;
            }
            IWebElement SignInButton;
            if (!SetElementByID(driver, "idSIButton9", out SignInButton))
            {
                return false;
            }
            if (ClickElement(SignInButton))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
