using Microsoft.AspNetCore.Http;

using TWN.DiscordBot.Interfaces.Types;

namespace TWN.DiscordBot.WebHost.Services;
internal interface IDataStoreServiceAsync
{
  Task<IResult> DeleteAnnouncementAsync(string twitchUser, ulong guildID, ulong? channelID, CancellationToken cancellationToken);
  Task<IResult> AddAnnouncementAsync(string twitchUser, ulong guildID, ulong channelID, CancellationToken cancellationToken);
  Task<IResult> GetAnnouncementsAsync(CancellationToken cancellationToken);
}