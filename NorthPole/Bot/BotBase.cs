using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthPole.Bot
{
    public abstract class BotBase
    {
        protected IWebDriver driver;

        public BotBase()
        {
        }

        public abstract void Setup();
        public abstract void TearDown();
        public abstract void StartBot();
    }
}
