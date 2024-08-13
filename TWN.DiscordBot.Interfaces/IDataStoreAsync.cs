using TWN.DiscordBot.Interfaces.Types;

namespace TWN.DiscordBot.Interfaces;
public interface IDataStoreAsync : IHealthCheckProviderAsync<bool>
{
  Task DeleteAnnouncementAsync(string twitchUser, ulong guildID, ulong[] channels, CancellationToken cancellationToken);
  Task<Announcement> AddAnnouncementAsync(string twitchUser, ulong guildID, ulong channelID, CancellationToken cancellationToken);
  Task<IEnumerable<Announcement>> GetAnnouncementsAsync(CancellationToken cancellationToken);
  Task<Data> GetDataAsync(CancellationToken cancellationToken);
  Task StoreDataAsync(Data data, CancellationToken cancellationToken);
}
