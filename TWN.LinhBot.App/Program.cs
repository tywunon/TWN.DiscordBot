using System;

using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace TWN.LinhBot.App;

internal class Program
{
  private static async Task Main() => await MainAsync();

  private static readonly DiscordSocketClient client = new(new()
  {
    GatewayIntents = GatewayIntents.GuildMembers | GatewayIntents.All
  });

  private static Settings? settings;
  static async Task MainAsync()
  {
    var config = new ConfigurationBuilder()
      .AddJsonFile("appsettings.json", false, true)
      .Build();

    settings = config.GetRequiredSection(nameof(Settings))
      .Get<Settings>() ?? new Settings()
      {
        DiscordAppToken = string.Empty,
        StreamObserverSettings = [],
      };

    client.Log += HandleLog_Client;

    await client.LoginAsync(TokenType.Bot, settings.DiscordAppToken);
    await client.StartAsync();

    client.Ready += HandleReady_Client;

    await Task.Delay(-1);
  }

  static async Task HandleReady_Client()
  {
    await client.SetCustomStatusAsync($"{DateTime.Now:G}");

    if (settings is null)
      return;

    InitStreamerObserver(settings.StreamObserverSettings);
  }

  private static void InitStreamerObserver(IEnumerable<StreamObserverSettingsItem>? guildStreamSettings)
  {
    if (guildStreamSettings is null) return;

    var configuredGuilds = client.Guilds.Join(guildStreamSettings, o => o.Id, i => i.GuildID, (o, i) => (socketGuild: o, settings: i));

    foreach(var configuredGuild in configuredGuilds)
    {
      new ObserverTimer(configuredGuild.socketGuild, configuredGuild.settings).Start();
    }
  }

  static Task HandleLog_Client(LogMessage message)
  {
    Console.WriteLine(message.ToString());
    return Task.CompletedTask;
  }
}
