using TWN.DiscordBot.Interfaces;
using TWN.DiscordBot.Interfaces.Types;

namespace TWN.DiscordBot.WebHost.Services;
internal class DataStoreService(IDataStoreAsync dataStore) : IDataStoreServiceAsync
{
  public async Task<IEnumerable<Announcement>> GetAnnouncementsAsync(CancellationToken cancellationToken)
    => await dataStore.GetAnnouncementsAsync(cancellationToken);

  public async Task<Announcement> AddAnnouncementAsync(string twitchUser, ulong guildID, ulong channelID, CancellationToken cancellationToken)
    => await dataStore.AddAnnouncementAsync(twitchUser, guildID, channelID, cancellationToken);

  public async Task DeleteAnnouncementAsync(string twitchUser, ulong guildID, ulong? channelID, CancellationToken cancellationToken)
    => await dataStore.DeleteAnnouncementAsync(twitchUser, guildID, channelID.HasValue ? [channelID.Value] : [] , cancellationToken);
}
