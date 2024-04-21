using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Discord.WebSocket;
using Discord;

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
        DiscordAPI = new() { AppToken = string.Empty },
        TwitchAPI = new()
        {
          OAuthURL = string.Empty,
          BaseURL = string.Empty,
          ClientID = string.Empty,
          ClientSecret = string.Empty,
        },
        StreamObserverSettings = [],
      };

    builder.Services.AddHttpClient("TwitchAPI", client =>
    {
      client.BaseAddress = new(settings.TwitchAPI.BaseURL);
    });

    builder.Services.AddHttpClient("TwitchOAuth", client =>
    {
      client.BaseAddress = new(settings.TwitchAPI.OAuthURL);
    });

    builder.Services.AddSingleton(settings.TwitchAPI);
    builder.Services.AddSingleton(settings.DiscordAPI);
    builder.Services.AddSingleton(settings.StreamObserverSettings);
    builder.Services.AddSingleton<DiscordSocketClient, DiscordSocketClient>(sp =>
    {
      return new DiscordSocketClient(new()
      {
        GatewayIntents = GatewayIntents.GuildMessages,
      });
    });
    builder.Services.AddSingleton<Twitch.Client, Twitch.Client>();
    builder.Services.AddSingleton<Discord.Client, Discord.Client>();

    var host = builder.Build();

    var discordClient = host.Services.GetService<Discord.Client>();
    discordClient?.Start();

    await host.RunAsync();
  }
}
