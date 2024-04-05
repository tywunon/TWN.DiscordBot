using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace TWN.LinhBot.App.Discord;
internal class ObserverTimer(SocketGuild socketGuild, StreamObserverSettingsItem settings)
{
    private readonly Dictionary<string, DateTime> onlineCache = [];
    internal void Start()
    {
        LogMessage($"Creating Timer with {settings.TimerInterval}ms interval");
        var timer = new Timer(settings.TimerInterval);
        timer.Elapsed += HandleElapsed;
        LogMessage($"Staring Timer");
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
            foreach (var streamer in users)
            {
                LogMessage($"Checking User {streamer.Username}");
                var streamActivities = streamer.Activities.Where(a => a.Type == ActivityType.Streaming);
                if (streamActivities.Any())
                {
                    if (onlineCache.ContainsKey(streamer.Username))
                        continue;
                    onlineCache.Upsert(streamer.Username, DateTime.Now);
                }
                else
                {
                    onlineCache.Remove(streamer.Username);
                    continue;
                }

                var startStream = onlineCache[streamer.Username];

                foreach (StreamingGame activity in streamActivities.OfType<StreamingGame>())
                {
                    //await channel.SendMessageAsync($"{streamer.DisplayName} is Streaming");
                    LogMessage($"\tChecking activity {activity.Name}");
                }
            }
        }
    }

    private void LogMessage(string message)
    {
        Console.WriteLine($"[{DateTime.Now:G}][{socketGuild.Name}] {message}");
    }
}
