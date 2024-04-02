using System.Linq;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using TWN.LinhBot.App;

internal class Program
{
  private static async Task Main (string[] args)
  {

    await MainAsync(args);
  }

  private static readonly DiscordSocketClient client = new(new()
  {
    GatewayIntents = GatewayIntents.GuildMembers | GatewayIntents.All
  });
  static async Task MainAsync (string[] args)
  {
    var config = new ConfigurationBuilder()
      .AddJsonFile("appsettings.json", false, true)
      .Build();

    Settings? settings = config.GetRequiredSection(nameof(Settings))
      .Get<Settings>() ?? new Settings() 
      { 
        DiscordAppToken = string.Empty 
      };

    client.Log += HandleLog_Client;

    await client.LoginAsync(TokenType.Bot, settings.DiscordAppToken);
    await client.StartAsync();

    client.Ready += HandleReady_Client;
    client.MessageReceived += HandleMessageReceived_Client;
    client.SlashCommandExecuted += HandleSlashCommandExecuted_Client;

    await Task.Delay(-1);
  }

  private static async Task HandleSlashCommandExecuted_Client (SocketSlashCommand command)
  {
    if (command.Channel.Name == "section-31")
    {
      var silent = command.Data.Options.FirstOrDefault(o => o.Name == "silent")?.Value is bool silentValue && silentValue;
      var text = "Wusch die Waldfee!";
      if (silent)
      {
        await command.Channel.SendMessageAsync(text);
        await command.RespondAsync("🤫", ephemeral: true);
      }
      else
      {
        await command.RespondAsync(text, ephemeral: false);
      }
    }
  }

  private static async Task HandleMessageReceived_Client (SocketMessage message)
  {
    if (message.Author.Id == client.CurrentUser.Id)
      return;
    if (message.Channel.Name == "section-31")
    {
      await message.Channel.SendMessageAsync(message.CleanContent);
    }
  }

  static async Task HandleReady_Client ()
  {
    await client.SetCustomStatusAsync($"{DateTime.Now:G}");

    await InitSlashCommands();
    await InitRollsObserver();
  }

  private static async Task InitRollsObserver ()
  {
    //Roletests
    var guild = client.Guilds.FirstOrDefault();
    if (guild is null)
      return;
    //if (guild is not null)
    {
      var userID = guild.OwnerId;
      var roles = string.Join(',', guild.Roles.Select(r => $"{r.Id}:{r.Name}"));
      Console.WriteLine($"roles: {roles}");
      await guild.DownloadUsersAsync();
      var users = guild.Users.Where(u => !u.IsBot);
      foreach (var user in users)
      {
        Console.WriteLine($"{user.DisplayName}|{user.Nickname}|{user.Username}|{user.GlobalName}|{user.Status}");
        foreach (var activity in user.Activities)
        {
          Console.WriteLine($"\t{activity.Type}|{activity.Name}|{activity.Flags}|{activity.Details}");
        }
      }
    }
  }

  private static async Task InitSlashCommands ()
  {
    var command = new SlashCommandBuilder()
          .WithName("wusch")
          .WithDescription("Makes Wusch")
          .AddOption("silent", ApplicationCommandOptionType.Boolean, "Shh", isRequired: true);

    try
    {
      await client.Rest.CreateGuildCommand(command.Build(), 172612129679998977);
    }
    catch (HttpException ex)
    {
      var json = JsonConvert.SerializeObject(ex.Errors, Formatting.Indented);
      Console.WriteLine(json);
    }
  }

  static Task HandleLog_Client (LogMessage message)
  {
    Console.WriteLine(message.ToString());
    return Task.CompletedTask;
  }
}
