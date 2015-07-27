using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace NorthPole.Helpers
{
    public abstract class SignInHelperBase
    {
        protected IWebDriver driver;
        protected String email;
        protected String password;
        protected bool mobile;
        protected int defaultSleep = 3000;

        public SignInHelperBase(IWebDriver driver, String email, String password, bool mobile)
        {
            this.driver = driver;
            this.email = email;
            this.password = password;
            this.mobile = mobile;
        }

        public abstract void SignIn();
    }
}
