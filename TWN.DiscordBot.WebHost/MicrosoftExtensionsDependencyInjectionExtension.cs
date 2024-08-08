using TWN.DiscordBot.WebHost.Services;

namespace Microsoft.Extensions.DependencyInjection;
public static class MicrosoftExtensionsDependencyInjectionExtension
{
  public static IServiceCollection AddBotAPIServices(this IServiceCollection serviceCollection)
  {
    return serviceCollection
      .AddSingleton<IDataStoreService, DataStoreService>()
      .AddEndpointsApiExplorer()
      .AddSwaggerGen(sgo =>
      {
      })
    ;
  }
}
