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

                RunScenario(
                  "Login with invalid credentials",
                  "LDJ-2 / LDJ-8",
                  "LDJ-8-login-invalid",
                  LoginFailsWithInvalidCredentials);

                RunScenario(
                  "Login with empty fields",
                  "LDJ-2 / LDJ-9",
                  "LDJ-9-login-empty",
                  LoginFailsWithEmptyFields);

                RunScenario(
                   "List books with empty catalog",
                   "LDJ-4 / LDJ-15",
                   "LDJ-15-list-books-empty",
                   ListBooksSucceedsWithEmptyCatalog);

                RunScenario(
                  "Create book with valid data",
                  "LDJ-3 / LDJ-10",
                  "LDJ-10-create-book-valid",
                  CreateBookSucceeds);

                RunScenario(
                   "Create book with negative values",
                    "LDJ-3 / LDJ-11",
                    "LDJ-11-create-book-invalid",
                    CreateBookFailsWithNegativeValues);

                RunScenario(
                   "Create book with zero values",
                   "LDJ-3 / LDJ-12",
                   "LDJ-12-create-book-boundary",
                   CreateBookSucceedsWithZeroValues);

                RunScenario(
                   "List registered books",
                   "LDJ-4 / LDJ-13",
                   "LDJ-13-list-books-valid",
                   ListBooksSucceeds);

                RunScenario(
                  "List books without authentication",
                  "LDJ-4 / LDJ-14",
                  "LDJ-14-list-books-unauthorized",
                  ListBooksFailsWithoutAuthentication);
            }

            return _results
            .OrderBy(result => int.Parse(result.UserStory.Split('-').Last()))
            .ToList();

        }

        #region TEST LOGIN
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

        private void LoginFailsWithInvalidCredentials()
        {
            _driver.Manage().Cookies.DeleteAllCookies();

            _driver.Navigate().GoToUrl($"{baseUrl}/Account/Login");

            _driver.FindElement(By.Id("Username"))
                .SendKeys(DemoUsername);

            _driver.FindElement(By.Id("Password"))
                .SendKeys("password123");

            _driver.FindElement(By.CssSelector("button[type='submit']"))
            .Click();

            var wait = new WebDriverWait(
                _driver, TimeSpan.FromSeconds(10));

            wait.Until(driver =>
                driver.FindElements(
                    By.CssSelector(".validation-summary-errors"))
                    .Any(element => element.Displayed));

            var errorMessage = _driver.FindElement(
                By.CssSelector(".validation-summary-errors"))
                .Text;

            Assert.That(errorMessage,
            Does.Contain("Invalid username or password."));

            Assert.That(
            _driver.Url,
            Does.Contain("/Account/Login"));

            Assert.That(
            _driver.FindElements(
            By.CssSelector("form[action='/Account/Logout']")),
            Is.Empty);
        }

        private void LoginFailsWithEmptyFields()
        {
            _driver.Manage().Cookies.DeleteAllCookies();

            _driver.Navigate().GoToUrl($"{baseUrl}/Account/Login");

            _driver.FindElement(By.Id("Username")).Clear();
            _driver.FindElement(By.Id("Password")).Clear();

            _driver.FindElement(By.CssSelector("button[type='submit']"))
                .Click();

            var wait = new WebDriverWait(
                _driver, TimeSpan.FromSeconds(10));

            wait.Until(driver =>
                driver.FindElements(By.CssSelector(".field-validation-error"))
                    .Count(element => element.Displayed) == 2);

            var usernameError = _driver.FindElement(
                By.CssSelector("span[data-valmsg-for='Username']")).Text;

            var passwordError = _driver.FindElement(
                By.CssSelector("span[data-valmsg-for='Password']")).Text;

            Assert.That(usernameError,
                Does.Contain("Username is required."));

            Assert.That(passwordError,
                Does.Contain("Password is required."));

            Assert.That(
                _driver.Url,
                Does.Contain("/Account/Login"));

            Assert.That(
                _driver.FindElements(
                    By.CssSelector("form[action='/Account/Logout']")),
                Is.Empty);
        }
        #endregion

        #region TEST CREATE BOOKS
        private void CreateBookSucceeds()
        {
            LoginAsAdmin();

            _driver.Navigate().GoToUrl($"{baseUrl}/Books/Create");

            _driver.FindElement(By.Id("Title"))
                .SendKeys("El Alquimista");

            _driver.FindElement(By.Id("Author"))
                .SendKeys("Paulo Coelho");

            _driver.FindElement(By.Id("Description"))
                .SendKeys("Esta es la historia de Santiago, un joven pastor andaluz que viaja desde su tierra natal hacia el desierto egipcio en busca de un tesoro oculto en las pirámides. Nadie sabe lo que contiene el tesoro, ni si Santiago será capaz de superar los obstáculos del camino. Pero lo que comienza como un viaje en busca de riquezas se convierte en un descubrimiento del tesoro interior.");

            _driver.FindElement(By.Id("CoverUrl"))
                .SendKeys("https://m.media-amazon.com/images/I/41u2QqDJuNL._SY445_SX342_ML2_.jpg");

            _driver.FindElement(By.Id("Isbn"))
                .SendKeys("9788408144755");

            _driver.FindElement(By.Id("Price")).Clear();
            _driver.FindElement(By.Id("Price")).SendKeys("1076");

            _driver.FindElement(By.Id("Stock")).Clear();
            _driver.FindElement(By.Id("Stock")).SendKeys("30");

            _driver.FindElement(
                By.CssSelector("form[action='/Books/Create'] button[type='submit']"))
                .Click();

            var wait = new WebDriverWait(
                _driver, TimeSpan.FromSeconds(10));

            wait.Until(driver =>
                driver.FindElements(By.CssSelector(".alert-success"))
                    .Any(element => element.Displayed)
                || driver.Url.TrimEnd('/') == $"{baseUrl}/Books"
                || driver.Url.TrimEnd('/') == $"{baseUrl}/Books/Index");

            _driver.Navigate().GoToUrl($"{baseUrl}/Books");

            var row = wait.Until(driver =>
                driver.FindElements(By.CssSelector("table tbody tr"))
                    .FirstOrDefault(element =>
                        element.Text.Contains("9788408144755")));

            var cells = row.FindElements(By.CssSelector("td"));

            Assert.That(cells[1].Text, Is.EqualTo("El Alquimista"));
            Assert.That(cells[2].Text, Is.EqualTo("Paulo Coelho"));
            Assert.That(cells[3].Text, Is.EqualTo(
                "Esta es la historia de Santiago, un joven pastor andaluz que viaja desde su tierra natal hacia el desierto egipcio en busca de un tesoro oculto en las pirámides. Nadie sabe lo que contiene el tesoro, ni si Santiago será capaz de superar los obstáculos del camino. Pero lo que comienza como un viaje en busca de riquezas se convierte en un descubrimiento del tesoro interior."));
            Assert.That(cells[4].Text, Is.EqualTo("9788408144755"));
            Assert.That(cells[5].Text, Is.EqualTo("1076.00"));
            Assert.That(cells[6].Text, Is.EqualTo("30"));
        }

        private void CreateBookFailsWithNegativeValues()
        {
            LoginAsAdmin();

            _driver.Navigate().GoToUrl($"{baseUrl}/Books/Create");

            _driver.FindElement(By.Id("Title"))
                .SendKeys("La Quinta Montaña");

            _driver.FindElement(By.Id("Author"))
                .SendKeys("Paulo Coelho");

            _driver.FindElement(By.Id("Description"))
                .SendKeys("La Quinta Montaña cuenta la historia del profeta Elías, un hombre que recibe de Dios la orden de abandonar Israel. En un mundo regido por supersticiones, conflictos religiosos y tradiciones profundamente arraigadas, el joven profeta deberá enfrentarse a una avalancha de acontecimientos que le conducirán a un definitivo cara a cara con Dios. La historia del profeta Elías se convierte así en una valiosa lección de esperanza para el hombre contemporáneo, y nos plantea hasta qué punto podemos determinar nuestro destino.");

            _driver.FindElement(By.Id("CoverUrl"))
                .SendKeys("https://m.media-amazon.com/images/I/41Wr7PpgwUL._SY445_SX342_ML2_.jpg");

            _driver.FindElement(By.Id("Isbn"))
                .SendKeys("9788408045106");

            _driver.FindElement(By.Id("Price")).Clear();
            _driver.FindElement(By.Id("Price")).SendKeys("-957");

            _driver.FindElement(By.Id("Stock")).Clear();
            _driver.FindElement(By.Id("Stock")).SendKeys("-15");

            _driver.FindElement(
                By.CssSelector("form[action='/Books/Create'] button[type='submit']"))
                .Click();

            var wait = new WebDriverWait(
                _driver, TimeSpan.FromSeconds(10));

            wait.Until(driver =>
                driver.FindElements(By.CssSelector(".field-validation-error"))
                    .Count(element => element.Displayed) == 2);

            var priceError = _driver.FindElement(
                By.CssSelector("span[data-valmsg-for='Price']")).Text;

            var stockError = _driver.FindElement(
                By.CssSelector("span[data-valmsg-for='Stock']")).Text;

            Assert.That(priceError, Is.Not.Empty);
            Assert.That(stockError, Is.Not.Empty);

            Assert.That(
                _driver.Url,
                Does.Contain("/Books/Create"));
        }

        private void CreateBookSucceedsWithZeroValues()
        {
            LoginAsAdmin();

            _driver.Navigate().GoToUrl($"{baseUrl}/Books/Create");

            _driver.FindElement(By.Id("Title"))
                .SendKeys("La Quinta Montaña");

            _driver.FindElement(By.Id("Author"))
                .SendKeys("Paulo Coelho");

            _driver.FindElement(By.Id("Description"))
                .SendKeys("La Quinta Montaña cuenta la historia del profeta Elías, un hombre que recibe de Dios la orden de abandonar Israel. En un mundo regido por supersticiones, conflictos religiosos y tradiciones profundamente arraigadas, el joven profeta deberá enfrentarse a una avalancha de acontecimientos que le conducirán a un definitivo cara a cara con Dios. La historia del profeta Elías se convierte así en una valiosa lección de esperanza para el hombre contemporáneo, y nos plantea hasta qué punto podemos determinar nuestro destino.\r\n\r\n");

            _driver.FindElement(By.Id("CoverUrl"))
                .SendKeys("https://m.media-amazon.com/images/I/61nwLM2bzbL._SY466_.jpg");

            _driver.FindElement(By.Id("Isbn"))
                .SendKeys("9788408045106");

            _driver.FindElement(By.Id("Price")).Clear();
            _driver.FindElement(By.Id("Price")).SendKeys("0");

            _driver.FindElement(By.Id("Stock")).Clear();
            _driver.FindElement(By.Id("Stock")).SendKeys("0");

            _driver.FindElement(
                By.CssSelector("form[action='/Books/Create'] button[type='submit']"))
                .Click();

            var wait = new WebDriverWait(
                _driver, TimeSpan.FromSeconds(10));

            wait.Until(driver =>
                driver.FindElements(By.CssSelector(".alert-success"))
                    .Any(element => element.Displayed)
                || driver.Url.TrimEnd('/') == $"{baseUrl}/Books"
                || driver.Url.TrimEnd('/') == $"{baseUrl}/Books/Index");

            _driver.Navigate().GoToUrl($"{baseUrl}/Books");

            var row = wait.Until(driver =>
                driver.FindElements(By.CssSelector("table tbody tr"))
                    .FirstOrDefault(element =>
                        element.Text.Contains("9788408045106")));

            var cells = row.FindElements(By.CssSelector("td"));

            Assert.That(cells[1].Text, Is.EqualTo("La Quinta Montaña"));
            Assert.That(cells[4].Text, Is.EqualTo("9788408045106"));
            Assert.That(cells[5].Text, Is.EqualTo("0.00"));
            Assert.That(cells[6].Text, Is.EqualTo("0"));
        }
        #endregion

        #region TEST LIST BOOKS
        private void ListBooksSucceeds()
        {
            LoginAsAdmin();

            _driver.Navigate().GoToUrl($"{baseUrl}/Books");

            var wait = new WebDriverWait(
                _driver, TimeSpan.FromSeconds(10));

            wait.Until(driver =>
                driver.FindElements(By.CssSelector("table tbody tr"))
                    .Count >= 2);

            var rows = _driver.FindElements(
                By.CssSelector("table tbody tr"));

            Assert.That(rows.Count, Is.EqualTo(2));

            var firstBook = rows.First(row =>
                row.Text.Contains("9788408144755"));

            var firstCells = firstBook.FindElements(By.CssSelector("td"));

            Assert.That(firstCells[1].Text, Is.EqualTo("El Alquimista"));
            Assert.That(firstCells[2].Text, Is.EqualTo("Paulo Coelho"));
            Assert.That(firstCells[4].Text, Is.EqualTo("9788408144755"));
            Assert.That(firstCells[5].Text, Is.EqualTo("1076.00"));
            Assert.That(firstCells[6].Text, Is.EqualTo("30"));

            var secondBook = rows.First(row =>
                row.Text.Contains("9788408045106"));

            var secondCells = secondBook.FindElements(By.CssSelector("td"));

            Assert.That(secondCells[1].Text, Is.EqualTo("La Quinta Montaña"));
            Assert.That(secondCells[2].Text, Is.EqualTo("Paulo Coelho"));
            Assert.That(secondCells[4].Text, Is.EqualTo("9788408045106"));
            Assert.That(secondCells[5].Text, Is.EqualTo("0.00"));
            Assert.That(secondCells[6].Text, Is.EqualTo("0"));
        }

        private void ListBooksFailsWithoutAuthentication()
        {
            _driver.Manage().Cookies.DeleteAllCookies();

            _driver.Navigate().GoToUrl($"{baseUrl}/Books");

            var wait = new WebDriverWait(
                _driver, TimeSpan.FromSeconds(10));

            wait.Until(driver =>
                driver.Url.Contains("/Account/Login"));

            Assert.That(
                _driver.Url,
                Does.Contain("/Account/Login"));

            Assert.That(
                _driver.FindElement(By.Id("Username")).Displayed,
                Is.True);

            Assert.That(
                _driver.FindElement(By.Id("Password")).Displayed,
                Is.True);

            Assert.That(
                _driver.FindElements(By.CssSelector("table tbody tr")),
                Is.Empty);

            Assert.That(
                _driver.FindElements(
                    By.CssSelector("form[action='/Account/Logout']")),
                Is.Empty);
        }

        private void ListBooksSucceedsWithEmptyCatalog()
        {
            LoginAsAdmin();

            _driver.Navigate().GoToUrl($"{baseUrl}/Books");

            var wait = new WebDriverWait(
                _driver, TimeSpan.FromSeconds(10));

            wait.Until(driver =>
                driver.FindElements(By.CssSelector("p.text-muted"))
                    .Any(element =>
                        element.Text == "No books registered."));

            Assert.That(
                _driver.FindElements(By.CssSelector("table tbody tr")),
                Is.Empty);

            Assert.That(
                _driver.FindElement(By.CssSelector("p.text-muted")).Text,
                Is.EqualTo("No books registered."));
        }
        #endregion

        private void LoginAsAdmin()
        {
            _driver.Manage().Cookies.DeleteAllCookies();

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
                    By.CssSelector("form[action='/Account/Logout']"))
                    .Any(element => element.Displayed));
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
