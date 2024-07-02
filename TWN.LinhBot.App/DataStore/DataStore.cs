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

  public async Task<List<Data>> GetDataAsync()
  {
    if (!File.Exists(_dataStoreSettings.FilePath))
      return [];

    var json = await File.ReadAllTextAsync(_dataStoreSettings.FilePath, Encoding.UTF8);
    return JsonSerializer.Deserialize<List<Data>>(json, jsonSerializerOptions) ?? [];
  }

  public async Task StoreData(IEnumerable<Data> data)
  {
    var json = JsonSerializer.Serialize(data, jsonSerializerOptions);
    await File.WriteAllTextAsync(_dataStoreSettings.FilePath, json, Encoding.UTF8);
  }

  public async Task<Data> AddDataAsync(string twitchUser, ulong guildID, ulong channelID)
  {
    var existingData = await GetDataAsync() ?? [];

    var mayExistingData = existingData.FirstOrDefault(d => d.TwitchUser == twitchUser && d.GuildID == guildID && d.ChannelID == channelID);
    if (mayExistingData is null)
    {
      var newData = new Data(twitchUser, guildID, channelID);
      existingData.Add(newData);
      await StoreData(existingData);
      return newData;
    }
    return mayExistingData;
  }

  internal async Task DeleteData(string twitchUser, ulong? guildID, ulong[] channels)
  {
    var data = await GetDataAsync();
    var userData = data.Where(d => d.TwitchUser == twitchUser && d.GuildID == guildID && channels.Contains(d.ChannelID));
    var keepData = data.Except(userData);
    await StoreData(keepData);
  }
}

public sealed record Data(string TwitchUser, ulong GuildID, ulong ChannelID) { }
