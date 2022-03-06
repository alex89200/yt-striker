using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace YTStriker.Model
{
    public class BrowserSession
    {
        private static int _sessionId;

        public int Sid { get; }
        public WebDriver Driver { get; }
        public WebDriverWait Wait { get; }

        public BrowserSession(WebDriver driver, int timeout)
        {
            Driver = driver;
            Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeout));
            Sid = _sessionId++;
        }
    }
}