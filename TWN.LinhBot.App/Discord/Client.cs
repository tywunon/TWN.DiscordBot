using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

namespace TWN.LinhBot.App.Discord;
internal class Client(DiscordAPISettings discordAPISettings, IEnumerable<StreamObserverSettingsItem> settings, Twitch.Client twitchClient, DiscordSocketClient discordSocketClient)
{
  readonly DiscordAPISettings _discordAPISettings = discordAPISettings;
  readonly IEnumerable<StreamObserverSettingsItem> _settings = settings;
  readonly Twitch.Client _twitchClient = twitchClient;
  readonly DiscordSocketClient _discordSocketClient = discordSocketClient;

  public async void Start()
  {
    await _discordSocketClient.LoginAsync(TokenType.Bot, _discordAPISettings.AppToken);

    _discordSocketClient.Log += HandleLog_Client;
    _discordSocketClient.Ready += HandleReady_Client;

    await _discordSocketClient.StartAsync();
  }

  async Task HandleReady_Client()
  {
    await _discordSocketClient.SetCustomStatusAsync($"{DateTime.Now:G}");

    if (_settings is null) return;

    var configuredGuilds = _discordSocketClient.Guilds.Join(_settings, o => o.Id, i => i.GuildID, (o, i) => (socketGuild: o, settings: i));

    foreach (var configuredGuild in configuredGuilds)
    {
    }
  }

  static Task HandleLog_Client(LogMessage message)
  {
    Console.WriteLine(message.ToString());
    return Task.CompletedTask;
  }
}
