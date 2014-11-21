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
        private Dictionary<string, int> accountPoints;

        public BotManager()
        {
            random = new Random();
            accountPoints = new Dictionary<string, int>();
        }

        public void StartUp()
        {
            Console.WriteLine("##############BotManager v1.0##############");
            Console.WriteLine("BotManager is starting up...");

            Console.WriteLine("Loading wordlist...");
            try
            {
                searchList = LoadWordSearchFile(@"wordlist.txt");
            }
            catch (Exception e)
            {
                string msg = "Failed to load word list file.";
                Console.WriteLine(msg);
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return;
            }
            Console.WriteLine("BotManager: wordlist loaded. Word count: " + searchList.Count());
            Console.WriteLine("BotManager: Loading accounts...");
            accounts = LoadAccounts();
            Console.WriteLine("BotManager: Accounts loaded. Number of accounts loaded: " + accounts.Count());
            Console.WriteLine("BotManager: Start-up complete.");
            //ExecuteAccounts(accounts, searchList);
            ExecuteAccountTest();
            DisplayHASHString();
        }

        public void ShutDown()
        {
            DisplayStats();
            Console.WriteLine("BotManager: Shutting down.");
        }

        public void ExecuteAccountTest()
        {
            ExecuteAccount("", "", searchList);
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
            BingBot bot = new BingBot();
            foreach (var account in accountList)
            {
                Console.WriteLine("BotManager: Starting search on account " + account.Key);
                bool temp = false;
                for (int i = 0; i < 2; i++)
	            {
                    try
                    {
                        bot.StartBot(account.Key, account.Value, temp, random, searchList);
                    }
                    catch (Exception e)
                    {
                        string str = temp ? "on Desktop search." : "on Moible search.";
                        string msg = "BotManager: Bot failed on " + account.Key + ", " + str;
                        Console.WriteLine(msg);
                        Console.WriteLine(e.Message);
                        Console.WriteLine(e.StackTrace);
                    }
                    if (!accountPoints.ContainsKey(bot.username))
                    {
                        accountPoints.Add(bot.username, bot.TOTAL_SESSION_POINTS_EARNED_STATS);
                    }
                    else
                    {
                        accountPoints[bot.username] += bot.TOTAL_SESSION_POINTS_EARNED_STATS;
                    }

                    if (!temp)
                    {
                        Console.WriteLine("Sleeping for 2mins.");
                        System.Threading.Thread.Sleep(60000 * 2);
                    }
                    temp = true;
	            }
                Console.WriteLine("Sleeping for 3mins.");
                System.Threading.Thread.Sleep(60000 * 3);
            }
            Console.WriteLine("BotManager: All accounts executed.");
            ShutDown();
        }

        private void DisplayStats()
        {
            Console.WriteLine("##############BotManager Stats##############");
            Console.WriteLine(" ACCOUNT      :    TOTAL POINTS EARNED ");
            foreach (var account in accountPoints)
            {
                Console.WriteLine(account.Key + "               " + account.Value);
            }
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
            return result;
        }
        private List<string> LoadWordSearchFile(string path)
        {
            string[] temp = File.ReadAllLines(path);
            return new List<string>(temp);
        }
    }
}
