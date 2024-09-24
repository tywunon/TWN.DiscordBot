using TWN.DiscordBot.WebHost.HealthChecks;
using TWN.DiscordBot.WebHost.Services;

namespace Microsoft.Extensions.DependencyInjection;
public static class ServiceCollectionExtension
{
  public static IServiceCollection AddBotAPIServices(this IServiceCollection serviceCollection)
  {
    return serviceCollection
      .AddSingleton<IDataStoreServiceAsync, DataStoreService>()
      .AddSingleton<IDiscordClientServiceAsync, DiscordClientService>()
      .AddSingleton<ITwitchClientServiceAsync, TwitchClientService>()
      .AddEndpointsApiExplorer()
      .AddHealthChecks()
        .AddCheck<TwitchHealthCheck>("Twitch API", Diagnostics.HealthChecks.HealthStatus.Unhealthy)
        .AddCheck<DiscordHealthCheck>("Discord.Net", Diagnostics.HealthChecks.HealthStatus.Unhealthy)
        .AddCheck<DataStoreHealthCheck>("Datastore", Diagnostics.HealthChecks.HealthStatus.Unhealthy)
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
