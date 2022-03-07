using System;
using System.Threading.Tasks;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using YTStriker.Helpers;
using YTStriker.Model;

namespace YTStriker.ReportStrategies
{
    public abstract class ReportStrategyBase : IReportStrategy
    {
        protected readonly CommandLineArguments _args;
        private readonly ILogger _logger;

        protected ReportStrategyBase(CommandLineArguments args, ILogger logger)
        {
            _args = args;
            _logger = logger;
        }

        protected BrowserSession CreateSession(WebBrowser browser)
        {
            BrowserSession session;

            switch (browser)
            {
                case WebBrowser.chrome:
                {
                    ChromeOptions options = new ChromeOptions();
                    options.AddArgument($"user-data-dir={Environment.GetEnvironmentVariable("LocalAppData")}\\Google\\Chrome\\User Data");
                    session = new BrowserSession(new ChromeDriver(options), _args.Timeout);
                    break;
                }

                case WebBrowser.firefox:
                {
                    FirefoxOptions options = new FirefoxOptions();
                    FirefoxProfileManager profileManager = new FirefoxProfileManager();

                    foreach (string profileName in profileManager.ExistingProfiles)
                    {
                        if (profileName.Contains("default"))
                        {
                            FirefoxProfile profile = profileManager.GetProfile(profileName);
                            options.Profile = profile;
                        }
                    }

                    session = new BrowserSession(new FirefoxDriver(options), _args.Timeout);
                    break;
                }

                case WebBrowser.edge:
                {
                    EdgeOptions options = new EdgeOptions();
                    options.AddArguments($"user-data-dir={Environment.GetEnvironmentVariable("LocalAppData")}\\Microsoft\\Edge\\User Data");
                    options.AddArguments("profile-directory=Default");
                    session = new BrowserSession(new EdgeDriver(options), _args.Timeout);
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(browser), browser, "Unknown browser");
            }

            
            Log("Session created.", session.Sid);

            return session;
        }

        protected void CloseSession(BrowserSession session)
        {
            session?.Driver?.Quit();

            Log("Session closed.", session?.Sid ?? -1);
        }

        public abstract Task Process();

        protected void Log(string message, int sid = -1, bool verbose = false)
        {
            _logger.Log(sid > -1 ? $"[SID: {sid}] {message}" : message, verbose);
        }
    }
}
