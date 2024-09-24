using HealthChecks.UI.Client;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using TWN.DiscordBot.WebHost.Services;

namespace Microsoft.AspNetCore.Builder;

public static class MicrosoftAspNetCoreBuilderExtension
{
  public static void InitBotAPI(this WebApplication webApplication)
  {
    webApplication.UseHealthChecks();

    webApplication
#if DEBUG
    .UseSwagger()
    .UseSwaggerUI()
#endif
    ;
    webApplication.UseDeveloperExceptionPage();

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
      opt.UIPath = "/healthcheck-ui";
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
      });
    webApplication
      .MapPost("/api/data/announcements", async (IDataStoreServiceAsync dataStoreService, string twitchUser, ulong guildID, ulong channelID)
        => await dataStoreService.AddAnnouncementAsync(twitchUser, guildID, channelID, new CancellationTokenSource().Token))
      .WithName("AddAnnouncement")
      .WithOpenApi(x => new OpenApiOperation(x)
      {
      });
    webApplication
      .MapDelete("/api/data/announcements", async (IDataStoreServiceAsync dataStoreService, string twitchUser, ulong guildID, ulong? channelID)
        => await dataStoreService.DeleteAnnouncementAsync(twitchUser, guildID, channelID, new CancellationTokenSource().Token))
      .WithName("DeleteAnnouncement")
      .WithOpenApi(x => new OpenApiOperation(x)
      {
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
      });
    webApplication
      .MapGet("/api/discord/guildName", (IDiscordClientServiceAsync discordClientService, ulong guildID)
        => discordClientService.GetGuildName(guildID))
      .WithName("GetGuildName")
      .WithOpenApi(x => new OpenApiOperation(x)
      {
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
      });
    webApplication
      .MapGet("/api/twitch/getUser", async (ITwitchClientServiceAsync twitchClientService, string username)
        => await twitchClientService.GetUserDataAsync(username, new CancellationTokenSource().Token))
      .WithName("GetUserData")
      .WithOpenApi(x => new OpenApiOperation(x)
      {
      });
  }
}
