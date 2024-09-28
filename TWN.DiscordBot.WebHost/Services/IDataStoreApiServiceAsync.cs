using Microsoft.AspNetCore.Http;

namespace TWN.DiscordBot.WebHost.Services;
internal interface IDataStoreApiServiceAsync
{
  Task<IResult> DeleteAnnouncementAsync(string twitchUser, ulong guildID, ulong? channelID, CancellationToken cancellationToken);
  Task<IResult> AddAnnouncementAsync(string twitchUser, ulong guildID, ulong channelID, CancellationToken cancellationToken);
  Task<IResult> GetAnnouncementsAsync(CancellationToken cancellationToken);
}