using System.Text;
using System.Text.Json;

using TWN.DiscordBot.Interfaces;
using TWN.DiscordBot.Interfaces.Types;
using TWN.DiscordBot.Settings;

namespace TWN.DiscordBot.DataStore;
public class JSONDataStore(DataStoreSettings dataStoreSettings)
: IDataStore
{
  private readonly JsonSerializerOptions jsonSerializerOptions = new()
  {
    WriteIndented = true,
  };

  public async Task<Data> GetDataAsync()
  {
    if (!File.Exists(dataStoreSettings.FilePath))
      return new Data([]);

    var json = await File.ReadAllTextAsync(dataStoreSettings.FilePath, Encoding.UTF8);
    return JsonSerializer.Deserialize<Data>(json, jsonSerializerOptions) ?? new([]);
  }

  public async Task StoreDataAsync(Data data)
  {
    var json = JsonSerializer.Serialize(data, jsonSerializerOptions);
    await File.WriteAllTextAsync(dataStoreSettings.FilePath, json, Encoding.UTF8);
  }

  public async Task<IEnumerable<Announcement>> GetAnnouncementsAsync()
  {
    var data = await GetDataAsync();
    return data.Announcements;
  }

  public async Task<Announcement> AddAnnouncementAsync(string twitchUser, ulong guildID, ulong channelID)
  {
    var existingData = await GetDataAsync();

    var mayExistingData = existingData.Announcements.FirstOrDefault(d => d.TwitchUser == twitchUser && d.GuildID == guildID && d.ChannelID == channelID);
    if (mayExistingData is null)
    {
      var newData = new Announcement(twitchUser, guildID, channelID);
      existingData.Announcements.Add(newData);
      await StoreDataAsync(existingData);
      return newData;
    }
    return mayExistingData;
  }

  public async Task DeleteAnnouncementAsync(string twitchUser, ulong guildID, ulong[] channels)
  {
    var data = await GetDataAsync();
    var deleteData = data.Announcements.Where(d => d.TwitchUser == twitchUser && d.GuildID == guildID);
    if (channels.Length != 0)
      deleteData = deleteData.Where(d => channels.Contains(d.ChannelID));
    foreach (var deleteDate in deleteData.Freeze())
      data.Announcements.Remove(deleteDate);
    await StoreDataAsync(data);
  }

  public Task<bool> HealthCheckAsync(CancellationToken cancellationToken) 
    => Task.FromResult(File.Exists(dataStoreSettings.FilePath));
}
