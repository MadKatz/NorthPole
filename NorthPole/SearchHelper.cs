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

        public void DoSearch(IWebDriver driver, List<string> searchList, ref int search_value_to_increment, Random random)
        {
            IWebElement searchBar = driver.FindElement(By.Id("sb_form_q"));
            string randomSearchString = searchList[random.Next(0, searchList.Count())];
            Console.WriteLine("BingBot: Performing search on: : " + randomSearchString);
            searchBar.Clear();
            searchBar.SendKeys(randomSearchString);
            searchBar.SendKeys(Keys.Enter);
            search_value_to_increment++;
            BingBotUtils.Wait(random);
        }

        public bool DoRelatedSearch(IWebDriver driver, bool mobile, ref int search_value_to_increment, Random random)
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
                if (!FindRelatedSearch(RelatedSearchElements, ref search_value_to_increment, random))
                {
                    return false;
                }
            }
            else
            {
                var RelatedSearchElements = driver.FindElements(By.ClassName("b_ans"));
                if (!FindRelatedSearch(RelatedSearchElements, ref search_value_to_increment, random))
                {
                    return false;
                }
            }
            return true;
        }

        private bool FindRelatedSearch(System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> elements, ref int search_value_to_increment, Random random)
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

                    resultList[rndLink].Click();

                    search_value_to_increment++;
                    result = true;
                    break;
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
