using System.Text;
using System.Text.Json;

namespace TWN.LinhBot.App.DataStore;
internal class DataStore(DataStoreSettings dataStoreSettings)
{
  private readonly DataStoreSettings _dataStoreSettings = dataStoreSettings;
  readonly JsonSerializerOptions jsonSerializerOptions = new()
  {
    WriteIndented = true,
  };

  public async Task<Data> GetDataAsync()
  {
    if (!File.Exists(_dataStoreSettings.FilePath))
      return new Data([]);

    var json = await File.ReadAllTextAsync(_dataStoreSettings.FilePath, Encoding.UTF8);
    return JsonSerializer.Deserialize<Data>(json, jsonSerializerOptions) ?? new([]);
  }

  public async Task StoreData(Data data)
  {
    var json = JsonSerializer.Serialize(data, jsonSerializerOptions);
    await File.WriteAllTextAsync(_dataStoreSettings.FilePath, json, Encoding.UTF8);
  }

  public async Task<Announcement> AddAnnouncementAsync(string twitchUser, ulong guildID, ulong channelID)
  {
    var existingData = await GetDataAsync();

    var mayExistingData = existingData.Announcements.FirstOrDefault(d => d.TwitchUser == twitchUser && d.GuildID == guildID && d.ChannelID == channelID);
    if (mayExistingData is null)
    {
      var newData = new Announcement(twitchUser, guildID, channelID);
      existingData.Announcements.Add(newData);
      await StoreData(existingData);
      return newData;
    }
    return mayExistingData;
  }

  internal async Task DeleteAnnouncement(string twitchUser, ulong? guildID, ulong[] channels)
  {
    var data = await GetDataAsync();
    var deleteData = data.Announcements.Where(d => d.TwitchUser == twitchUser && d.GuildID == guildID && channels.Contains(d.ChannelID));
    foreach(var deleteDate in deleteData)
      data.Announcements.Remove(deleteDate);
    await StoreData(data);
  }
}

public sealed record Data(ICollection<Announcement> Announcements) { }

public sealed record Announcement(string TwitchUser, ulong GuildID, ulong ChannelID) { }
