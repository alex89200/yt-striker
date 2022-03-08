using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using YTStriker.Helpers;
using YTStriker.Model;

namespace YTStriker.ReportStrategies
{
    public class ReportVideosStrategy : ReportStrategyBase
    {
        public ReportVideosStrategy(CommandLineArguments args, ILogger logger)
            : base(args, logger)
        { }

        public override async Task Process()
        {
            BrowserSession session = null;

            try
            {
                session =  CreateSession(_args.Browser);

                List<string> channels = GetChannelsList(session);

                foreach (string channelName in channels)
                {
                    ICollection<string> videos = await GetVideosUrls(session, channelName, _args.Limit);
                    Log($"Videos to process: {videos.Count}", session.Sid);

                    #region Verbose log videos

                    if (_args.Verbose)
                    {
                        foreach (string url in videos)
                        {
                            Log($"  {url}", session.Sid, true);
                        }
                    }

                    #endregion

                    Log("--------------------", session.Sid);

                    string description = File.ReadAllText(_args.DescriptionFile);

                    // Report videos
                    foreach (string url in videos)
                    {
                        try
                        {
                            Log($"Processing: {url}", session.Sid, false, ConsoleColor.DarkYellow);
                            session.Driver.Navigate().GoToUrl(url);
                            await Task.Delay(1000);

                            ReportVideoOpenDialog(session);
                            await ReportVideoChooseComplaint(session, _args.MainComplaint, _args.SubComplaint);
                            ReportVideoSubmit(session, description);

                            await Task.Delay(2000);
                        }
                        catch (Exception e)
                        {
                            Log($"  [FAIL] {e.Message}", session.Sid, false, ConsoleColor.Red);
                        }
                    }
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

        private async Task<ICollection<string>> GetVideosUrls(BrowserSession session, string channelName, int limit)
        {
            HashSet<string> result = new HashSet<string>();

            Uri videosUri = new Uri($"https://www.youtube.com/c/{channelName}/videos");
            session.Driver.Navigate().GoToUrl(videosUri);
            await Task.Delay(2000);

            while (result.Count < limit)
            {
                int curCount = result.Count;

                // Find loaded videos on the page
                ReadOnlyCollection<IWebElement> videoTitles = session.Driver.FindElements(By.Id("video-title"));

                // Fill results taking limit in consideration
                int count = Math.Min(videoTitles.Count, limit);
                for (int i = 0; i < count; i++)
                {
                    string url = videoTitles[i].GetAttribute("href");
                    result.Add(url);
                }

                // Exit if no additional videos are loaded or we hit a limit
                // Scroll down to the last element in order to load more videos otherwise
                if (result.Count != curCount && result.Count < limit)
                {
                    Actions scroll = new Actions(session.Driver);
                    scroll.MoveToElement(session.Driver.FindElement(By.Id("footer")));
                    scroll.Perform();
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        private void ReportVideoOpenDialog(BrowserSession session)
        {
            WebDriverWait wait = session.Wait;
            Log("  Trying to open a Report dialog...", session.Sid, true);

            // Click on "..." under the video
            IWebElement button = wait.Until(p => p.FindElement(By.CssSelector(@"#info #menu #button.dropdown\-trigger")));
            button.Click();

            // Find and click on the Report button
            bool reportClicked = false;
            ReadOnlyCollection<IWebElement> reportButtonCandidates =
                wait.Until(p => p.FindElements(By.CssSelector(@"#items > *"))); //ytd\-menu\-service\-item\-renderer

            // If there is only one option in the menu - it's definitely Report - click it.
            // Make a full search of the Report button otherwise
            if (reportButtonCandidates.Count == 1)
            {
                reportButtonCandidates[0].Click();
            }
            else
            {
                foreach (IWebElement elem in reportButtonCandidates)
                {
                    try
                    {
                        string text = elem.FindElement(By.CssSelector(@"tp\-yt\-paper\-item > yt\-formatted\-string"))?.Text.ToLowerInvariant();

                        if (StaticData.ReportNames.Contains(text))
                        {
                            elem.Click();
                            reportClicked = true;
                            break;
                        }
                    }
                    catch (NoSuchElementException)
                    { }
                }

                if (!reportClicked)
                    throw new InvalidOperationException("Can't open a Report Dialog");
            }
        }

        private async Task ReportVideoChooseComplaint(BrowserSession session, int optionIndex, int subOptionIndex)
        {
            WebDriverWait wait = session.Wait;
            Log("  Looking for possible complaint options...", session.Sid, true);

            // Parse possible report options
            wait.Until(p => p.FindElement(By.CssSelector(@"tp\-yt\-paper\-dialog\-scrollable#scroller")));
            ReadOnlyCollection<IWebElement> options = wait.Until(p => p.FindElements(By.CssSelector(@"#yt\-options\-renderer\-options > *")));

            List<KeyValuePair<int, int>> optToSubMap = new List<KeyValuePair<int, int>>();
            int lastOptionIndex = -1;
            for (int i = 0; i < options.Count; i++)
            {
                IWebElement elem = options[i];
                switch (elem.TagName)
                {
                    case "tp-yt-paper-radio-button":
                    {
                        // If we have a previously saved index - add it now. It doesn't have related sub-options.
                        if (lastOptionIndex > -1)
                        {
                            optToSubMap.Add(new KeyValuePair<int, int>(lastOptionIndex, -1));
                        }

                        // If this is the last element - add it immediately instead of saving its index
                        // for next iterations, because this is the last iteration.
                        if (i == options.Count - 1)
                        {
                            optToSubMap.Add(new KeyValuePair<int, int>(i, -1));
                        }

                        // Remember index for next iterations to check if there are related sub-options
                        lastOptionIndex = i;
                        break;
                    }

                    case "tp-yt-paper-dropdown-menu":
                    {
                        // Add saved option with current sub-options
                        optToSubMap.Add(new KeyValuePair<int, int>(lastOptionIndex, i));
                        lastOptionIndex = -1;
                        break;
                    }
                }
            }

            Log($"  Options found. Selecting option: {optionIndex}...",  session.Sid, true);

            // Select needed option
            KeyValuePair<int, int> curOptionMap = optToSubMap[optionIndex];
            IWebElement selectedOption = options[curOptionMap.Key];
            selectedOption.Click();
            await Task.Delay(1000);

            // If sub-option dropdown exists
            if (curOptionMap.Value > -1)
            {
                Log($"  Selecting sub-option: {subOptionIndex}...", session.Sid, true);

                // Click it
                wait.Until(p => options[curOptionMap.Value].Displayed);
                IWebElement dropDown = options[curOptionMap.Value];
                dropDown.Click();

                // Parse sub-options
                ReadOnlyCollection<IWebElement> subOpt = wait.Until(p =>
                    dropDown.FindElements(By.CssSelector(@"tp\-yt\-paper\-listbox tp\-yt\-paper\-item")));

                // Click needed sub-option. The first element is the dropdown-trigger itself - skip it.
                subOpt[subOptionIndex + 1].Click();
                await Task.Delay(1000);
            }
            else
            {
                Log("  Sub-option selection skipped", session.Sid, true);
            }

            // Click on Next
            IWebElement next = wait.Until(p => p.FindElement(By.CssSelector(@"yt\-button\-renderer#submit\-button")));
            next.Click();
        }

        private void ReportVideoSubmit(BrowserSession session, string description)
        {
            WebDriverWait wait = session.Wait;
            Log("  Filling description and submitting report...", session.Sid, true);

            // Enter description
            IWebElement textArea = wait.Until(p => p.FindElement(By.CssSelector(@"#description\-text #textarea")));
            textArea.SendKeys(description);

            if (!_args.DryRun)
            {
                // Submit
                IWebElement submit = wait.Until(p => p.FindElement(By.CssSelector(@"#buttons #submit\-button")));
                submit.Click();

                wait.Until(p => p.FindElement(By.CssSelector(@"yt\-confirm\-dialog\-renderer #main")));

                Log("  [OK] Report sent!", session.Sid, true, ConsoleColor.DarkGreen);
            }
            else
            {
                Log("  [OK] Dry run. Skip sending report.", session.Sid, true, ConsoleColor.DarkGreen);
            }
        }
    }
}