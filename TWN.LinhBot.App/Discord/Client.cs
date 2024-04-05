using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

namespace TWN.LinhBot.App.Discord;
internal class Client(string token, IEnumerable<StreamObserverSettingsItem> settings)
{
  private readonly DiscordSocketClient client = new(new()
  {
    GatewayIntents = GatewayIntents.GuildMembers | GatewayIntents.Guilds | GatewayIntents.GuildMessages
  });

  readonly string token = token;
  readonly IEnumerable<StreamObserverSettingsItem> settings = settings;

  public async void Start()
  {
    await client.LoginAsync(TokenType.Bot, token);
    await client.StartAsync();

    client.Log += HandleLog_Client;
    client.Ready += HandleReady_Client;
  }

  async Task HandleReady_Client()
  {
    await client.SetCustomStatusAsync($"{DateTime.Now:G}");

    if (settings is null) return;

    var configuredGuilds = client.Guilds.Join(settings, o => o.Id, i => i.GuildID, (o, i) => (socketGuild: o, settings: i));

    foreach (var configuredGuild in configuredGuilds)
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
