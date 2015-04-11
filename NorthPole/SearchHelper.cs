using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using OpenQA.Selenium;

namespace NorthPole
{
    class SearchHelper
    {
        private List<string> usedSearchWordList;
        private List<string> usedRelatedSearchLinkList;

        public SearchHelper()
        {
            usedSearchWordList = new List<string>();
            usedRelatedSearchLinkList = new List<string>();
        }

        public void DoSearch(IWebDriver driver, List<string> searchList, Random random)
        {
            IWebElement searchBar = driver.FindElement(By.Id("sb_form_q"));
            string randomSearchString = searchList[random.Next(0, searchList.Count())];
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
                    randomSearchString = searchList[random.Next(0, searchList.Count())];
                }
            }
            usedSearchWordList.Add(randomSearchString);
            Console.WriteLine("BingBot: Performing search on: : " + randomSearchString);
            searchBar.Clear();
            searchBar.SendKeys(randomSearchString);
            searchBar.SendKeys(Keys.Enter);
            BingBotUtils.Wait(random);
        }

        public bool DoRelatedSearch(IWebDriver driver, bool mobile, Random random)
        {
            //put in logic for webelement timeout ( WebDriverExpection )
            //possible fix for element no longer attached to the DOM
            BingBotUtils.Wait(random);
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
                if (FindRelatedSearch(RelatedSearchElements, random))
                {
                    return true;
                }
            }
            else
            {
                var RelatedSearchElements = driver.FindElements(By.ClassName("b_ans"));
                if (FindRelatedSearch(RelatedSearchElements, random))
                {
                    return true;
                }
            }
            return false;
        }

        private bool FindRelatedSearch(System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> elements, Random random)
        {
            bool result = false;
            foreach (var item in elements)
            {
                if (item.Text.Contains("Related searches") || item.Text.Contains("RELATED SEARCHES"))
                {
                    var resultList = item.FindElements(By.TagName("a"));
                    if (resultList.Count == 0)
                    {
                        Console.WriteLine("BingBot: failed to find any related searches");
                        return result;
                    }
                    Debug.WriteLine("found a related search link");
                    //get new relatedSearchLink
                    //check if new relatedSearchLink has been used before
                    //  if true get new relatedSearchLink and repeat
                    //  if false, add relatedSearchLink to usedRelatedSearchLinkList
                    int rndLink = random.Next(0, resultList.Count());
                    bool newRelatedSearchLink = false;
                    while (!newRelatedSearchLink)
                    {
                        newRelatedSearchLink = true;
                        foreach (string link in usedRelatedSearchLinkList)
                        {
                            if (resultList[rndLink].Text.Equals(link))
                            {
                                newRelatedSearchLink = false;
                            }
                        }
                        if (!newRelatedSearchLink)
                        {
                            if (resultList.Count < 3)
                            {
                                Console.WriteLine("BingBot: failed to find any unused related searches");
                                return result;
                            }
                            rndLink = random.Next(0, resultList.Count());
                        }
                    }
                    usedRelatedSearchLinkList.Add(resultList[rndLink].Text);
                    resultList[rndLink].SendKeys("");
                    resultList[rndLink].Click();
                    return true;
                }
            }
            if (!result)
            {
                Console.WriteLine("BingBot: failed to find any related searches");
            }
            BingBotUtils.Wait(random);
            return result;
        }
    }
}
