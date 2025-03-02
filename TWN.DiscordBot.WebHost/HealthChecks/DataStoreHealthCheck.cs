﻿using Microsoft.Extensions.Diagnostics.HealthChecks;

using TWN.DiscordBot.Interfaces;

namespace TWN.DiscordBot.WebHost.HealthChecks;
internal class DataStoreHealthCheck(IDataStoreAsync dataStore) : IHealthCheck
{
  async Task<HealthCheckResult> IHealthCheck.CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
  {
    try
    {
      return await dataStore.HealthCheckAsync(cancellationToken)
        ? HealthCheckResult.Healthy("Datastore is healthy")
        : HealthCheckResult.Unhealthy("Datastore is unhealthy");
    }
    catch (Exception ex)
    {
      return HealthCheckResult.Unhealthy(exception: ex);
    }
  }
}
