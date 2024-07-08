namespace TWN.LinhBot.App.DataStore;
internal interface IDataStore
{
  Task DeleteAnnouncement(string twitchUser, ulong? guildID, ulong[] channels);
  Task<Announcement> AddAnnouncementAsync(string twitchUser, ulong guildID, ulong channelID);
  Task<Data> GetDataAsync();
  Task StoreData(Data data);
}
