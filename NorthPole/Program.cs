﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthPole
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Count() > 0)
            {
                for (int i = 0; i < args.Count(); i++)
                {
                    if (args[i] == "-a")
                    {
                        ExecuteAccountFile();
                    }
                    else if (args[i] == "-s")
                    {
                        if (args.Count() < 3)
                        {
                            Console.WriteLine("Not enough arguments.");
                            Environment.Exit(0);
                        }
                        else
                        {
                            BotManager bm = new BotManager();
                            bm.SetUp();
                            bm.ExecuteAccount(args[1], args[2]);
                        }
                    }
                }
            }
            else
            {
                string input = "-1";
                bool valid_input = false;
                Console.WriteLine("Project NorthPole: Bingbot v1.0");
                Console.WriteLine("Usage:");
                Console.WriteLine("Enter '1' to execute a single account.");
                Console.WriteLine("Enter '2' to all accounts from accountfile.");
                Console.WriteLine("Enter '3' to quit.");
                Console.Write("Input: ");
                while (!valid_input)
                {
                    input = Console.ReadLine();
                    valid_input = CheckInput(input);
                    if (!valid_input)
                    {
                        Console.WriteLine("Incorrect input.");
                        Console.Write("Input: ");
                    }
                }
                if (input == "3")
                {
                    Environment.Exit(0);
                }
                else if (input == "1")
                {
                    ExecuteSingleAccount();
                }
                else if (input == "2")
                {
                    ExecuteAccountFile();
                }

                Console.WriteLine("Program complete.");
                Console.WriteLine("Press any key to quit.");
                while ((input = Console.ReadLine()) != null)
                {

                }
            }
        }

        private static bool CheckInput(string input)
        {
            if (input == "0" || input == "1" || input == "3")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void ExecuteSingleAccount()
        {
            string email;
            string password;
            Console.Write("Enter email: ");
            email = Console.ReadLine();
            Console.Write("Enter password: ");
            password = Console.ReadLine();
            BotManager bm = new BotManager();
            bm.SetUp();
            bm.ExecuteAccount(email, password);
        }

        private static void ExecuteAccountFile()
        {
            BotManager bm = new BotManager();
            bm.SetUp();
            bm.Start();
        }
    }
}
