using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Diagnostics.HealthChecks;

using TWN.DiscordBot.Interfaces;

namespace TWN.DiscordBot.WebHost.HealthChecks;
internal class DataStoreHealthCheck(IDataStore dataStore) : IHealthCheck
{
  async Task<HealthCheckResult> IHealthCheck.CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
  {
    return await dataStore.HealthCheckAsync(cancellationToken)
      ? HealthCheckResult.Healthy("Datastore is healthy")
      : HealthCheckResult.Unhealthy("Datastore is unhealthy");
  }
}
