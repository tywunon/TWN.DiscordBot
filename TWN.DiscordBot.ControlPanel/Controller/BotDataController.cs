﻿using TWN.DiscordBot.Settings;
using TWN.DiscordBot.WebClient;
using TWN.DiscordBot.Utils;
using System.Numerics;
using OneOf;
using OneOf.Types;
using System.Threading.Channels;

namespace TWN.DiscordBot.ControlPanel.Controller;

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
        try
        {
          var userDataResult = await botClient.GetUserDataAsync(a.TwitchUser);
          var streamDataResult = await botClient.GetStreamDataAsync(a.TwitchUser);
          var guildNameResult = await botClient.GetGuildNameAsync(a.GuildID);
          var guildIconUrlResult = await botClient.GetGuildIconUrlAsync(a.GuildID);
          var channelNameResult = await botClient.GetChannelNameAsync(a.ChannelID);

          var isOnline = streamDataResult.Success && streamDataResult.Payload.IsOnline;

          var guildID = guildNameResult.Success ? guildNameResult.Payload.GuildID : a.GuildID;
          var guildName = guildNameResult.Success ? guildNameResult.Payload.GuildName : a.GuildID.ToString();
          var guildIconUrl = guildIconUrlResult.Success ? guildIconUrlResult.Payload.GuildIconUrl : string.Empty;
          var channelID = channelNameResult.Success ? channelNameResult.Payload.ChannelID : a.ChannelID;
          var channelName = channelNameResult.Success ? channelNameResult.Payload.ChannelName : a.ChannelID.ToString();

          var announcementDiscordData = new AnnouncementDiscordData(guildID, guildName, guildIconUrl, channelID, channelName);

          var announcementTwitchUserData = userDataResult.Success
            ? new AnnouncementTwitchUserData(
                userDataResult.Payload.UsersResponse.Id,
                userDataResult.Payload.UsersResponse.Login,
                userDataResult.Payload.UsersResponse.Display_Name,
                userDataResult.Payload.UsersResponse.Type,
                userDataResult.Payload.UsersResponse.Broadcaster_Type,
                userDataResult.Payload.UsersResponse.Description,
                userDataResult.Payload.UsersResponse.Profile_Image_Url,
                userDataResult.Payload.UsersResponse.Offline_Image_Url,
                userDataResult.Payload.UsersResponse.View_Count,
                userDataResult.Payload.UsersResponse.Created_At)
            : new AnnouncementTwitchUserData(
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                default,
                default);

          var announcementTwitchStreamData = streamDataResult.Success
            ? new AnnouncementTwitchStreamData(
                streamDataResult.Payload.StreamsResponse?.Id ?? string.Empty,
                streamDataResult.Payload.StreamsResponse?.User_Id ?? string.Empty,
                streamDataResult.Payload.StreamsResponse?.User_Login ?? string.Empty,
                streamDataResult.Payload.StreamsResponse?.User_Name ?? string.Empty,
                streamDataResult.Payload.StreamsResponse?.Game_ID ?? string.Empty,
                streamDataResult.Payload.StreamsResponse?.Game_Name ?? string.Empty,
                streamDataResult.Payload.StreamsResponse?.Type ?? string.Empty,
                streamDataResult.Payload.StreamsResponse?.Title ?? string.Empty,
                streamDataResult.Payload.StreamsResponse?.Viewer_Count ?? default,
                streamDataResult.Payload.StreamsResponse?.Started_At ?? default,
                streamDataResult.Payload.StreamsResponse?.Language ?? string.Empty,
                streamDataResult.Payload.StreamsResponse?.Thumbnail_Url ?? string.Empty,
                streamDataResult.Payload.StreamsResponse?.Tag_IDs ?? [],
                streamDataResult.Payload.StreamsResponse?.Tags ?? [],
                streamDataResult.Payload.StreamsResponse?.Is_Mature ?? default
              )
            : new AnnouncementTwitchStreamData(
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                default,
                default,
                string.Empty,
                string.Empty,
                [],
                [],
                default);

          var newAnnouncementData = new AnnouncementData(a.TwitchUser, isOnline,
            AnnouncementDiscordData: announcementDiscordData,
            AnnouncementTwitchUserData: announcementTwitchUserData,
            AnnouncementTwitchStreamData: announcementTwitchStreamData);
          return newAnnouncementData;
        }
        catch (Exception ex)
        {
          logger.LogException(ex, $"GetBotAnnouncementsAsync({a.TwitchUser})");
          return new AnnouncementData(a.TwitchUser, false, 
            AnnouncementDiscordData: new AnnouncementDiscordData(a.GuildID, 
                                       ex.Message, 
                                       string.Empty, 
                                       a.ChannelID,
                                       ex.Message), 
            AnnouncementTwitchUserData: new AnnouncementTwitchUserData(string.Empty, 
                                          string.Empty, 
                                          string.Empty, 
                                          string.Empty, 
                                          string.Empty, 
                                          string.Empty, 
                                          string.Empty, 
                                          string.Empty, 
                                          default, 
                                          default), 
            AnnouncementTwitchStreamData: new AnnouncementTwitchStreamData(string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            default,
                                            default,
                                            string.Empty,
                                            string.Empty,
                                            [],
                                            [],
                                            default));
        }
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

  public async Task<OneOf<Success, Error<string>>> AddAnnouncementAsync(string? botID, string twitchUser, long guildID, long channelID, CancellationToken cancellationToken)
  {
    try
    {
      var botClient = GetBotClient(botID, TimeSpan.FromMilliseconds(2000));
      if (botClient is null)
        return new Error<string>($"no BotClient found for {botID}");

      var addAnnouncementResult = await botClient.AddAnnouncementAsync(twitchUser, guildID, channelID, cancellationToken);
      if (addAnnouncementResult.Success)
        return new Success();
      return new Error<string>(addAnnouncementResult.Message);
    }
    catch (Exception ex)
    {
      logger.LogException(ex, "AddAnnouncement");
      return new Error<string>($"{ex.Message}");
    }
  }

  public async Task<OneOf<Success, Error<string>>> DeleteAnnouncementAsync(string? botID, string twitchUser, long guildID, long? channelID, CancellationToken cancellationToken)
  {
    try
    {
      var botClient = GetBotClient(botID, TimeSpan.FromMilliseconds(2000));
      if (botClient is null)
        return new Error<string>($"no BotClient found for {botID}");

      var deleteAnnouncementResult = await botClient.DeleteAnnouncementAsync(twitchUser, guildID, channelID, cancellationToken);
      if (deleteAnnouncementResult.Success)
        return new Success();
      return new Error<string>(deleteAnnouncementResult.Message);
    }
    catch (Exception ex)
    {
      logger.LogException(ex, "DeleteAnnouncementAsync");
      return new Error<string>($"{ex.Message}");
    }
  }

  public async Task<OneOf<Success, Error<string>>> PostTwitchAnnouncementAsync(string? botID, string twitchUser, long guildID, long channelID, CancellationToken cancellationToken)
  {
    try
    {
      var botClient = GetBotClient(botID, TimeSpan.FromMilliseconds(2000));
      if (botClient is null)
        return new Error<string>($"no BotClient found for {botID}");

      var postTwitchAnnouncementResult = await botClient.PostTwitchAnnouncementAsync(twitchUser, guildID, channelID, cancellationToken);
      if (postTwitchAnnouncementResult.Success)
        return new Success();
      return new Error<string>(postTwitchAnnouncementResult.Message);
    }
    catch (Exception ex)
    {
      logger.LogException(ex, "PostTwitchAnnouncementAsync");
      return new Error<string>($"{ex.Message}");
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