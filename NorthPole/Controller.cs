using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NorthPole.Utils;
using NorthPole.Account;
using NorthPole.Bot;

namespace NorthPole
{
    public class Controller
    {
        private Random random;
        List<string> searchList;
        private List<AccountInfo> accountInfoList;
        private Dictionary<string, string> accounts;

        public Controller()
        {
            random = new Random();
            accountInfoList = new List<AccountInfo>();
            accounts = new Dictionary<string, string>();
        }

        public void SetUp()
        {
            Console.WriteLine("Welcome to NorthPole v1.2");
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
            Console.WriteLine("wordlist loaded. Word count: " + searchList.Count());
            Console.WriteLine("Loading accounts...");
            accounts = LoadAccounts("accountfile.txt");
            Console.WriteLine("Accounts loaded. Number of accounts loaded: " + accounts.Count());
        }

        public void ShutDown()
        {
            Console.WriteLine("Shutting down.");
        }

        public void Start()
        {
            if (searchList.Count > 0)
            {
                ExecuteAccounts(accounts, searchList);
                //ExecuteAccount("", "", searchList);
                DisplayStats();
                ShutDown();
            }
            else
            {
                Console.WriteLine("search list is empty, cannot start.");
                return;
            }
        }

        public void ExecuteAccount(string email, string password, List<string> searchList)
        {
            Console.WriteLine("Executing Search on account " + email);
            AccountContext accountContext = new AccountContext(email, password);
            Execute(accountContext, searchList);
            //AccountInfo accountInfo = Execute(username, password, searchList);
            //accountInfoList.Add(accountInfo);
        }

        public void ExecuteAccount(string username, string password)
        {
            if (searchList.Count > 0)
            {
                ExecuteAccount(username, password, searchList);
                DisplayStats();
                ShutDown();
            }
            else
            {
                Console.WriteLine("search list is empty, cannot start.");
                return;
            }
        }

        public void ExecuteAccounts(Dictionary<string, string> accountList, List<string> searchList)
        {
            Console.WriteLine("Executing Search on all acounts.");
            //create thread pool
            //create threads and add each to pool
            //start each thread in pool
            //loop (are all threads complete?)
            // if yes, break & report.
            foreach (var account in accountList)
            {
                ExecuteAccount(account.Key, account.Value, searchList);
                Console.WriteLine("Sleeping for 3mins.");
                BotUtils.Wait(60000 * 3);
            }
            Console.WriteLine("BotManager: All accounts executed.");
        }

        private void Execute(AccountContext accountContext, List<string> searchList)
        {
            DesktopBot db = new DesktopBot(accountContext, searchList, random);
            db.StartBot();
        }

        private void DisplayStats()
        {
            Console.WriteLine("##############NorthPole Stats##############");
            int maxStringLength = DisplayUtils.GetMaxAccountStrLength(accounts);
            Console.WriteLine(DisplayUtils.GetStatHeaderString(maxStringLength));
            foreach (var accountInfo in accountInfoList)
            {
                Console.WriteLine(DisplayUtils.GetAccountStatString(accountInfo, maxStringLength));
                Console.WriteLine(Constants.HASH_STRING);
            }
        }

        public Dictionary<string, string> LoadAccounts(string filepath)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            //TODO: Load from file;
            //string[] accountInfo;
            //bool have_username;
            //Read each line
            //if (line.count > 0)
            //  if (first char isnt not "#")
            //      string[] x = string.split("=", line);
            //      if (x[0] == "account")
            //          accountInfo[0] = x[1];
            //          have_username = true;
            //
            StreamReader sr;
            string line;
            string username = null;
            string[] temp;

            try
            {
                using (sr = new StreamReader(filepath))
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.Count() > 0)
                        {
                            if (line[0] != '#')
                            {
                                temp = line.Split('=');
                                if (temp.Count() > 1)
                                {
                                    if (temp[0] == "email")
                                    {
                                        username = temp[1];
                                    }
                                    else if (temp[0] == "password" && !string.IsNullOrEmpty(username))
                                    {
                                        result.Add(username, temp[1]);
                                        username = null;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("failed to load account file.");
                Console.WriteLine(e.Message);
            }
            return result;
        }

        private List<string> LoadWordSearchFile(string path)
        {
            string[] temp = File.ReadAllLines(path);
            return new List<string>(temp);
        }
    }
}
