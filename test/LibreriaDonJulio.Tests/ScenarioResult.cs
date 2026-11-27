using System;
using System.Collections.Generic;
using System.Text;

namespace LibreriaDonJulio.Tests
{
    public sealed record ScenarioResult(
      string Name,
      string UserStory,
      bool Passed,
      TimeSpan Duration,
      string Details,
      string ScreenshotFile);
}
