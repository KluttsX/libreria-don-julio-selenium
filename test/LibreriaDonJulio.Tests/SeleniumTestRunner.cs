using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace LibreriaDonJulio.Tests
{
    public sealed class SeleniumTestRunner(
      string baseUrl,
      string resultsDirectory,
      bool headed)
    {
        private readonly List<ScenarioResult> _results = [];
        private IWebDriver _driver = null!;

        private const string DemoUsername = "Tuco";
        private const string DemoPassword = "thebest2026";

        public IReadOnlyList<ScenarioResult> Run()
        {
            Directory.CreateDirectory(resultsDirectory);

            var options = new ChromeOptions();

            if (!headed)
            {
                options.AddArgument("--headless=new");
            }

            options.AddArgument("--window-size=1440,1000");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--disable-search-engine-choice-screen");

            using (_driver = new ChromeDriver(options))
            {
                _driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;

                RunScenario(
                  "Successful login",
                  "LDJ-2 / LDJ-7",
                  "LDJ-7-login-valid",
                  LoginSucceeds);
            }

            return _results;

        }

        private void LoginSucceeds()
        {
            _driver.Navigate().GoToUrl($"{baseUrl}/Account/Login");

            _driver.FindElement(By.Id("Username"))
                .SendKeys(DemoUsername);

            _driver.FindElement(By.Id("Password"))
                .SendKeys(DemoPassword);

            _driver.FindElement(By.CssSelector("button[type='submit']"))
                .Click();

            var wait = new WebDriverWait(
                _driver, TimeSpan.FromSeconds(10));

            wait.Until(driver =>
                driver.FindElements(
                    By.CssSelector("a[href='/Books']"))
                    .Any(element => element.Displayed));

            Assert.That(
                _driver.FindElements(
                    By.CssSelector("form[action='/Account/Logout']"))
                    .Any(element => element.Displayed),
                Is.True,
                "The user could not sign in.");
        }

        private void RunScenario(
        string name,
        string userStory,
        string screenshotName,
        Action scenario)
        {
            var stopwatch = Stopwatch.StartNew();
            bool passed = false;
            string details;

            try
            {
                scenario();

                passed = true;

                details = "Scenario completed successfully.";
            }
            catch (Exception ex)
            {
                details = ex.Message;
            }

            stopwatch.Stop();

            var screenshotFile = $"{screenshotName}.png";
            var screenshotPath = Path.Combine(resultsDirectory, screenshotFile);

            try
            {
                ((ITakesScreenshot)_driver)
                .GetScreenshot()
                .SaveAsFile(screenshotPath);
            }
            catch (Exception ex)
            {
                passed = false;
                details += $" Screenshot error: {ex.Message}";
                screenshotFile = string.Empty;
            }

            _results.Add(new ScenarioResult(
            name,
            userStory,
            passed,
            stopwatch.Elapsed,
            details,
            screenshotFile));
        }


    }
}
