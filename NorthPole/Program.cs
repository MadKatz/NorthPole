using System;
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
                if (args[0] == "-a")
                {
                    ExecuteAccountFile();
                }
                else if (args[0] == "-s")
                {
                    if (args.Count() < 3)
                    {
                        Console.WriteLine("Not enough arguments.");
                        DisplayUsage();
                        Environment.Exit(0);
                    }
                    else
                    {
                        Controller controller = new Controller();
                        controller.Start(true, args);
                    }
                }
                else
                {
                    DisplayUsage();
                    Environment.Exit(0);
                }
            }
            else
            {
                //move to displayutils?
                string input = "-1";
                bool valid_input = false;
                Console.WriteLine("Project NorthPole: Bingbot " + Constants.VERSION);
                Console.WriteLine("Usage:");
                Console.WriteLine("Enter '1' to execute a single account.");
                Console.WriteLine("Enter '2' to all accounts from account file.");
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
                ConsoleKeyInfo key = Console.ReadKey();
                Environment.Exit(0);
            }
        }

        private static bool CheckInput(string input)
        {
            if (input == "1" || input == "2" || input == "3")
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
            String[] args = new String[3];
            Console.Write("Enter email: ");
            args[1] = Console.ReadLine();
            args[2] = GetPasswordAndDisableClearText();
            Controller controller = new Controller();
            controller.Start(true, args);
        }

        private static void ExecuteAccountFile()
        {
            Controller controller = new Controller();
            controller.Start(false, new String[3]);
        }

        private static void DisplayUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("'-a' to execute all accounts from account file.");
            Console.WriteLine("'-s [email] [password]' to execute single account with given email and password.");
        }

        private static String GetPasswordAndDisableClearText()
        {
            StringBuilder password = new StringBuilder();
            Console.Write("Enter password: ");
            ConsoleKeyInfo keyInfo;

            do
            {
                keyInfo = Console.ReadKey(true);
                // Backspace Should Not Work
                if (keyInfo.Key != ConsoleKey.Backspace && keyInfo.Key != ConsoleKey.Enter)
                {
                    password.Append(keyInfo.KeyChar);
                    Console.Write("*");
                }
                else
                {
                    if (keyInfo.Key == ConsoleKey.Backspace && password.Length > 0)
                    {
                        password = password.Remove(password.Length - 1, 1);
                        Console.Write("\b \b");
                    }
                }
            }
            // Stops Receving Keys Once Enter is Pressed
            while (keyInfo.Key != ConsoleKey.Enter);
            Console.WriteLine();
            Console.WriteLine("Your password is " + password.ToString());
            return password.ToString();
        }
    }
}
