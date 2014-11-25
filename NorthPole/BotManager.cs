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
        List<string> searchList;
        private List<AccountInfo> accountInfoList;
        private Dictionary<string, string> accounts;

        public BotManager()
        {
            random = new Random();
            accountInfoList = new List<AccountInfo>();
        }

        public void SetUp()
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
        }

        public void ShutDown()
        {
            Console.WriteLine("BotManager: Shutting down.");
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
                Console.WriteLine("BotManager: search list is empty, cannot start.");
                return;
            }
        }

        public void ExecuteAccount(string username, string password, List<string> searchList)
        {
            Console.WriteLine(Constants.HASH_STRING);
            Console.WriteLine("BotManager: Executing Bing Search on account " + username);
            AccountInfo accountInfo = Execute(username, password, searchList);
            accountInfoList.Add(accountInfo);
        }

        public void ExecuteAccounts(Dictionary<string, string> accountList, List<string> searchList)
        {
            Console.WriteLine(Constants.HASH_STRING);
            Console.WriteLine("BotManager: Executing Bing Search on all acounts.");
            foreach (var account in accountList)
            {
                ExecuteAccount(account.Key, account.Value, searchList);
                Console.WriteLine("BotManager: Sleeping for 3mins.");
                BingBotUtils.Wait(60000 * 3);
            }
            Console.WriteLine("BotManager: All accounts executed.");
        }

        private AccountInfo Execute(string username, string password, List<string> searchList)
        {
            BingBot bot = new BingBot();
            AccountInfo accountInfo = new AccountInfo();
            accountInfo.AccountName = username;
            bool mobile = false;
            for (int i = 0; i < 2; i++)
            {
                try
                {
                    bot.StartBot(username, password, mobile, random, searchList);
                }
                catch (Exception e)
                {
                    string str = mobile ? "on Moible search." : "on Desktop search.";
                    string msg = "BotManager: Bot failed on " + username + ", " + str;
                    Console.WriteLine(msg);
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
                finally
                {
                    if (!mobile)
                    {
                        accountInfo.Current_PC = bot.Total_DailyPoints_So_Far;
                        accountInfo.Max_PC = bot.Daily_Max_Search_RP;
                        accountInfo.Current_Offer = bot.Offer_Count;
                        accountInfo.Max_Offer = bot.Offer_Max;
                        Console.WriteLine("BotManager: Sleeping for 2mins.");
                        BingBotUtils.Wait(60000 * 2);
                    }
                    else
                    {
                        accountInfo.Current_Mobile = bot.Total_DailyPoints_So_Far;
                        accountInfo.Max_Mobile = bot.Daily_Max_Search_RP;
                    }
                }

                mobile = true;
            }
            accountInfo.Current_RP = bot.Current_RP_Count_Actual;
            return accountInfo;
        }

        private void DisplayStats()
        {
            Console.WriteLine("##############BotManager Stats##############");
            DisplayStatHeader();
            foreach (var accountInfo in accountInfoList)
            {
                DisplayAccountStat(accountInfo);
                Console.WriteLine(Constants.HASH_STRING);
            }
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

        private void DisplayStatHeader()
        {
            string account_String = "Account";
            StringBuilder sb = new StringBuilder();
            sb.Append("# ");

            int maxlength = GetMaxAccountStrLength();
            maxlength = maxlength - account_String.Length;

            for (int i = 0; i < (maxlength / 2) - 1; i++)
            {
                sb.Append(" ");
            }
            sb.Append(account_String);
            for (int i = 0; i < (maxlength / 2) + 1; i++)
            {
                sb.Append(" ");
            }
            sb.Append(" :  Current RP  :   PC    :  Mobile :  Offers  :  Total  ");
            Console.WriteLine(sb.ToString());
        }

        private void DisplayAccountStat(AccountInfo accountInfo)
        {
            StringBuilder sb = new StringBuilder();
            int currentRP_string_count = 14;
            int PC_string_count = 9;
            int mobile_string_count = 9;
            int offer_string_count = 10;
            // Set Account Name
            sb.Append("# " + accountInfo.AccountName);
            int maxlength = GetMaxAccountStrLength();
            maxlength = maxlength - accountInfo.AccountName.Length;
            sb.Append(GetEmptyString(maxlength));
            sb.Append(" :");
            // Set Current RP 
            int temp = currentRP_string_count - accountInfo.Current_RP.ToString().Length;
            sb.Append(GetEmptyString(temp / 2));
            sb.Append(accountInfo.Current_RP);
            sb.Append(GetEmptyString(temp / 2));
            sb.Append(":");
            // Set PC
            temp = PC_string_count - accountInfo.GetPC_String().Length;
            sb.Append(GetEmptyString(temp / 2));
            sb.Append(accountInfo.GetPC_String());
            sb.Append(GetEmptyString(temp / 2));
            sb.Append(":");
            // Set Mobile
            temp = mobile_string_count - accountInfo.GetMobile_String().Length;
            sb.Append(GetEmptyString(temp / 2));
            sb.Append(accountInfo.GetMobile_String());
            sb.Append(GetEmptyString(temp / 2));
            sb.Append(":");
            // Set Offer
            temp = offer_string_count - accountInfo.GetOffer_String().Length;
            sb.Append(GetEmptyString(temp / 2));
            sb.Append(accountInfo.GetOffer_String());
            sb.Append(GetEmptyString(temp / 2));
            sb.Append(":");
            // Set Total
            sb.Append(GetEmptyString(2));
            sb.Append(accountInfo.GetTotal_String());

            Console.WriteLine(sb.ToString());
        }

        private int GetMaxAccountStrLength()
        {
            int maxlength = 0;
            foreach (var account in accounts)
            {
                if (account.Key.Length > maxlength)
                {
                    maxlength = account.Key.Length;
                }
            }
            return maxlength;
        }

        private string GetEmptyString(int length)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                sb.Append(" ");
            }
            return sb.ToString();
        }
    }
}
