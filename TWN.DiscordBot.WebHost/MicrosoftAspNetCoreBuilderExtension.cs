using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using TWN.DiscordBot.WebHost.Services;

namespace Microsoft.AspNetCore.Builder;

public static class MicrosoftAspNetCoreBuilderExtension
{
  public static void MapBotAPI(this WebApplication webApplication)
  {
#if DEBUG
    webApplication.UseSwagger();
    webApplication.UseSwaggerUI();
#endif

    webApplication.MapGet("/data/announcements", async (IDataStoreService dataStoreService) => await dataStoreService.GetAnnouncementsAsync())
      .WithName("GetAnnouncements")
      .WithOpenApi(x => new OpenApiOperation(x)
      {
      });
    webApplication.MapPost("/data/announcements", async (IDataStoreService dataStoreService, string twitchUser, ulong guildID, ulong channelID) => await dataStoreService.AddAnnouncementAsync(twitchUser, guildID, channelID))
      .WithName("AddAnnouncement")
      .WithOpenApi(x => new OpenApiOperation(x)
      {

      });
    webApplication.MapDelete("/data/announcements", async (IDataStoreService dataStoreService, string twitchUser, ulong guildID, ulong? channelID) => await dataStoreService.DeleteAnnouncementAsync(twitchUser, guildID, channelID))
      .WithName("DeleteAnnouncement")
      .WithOpenApi(x => new OpenApiOperation(x)
      {

      });
  }
}
