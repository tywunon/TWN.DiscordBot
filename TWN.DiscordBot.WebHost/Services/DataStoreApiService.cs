using Microsoft.AspNetCore.Http;

using TWN.DiscordBot.Interfaces;
using TWN.DiscordBot.Interfaces.Types;

namespace TWN.DiscordBot.WebHost.Services;
internal class DataStoreApiService(IDataStoreAsync dataStore, ITwitchClientAsync twitchClient) : IDataStoreApiServiceAsync
{
  public async Task<IResult> GetAnnouncementsAsync(CancellationToken cancellationToken)
  {
    var result = await dataStore.GetAnnouncementsAsync(cancellationToken);
    return Results.Ok(new ResultMessage<Payloads.AnnouncementsPayload>()
    {
      Success = true,
      Message = string.Empty,
      Payload = new (result),
    });
  }

  public async Task<IResult> AddAnnouncementAsync(string twitchUser, ulong guildID, ulong channelID, CancellationToken cancellationToken)
  {
    var userResult = await twitchClient.GetUsersAsync([twitchUser], cancellationToken);
    return await userResult.Match(
      async s => 
      {
        var result = await dataStore.AddAnnouncementAsync(twitchUser, guildID, channelID, cancellationToken);
        return Results.Ok(new ResultMessage<Payloads.AnnouncementPayload>()
        {
          Success = true,
          Message = string.Empty,
          Payload = new(result),
        });
      },
      err => 
      {
        return Task.FromResult(Results.Ok(new ResultMessage<Payloads.AnnouncementPayload>()
        {
          Success = false,
          Message = err.Value.Message ?? string.Empty,
          Payload = new Payloads.AnnouncementPayload(new Announcement(twitchUser, 0L, 0L)),
        }));
      }
      );
  }

  public async Task<IResult> DeleteAnnouncementAsync(string twitchUser, ulong guildID, ulong? channelID, CancellationToken cancellationToken)
  {
    await dataStore.DeleteAnnouncementAsync(twitchUser, guildID, channelID.HasValue ? [channelID.Value] : [], cancellationToken);
    return Results.Ok(new ResultMessage<Payloads.EmptyPayload>()
    {
      Success = true,
      Message = string.Empty,
      Payload = new (),
    });
  }
}
