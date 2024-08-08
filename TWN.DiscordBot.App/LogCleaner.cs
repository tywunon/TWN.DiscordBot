using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TWN.LinhBot.App;
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
          foreach (var logfile in Directory.EnumerateFiles(@".\logs\", "*.log"))
            if (File.GetCreationTimeUtc(logfile) < DateTime.UtcNow.AddDays(-7))
              File.Delete(logfile);
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
