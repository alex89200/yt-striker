using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using OpenQA.Selenium;
using YTStriker.Helpers;
using YTStriker.Model;
using OpenQA.Selenium.Support.UI;

namespace YTStriker.ReportStrategies
{
    public class LoginStrategy : ReportStrategyBase
    {
        public LoginStrategy(CommandLineArguments args, ILogger logger) : base(args, logger)
        { }

        public override async Task Process()
        {
            BrowserSession session = null;

            try
            {
                session = CreateSession(_args.Browser);

                Log("Openning YouTube main page", session.Sid);

                session.Driver.Navigate().GoToUrl("https://www.youtube.com");
                await Task.Delay(3000);

                // Early exit if signed in
                if (IsSignedIn(session))
                {
                    Log("Already signed in", session.Sid, false, ConsoleColor.DarkYellow);
                    return;
                }

                Log("Looking for Login button", session.Sid, true);
                bool success = false;
                ReadOnlyCollection<IWebElement> loginCandidates = session.Wait.Until(p => p.FindElements(By.CssSelector("#buttons a")));
                foreach (IWebElement a in loginCandidates)
                {
                    string href = a.GetAttribute("href");
                    if (href?.Contains("accounts.google.com") == true)
                    {
                        session.Driver.Navigate().GoToUrl(href);

                        Log("Waiting for signin. You have 5 minutes to login to YouTube account.", session.Sid);

                        // User has 5 minutes to sign in
                        WebDriverWait loginWait = new WebDriverWait(session.Driver, TimeSpan.FromMinutes(5));
                        loginWait.Until(p => p.FindElement(By.CssSelector(@"#avatar\-btn")));
                        success = true;

                        // Wait before exit just in case
                        await Task.Delay(3000);
                        break;
                    }
                }

                if (success)
                {
                    Log("Success", session.Sid, false, ConsoleColor.DarkGreen);
                }
                else
                {
                    Log("Can't sign in. Something went wrong", session.Sid, false, ConsoleColor.Red);
                }
            }
            catch (Exception e)
            {
                Log($"ERROR: {e.Message}", session?.Sid ?? -1, false, ConsoleColor.Red);
            }
            finally
            {
                CloseSession(session);
            }
        }

        private bool IsSignedIn(BrowserSession session)
        {
            try
            {
                var avatarCheckWait = new WebDriverWait(session.Driver, TimeSpan.FromSeconds(10));
                avatarCheckWait.Until(p => p.FindElement(By.CssSelector(@"#avatar\-btn")));

                return true;

            }
            catch (WebDriverTimeoutException) 
            {
                return false;
            }
            catch (NotFoundException)
            {
                return false;
            }
        }
    }
}
