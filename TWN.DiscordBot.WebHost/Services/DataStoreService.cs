using Microsoft.AspNetCore.Http;

using TWN.DiscordBot.Interfaces;

namespace TWN.DiscordBot.WebHost.Services;
internal class DataStoreService(IDataStoreAsync dataStore) : IDataStoreServiceAsync
{
  public async Task<IResult> GetAnnouncementsAsync(CancellationToken cancellationToken)
  {

    var result = await dataStore.GetAnnouncementsAsync(cancellationToken);
    return Results.Ok(new ResultMessage()
    {
      Success = true,
      Message = string.Empty,
      Payload = new { announcements = result },
    });
  }

  public async Task<IResult> AddAnnouncementAsync(string twitchUser, ulong guildID, ulong channelID, CancellationToken cancellationToken)
  {
    var result = await dataStore.AddAnnouncementAsync(twitchUser, guildID, channelID, cancellationToken);
    return Results.Ok(new ResultMessage()
    {
      Success = true,
      Message = string.Empty,
      Payload = new { announcement = result },
    });
  }

  public async Task<IResult> DeleteAnnouncementAsync(string twitchUser, ulong guildID, ulong? channelID, CancellationToken cancellationToken)
  {
    await dataStore.DeleteAnnouncementAsync(twitchUser, guildID, channelID.HasValue ? [channelID.Value] : [], cancellationToken);
    return Results.Ok(new ResultMessage()
    {
      Success = true,
      Message = string.Empty,
      Payload = new { },
    });
  }
}
