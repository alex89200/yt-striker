using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using YTStriker.Helpers;
using YTStriker.Model;

namespace YTStriker.ReportStrategies
{
    public class ReportChannelStrategy : ReportStrategyBase
    {
        public ReportChannelStrategy(CommandLineArguments args, ILogger logger) 
            : base(args, logger)
        { }

        public override async Task Process()
        {
            BrowserSession session = null;

            try
            {
                session = CreateSession(_args.Browser);

                string description = File.ReadAllText(_args.DescriptionFile);
                List<string> channels = GetChannelsList(session);

                // Process channels
                foreach (string channelUrlRaw in channels)
                {
                    string channelUrl = channelUrlRaw.EndsWith("/") ? channelUrlRaw : $"{channelUrlRaw}/";

                    Log($"Processing channel: {channelUrl}", session.Sid, false, ConsoleColor.DarkYellow);

                    try
                    {
                        Uri aboutUri = new Uri(channelUrl);
                        aboutUri = new Uri(aboutUri, "about");
                        session.Driver.Navigate().GoToUrl(aboutUri);

                        await Task.Delay(3000);

                        await ReportUserOpenDialog(session);
                        await ReportUserSelectComplaint(session, _args.MainComplaint, _args.SubComplaint);
                        await ReportUserSelectAbusiveVideos(session);
                        await ReportChannelSubmit(session, description);
                    }
                    catch (Exception e)
                    {
                        Log($"  [FAIL] {e.Message}", session.Sid, false, ConsoleColor.Red);
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


        private async Task ReportUserOpenDialog(BrowserSession session)
        {
            WebDriverWait wait = session.Wait;
            Log("  Trying to open a Report User dialog...", session.Sid, true);

            bool flagClicked = false;
            ReadOnlyCollection<IWebElement> buttons = wait.Until(p => p.FindElements(By.CssSelector(@"#action\-buttons button#button")));
            foreach (IWebElement but in buttons)
            {
                string label = but.GetAttribute("aria-label").ToLowerInvariant();
                if (StaticData.ReportUserNames.Contains(label))
                {
                    but.Click();
                    flagClicked = true;
                    break;
                }
            }

            if (!flagClicked)
                throw new InvalidOperationException("Can't find the Report User button");

            // Find and click on the Report User button
            bool reportClicked = false;
            ReadOnlyCollection<IWebElement> reportButtonCandidates =
                wait.Until(p => p.FindElements(By.CssSelector(@"#contentWrapper #items > *"))); //ytd\-menu\-service\-item\-renderer

            await Task.Delay(1000);

            // If there is only one option in the menu - it's definitely Report User - click it.
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

                        if (StaticData.ReportUserNames.Contains(text))
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

        private async Task ReportUserSelectComplaint(BrowserSession session, int optionIndex, int subOptionIndex)
        {
            WebDriverWait wait = session.Wait;
            Log("  Looking for possible complaint options...", session.Sid, true);

            // Parse possible report options
            wait.Until(p => p.FindElement(By.CssSelector(@"tp\-yt\-paper\-dialog #container #sections #yt\-options\-renderer\-options")));
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
            }
            else
            {
                Log("  Sub-option selection skipped", session.Sid, true);
            }

            // Click on Next
            IWebElement next = wait.Until(p => p.FindElement(By.CssSelector(@"ytd\-button\-renderer#next\-button")));
            await Task.Delay(1000);
            next.Click();
        }

        private async Task ReportUserSelectAbusiveVideos(BrowserSession session)
        {
            WebDriverWait wait = session.Wait;
            Log("  Don't select any videos, just proceed next", session.Sid, true);

            // Click on Next
            IWebElement next = wait.Until(p => p.FindElement(By.CssSelector(@"ytd\-button\-renderer#next\-button")));
            await Task.Delay(3000);
            next.Click();
        }

        private async Task ReportChannelSubmit(BrowserSession session, string description)
        {
            WebDriverWait wait = session.Wait;
            Log("  Filling description and submitting report...", session.Sid, true);

            // Enter description
            IWebElement textArea = wait.Until(p => p.FindElement(By.CssSelector(@"#labelAndInputContainer #textarea")));
            textArea.SendKeys(description);

            // Submit
            if (!_args.DryRun)
            {
                IWebElement submit = wait.Until(p => p.FindElement(By.CssSelector(@"#footer #next\-button")));
                await Task.Delay(2000);
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
