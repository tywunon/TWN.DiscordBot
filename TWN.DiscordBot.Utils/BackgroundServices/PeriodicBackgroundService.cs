using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using TWN.DiscordBot.Utils;

namespace TWN.DiscordBot.Utils.BackgroundServices;
public abstract class PeriodicBackgroundService(ILogger<PeriodicBackgroundService> logger) : BackgroundService
{
  public bool ShouldStop { get; protected set; } = false;
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    await InitAsync(stoppingToken);
    var interval = await GetInterval(stoppingToken);
    var timer = new PeriodicTimer(interval);
    logger.LogDebug("PeriodicTimer created ({Delay}ms)", interval.TotalMilliseconds);
    while (await timer.WaitForNextTickAsync(stoppingToken))
      try
      {
        if (stoppingToken.IsCancellationRequested)
          break;
        else if (ShouldStop)
          break;
        else
          await ExecutePeriodicAsync(stoppingToken);
      }
      catch (Exception ex)
      {
        logger.LogException(ex, "ExecutePeriodicAsync");
      }
    logger.LogInformation("Execution of {}:{} was stopped.", typeof(PeriodicBackgroundService).Name, this.GetType().Name);
  }

  protected async virtual Task InitAsync(CancellationToken cancellationToken)
    => await Task.CompletedTask;
  protected abstract Task<TimeSpan> GetInterval(CancellationToken cancellationToken);
  protected abstract Task ExecutePeriodicAsync(CancellationToken cancellationToken);
}
