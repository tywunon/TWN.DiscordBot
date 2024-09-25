using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TWN.DiscordBot.Bot.BackgroundServices;
internal abstract class PeriodicBackgroundService(ILogger<PeriodicBackgroundService> logger) : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    await InitAsync(stoppingToken);
    var interval = await GetInterval(stoppingToken);
    PeriodicTimer timer = new PeriodicTimer(interval);
    logger.LogDebug("PeriodicTimer created ({Delay}ms)", interval.TotalMilliseconds);
    while (await timer.WaitForNextTickAsync(stoppingToken))
    {
      try
      {
        await ExecutePeriodicAsync(stoppingToken);
      }
      catch(Exception ex)
      {
        logger.LogError(ex, "An exception was thrown in periodic execution");
      }
    }
  }

  protected async virtual Task InitAsync(CancellationToken cancellationToken) 
    => await Task.CompletedTask;
  protected abstract Task<TimeSpan> GetInterval(CancellationToken cancellationToken);
  protected abstract Task ExecutePeriodicAsync(CancellationToken cancellationToken);
}
