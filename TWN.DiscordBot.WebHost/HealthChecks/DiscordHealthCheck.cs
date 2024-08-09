using System.Collections.ObjectModel;

using Microsoft.Extensions.Diagnostics.HealthChecks;

using TWN.DiscordBot.Interfaces;

namespace TWN.DiscordBot.WebHost.HealthChecks;
internal class DiscordHealthCheck(IDiscordClient discordClient) : IHealthCheck
{
  async Task<HealthCheckResult> IHealthCheck.CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken) 
    => await discordClient.HealthCheck() switch
    {
      Interfaces.Types.DiscordConnectionState.Disconnected
        => HealthCheckResult.Unhealthy("Discord is disconnected"),
      Interfaces.Types.DiscordConnectionState.Connecting
        => HealthCheckResult.Unhealthy("Discord is connecting"),
      Interfaces.Types.DiscordConnectionState.Connected
        => HealthCheckResult.Healthy("Discord is connected"),
      Interfaces.Types.DiscordConnectionState.Disconnecting
        => HealthCheckResult.Unhealthy("Discord is disconnecting"),
      _ => HealthCheckResult.Degraded("Discord connection state is unknown"),
    };
}
