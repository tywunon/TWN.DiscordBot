using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.Logging;

using Timer = System.Timers.Timer;

namespace TWN.LinhBot.App;
internal class ObserverTimer(Discord.WebSocket.SocketGuild socketGuild, StreamObserverSettingsItem settings)
{
  private Dictionary<string, DateTime> onlineCache = new Dictionary<string, DateTime>();
  internal void Start()
  {
    LogMessage($"[{socketGuild.Name}] Creating Timer with {settings.TimerInterval}ms interval");
    var timer = new Timer(settings.TimerInterval);
    timer.Elapsed += HandleElapsed;
    LogMessage($"[{socketGuild.Name}] Staring Timer");
    timer.Start();
  }

  private async void HandleElapsed(object? sender, ElapsedEventArgs e)
  {
    LogMessage($"Elapsed");
    LogMessage($"Downloading Users");
    await socketGuild.DownloadUsersAsync();

    foreach (var checkSettingsItem in settings.CheckSettings)
    {
      LogMessage($"Getting Role");
      if (socketGuild.GetRole(checkSettingsItem.RoleID) is not SocketRole role)
      {
        LogMessage($"Unable to find Role {checkSettingsItem.RoleID}");
        continue;
      }

      LogMessage($"Getting Channel");

      if (socketGuild.GetChannel(checkSettingsItem.ChannelID) is not SocketTextChannel channel) 
      {
        LogMessage($"Unable to find Channel {checkSettingsItem.ChannelID}");
        continue; 
      }

      LogMessage($"Using Role ({role.Name}) and Channel({channel.Name})");
      //await channel.SendMessageAsync("Jup");
      var users = socketGuild.Users.Where(u => !u.IsBot && u.Roles.Contains(role));
      foreach(var streamer in users)
      {
        LogMessage($"Checking User {streamer.Username}");
        var streamActivities = streamer.Activities.Where(a => a.Type == ActivityType.Streaming);
        if (streamActivities.Any())
          onlineCache.TryAdd(streamer.Username, DateTime.Now);
        else
        {
          onlineCache.Remove(streamer.Username);
          continue;
        }

        var startStream = onlineCache[streamer.Username];
        if ((DateTime.Now - startStream).Ticks > settings.TimerInterval * 10E4)
          continue;

        foreach (StreamingGame activity in streamActivities.OfType<StreamingGame>())
        {
          LogMessage($"Checking activity {activity.Name}");
        }
      }
    }
  }

  private void LogMessage(string message)
  {
    Console.WriteLine($"[{DateTime.Now:G}][{socketGuild.Name}] {message}");
  }
}
