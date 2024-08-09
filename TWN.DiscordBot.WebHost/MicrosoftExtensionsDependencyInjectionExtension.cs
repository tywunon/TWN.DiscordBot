﻿using TWN.DiscordBot.WebHost.HealthChecks;
using TWN.DiscordBot.WebHost.Services;

namespace Microsoft.Extensions.DependencyInjection;
public static class MicrosoftExtensionsDependencyInjectionExtension
{
  public static IServiceCollection AddBotAPIServices(this IServiceCollection serviceCollection)
  {
    return serviceCollection
      .AddSingleton<IDataStoreService, DataStoreService>()
      .AddEndpointsApiExplorer()
      .AddHealthChecks()
        .AddCheck<TwitchHealthCheck>("Twitch API", Diagnostics.HealthChecks.HealthStatus.Unhealthy)
        .AddCheck<DiscordHealthCheck>("Discord.Net", Diagnostics.HealthChecks.HealthStatus.Unhealthy)
        .Services
      .AddHealthChecksUI(opt =>
      {
        opt
          .SetEvaluationTimeInSeconds(10)
          .MaximumHistoryEntriesPerEndpoint(60)
          .SetApiMaxActiveRequests(1)
          .AddHealthCheckEndpoint("feedback api", "/api/health")
        ;
      })
        .AddInMemoryStorage()
        .Services
      .AddSwaggerGen(sgo =>
      {
      })
    ;
  }
}
