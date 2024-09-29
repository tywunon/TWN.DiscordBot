using Microsoft.Extensions.Logging;

namespace TWN.DiscordBot.Utils.BackgroundServices;
public class LogCleaner(ILogger<LogCleaner> logger) : PeriodicBackgroundService(logger)
{
  protected override Task<TimeSpan> GetInterval(CancellationToken cancellationToken)
    => Task.FromResult(TimeSpan.FromHours(4));
  protected override Task ExecutePeriodicAsync(CancellationToken cancellationToken)
  {
    try
    {
      foreach (var logFile in Directory.EnumerateFiles(@".\logs\", "*.log"))
        if (File.GetCreationTimeUtc(logFile) < DateTime.UtcNow.AddDays(-7))
        {
          File.Delete(logFile);
          logger.LogInformation("Log file deleted ({logFile})", logFile);
        }
    }
    catch (Exception ex)
    {
      logger.LogException(ex, "ExecutePeriodicAsync");
    }
    return Task.CompletedTask;
  }
}
