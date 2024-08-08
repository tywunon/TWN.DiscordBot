using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TWN.DiscordBot.Interfaces;
using TWN.DiscordBot.Interfaces.Types;

namespace TWN.DiscordBot.WebHost.Services;
internal class DataStoreService(IDataStore dataStore) : IDataStoreService
{
  public async Task<IEnumerable<Announcement>> GetAnnouncementsAsync()
    => await dataStore.GetAnnouncementsAsync();

  public async Task<Announcement> AddAnnouncementAsync(string twitchUser, ulong guildID, ulong channelID)
    => await dataStore.AddAnnouncementAsync(twitchUser, guildID, channelID);

  public async Task DeleteAnnouncementAsync(string twitchUser, ulong guildID, ulong? channelID)
    => await dataStore.DeleteAnnouncementAsync(twitchUser, guildID, channelID.HasValue ? [channelID.Value] : [] );
}
