using System.Text.Json.Serialization;

using HealthChecks.UI.Client;
using HealthChecks.UI.Core;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
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
      .AddSingleton<IDataStoreApiServiceAsync, DataStoreApiService>()
      .AddSingleton<IDiscordClientApiServiceAsync, DiscordClientApiService>()
      .AddSingleton<ITwitchClientApiServiceAsync, TwitchClientApiService>()
      .AddSingleton<IHealthCheckApiServiceAsync, HealthCheckApiService>()
      .AddEndpointsApiExplorer()
      .AddHealthChecks()
        .AddCheck<TwitchAPIHealthCheck>("Twitch API")
        .AddCheck<DiscordAPIHealthCheck>("Discord API")
        .AddCheck<DiscordDotnetHealthCheck>("Discord.Net")
        .AddCheck<DataStoreHealthCheck>("Datastore")
        .Services
      .AddHealthChecksUI(opt =>
      {
        opt
          .SetEvaluationTimeInSeconds(10)
          .MaximumHistoryEntriesPerEndpoint(60)
          .SetApiMaxActiveRequests(1)
          .AddHealthCheckEndpoint("health-api", "/api/health")
        ;
      })
        .AddInMemoryStorage()
        .Services
        .AddControllers()
        .AddJsonOptions(jsonOptions => jsonOptions.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<UIHealthStatus>()))
        .Services
      .AddSwaggerGen(sgo =>
      {
        sgo.UseAllOfToExtendReferenceSchemas();
      })
    ;
  }

  public static void UseBotAPI(this WebApplication webApplication)
  {
#if DEBUG
    webApplication
    .UseSwagger()
    .UseSwaggerUI();
#endif
    webApplication.UseDeveloperExceptionPage();
  }

  public static void MapBotAPI(this WebApplication webApplication)
  {
    webApplication.MapHealthChecks();
    webApplication.MapDataAPI();
    webApplication.MapDiscordAPI();
    webApplication.MapTwitchAPI();

  }

  private static void MapHealthChecks(this WebApplication webApplication)
  {
    webApplication
      .MapGet("/api/health", async (IHealthCheckApiServiceAsync healthCheckService, HttpContext context)
        => await healthCheckService.CheckHealthAsync(context, CancellationToken.None))
      .WithName("HealthCheck")
      .WithOpenApi(x => new(x)
      {
        Tags =
        [
          new()
      {
        Name = "Health"
      }
        ]
      })
      .Produces<Payloads.HealthCheckPayload>(StatusCodes.Status200OK);
  }

  private static void MapDataAPI(this WebApplication webApplication)
  {
    webApplication
      .MapGet("/api/data/announcements", async (IDataStoreApiServiceAsync dataStoreService)
        => await dataStoreService.GetAnnouncementsAsync(CancellationToken.None))
      .WithName("GetAnnouncements")
      .WithOpenApi(x => new(x)
      {
        Tags =
        [
          new ()
          {
            Name = "DataStore"
          },
        ]
      })
      .Produces<ResultMessage<Payloads.AnnouncementsPayload>>(StatusCodes.Status200OK);

    webApplication
      .MapPost("/api/data/announcement", async (IDataStoreApiServiceAsync dataStoreService, string twitchUser, ulong guildID, ulong channelID)
        => await dataStoreService.AddAnnouncementAsync(twitchUser, guildID, channelID, CancellationToken.None))
      .WithName("AddAnnouncement")
      .WithOpenApi(x => new OpenApiOperation(x)
      {
        Tags =
        [
          new ()
          {
            Name = "DataStore"
          },
        ]
      })
      .Produces<ResultMessage<Payloads.AnnouncementPayload>>(StatusCodes.Status200OK);

    webApplication
      .MapDelete("/api/data/announcement", async (IDataStoreApiServiceAsync dataStoreService, string twitchUser, ulong guildID, ulong? channelID)
        => await dataStoreService.DeleteAnnouncementAsync(twitchUser, guildID, channelID, CancellationToken.None))
      .WithName("DeleteAnnouncement")
      .WithOpenApi(x => new(x)
      {
        Tags =
        [
          new ()
          {
            Name = "DataStore"
          },
        ]
      })
      .Produces<ResultMessage<Payloads.EmptyPayload>>(StatusCodes.Status200OK);
  }

  private static void MapDiscordAPI(this WebApplication webApplication)
  {
    webApplication
      .MapGet("/api/discord/channelName", async (IDiscordClientApiServiceAsync discordClientService, ulong channelID)
        => await discordClientService.GetChannelNameAsync(channelID, CancellationToken.None))
      .WithName("GetChannelName")
      .WithOpenApi(x => new(x)
      {
        Tags =
        [
          new ()
          {
            Name = "Discord"
          },
        ]
      })
      .Produces<ResultMessage<Payloads.ChannelNamePayload>>(StatusCodes.Status200OK);

    webApplication
      .MapGet("/api/discord/guildName", (IDiscordClientApiServiceAsync discordClientService, ulong guildID)
        => discordClientService.GetGuildName(guildID))
      .WithName("GetGuildName")
      .WithOpenApi(x => new(x)
      {
        Tags =
        [
          new ()
          {
            Name = "Discord"
          },
        ]
      })
      .Produces<ResultMessage<Payloads.GuildNamePayload>>(StatusCodes.Status200OK);

    webApplication
      .MapGet("/api/discord/guildIconUrl", (IDiscordClientApiServiceAsync discordClientService, ulong guildID)
        => discordClientService.GetGuildIconUrl(guildID))
      .WithName("GetGuildIconUrl")
      .WithOpenApi(x => new(x)
      {
        Tags =
        [
          new ()
          {
            Name = "Discord"
          },
        ]
      })
      .Produces<ResultMessage<Payloads.GuildIconUrlPayload>>(StatusCodes.Status200OK);

    webApplication
      .MapGet("/api/discord/clientData", (IDiscordClientApiServiceAsync discordClientService)
        => discordClientService.GetClientData())
      .WithName("GetClientData")
      .WithOpenApi(x => new(x)
      {
        Tags =
        [
          new ()
          {
            Name = "Discord"
          },
        ]
      })
      .Produces<ResultMessage<Payloads.DiscordClientDataPayload>>(StatusCodes.Status200OK)
      .Produces<ResultMessage<Payloads.DiscordClientDataPayload>>(StatusCodes.Status500InternalServerError);

    webApplication
      .MapPost("/api/discord/postTwitchAnnouncement", async (IDiscordClientApiServiceAsync discordClientService, string twitchUser, ulong guildID, ulong channelID)
        => await discordClientService.PostTwitchAnnouncementAsync(twitchUser, guildID, channelID, CancellationToken.None))
      .WithName("PostTwitchAnnouncement")
      .WithOpenApi(x => new(x)
      {
        Tags =
        [
          new ()
              {
                Name = "Discord"
              },
        ]
      })
      .Produces<ResultMessage<Payloads.EmptyPayload>>(StatusCodes.Status200OK)
      .Produces<ResultMessage<Payloads.EmptyPayload>>(StatusCodes.Status500InternalServerError);
  }

  private static void MapTwitchAPI(this WebApplication webApplication)
  {
    webApplication
      .MapGet("/api/twitch/getStream", async (ITwitchClientApiServiceAsync twitchClientService, string username)
        => await twitchClientService.GetStreamDataAsync(username, CancellationToken.None))
      .WithName("GetStreamData")
      .WithOpenApi(x => new(x)
      {
        Tags =
        [
          new ()
          {
            Name = "Twitch"
          },
        ]
      })
      .Produces<ResultMessage<Payloads.StreamDataPayload>>(StatusCodes.Status200OK)
      .Produces<ResultMessage<Payloads.StreamDataPayload>>(StatusCodes.Status500InternalServerError);

    webApplication
      .MapGet("/api/twitch/getUser", async (ITwitchClientApiServiceAsync twitchClientService, string username)
        => await twitchClientService.GetUserDataAsync(username, CancellationToken.None))
      .WithName("GetUserData")
      .WithOpenApi(x => new(x)
      {
        Tags =
        [
          new ()
          {
            Name = "Twitch"
          },
        ]
      })
      .Produces<ResultMessage<Payloads.UserDataPayload>>(StatusCodes.Status200OK)
      .Produces<ResultMessage<Payloads.UserDataPayload>>(StatusCodes.Status500InternalServerError);
  }
}
