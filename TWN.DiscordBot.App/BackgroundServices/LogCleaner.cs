using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TWN.DiscordBot.App.BackgroundServices;
internal class LogCleaner(ILogger<LogCleaner> logger) : BackgroundService
{
    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromHours(4));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
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
                    logger.LogError(ex, "An Exception was thrown");
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An Exception was thrown");
        }
    }
}
