using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LanguageExt.Pipes;

using Microsoft.Extensions.Diagnostics.HealthChecks;

using TWN.DiscordBot.Interfaces;
using TWN.DiscordBot.Interfaces.Types;
using TWN.DiscordBot.Settings;

namespace TWN.DiscordBot.WebHost.HealthChecks;
internal class TwitchHealthCheck(ITwitchClient twitchClient) : IHealthCheck
{
  async Task<HealthCheckResult> IHealthCheck.CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
  {
    return await twitchClient.HealthCheck()
      ? HealthCheckResult.Healthy("Twitch is healthy")
      : HealthCheckResult.Unhealthy("Twitch is unhealthy");
  }
}
