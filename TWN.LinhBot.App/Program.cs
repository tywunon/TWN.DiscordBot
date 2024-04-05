using System;
using Microsoft.Extensions.Configuration;

namespace TWN.LinhBot.App;

internal class Program
{
  private static async Task Main() => await MainAsync();

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

    var client = new Discord.Client(settings.DiscordAppToken, settings.StreamObserverSettings);
    client.Start();

    await Task.Delay(-1);
  }
}
