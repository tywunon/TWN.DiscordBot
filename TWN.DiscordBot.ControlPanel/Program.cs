using TWN.DiscordBot.ControlPanel.Components;
using TWN.DiscordBot.ControlPanel.Provider;

namespace TWN.DiscordBot.ControlPanel;
internal class Program
{
  private static void Main(string[] args)
  {
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddBlazorBootstrap();
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    builder.Configuration
      .AddJsonFile($"appsettings.json", false, true);

    var settings = builder.Configuration.GetRequiredSection("Settings")
      .Get<Settings.ControlPanelSettings>()
      ?? new Settings.ControlPanelSettings(
        WebClient: [],
        Twitch: new(OAuthURL: string.Empty,
                    BaseURL: string.Empty,
                    ClientID: string.Empty,
                    ClientSecret: string.Empty));

    //foreach (var webClient in settings.WebClient)
    //{
    //  builder.Services.AddHttpClient(webClient.ID, client =>
    //  {
    //    client.BaseAddress = new(webClient.BaseURL);
    //    client.Timeout = TimeSpan.FromMilliseconds(500);
    //  });
    //}
    builder.Services.AddHttpClient();

    builder.Services
      .AddSingleton<IBotDataController, BotDataController>()
      .AddSingleton(settings.WebClient);

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
      app.UseExceptionHandler("/Error", createScopeForErrors: true);

    app.UseStaticFiles();
    app.UseAntiforgery();

    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    app.Run();
  }
}