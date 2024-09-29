using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using TWN.DiscordBot.Utils;

namespace TWN.DiscordBot.Utils.BackgroundServices;
public abstract class PeriodicBackgroundService(ILogger<PeriodicBackgroundService> logger) : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    await InitAsync(stoppingToken);
    var interval = await GetInterval(stoppingToken);
    var timer = new PeriodicTimer(interval);
    logger.LogDebug("PeriodicTimer created ({Delay}ms)", interval.TotalMilliseconds);
    while (await timer.WaitForNextTickAsync(stoppingToken))
      try
      {
        await ExecutePeriodicAsync(stoppingToken);
      }
      catch (Exception ex)
      {
        logger.LogException(ex, "ExecutePeriodicAsync");
      }
  }

  protected async virtual Task InitAsync(CancellationToken cancellationToken)
    => await Task.CompletedTask;
  protected abstract Task<TimeSpan> GetInterval(CancellationToken cancellationToken);
  protected abstract Task ExecutePeriodicAsync(CancellationToken cancellationToken);
}
