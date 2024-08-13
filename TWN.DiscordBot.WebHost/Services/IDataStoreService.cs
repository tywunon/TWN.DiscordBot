using TWN.DiscordBot.Interfaces.Types;

namespace TWN.DiscordBot.WebHost.Services;
internal interface IDataStoreService
{
  Task DeleteAnnouncementAsync(string twitchUser, ulong guildID, ulong? channelID);
  Task<Announcement> AddAnnouncementAsync(string twitchUser, ulong guildID, ulong channelID);
  Task<IEnumerable<Announcement>> GetAnnouncementsAsync();
}