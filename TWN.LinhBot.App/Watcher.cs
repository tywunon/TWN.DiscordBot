using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using TWN.LinhBot.App.Twitch;

namespace TWN.LinhBot.App;
internal class Watcher(WatcherSettings settings, Discord.DiscordClient discordClient, Twitch.TwitchClient twitchClient, DataStore.DataStore dataStore, ILogger<Watcher> logger) 
  : BackgroundService
{
  private readonly WatcherSettings _settings = settings;
  private readonly Discord.DiscordClient _discordClient = discordClient;
  private readonly TwitchClient _twitchClient = twitchClient;
  private readonly DataStore.DataStore _dataStore = dataStore;
  private readonly ILogger<Watcher> _logger = logger;
  private readonly List<string> onlineCache = [];

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    await _discordClient.StartAsync();
    while (!stoppingToken.IsCancellationRequested)
    {
      try
      {
        var lookUpData = await _dataStore.GetDataAsync();
        if (lookUpData.Count != 0)
        {
          var twitchUsers = lookUpData.Select(lud => lud.TwitchUser).Distinct();
          var twitchStreamData = await _twitchClient.GetStreams(twitchUsers, stoppingToken) ?? new([], new(string.Empty));

          var onlineUser = twitchStreamData.Data.Select(td => td.User_Login);
          var offlineUser = onlineCache.Except(onlineUser);
          onlineCache.RemoveAll(s => offlineUser.Contains(s));

          if (onlineUser.Any())
          {
            var twitchUserData = await _twitchClient.GetUsers(onlineUser, stoppingToken) ?? new([]);

            var dataGroups = lookUpData
              .Join(twitchStreamData.Data, o => o.TwitchUser, i => i.User_Login, (o, i) => (lookUpData: o, twitchStreamData: i))
              .Join(twitchUserData.Data, o => o.twitchStreamData.User_Login, i => i.Login, (o, i) => (lookUpData: o.lookUpData, twitchStreamData: o.twitchStreamData, twitchUserData: i))
              .GroupBy(d => d.lookUpData.TwitchUser);

            foreach (var dataGroup in dataGroups)
            {
              if (onlineCache.Contains(dataGroup.Key))
                continue;
              onlineCache.Add(dataGroup.Key);
              foreach (var data in dataGroup)
              {
                await _discordClient.SendTwitchMessage(data.lookUpData.GuildID, data.lookUpData.ChannelID, new Discord.TwitchEmnbedData()
                {
                  Title = data.twitchStreamData.Title,
                  UserLogin = data.twitchStreamData.User_Login,
                  UserName = data.twitchStreamData.User_Name,
                  GameName = data.twitchStreamData.Game_Name,
                  UserImage = data.twitchUserData.Profile_Image_Url,
                  ThumbnailURL = data.twitchStreamData.Thumbnail_Url,
                  StartedAt = data.twitchStreamData.Started_At,
                });
              }
            }
          }
        }
      }
      catch(Exception ex)
      {
        _logger.LogError(ex, "An Exception was thrown while watching");
      }
      await Task.Delay(_settings.Delay, stoppingToken);
    }
  }
}
