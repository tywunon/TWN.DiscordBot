using TWN.DiscordBot.Interfaces.Types;

namespace TWN.DiscordBot.WebHost.Services;
internal interface IDataStoreServiceAsync
{
  Task DeleteAnnouncementAsync(string twitchUser, ulong guildID, ulong? channelID, CancellationToken cancellationToken);
  Task<Announcement> AddAnnouncementAsync(string twitchUser, ulong guildID, ulong channelID, CancellationToken cancellationToken);
  Task<IEnumerable<Announcement>> GetAnnouncementsAsync(CancellationToken cancellationToken);
}