using HealthChecks.UI.Client;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;

using TWN.DiscordBot.WebHost.Services;

namespace Microsoft.AspNetCore.Builder;

public static class MicrosoftAspNetCoreBuilderExtension
{
  public static void UseBotAPI(this WebApplication webApplication)
  {
    webApplication
#if DEBUG
      .UseSwagger()
      .UseSwaggerUI()
#endif
      .UseDeveloperExceptionPage()
      ;
  }

  public static void UseHealthChecks(this WebApplication webApplication)
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

  public static void MapDataAPI(this WebApplication webApplication)
  {
    webApplication.MapGet("/api/data/announcements", async (IDataStoreService dataStoreService) => await dataStoreService.GetAnnouncementsAsync())
      .WithName("GetAnnouncements")
      .WithOpenApi(x => new OpenApiOperation(x)
      {
      });
    webApplication.MapPost("/api/data/announcements", async (IDataStoreService dataStoreService, string twitchUser, ulong guildID, ulong channelID) => await dataStoreService.AddAnnouncementAsync(twitchUser, guildID, channelID))
      .WithName("AddAnnouncement")
      .WithOpenApi(x => new OpenApiOperation(x)
      {

      });
    webApplication.MapDelete("/api/data/announcements", async (IDataStoreService dataStoreService, string twitchUser, ulong guildID, ulong? channelID) => await dataStoreService.DeleteAnnouncementAsync(twitchUser, guildID, channelID))
      .WithName("DeleteAnnouncement")
      .WithOpenApi(x => new OpenApiOperation(x)
      {

      });
  }
}
