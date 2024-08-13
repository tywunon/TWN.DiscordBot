using Microsoft.Extensions.Diagnostics.HealthChecks;

using TWN.DiscordBot.Interfaces;

namespace TWN.DiscordBot.WebHost.HealthChecks;
internal class TwitchHealthCheck(ITwitchClient twitchClient) : IHealthCheck
{
  async Task<HealthCheckResult> IHealthCheck.CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
  {
    return await twitchClient.HealthCheck(cancellationToken)
      ? HealthCheckResult.Healthy("Twitch is healthy")
      : HealthCheckResult.Unhealthy("Twitch is unhealthy");
  }
}
