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

    PeriodicTimer timer = new(TimeSpan.FromMilliseconds(_settings.Delay));
    _logger.LogDebug("{obj} created ({Delay})", nameof(timer), _settings.Delay);

    try
    {
      while (await timer.WaitForNextTickAsync(stoppingToken))
      {
        try
        {
          var lookUpData = await _dataStore.GetDataAsync();
          _logger.Log(LogLevel.Debug, new EventId(), lookUpData, null, (s, ex) => "lookUpData:" + string.Join(", ", s.Select(_s => $"{(_s.TwitchUser, _s.GuildID, _s.ChannelID)}")));
          if (lookUpData.Count != 0)
          {
            var twitchUsers = lookUpData.Select(lud => lud.TwitchUser).Distinct();
            _logger.Log(LogLevel.Debug, new EventId(), twitchUsers, null, (s, ex) => "twitchUsers:" + string.Join(", ", s));
            var twitchStreamData = await _twitchClient.GetStreams(twitchUsers, stoppingToken) ?? new([], new(string.Empty));
            _logger.Log(LogLevel.Debug, new EventId(), twitchStreamData, null, (s, ex) => "twitchStreamData:" + string.Join(", ", s.Data.Select(_s => $"{_s.User_Login}")));

            var onlineUser = twitchStreamData.Data.Select(td => td.User_Login);
            _logger.Log(LogLevel.Debug, new EventId(), onlineUser, null, (s, ex) => "onlineUser:" + string.Join(", ", s));
            var offlineUser = onlineCache.Except(onlineUser);
            _logger.Log(LogLevel.Debug, new EventId(), offlineUser, null, (s, ex) => "offlineUser:" + string.Join(", ", s));

            onlineCache.RemoveAll(s => offlineUser.Contains(s));
            _logger.Log(LogLevel.Debug, new EventId(), onlineCache, null, (s, ex) => "onlineCache:" + string.Join(", ", s));

            if (onlineUser.Any())
            {
              var twitchUserData = await _twitchClient.GetUsers(onlineUser, stoppingToken) ?? new([]);
              _logger.Log(LogLevel.Debug, new EventId(), twitchUserData, null, (s, ex) => "twitchUserData:" + string.Join(", ", s.Data.Select(_s => $"{_s.Login}")));

              var dataGroups = lookUpData
                .Join(twitchStreamData.Data, o => o.TwitchUser, i => i.User_Login, (o, i) => (lookUpData: o, twitchStreamData: i))
                .Join(twitchUserData.Data, o => o.twitchStreamData.User_Login, i => i.Login, (o, i) => (lookUpData: o.lookUpData, twitchStreamData: o.twitchStreamData, twitchUserData: i))
                .GroupBy(d => (twitchUser: d.lookUpData.TwitchUser, d.twitchStreamData.Game_Name));
              _logger.Log(LogLevel.Debug, new EventId(), dataGroups, null, (s, ex) => "dataGroups:" + string.Join(", ", s.Select(_s => $"{(_s.Key, string.Join(", ", _s.Select(_s1 => (_s1.lookUpData.GuildID, _s1.lookUpData.ChannelID, _s1.twitchStreamData.Game_Name))))}")));

              foreach (var dataGroup in dataGroups)
              {
                _logger.Log(LogLevel.Debug, new EventId(), dataGroup, null, (s, ex) => "dataGroup:" + string.Join(", ", s.Select(_s => (_s.lookUpData.GuildID, _s.lookUpData.ChannelID, _s.twitchStreamData.Game_Name))));
                _logger.Log(LogLevel.Debug, new EventId(), onlineCache, null, (s, ex) => "onlineCache-pre:" + dataGroup.Key + ":" + string.Join(", ", s));
                if (onlineCache.Contains(dataGroup.Key.twitchUser))
                  continue;

                onlineCache.Add(dataGroup.Key.twitchUser);
                _logger.Log(LogLevel.Debug, new EventId(), onlineCache, null, (s, ex) => "onlineCache-post:" + string.Join(", ", s));

                foreach (var data in dataGroup)
                {
                  _logger.Log(LogLevel.Debug, new EventId(), data, null, (s, ex) => "data:" + dataGroup.Key + ":" + (s.lookUpData.GuildID, s.lookUpData.ChannelID, s.twitchStreamData.Game_Name));
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
        catch (Exception ex)
        {
          _logger.LogError(ex, "An Exception was thrown while watching");
        }
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "An Exception was thrown trying to watch");
    }
  }
}
