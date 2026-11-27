using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace LibreriaDonJulio.Tests.Reportig
{
    public static class HtmlReportWriter
    {
        public static void Write(
        string resultsDirectory,
        IReadOnlyList<ScenarioResult> results)
        {
            Directory.CreateDirectory(resultsDirectory);

            var total = results.Count;
            var passed = results.Count(r => r.Passed);
            var failed = total - passed;

            var html = new StringBuilder();

            html.AppendLine("""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="UTF-8">
                <title>Libreria Don Julio - Selenium Report</title>
                <style>
                    body {
                        font-family: Arial, sans-serif;
                        background: #f4f5f7;
                        margin: 40px;
                        color: #222;
                    }
                    h1 { margin-bottom: 8px; }
                    .summary {
                        display: flex;
                        gap: 16px;
                        margin: 24px 0;
                    }
                    .card {
                        background: white;
                        padding: 20px;
                        border-radius: 8px;
                        flex: 1;
                    }
                    table {
                        width: 100%;
                        border-collapse: collapse;
                        background: white;
                    }
                    th, td {
                        padding: 14px;
                        text-align: left;
                        border-bottom: 1px solid #ddd;
                    }
                    th {
                        background: #243447;
                        color: white;
                    }
                    .passed {
                        color: #198754;
                        font-weight: bold;
                    }
                    .failed {
                        color: #dc3545;
                        font-weight: bold;
                    }
                    img {
                        width: 180px;
                        border-radius: 4px;
                        border: 1px solid #ddd;
                    }
                </style>
            </head>
            <body>
                <h1>Libreria Don Julio</h1>
                <p>Automated Selenium Test Report</p>
            """);

            html.AppendLine($"<p>Execution: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");

            html.AppendLine($"""
            <div class="summary">
                <div class="card">
                    Total scenarios: <strong>{total}</strong>
                </div>
                <div class="card passed">
                    Passed: {passed}
                </div>
                <div class="card failed">
                    Failed: {failed}
                </div>
            </div>

            <table>
                <thead>
                    <tr>
                        <th>Scenario</th>
                        <th>Jira</th>
                        <th>Status</th>
                        <th>Duration</th>
                        <th>Details</th>
                        <th>Evidence</th>
                    </tr>
                </thead>
                <tbody>
            """);

            foreach (var result in results)
            {
                var status = result.Passed ? "PASSED" : "FAILED";
                var cssClass = result.Passed ? "passed" : "failed";

                var screenshot = WebUtility.HtmlEncode(
                    result.ScreenshotFile);

                var evidence = string.IsNullOrWhiteSpace(screenshot)
                    ? "Not available"
                    : $"""
                    <a href="{screenshot}" target="_blank">
                        <img src="{screenshot}" alt="Screenshot">
                    </a>
                    """;

                html.AppendLine($"""
                <tr>
                    <td>{WebUtility.HtmlEncode(result.Name)}</td>
                    <td>{WebUtility.HtmlEncode(result.UserStory)}</td>
                    <td class="{cssClass}">{status}</td>
                    <td>{result.Duration.TotalSeconds:F2}s</td>
                    <td>{WebUtility.HtmlEncode(result.Details)}</td>
                    <td>{evidence}</td>
                </tr>
                """);
            }

            html.AppendLine("""
                </tbody>
            </table>
            </body>
            </html>
            """);

            File.WriteAllText(
                Path.Combine(resultsDirectory, "index.html"),
                html.ToString(),
                Encoding.UTF8);
        }
    }
}
