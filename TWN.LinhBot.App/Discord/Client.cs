using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using LanguageExt.Pipes;

using Microsoft.Extensions.Logging;

namespace TWN.LinhBot.App.Discord;
internal class Client(DiscordSettings discordSettings, Twitch.Client twitchClient, DataStore.DataStore dataStore, IEnumerable<GuildConfig> guildConfig)
{
  readonly DiscordSettings _discordSettings = discordSettings;
  private readonly Twitch.Client _twitchClient = twitchClient;
  private readonly DataStore.DataStore _dataStore = dataStore;
  private readonly IEnumerable<GuildConfig> _guildConfig = guildConfig;
  readonly DiscordSocketClient discordSocketClient = new(new()
  {
    GatewayIntents = GatewayIntents.GuildMessages
  });
  bool ready = false;

  public async Task StartAsync()
  {
    await discordSocketClient.LoginAsync(TokenType.Bot, _discordSettings.AppToken);

    discordSocketClient.Log += HandleLog_Client;
    discordSocketClient.Ready += HandleReady_Client;

    await discordSocketClient.StartAsync();

    while (!ready) { await Task.Delay(100); }
  }

  async Task HandleReady_Client()
  {
    await discordSocketClient.SetCustomStatusAsync(_discordSettings.Status);
    await discordSocketClient.SetStatusAsync(UserStatus.Idle);

    try
    {
      var guildCommand = new SlashCommandBuilder()
        .WithName("add-stream")
        .WithDescription("Adds annother Stream to the Announcement Queue")
        .WithDefaultMemberPermissions(GuildPermission.ModerateMembers)
        .AddOption(name: "twitch-user", type: ApplicationCommandOptionType.String, description: "Twitch User Name", isRequired: true)
        .AddOption(name: "channel", type: ApplicationCommandOptionType.Channel, description: "Channel", isRequired: true)
        .Build();

      await discordSocketClient.CreateGlobalApplicationCommandAsync(guildCommand);

      guildCommand = new SlashCommandBuilder()
        .WithName("remove-stream")
        .WithDescription("removes Stream from the Announcement Queue")
        .WithDefaultMemberPermissions(GuildPermission.ModerateMembers)
        .AddOption(name: "twitch-user", type: ApplicationCommandOptionType.String, description: "Twitch User Name", isRequired: true)
        .AddOption(name: "channel", type: ApplicationCommandOptionType.Channel, description: "Channel", isRequired: false)
        .Build();

      await discordSocketClient.CreateGlobalApplicationCommandAsync(guildCommand);

      discordSocketClient.SlashCommandExecuted += HandleSlashCommandExecuted_Client;
    }
    catch (Exception ex)
    {
      await HandleLog_Client(new LogMessage(LogSeverity.Error, "slashCreation", ex.Message, ex));
    }

    ready = true;
  }

  private async Task HandleSlashCommandExecuted_Client(SocketSlashCommand command)
  {
    if (command is null)
      return;
    switch (command.CommandName)
    {
      case "add-stream":
        {
          await AddStream(command);
        }
        break;
      case "remove-stream":
        {
          await RemoveStream(command);
        }
        break;
      default: break;
    }
    return;
  }

  private async Task AddStream(SocketSlashCommand command)
  {
    var twitchUserOption = command.Data.Options.FirstOrDefault(o => o.Name == "twitch-user");
    if (twitchUserOption is null)
    {
      await command.RespondAsync("twitch-user not specified");
      return;
    }

    var channelOption = command.Data.Options.FirstOrDefault(o => o.Name == "channel");
    if (channelOption is null)
    {
      await command.RespondAsync("channel not specified");
      return;
    }

    var twitchUser = twitchUserOption.Value is string _twitchUser ? _twitchUser : string.Empty;
    if (string.IsNullOrWhiteSpace(twitchUser))
    {
      await command.RespondAsync($"twitch-user ({twitchUser}) is empty");
      return;
    }

    var cancellationToken = new CancellationTokenSource().Token;

    var twitchUserInfo = await _twitchClient.GetUsers([twitchUser], cancellationToken);
    if(twitchUserInfo?.Data.Length == 0)
    {
      await command.RespondAsync($"twitch-user ({twitchUser}) not found");
      return;
    }

    if (channelOption.Value is not ITextChannel channel)
    {
      await command.RespondAsync($"channel ({channelOption.Value}) is not a text-channel");
      return;
    }

    await _dataStore.AddDataAsync(twitchUser, channel.GuildId, channel.Id);

    await command.RespondAsync($"**{twitchUser}**'s streams will be announced in {channel.Mention}");
  }

  public async Task SendTwitchMessage(ulong guildID, ulong channelID, TwitchEmnbedData twitchData)
  {
    if (twitchData is null) return;

    var guildConfig = _guildConfig.FirstOrDefault(gc => gc.GuildID == guildID);
    if (guildConfig is null) return;

    var guild = discordSocketClient.GetGuild(guildID);
    if (guild is null) return;
    var channel = guild.GetTextChannel(channelID);
    if (channel is null) return;

    var color = uint.TryParse(guildConfig.Color.Replace("#", ""), System.Globalization.NumberStyles.HexNumber, null, out uint _value) ? _value : 0;

    var thumbnailURL = twitchData.ThumbnailURL
      .Replace("{width}", $"{guildConfig.ThumbnailWidth}")
      .Replace("{height}", $"{guildConfig.ThumbnailHeight}");

    var embed = new EmbedBuilder()
      .WithColor(color)
      .WithAuthor(twitchData.UserName)
      .WithTitle($"{twitchData.UserName} ist online mit {twitchData.GameName}")
      .WithDescription(twitchData.Title)
      .WithThumbnailUrl(twitchData.UserImage)
      .WithImageUrl(thumbnailURL)
      .WithUrl($"https://twitch.tv/{twitchData.UserLogin}")
      .WithTimestamp(twitchData.StartedAt)
      .Build();

    await channel.SendMessageAsync(text: guildConfig.Text, embed: embed);
  }

  private async Task RemoveStream(SocketSlashCommand command)
  {
    var twitchUserOption = command.Data.Options.FirstOrDefault(o => o.Name == "twitch-user");
    if (twitchUserOption is null)
    {
      await command.RespondAsync("twitch-user not specified");
      return;
    }

    var channelOption = command.Data.Options.FirstOrDefault(o => o.Name == "channel");
  }

  static Task HandleLog_Client(LogMessage message)
  {
    Console.WriteLine(message.ToString());
    return Task.CompletedTask;
  }
}

public sealed class TwitchEmnbedData
{
  public required string Title { get; init; }
  public required string UserLogin { get; init; }
  public required string UserName { get; init; }
  public required string GameName { get; init; }
  public required string UserImage { get; init; }
  public required string ThumbnailURL { get; init; }
  public required DateTime StartedAt { get; init; }
}
