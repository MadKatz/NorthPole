using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NorthPole.Utils;

namespace NorthPole.Helpers
{
    public class SignInHelper : SignInHelperBase
    {

        protected String emailBoxName = "loginfmt";
        protected String passwordBoxname = "passwd";
        protected String loginButtonID = "idSIButton9";
        protected String mobileSignInButtonClass = "white";

        public SignInHelper(IWebDriver driver, String email, String password, bool mobile)
            : base(driver, email, password, mobile)
        {
        }

        public override void SignIn()
        {
            if (!mobile)
                GoToLoginPageDesktop();
            else
                GoToLoginPageMobile();
            Login();
            BotUtils.Wait(defaultSleep);
        }

        public void GoToLoginPageDesktop()
        {
            var elements = driver.FindElements(By.ClassName("identityOption"));
            foreach (var item in elements)
            {
                if (item.Text.Contains("Microsoft account"))
                {
                    item.FindElement(By.TagName("a")).Click();
                    break;
                }
            }
        }

        public void GoToLoginPageMobile()
        {
            driver.FindElement(By.ClassName(mobileSignInButtonClass)).Click();
        }

        public void Login()
        {
            BotUtils.Wait(defaultSleep);
            IWebElement emailBox = driver.FindElement(By.Name(emailBoxName));
            emailBox.SendKeys(email);
            IWebElement passwordBox = driver.FindElement(By.Name(passwordBoxname));
            passwordBox.SendKeys(password);
            driver.FindElement(By.Id(loginButtonID)).Click();
        }
    }
}
