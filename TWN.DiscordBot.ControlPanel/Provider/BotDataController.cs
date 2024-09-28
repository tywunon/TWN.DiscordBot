using TWN.DiscordBot.Settings;
using TWN.DiscordBot.WebClient;
using TWN.DiscordBot.Utils;
using System.Numerics;

namespace TWN.DiscordBot.ControlPanel.Provider;

public class BotDataController(IEnumerable<WebClientConfig> webClientConfig, IHttpClientFactory httpClientFactory, ILogger<BotDataController> logger) : IBotDataController
{
  private readonly IEnumerable<WebClientConfig> webClientConfig = webClientConfig.DistinctBy(x => x.Name);

  public async Task<IEnumerable<BotMetaData>> GetBotMetaDataAsync(CancellationToken cancellationToken)
  {
    var result = await Task.WhenAll(webClientConfig.Select(async c =>
    {
      try
      {
        var botClient = GetBotClient(c.ID, TimeSpan.FromMilliseconds(500));
        if (botClient is null)
          return new BotMetaData(c.Name, c.ID, BotMetaDataStatus.Unhealthy);

        var healthReport = await botClient.HealthCheckAsync(cancellationToken);
        return new BotMetaData(c.Name, c.ID, healthReport.Status switch
        {
          nameof(BotMetaDataStatus.Healthy) => BotMetaDataStatus.Healthy,
          nameof(BotMetaDataStatus.Degraded) => BotMetaDataStatus.Degraded,
          nameof(BotMetaDataStatus.Unhealthy) => BotMetaDataStatus.Unhealthy,
          _ => BotMetaDataStatus.Unhealthy
        });
      }
      catch (Exception ex)
      {
        logger.LogException(ex, "GetBotMetaDataAsync");
        return new BotMetaData(c.Name, c.ID, BotMetaDataStatus.Unhealthy);
      }
    }));
    return [.. result];
  }

  public Task<bool> IsBotConfiguredAsync(string? botID, CancellationToken cancellationToken)
    => Task.FromResult(webClientConfig.Any(wcc => wcc.ID == botID));

  public async Task<IEnumerable<AnnouncementData>> GetBotAnnouncementsAsync(string? botID, CancellationToken cancellationToken)
  {
    try
    {
      var botClient = GetBotClient(botID, TimeSpan.FromMilliseconds(2000));
      if (botClient is null)
        return [];

      var announcementsResult = await botClient.GetAnnouncementsAsync(cancellationToken);
      if (!announcementsResult.Success)
        return [];

      var result = await Task.WhenAll(announcementsResult.Payload.Announcements.Select(async (a, idx) =>
      {
        var userDataResult = await botClient.GetUserDataAsync(a.TwitchUser);
        var streamDataResult = await botClient.GetStreamDataAsync(a.TwitchUser);
        var guildNameResult = await botClient.GetGuildNameAsync(a.GuildID);
        var guildIconUrlResult = await botClient.GetGuildIconUrlAsync(a.GuildID);
        var channelNameResult = await botClient.GetChannelNameAsync(a.ChannelID);

        var twitchLogin = userDataResult.Success ? userDataResult.Payload.UsersResponse.Login : a.TwitchUser;
        var twitchDisplayName = userDataResult.Success ? userDataResult.Payload.UsersResponse.Display_Name : a.TwitchUser;
        var twitchProfilePicture = userDataResult.Success ? userDataResult.Payload.UsersResponse.Profile_Image_Url : string.Empty;
        var isOnline = streamDataResult.Success && streamDataResult.Payload.IsOnline;
        var guildName = guildNameResult.Success ? guildNameResult.Payload.GuildName : a.GuildID.ToString();
        var guildIconUrl = guildIconUrlResult.Success ? guildIconUrlResult.Payload.GuildIconUrl : string.Empty;
        var channelName = channelNameResult.Success ? channelNameResult.Payload.ChannelName : a.GuildID.ToString();

        var newAnnouncementData = new AnnouncementData(a.TwitchUser, isOnline,
          AnnouncementDiscordData: new(channelName, guildIconUrl, channelName),
          AnnouncementTwitchUserData: new(twitchLogin, twitchDisplayName, twitchProfilePicture),
          AnnouncementTwitchStreamData: new());
        return newAnnouncementData;
      }));
      return [.. result];
    }
    catch (Exception ex)
    {
      logger.LogException(ex, "GetBotAnnouncementsAsync");
      return [];
    }
  }

  public Task<string?> GetBotNameAsync(string? botID, CancellationToken cancellationToken)
  {
    return Task.FromResult(GetWebClientConfig(botID)?.Name);
  }

  public async Task<DiscordClientData> GetDiscordClientDataAsync(string? botID, CancellationToken cancellationToken)
  {
    try
    {
      var botClient = GetBotClient(botID, TimeSpan.FromMilliseconds(2000));
      if (botClient is null)
        return new([]);

      var clientData = await botClient.GetClientDataAsync(cancellationToken);
      if (clientData.Success)
        return new(clientData.Payload.DiscordClientData.GuildData.Select(gd => new DiscordClientGuildData(gd.GuildID, gd.GuildName, gd.GuildIconUrl, gd.DiscordChannelData.Select(dcd => new DiscordClientChannelData(dcd.ChannelID, dcd.ChannelName, dcd.ChannelPosition, dcd.CategoryID, dcd.CategoryName, dcd.CategoryPosition)))));

      return new([]);
    }
    catch (Exception ex)
    {
      logger.LogException(ex, "GetBotNameAsync");
      return new([]);
    }
  }

  private BotClient? GetBotClient(string? botID, TimeSpan timeout)
  {
    var config = GetWebClientConfig(botID);
    if (config is null)
      return null;

    var httpClient = httpClientFactory.CreateClient();
    httpClient.Timeout = timeout;
    return new BotClient(config.BaseURL, httpClient);
  }

  private WebClientConfig? GetWebClientConfig(string? botID)
    => webClientConfig.FirstOrDefault(wcc => wcc.ID == botID);
}

public record BotMetaData(string Name, string ID, BotMetaDataStatus Status);
public enum BotMetaDataStatus
{
  Unhealthy = 0,
  Degraded = 1,
  Healthy = 2,
}

public record AnnouncementData(string TwitchUser, bool IsOnline, AnnouncementDiscordData AnnouncementDiscordData, AnnouncementTwitchUserData AnnouncementTwitchUserData, AnnouncementTwitchStreamData AnnouncementTwitchStreamData);
public record AnnouncementDiscordData(string GuildName, string GuildIconUrl, string ChannelName);
public record AnnouncementTwitchUserData();

public record AnnouncementTwitchStreamData();
public record DiscordClientData(IEnumerable<DiscordClientGuildData> GuildData);
public record DiscordClientGuildData(long GuildID, string GuildName, string GuildIconUrl, IEnumerable<DiscordClientChannelData> DiscordChannelData);
public record DiscordClientChannelData(long ChannelID, string ChannelName, int ChannelPosition, long CategoryID, string CategoryName, int CategoryPosition);