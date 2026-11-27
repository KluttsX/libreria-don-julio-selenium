using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace LibreriaDonJulio.Tests
{
    public sealed class LocalApp : IDisposable
    {
#if DEBUG
        private const string Configuration = "Debug";
#else
    private const string Configuration = "Release";
#endif

        private static readonly TimeSpan StartupTimeout =
           TimeSpan.FromSeconds(90);

        private readonly Process _process;
        private readonly string _databaseDirectory;

        public string BaseUrl { get; }

        private LocalApp(
       Process process,
       string baseUrl,
       string databaseDirectory)
        {
            _process = process;
            BaseUrl = baseUrl;
            _databaseDirectory = databaseDirectory;
        }

        public static LocalApp Start()
        {
            const string baseUrl = "http://localhost:5004";

            if (IsResponding(baseUrl))
            {
                throw new InvalidOperationException(
                    $"The port is already in use: {baseUrl}");
            }

            var projectPath = Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "../../../../../src/LibreriaDonJulio/LibreriaDonJulio.csproj"));

            if (!File.Exists(projectPath))
            {
                throw new FileNotFoundException(
                    "Application project not found.", projectPath);
            }

            var databaseDirectory = Path.Combine(
                Path.GetTempPath(),
                $"LibreriaDonJulio.Tests-{Guid.NewGuid()}");

            Directory.CreateDirectory(databaseDirectory);

            var databasePath = Path.Combine(
                databaseDirectory, "library-test.db");

            var startInfo = new ProcessStartInfo("dotnet")
            {
                WorkingDirectory = Path.GetDirectoryName(projectPath)!,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            foreach (var argument in new[]
            {
            "run",
            "--no-build",
            "--project", projectPath,
            "--configuration", Configuration,
            "--no-launch-profile",
            "--urls", baseUrl
        })
            {
                startInfo.ArgumentList.Add(argument);
            }

            startInfo.Environment["ASPNETCORE_ENVIRONMENT"] =
                "Development";

            startInfo.Environment["Testing__Enabled"] = "true";

            startInfo.Environment["ConnectionStrings__DefaultConnection"] =
                $"Data Source={databasePath}";

            var log = new StringBuilder();

            var process = new Process
            {
                StartInfo = startInfo
            };

            process.OutputDataReceived += (_, e) => Append(log, e.Data);
            process.ErrorDataReceived += (_, e) => Append(log, e.Data);

            var app = new LocalApp(process, baseUrl, databaseDirectory);

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                var stopwatch = Stopwatch.StartNew();

                while (stopwatch.Elapsed < StartupTimeout)
                {
                    if (process.HasExited)
                    {
                        throw new InvalidOperationException(
                            $"Application exited with code {process.ExitCode}."
                            + Environment.NewLine + Read(log));
                    }

                    if (IsResponding(baseUrl))
                        return app;

                    Thread.Sleep(500);
                }

                throw new TimeoutException(
                    $"Application did not respond at {baseUrl}."
                    + Environment.NewLine + Read(log));
            }
            catch
            {
                app.Dispose();
                throw;
            }
        }

        private static bool IsResponding(string baseUrl)
        {
            using var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(2)
            };

            try
            {
                using var response = client.GetAsync(baseUrl)
                    .GetAwaiter().GetResult();

                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
            catch (TaskCanceledException)
            {
                return false;
            }
        }

        private static void Append(StringBuilder log, string? message)
        {
            if (message is null)
                return;

            lock (log)
            {
                log.AppendLine(message);
            }
        }

        private static string Read(StringBuilder log)
        {
            lock (log)
            {
                return log.ToString();
            }
        }

        public void Dispose()
        {
            try
            {
                if (!_process.HasExited)
                {
                    _process.Kill(entireProcessTree: true);
                    _process.WaitForExit();
                }
            }
            catch (InvalidOperationException)
            {
            }
            finally
            {
                _process.Dispose();

                if (Directory.Exists(_databaseDirectory))
                {
                    Directory.Delete(
                        _databaseDirectory, recursive: true);
                }
            }
        }
    }
}
