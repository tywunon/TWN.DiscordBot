using System.Text;
using System.Text.Json;

using TWN.DiscordBot.Interfaces;
using TWN.DiscordBot.Interfaces.Types;
using TWN.DiscordBot.Settings;

namespace TWN.DiscordBot.DataStore;
public class JSONDataStore(DataStoreSettings dataStoreSettings)
: IDataStoreAsync
{
  private readonly JsonSerializerOptions jsonSerializerOptions = new()
  {
    WriteIndented = true,
  };

  public async Task<Data> GetDataAsync(CancellationToken cancellationToken)
  {
    if (!File.Exists(dataStoreSettings.FilePath))
      return new Data([]);

    var json = await File.ReadAllTextAsync(dataStoreSettings.FilePath, Encoding.UTF8, cancellationToken);
    return JsonSerializer.Deserialize<Data>(json, jsonSerializerOptions) ?? new([]);
  }

  public async Task StoreDataAsync(Data data, CancellationToken cancellationToken)
  {
    var json = JsonSerializer.Serialize(data, jsonSerializerOptions);
    await File.WriteAllTextAsync(dataStoreSettings.FilePath, json, Encoding.UTF8, cancellationToken);
  }

  public async Task<IEnumerable<Announcement>> GetAnnouncementsAsync(CancellationToken cancellationToken)
  {
    var data = await GetDataAsync(cancellationToken);
    return data.Announcements;
  }

  public async Task<Announcement> AddAnnouncementAsync(string twitchUser, ulong guildID, ulong channelID, CancellationToken cancellationToken)
  {
    var existingData = await GetDataAsync(cancellationToken);

    var mayExistingData = existingData.Announcements.FirstOrDefault(d => d.TwitchUser == twitchUser && d.GuildID == guildID && d.ChannelID == channelID);
    if (mayExistingData is null)
    {
      var newData = new Announcement(twitchUser, guildID, channelID);
      existingData.Announcements.Add(newData);
      await StoreDataAsync(existingData, cancellationToken);
      return newData;
    }
    return mayExistingData;
  }

  public async Task DeleteAnnouncementAsync(string twitchUser, ulong guildID, ulong[] channels, CancellationToken cancellationToken)
  {
    var data = await GetDataAsync(cancellationToken);
    var deleteData = data.Announcements.Where(d => d.TwitchUser == twitchUser && d.GuildID == guildID);
    if (channels.Length != 0)
      deleteData = deleteData.Where(d => channels.Contains(d.ChannelID));
    foreach (var deleteDate in deleteData.Freeze())
      data.Announcements.Remove(deleteDate);
    await StoreDataAsync(data, cancellationToken);
  }

  public Task<bool> HealthCheckAsync(CancellationToken cancellationToken) 
    => Task.FromResult(File.Exists(dataStoreSettings.FilePath));
}
