using TWN.DiscordBot.Interfaces.Types;

namespace TWN.DiscordBot.Interfaces;
public interface IDataStore
{
  Task DeleteAnnouncement(string twitchUser, ulong? guildID, ulong[] channels);
  Task<Announcement> AddAnnouncementAsync(string twitchUser, ulong guildID, ulong channelID);
  Task<Data> GetDataAsync();
  Task StoreData(Data data);
}
