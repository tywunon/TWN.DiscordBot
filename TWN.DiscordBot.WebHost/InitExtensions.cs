using HealthChecks.UI.Client;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;

using TWN.DiscordBot.WebHost;
using TWN.DiscordBot.WebHost.HealthChecks;
using TWN.DiscordBot.WebHost.Services;

namespace TWN.DiscordBot.WebHost;

public static class InitExtensions
{
  public static IServiceCollection AddBotAPIServices(this IServiceCollection serviceCollection)
  {
    return serviceCollection
      .AddSingleton<IDataStoreServiceAsync, DataStoreService>()
      .AddSingleton<IDiscordClientServiceAsync, DiscordClientService>()
      .AddSingleton<ITwitchClientServiceAsync, TwitchClientService>()
      .AddEndpointsApiExplorer()
      .AddHealthChecks()
        .AddCheck<TwitchHealthCheck>("Twitch API", HealthStatus.Unhealthy)
        .AddCheck<DiscordHealthCheck>("Discord.Net", HealthStatus.Unhealthy)
        .AddCheck<DataStoreHealthCheck>("Datastore", HealthStatus.Unhealthy)
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

  public static void USeBotAPI(this WebApplication webApplication)
  {
    webApplication.UseHealthChecks();

    webApplication
#if DEBUG
    .UseSwagger()
    .UseSwaggerUI()
#endif
    ;
    webApplication.UseDeveloperExceptionPage();
  }

  public static void MapBotAPI(this WebApplication webApplication)
  {
    webApplication.MapDataAPI();
    webApplication.MapDiscordAPI();
    webApplication.MapTwitchAPI();

  }

  private static void UseHealthChecks(this WebApplication webApplication)
  {
    webApplication.UseHealthChecks("/api/health", new HealthCheckOptions()
    {
      Predicate = _ => true,
      ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
    });
    webApplication.UseHealthChecksUI(opt =>
    {
      opt.UseRelativeApiPath = true;
      opt.UIPath = "/health-check-ui";
      opt.ApiPath = "/api/health";
    });
  }

  private static void MapDataAPI(this WebApplication webApplication)
  {
    webApplication
      .MapGet("/api/data/announcements", async (IDataStoreServiceAsync dataStoreService)
        => await dataStoreService.GetAnnouncementsAsync(new CancellationTokenSource().Token))
      .WithName("GetAnnouncements")
      .WithOpenApi(x => new OpenApiOperation(x)
      {
        Tags = [
          new OpenApiTag() {
            Name = "DataStore"
          },
        ]
      });
    webApplication
      .MapPost("/api/data/announcement", async (IDataStoreServiceAsync dataStoreService, string twitchUser, ulong guildID, ulong channelID)
        => await dataStoreService.AddAnnouncementAsync(twitchUser, guildID, channelID, new CancellationTokenSource().Token))
      .WithName("AddAnnouncement")
      .WithOpenApi(x => new OpenApiOperation(x)
      {
        Tags = [
          new OpenApiTag() {
            Name = "DataStore"
          },
        ]
      });
    webApplication
      .MapDelete("/api/data/announcement", async (IDataStoreServiceAsync dataStoreService, string twitchUser, ulong guildID, ulong? channelID)
        => await dataStoreService.DeleteAnnouncementAsync(twitchUser, guildID, channelID, new CancellationTokenSource().Token))
      .WithName("DeleteAnnouncement")
      .WithOpenApi(x => new OpenApiOperation(x)
      {
        Tags = [
          new OpenApiTag() {
            Name = "DataStore"
          },
        ]
      });
  }

  private static void MapDiscordAPI(this WebApplication webApplication)
  {
    webApplication
      .MapGet("/api/discord/channelName", async (IDiscordClientServiceAsync discordClientService, ulong channelID)
        => await discordClientService.GetChannelNameAsync(channelID, new CancellationTokenSource().Token))
      .WithName("GetChannelName")
      .WithOpenApi(x => new OpenApiOperation(x)
      {
        Tags = [
          new OpenApiTag() {
            Name = "Discord"
          },
        ]
      });
    webApplication
      .MapGet("/api/discord/guildName", (IDiscordClientServiceAsync discordClientService, ulong guildID)
        => discordClientService.GetGuildName(guildID))
      .WithName("GetGuildName")
      .WithOpenApi(x => new OpenApiOperation(x)
      {
        Tags = [
          new OpenApiTag() {
            Name = "Discord"
          },
        ]
      });
  }

  private static void MapTwitchAPI(this WebApplication webApplication)
  {
    webApplication
      .MapGet("/api/twitch/getStream", async (ITwitchClientServiceAsync twitchClientService, string username)
        => await twitchClientService.GetStreamDataAsync(username, new CancellationTokenSource().Token))
      .WithName("GetStreamData")
      .WithOpenApi(x => new OpenApiOperation(x)
      {
        Tags = [
          new OpenApiTag() {
            Name = "Twitch"
          },
        ]
      });
    webApplication
      .MapGet("/api/twitch/getUser", async (ITwitchClientServiceAsync twitchClientService, string username)
        => await twitchClientService.GetUserDataAsync(username, new CancellationTokenSource().Token))
      .WithName("GetUserData")
      .WithOpenApi(x => new OpenApiOperation(x)
      {
        Tags = [
          new OpenApiTag() {
            Name = "Twitch"
          },
        ]
      });
  }
}
