using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace TWN.LinhBot.App;

internal class Program
{
  private static async Task Main(string[] args) => await MainAsync(args);

  private static Settings? settings;
  static async Task MainAsync(string[] args)
  {
    var builder = Host.CreateApplicationBuilder(args);

    builder.Configuration
      .AddJsonFile("appsettings.json", false, true);

    settings = builder.Configuration.GetRequiredSection(nameof(Settings))
      .Get<Settings>() ?? new Settings()
      {
        Watcher = new() { Delay = 1000, },
        Discord = new()
        {
          Status = string.Empty,
          AppToken = string.Empty,
        },
        Twitch = new()
        {
          OAuthURL = string.Empty,
          BaseURL = string.Empty,
          ClientID = string.Empty,
          ClientSecret = string.Empty,
        },
        GuildConfig = [],
        DataStore = new() { FilePath = "dataStore.twn" },
      };

    builder.Services.AddHttpClient("TwitchAPI", client =>
    {
      client.BaseAddress = new(settings.Twitch.BaseURL);
    });
    builder.Services.AddHttpClient("TwitchOAuth", client =>
    {
      client.BaseAddress = new(settings.Twitch.OAuthURL);
    });

    builder.Services
      .AddSingleton(settings.Watcher)
      .AddSingleton(settings.Discord)
      .AddSingleton(settings.Twitch)
      .AddSingleton(settings.GuildConfig)
      .AddSingleton(settings.DataStore)
      .AddHostedService<Watcher>()
      .AddSingleton<Discord.Client, Discord.Client>()
      .AddSingleton<Twitch.Client, Twitch.Client>()
      .AddSingleton<DataStore.DataStore, DataStore.DataStore>()
      ;

    var host = builder.Build();

    await host.RunAsync();
  }
}
