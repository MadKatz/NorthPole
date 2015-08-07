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
                return;
            }
            if (singleaccount)
            {
                Execute(new AccountContext(args[1], args[2]), searchList);
            }
            else
            {
                try
                {
                    Dictionary<string, string> accounts = LoadAccounts(Constants.ACCOUNTFILE);
                    ExecuteAccounts(accounts, searchList);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    Console.WriteLine("Failed to load account file. Aborting.");
                }
            }
        }
        public void ExecuteAccounts(Dictionary<string, string> accountList, List<string> searchList)
        {
            Console.WriteLine("Executing Desktop Search on all acounts.");
            foreach (var account in accountList)
            {
                Execute(new AccountContext(account.Key, account.Value), searchList);
                Console.WriteLine("Sleeping for 3mins.");
                BotUtils.Wait(60000 * 3);
            }
        }

        private void Execute(AccountContext accountContext, List<string> searchList)
        {
            DesktopBot db = new DesktopBot(accountContext, searchList, random);
            try
            {
                db.StartBot();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Console.WriteLine("Error occurred while executing Desktopbot.");
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
