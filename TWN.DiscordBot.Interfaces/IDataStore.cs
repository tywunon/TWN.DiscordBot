using TWN.DiscordBot.Interfaces.Types;

namespace TWN.DiscordBot.Interfaces;
public interface IDataStore : IHealthCheckProvider<bool>
{
  Task DeleteAnnouncementAsync(string twitchUser, ulong guildID, ulong[] channels);
  Task<Announcement> AddAnnouncementAsync(string twitchUser, ulong guildID, ulong channelID);
  Task<IEnumerable<Announcement>> GetAnnouncementsAsync();
  Task<Data> GetDataAsync();
  Task StoreDataAsync(Data data);
}
