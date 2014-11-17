using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace NorthPole
{
    class BotManager
    {
        private Random random;
        private List<string> searchList;
        private Dictionary<string, string> accounts;

        private int TOTAL_POINTS_EARNED_STATS;
        private int TOTAL_SEARCHES_STATS;

        public BotManager()
        {
            random = new Random();
        }

        public void StartUp()
        {
            Console.WriteLine("##############BotManager v1.0##############");
            Console.WriteLine("BotManager is starting up...");
            try
            {
                Console.WriteLine("Loading wordlist...");
                searchList = LoadWordSearchFile(@"wordlist.txt");
            }
            catch (Exception e)
            {
                string msg = "Failed to load word list file.";
                Console.WriteLine(msg);
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                throw e;
            }
            Console.WriteLine("BotManager: wordlist loaded. Word count: " + searchList.Count());
            Console.WriteLine("BotManager: Loading accounts...");
            accounts = LoadAccounts();
            Console.WriteLine("BotManager: Accounts loaded. Number of accounts loaded: " + accounts.Count());
            Console.WriteLine("BotManager: Start-up complete.");
            DisplayHASHString();
        }

        public void ShutDown()
        {
        }

        public void ExecuteAccountTest()
        {
            ExecuteAccount("craigsmiths@outlook.com", "jumpstart987", searchList);
        }

        public void ExecuteAccount(string username, string password, List<string> searchList)
        {
            DisplayHASHString();
            Console.WriteLine("BotManager: Executing Bing Search on account " + username);
            BingBot bot = new BingBot();
            try
            {
                bot.StartBot(username, password, false, random, searchList);
                bot.StartBot(username, password, true, random, searchList);
            }
            catch (Exception e)
            {
                string msg = "BotManager: Bot failed.";
                Console.WriteLine(msg);
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        public void ExecuteAccounts(Dictionary<string, string> accountList, List<string> searchList)
        {
            DisplayHASHString();
            Console.WriteLine("BotManager: Executing Bing Search on all acounts.");
            Console.WriteLine("BotManager: Initizing Stats.");
            TOTAL_POINTS_EARNED_STATS = 0;
            TOTAL_SEARCHES_STATS = 0;
        }

        private void DisplayStats()
        {
            Console.WriteLine("##############BotManager Stats##############");
            Console.WriteLine("TOTAL POINTS EARNED: " + TOTAL_POINTS_EARNED_STATS);
            Console.WriteLine("TOTAL SEARCHES DONE: " + TOTAL_SEARCHES_STATS);
            DisplayHASHString();
        }

        private void DisplayHASHString()
        {
            Console.WriteLine("############################################");
        }
        private Dictionary<string, string> LoadAccounts()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            //TODO: Load from file;
            result.Add("account1", "password1");
            return result;
        }
        private List<string> LoadWordSearchFile(string path)
        {
            string[] temp = File.ReadAllLines(path);
            return new List<string>(temp);
        }
    }
}
