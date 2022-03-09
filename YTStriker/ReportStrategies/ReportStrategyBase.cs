﻿using System;
using System.Collections.Generic;
using System.IO;
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
                        //options.AddArgument($"user-data-dir={Environment.GetEnvironmentVariable("LocalAppData")}\\Google\\Chrome\\User Data");
                        //options.AddArgument($"user-data-dir={Environment.GetFolderPath(Environment.SpecialFolder.Personal)}/Library/Application Support/Google/Chrome/");
                        options.AddArgument($"user-data-dir=./Profiles/Chrome/");
                        options.AddArguments("profile-directory=Default");
                        session = new BrowserSession(new ChromeDriver(options), _args.Timeout);
                        break;
                    }

                case WebBrowser.firefox:
                    {
                        FirefoxOptions options = new FirefoxOptions();
                        options.AddArguments($"user-data-dir=./Profiles/Firefox/");
                        options.AddArguments("profile-directory=Default");

                        /*
                        FirefoxProfileManager profileManager = new FirefoxProfileManager();

                        foreach (string profileName in profileManager.ExistingProfiles)
                        {
                            if (profileName.Contains("default"))
                            {
                                FirefoxProfile profile = profileManager.GetProfile(profileName);
                                options.Profile = profile;
                            }
                        }
                        */

                        session = new BrowserSession(new FirefoxDriver(options), _args.Timeout);
                        break;
                    }

                case WebBrowser.edge:
                    {
                        EdgeOptions options = new EdgeOptions();
                        //options.AddArguments($"user-data-dir={Environment.GetEnvironmentVariable("LocalAppData")}\\Microsoft\\Edge\\User Data");
                        options.AddArguments($"user-data-dir=./Profiles/Edge/");
                        options.AddArguments("profile-directory=Default");
                        session = new BrowserSession(new EdgeDriver(options), _args.Timeout);
                        break;
                    }

                case WebBrowser.opera:
                    {
#if PLATFORM_WINDOWS
                        OperaOptions options = new OperaOptions();
                        OperaDriverService service = OperaDriverService.CreateDefaultService("./", "operadriver");
                        options.AddArgument($"user-data-dir=./Profiles/Opera/");
                        options.AddArguments("profile-directory=Default");
                    


                        session = new BrowserSession(new OperaDriver(service, options), _args.Timeout);
                        break;              
#else
                        throw new InvalidOperationException("Opera is not supported on this platform");
#endif
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(browser), browser, "Unknown browser");
            }

            session.SetPageLoadTimeout(TimeSpan.FromMinutes(2));

            Log("Session created.", session.Sid);

            return session;
        }

        protected void CloseSession(BrowserSession session)
        {
            session?.Driver?.Quit();

            Log("Session closed.", session?.Sid ?? -1);
        }

        public abstract Task Process();

        protected List<string> GetChannelsList(BrowserSession session)
        {
            List<string> result = new List<string>();

            // Fill the channels list
            if (string.IsNullOrEmpty(_args.ChannelName) == false)
            {
                result.Add(_args.ChannelName);
            }
            else
            {
                result.AddRange(File.ReadLines(_args.InputFile));
            }

            Log($"Channels to process: {result.Count}", session.Sid);

            #region Verbose log channels
            if (_args.Verbose)
            {
                foreach (string channel in result)
                {
                    Log($"  {channel}", session.Sid, true);
                }
            }
            #endregion

            Log("-----------------", session.Sid);

            return result;
        }

        protected void Log(string message, int sid = -1, bool verbose = false, ConsoleColor color = ConsoleColor.Gray)
        {
            Console.ForegroundColor = color;
            _logger.Log(sid > -1 ? $"[SID: {sid}] {message}" : message, verbose);
            Console.ResetColor();
        }
    }
}
