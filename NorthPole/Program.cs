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
            string input = "-1";
            bool valid_input = false;
            Console.WriteLine("NorthPole Bingbot v1");
            Console.WriteLine("Enter 1 to start, 0 to exit.");
            while (!valid_input)
            {
                input = Console.ReadLine();
                valid_input = CheckInput(input);
                if (!valid_input)
                {
                    Console.WriteLine("Incorrect input.");
                }
            }
            if (input == "0")
            {
                Environment.Exit(0);
            }
            else if (input == "1")
            {
                //TestBot bot = new TestBot();
                BingBot bot = new BingBot();
                bot.StartBot();
                Console.WriteLine("Searching complete.");
                Console.ReadLine();
            }
        }

        private static bool CheckInput(string input)
        {
            if (input == "0" || input == "1")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
