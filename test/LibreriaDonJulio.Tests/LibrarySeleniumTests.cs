using LibreriaDonJulio.Tests.Reportig;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibreriaDonJulio.Tests
{
    [TestFixture]
    public class LibrarySeleniumTests
    {
      [Test]
      public void RunAllScenarios()
      {
            var resultsDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../TestResults/latest"));

            if (Directory.Exists(resultsDirectory))
            {
                Directory.Delete(resultsDirectory, recursive: true);
            }

            Directory.CreateDirectory(resultsDirectory);

            using var app = LocalApp.Start();

            var runner = new SeleniumTestRunner(
                app.BaseUrl,
                resultsDirectory,
                headed: true);

            var results = runner.Run();

            HtmlReportWriter.Write(resultsDirectory, results);

            TestContext.Out.WriteLine(
                $"HTML report: {Path.Combine(resultsDirectory, "index.html")}");

            Assert.That(results, Is.Not.Empty);

            Assert.That(
                results.All(result => result.Passed),
                Is.True,
                string.Join(Environment.NewLine,
                    results.Where(result => !result.Passed)
                        .Select(result => $"{result.Name}: {result.Details}")));
        }
    }
}
