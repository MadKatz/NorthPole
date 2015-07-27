using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NorthPole.Utils;
using System.Diagnostics;

namespace NorthPole.Helpers
{
    public class SearchHelper
    {
        protected IWebDriver driver;
        protected Random random;
        protected List<string> usedSearchWordList;
        protected List<string> searchWordList;
        protected String searchBarID = "sb_form_q";
        private int searchCounter;

        public int SearchCounter
        {
            get { return searchCounter; }
            set { searchCounter = value; }
        }

        public SearchHelper(IWebDriver driver, List<string> searchWordList, Random random)
        {
            this.driver = driver;
            this.random = random;
            this.searchWordList = searchWordList;
            usedSearchWordList = new List<string>();
            SearchCounter = 0;
        }

        public void DoSearch()
        {
            IWebElement searchBar = driver.FindElement(By.Id(searchBarID));
            string randomSearchString = searchWordList[random.Next(0, searchWordList.Count())];
            bool gotNewSearchWord = false;
            //get new searchWord
            //check if new search has been used before
            //  if true get new searchWord and repeat
            //  if false, add word to usedSearchWordList
            while (!gotNewSearchWord)
            {
                gotNewSearchWord = true;
                foreach (string word in usedSearchWordList)
                {
                    if (randomSearchString.Equals(word))
                    {
                        gotNewSearchWord = false;
                    }
                }
                if (!gotNewSearchWord)
                {
                    randomSearchString = searchWordList[random.Next(0, searchWordList.Count())];
                }
            }
            usedSearchWordList.Add(randomSearchString);
            searchBar.Clear();
            searchBar.SendKeys(randomSearchString);
            searchBar.SendKeys(Keys.Enter);
            SearchCounter++;
            BotUtils.Wait(random);
        }

        public bool DoRelatedSearch(bool mobile)
        {
            //put in logic for webelement timeout ( WebDriverExpection )
            //possible fix for element no longer attached to the DOM
            BotUtils.Wait(random);
            //end fix
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
                if (FindRelatedSearch(RelatedSearchElements))
                {
                    return true;
                }
            }
            else
            {
                var RelatedSearchElements = driver.FindElements(By.ClassName("b_ans"));
                if (FindRelatedSearch(RelatedSearchElements))
                {
                    return true;
                }
            }
            return false;
        }

        private bool FindRelatedSearch(System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> elements)
        {
            bool result = false;
            foreach (var item in elements)
            {
                if (item.Text.Contains("Related searches") || item.Text.Contains("RELATED SEARCHES"))
                {
                    var resultList = item.FindElements(By.TagName("a"));
                    if (resultList.Count == 0)
                    {
                        Debug.WriteLine("failed to find any related searches");
                        return result;
                    }
                    //TODO: add below logic:
                    //get new relatedSearchLink
                    //check if new relatedSearchLink has been used before
                    //  if true get new relatedSearchLink and repeat
                    //  if false, add relatedSearchLink to usedRelatedSearchLinkList
                    int rndLink = random.Next(0, resultList.Count());
                    resultList[rndLink].SendKeys("");//TODO: get rid of this line
                    resultList[rndLink].Click();
                    SearchCounter++;
                    return true;
                }
            }
            if (!result)
            {
                Debug.WriteLine("failed to find any related searches");
            }
            return result;
        }
    }
}
