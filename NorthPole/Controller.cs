using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NorthPole.Utils;
using NorthPole.Account;
using NorthPole.Bot;
using System.Diagnostics;

namespace NorthPole
{
    public class Controller
    {
        private Random random;
        private const int THREE_MINS = 60000 * 3;

        public Controller()
        {
            random = new Random();
        }
        public void Start(bool singleaccount, String[] args)
        {
            List<string> searchList = null;
            try
            {
                searchList = LoadWordSearchFile(Constants.WORDLISTFILE);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Console.WriteLine("Failed to load account file. Aborting.");
                throw e;
            }
            if (singleaccount)
            {
                Dictionary<string, string> accountList = new Dictionary<string, string>();
                accountList.Add(args[1], args[2]);
                ExecuteAccounts(accountList, searchList);
            }
            else
            {
                Dictionary<string, string> accounts = null;
                try
                {
                    accounts = LoadAccounts(Constants.ACCOUNTFILE);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    Console.WriteLine("Failed to load account file. Aborting.");
                    throw e;
                }
                ExecuteAccounts(accounts, searchList);
            }
        }
        public void ExecuteAccounts(Dictionary<string, string> accountList, List<string> searchList)
        {
            int numAccounts = accountList.Count;
            Console.WriteLine("Executing Desktop + Mobile Search on " + numAccounts + " acounts." + "\n");
            int count = 0;
            foreach (var account in accountList)
            {
                AccountContext aContext = new AccountContext(account.Key, account.Value);
                Execute(aContext, searchList);
                Console.Write(DisplayUtils.GetEndStatusString(aContext));
                count++;
                if (count != accountList.Count)
                {
                    Console.WriteLine("Sleeping for 3 minutes before starting next account. \n");
                    BotUtils.Wait(THREE_MINS);
                }
            }
        }

        private void Execute(AccountContext accountContext, List<string> searchList)
        {
            DesktopBot db = new DesktopBot(accountContext, searchList, random);
            MobileBot mb = new MobileBot(accountContext, searchList, random);
            try
            {
                db.StartBot();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error occurred while executing Desktopbot.");
                Console.WriteLine(e.Message);
            }
            bool mobilesearch_LessThenMax = accountContext.AccountCredits.MobileSearchCredits < accountContext.AccountCredits.MobileSearchMaxCredits ? true : false;
            bool invalidCredits = accountContext.AccountCredits.MobileSearchCredits == -1 ? true : false;
            if (invalidCredits == false)
            {
                invalidCredits = accountContext.AccountCredits.MobileSearchMaxCredits == -1 ? true : false;
            }

            if (mobilesearch_LessThenMax || invalidCredits)
            {
                try
                {
                    mb.StartBot();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error occurred while executing Mobilebot.");
                    Console.WriteLine(e.Message);
                }
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
            return result;
        }

        private List<string> LoadWordSearchFile(string path)
        {
            string[] temp = File.ReadAllLines(path);
            return new List<string>(temp);
        }
    }
}
