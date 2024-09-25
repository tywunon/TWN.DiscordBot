using Microsoft.Extensions.Diagnostics.HealthChecks;

using TWN.DiscordBot.Interfaces;

namespace TWN.DiscordBot.WebHost.HealthChecks;
internal class TwitchHealthCheck(ITwitchClientAsync twitchClient) : IHealthCheck
{
  async Task<HealthCheckResult> IHealthCheck.CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken) 
    => await twitchClient.HealthCheckAsync(cancellationToken)
      ? HealthCheckResult.Healthy("Twitch is healthy")
      : HealthCheckResult.Unhealthy("Twitch is unhealthy");
}
