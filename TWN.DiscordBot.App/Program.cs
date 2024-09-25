using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NReco.Logging.File;
using TWN.DiscordBot.App.BackgroundServices;
using TWN.DiscordBot.Interfaces;
using TWN.DiscordBot.WebHost;

namespace TWN.DiscordBot.App;

internal class Program
{
  private static async Task Main(string[] args) => await MainAsync(args);

  private static async Task MainAsync(string[] args)
  {
    //var builder = Host.CreateApplicationBuilder(args);
    var builder = WebApplication.CreateBuilder(args);

    builder.Configuration
      .AddJsonFile($"appsettings.json", false, true);

    var settings = builder.Configuration.GetRequiredSection(nameof(Settings))
      .Get<Settings.Settings>() 
      ?? new Settings.Settings(
        WebHost: new(Test: "Test", 
                     Urls: []),
        Watcher: new(Delay: 1000,
                     Horizon: 5 * 60 * 1000), // 5min
        Discord: new(Status: string.Empty,
                     AppToken: string.Empty),
        Twitch: new(OAuthURL: string.Empty,
                    BaseURL: string.Empty,
                    ClientID: string.Empty,
                    ClientSecret: string.Empty),
        GuildConfig: [],
        DataStore: new(FilePath: string.Empty),
        TCPProbe: new(Port: -1));

    builder.WebHost
      .UseUrls(settings.WebHost.Urls.Distinct().ToArray())
      ;

    builder.Services.AddLogging(b =>
    {
      b.AddConsole()
      .AddFile(@"logs/app_{0:yyyy}-{0:MM}-{0:dd}.log", flo =>
      {
        flo.Append = true;
        flo.FormatLogFileName = fName => string.Format(fName, DateTime.UtcNow);
        flo.FileSizeLimitBytes = 20 * 1024 * 1024; // 20MiB
        flo.MaxRollingFiles = 6;
      });
    });

    builder.Services.AddHttpClient("TwitchAPI", client =>
    {
      client.BaseAddress = new(settings.Twitch.BaseURL);
    });
    builder.Services.AddHttpClient("TwitchOAuth", client =>
    {
      client.BaseAddress = new(settings.Twitch.OAuthURL);
    });

    builder.Services
      .AddSingleton(settings.WebHost)
      .AddSingleton(settings.Watcher)
      .AddSingleton(settings.Discord)
      .AddSingleton(settings.Twitch)
      .AddSingleton(settings.GuildConfig)
      .AddSingleton(settings.DataStore)
      .AddSingleton(settings.TCPProbe)
      .AddSingleton<IDiscordClientAsync, Discord.DiscordClient>()
      .AddSingleton<ITwitchClientAsync, Twitch.TwitchClient>()
      .AddSingleton<IDataStoreAsync, DataStore.JSONDataStore>()
      .AddBotAPIServices()
      //.AddHostedService<Watcher>()
      .AddHostedService<TCPProbeProvider>()
      .AddHostedService<LogCleaner>()
      ;

    var host = builder.Build();
    host.USeBotAPI();
    host.UseStatusCodePages();
    host.UseHttpsRedirection();
    host.MapBotAPI();

    await host.RunAsync();
  }
}
