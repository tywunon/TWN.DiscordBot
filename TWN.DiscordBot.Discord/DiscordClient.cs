
using Discord;

using Discord.WebSocket;

using LanguageExt.Pipes;

using Microsoft.Extensions.Logging;

using OneOf;

using OneOf.Types;

using System.Web;

using TWN.DiscordBot.Interfaces;
using TWN.DiscordBot.Interfaces.Types;
using TWN.DiscordBot.Settings;
using TWN.DiscordBot.Utils;

namespace TWN.DiscordBot.Discord;
public class DiscordClient : Interfaces.IDiscordClientAsync
{
  private readonly DiscordSocketClient discordSocketClient = new(new()
  {
    GatewayIntents = GatewayIntents.GuildMessages | GatewayIntents.GuildIntegrations | GatewayIntents.Guilds,
    LogGatewayIntentWarnings = true,
    MaxWaitBetweenGuildAvailablesBeforeReady = 60000,
  });

  private readonly DiscordSettings discordSettings;
  private readonly ITwitchClientAsync twitchClient;
  private readonly IDataStoreAsync dataStore;
  private readonly IEnumerable<GuildConfig> guildConfigs;
  private readonly ILogger<DiscordClient> logger;

  private bool ready = false;

  public DiscordClient(DiscordSettings discordSettings,
                       ITwitchClientAsync twitchClient,
                       IDataStoreAsync dataStore,
                       IEnumerable<GuildConfig> guildConfigs,
                       ILogger<DiscordClient> logger)
  {
    this.discordSettings = discordSettings;
    this.twitchClient = twitchClient;
    this.dataStore = dataStore;
    this.guildConfigs = guildConfigs;
    this.logger = logger;

    InitSocketClient();
  }

  private void InitSocketClient()
  {
    discordSocketClient.Log += HandleLog_Client;
    discordSocketClient.Ready += HandleReady_Client;

    Task.Run(StartAsync).Wait();
  }

  private async Task StartAsync()
  {
    if (ready)
      return;

    await discordSocketClient.LoginAsync(TokenType.Bot, discordSettings.AppToken);
    await discordSocketClient.StartAsync();

    while (!ready)
      await Task.Delay(100);
  }

  private Task HandleLog_Client(LogMessage message)
  {
    WriteLog(message);
    return Task.CompletedTask;
  }

  private void WriteLog(LogMessage message)
  {
    var logLevel = message.Severity switch
    {
      LogSeverity.Critical => LogLevel.Critical,
      LogSeverity.Error => LogLevel.Error,
      LogSeverity.Warning => LogLevel.Warning,
      LogSeverity.Info => LogLevel.Information,
      LogSeverity.Verbose => LogLevel.Trace,
      LogSeverity.Debug => LogLevel.Debug,
      _ => LogLevel.Information
    };
    logger.Log(logLevel, message.Exception, "[{Source}] {Message}", message.Source, message.Message);
  }

  private async Task HandleReady_Client()
  {
    await discordSocketClient.SetCustomStatusAsync(discordSettings.Status);
    await discordSocketClient.SetStatusAsync(UserStatus.Idle);

    await CreateSlashCommands();

    ready = true;
  }

  private async Task CreateSlashCommands()
  {
    try
    {
      var guildCommand = new SlashCommandBuilder()
        .WithName("add-stream")
        .WithDescription("Adds another Stream to the Announcement Queue")
        .WithDefaultMemberPermissions(GuildPermission.ManageChannels)
        .AddOption(name: "twitch-user", type: ApplicationCommandOptionType.String, description: "Twitch User Name", isRequired: true)
        .AddOption(name: "channel", type: ApplicationCommandOptionType.Channel, description: "Channel", isRequired: true)
        .Build();

      await discordSocketClient.CreateGlobalApplicationCommandAsync(guildCommand);

      guildCommand = new SlashCommandBuilder()
        .WithName("list-streams")
        .WithDescription("Lists all stream in the Announcement Queue")
        .WithDefaultMemberPermissions(GuildPermission.ManageChannels)
        .Build();

      await discordSocketClient.CreateGlobalApplicationCommandAsync(guildCommand);

      guildCommand = new SlashCommandBuilder()
        .WithName("remove-stream")
        .WithDescription("removes Stream from the Announcement Queue")
        .WithDefaultMemberPermissions(GuildPermission.ManageChannels)
        .AddOption(name: "twitch-user", type: ApplicationCommandOptionType.String, description: "Twitch User Name", isRequired: true)
        .AddOption(name: "channel", type: ApplicationCommandOptionType.Channel, description: "Channel", isRequired: false)
        .Build();

      await discordSocketClient.CreateGlobalApplicationCommandAsync(guildCommand);

      discordSocketClient.SlashCommandExecuted += HandleSlashCommandExecuted_Client;
    }
    catch (Exception ex)
    {
      WriteLog(new LogMessage(LogSeverity.Error, "slashCreation", ex.Message, ex));
    }
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
      case "list-streams":
        {
          await ListStreams(command);
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
    try
    {
      var twitchUserOption = command.Data.Options.FirstOrDefault(o => o.Name == "twitch-user");
      if (twitchUserOption is null)
      {
        await command.RespondAsync("[twitch-user] not specified", ephemeral: true);
        return;
      }

      var channelOption = command.Data.Options.FirstOrDefault(o => o.Name == "channel");
      if (channelOption is null)
      {
        await command.RespondAsync("Channel not specified", ephemeral: true);
        return;
      }

      var twitchUser = twitchUserOption.Value is string _twitchUser ? _twitchUser : string.Empty;
      if (string.IsNullOrWhiteSpace(twitchUser))
      {
        await command.RespondAsync($"[twitch-user] ({twitchUser}) is empty", ephemeral: true);
        return;
      }

      var cancellationTokenSource = new CancellationTokenSource();
      var twitchUserInfo = await twitchClient.GetUsersAsync([twitchUser], cancellationTokenSource.Token);
      await twitchUserInfo.Match(
        async success =>
        {
          if (success.Value.Data.Length == 0)
          {
            await command.RespondAsync($"[twitch-user] ({twitchUser}) not found", ephemeral: true);
            return;
          }

          if (channelOption.Value is not ITextChannel channel)
          {
            await command.RespondAsync($"Channel ({channelOption.Value}) is not a text-channel", ephemeral: true);
            return;
          }

          await dataStore.AddAnnouncementAsync(twitchUser, channel.GuildId, channel.Id, cancellationTokenSource.Token);

          await command.RespondAsync($"**{twitchUser}**'s streams will be announced in {channel.Mention}", ephemeral: true);
        },
        error => Task.FromResult(error)
      );
    }
    catch (Exception ex)
    {
      WriteLog(new LogMessage(LogSeverity.Error, "AddStream", ex.Message, ex));
      await command.RespondAsync(ex.Message, ephemeral: true);
    }
  }
  private async Task ListStreams(SocketSlashCommand command)
  {
    try
    {
      var cancellationTokenSource = new CancellationTokenSource();
      var data = await dataStore.GetDataAsync(cancellationTokenSource.Token);
      var guildData = data.Announcements.Where(d => d.GuildID == command.GuildId);
      if (guildData.Any())
      {
        var guildConfig = guildConfigs.FirstOrDefault(gc => gc.GuildID == command.GuildId);
        var colorString = guildConfig?.Color.Replace("#", "");
        var color = uint.TryParse(colorString, System.Globalization.NumberStyles.HexNumber, null, out uint _value) ? _value : 0;
        var twitchUser = guildData.Select(gd => gd.TwitchUser).Distinct().Freeze();
        var twitchUserData = await twitchClient.GetUsersAsync(twitchUser, cancellationTokenSource.Token);

        await twitchUserData.Match
          (
            async success =>
            {
              var embeds = guildData
                .Join(success.Value.Data, o => o.TwitchUser, i => i.Login, (o, i) => (guildData: o, twitchUserData: i))
                .GroupBy(gd => (gd.guildData.TwitchUser, gd.twitchUserData.Profile_Image_Url, gd.twitchUserData.Login, gd.twitchUserData.Offline_Image_Url))
                .Select(gdg => new EmbedBuilder()
                  .WithAuthor(gdg.Key.TwitchUser)
                  .WithThumbnailUrl(AttachCacheBuster(gdg.Key.Profile_Image_Url))
                  .WithImageUrl(AttachCacheBuster(gdg.Key.Offline_Image_Url))
                  .WithFields(gdg.Select((gdgi, i) => new EmbedFieldBuilder().WithName($"{i + 1}.").WithValue($"<#{gdgi.guildData.ChannelID}>").WithIsInline(false)))
                  .WithUrl($"https://twitch.tv/{gdg.Key.Login}")
                  .WithCurrentTimestamp()
                  .Build());

              if (embeds.Any())
              {
                var rest = embeds;
                if (!command.HasResponded)
                {
                  var first10 = rest.Take(10);
                  await command.RespondAsync(string.Empty, first10.ToArray(), ephemeral: true);
                  rest = embeds.Except(first10);
                }
                if (rest.Any())
                {
                  var channel = await command.GetChannelAsync();
                  while (rest.Any())
                  {
                    var next10 = rest.Take(10);
                    rest = rest.Except(next10);
                    await channel.SendMessageAsync(string.Empty, embeds: next10.ToArray(), flags: MessageFlags.Ephemeral);
                  }
                }
              }
              else
                await command.RespondAsync("No streams in Announcement Queue", ephemeral: true);

            },
            error => Task.FromResult(error)
          );
      }
      else
        await command.RespondAsync("No streams in Announcement Queue", ephemeral: true);
    }
    catch (Exception ex)
    {
      WriteLog(new LogMessage(LogSeverity.Error, "ListStreams", ex.Message, ex));
      await command.RespondAsync(ex.Message, ephemeral: true);
    }
  }

  private async Task RemoveStream(SocketSlashCommand command)
  {
    try
    {
      var guildID = command.GuildId;
      if (guildID is null)
      {
        await command.RespondAsync("Guild not found", ephemeral: true);
        return;
      }

      var twitchUserOption = command.Data.Options.FirstOrDefault(o => o.Name == "twitch-user");
      if (twitchUserOption is null)
      {
        await command.RespondAsync("[twitch-user] not specified", ephemeral: true);
        return;
      }

      var twitchUser = twitchUserOption.Value is string _twitchUser ? _twitchUser : string.Empty;
      if (string.IsNullOrWhiteSpace(twitchUser))
      {
        await command.RespondAsync($"[twitch-user] ({twitchUser}) is empty", ephemeral: true);
        return;
      }

      var channelOption = command.Data.Options.FirstOrDefault(o => o.Name == "channel");
      ITextChannel? channel = default;
      if (channelOption is not null)
      {
        if (channelOption.Value is not ITextChannel _channel)
        {
          await command.RespondAsync($"Channel ({channelOption.Value}) is not a text-channel", ephemeral: true);
          return;
        }
        else
          channel = _channel;
      }
      var cancellationTokenSource = new CancellationTokenSource();
      var data = await dataStore.GetDataAsync(cancellationTokenSource.Token);
      var userData = data.Announcements.Where(d => d.GuildID == guildID && d.TwitchUser == twitchUser);

      var channels = channel is null ? userData.Select(ud => ud.ChannelID).ToArray() : [channel.Id];

      await dataStore.DeleteAnnouncementAsync(twitchUser, guildID ?? 0, channels, cancellationTokenSource.Token);
      await command.RespondAsync($"Announcements for user {twitchUser}: {string.Join(",", channels.Select(c => $"<#{c}>"))} removed", ephemeral: true);
    }
    catch (Exception ex)
    {
      WriteLog(new LogMessage(LogSeverity.Error, "RemoveStream", ex.Message, ex));
      await command.RespondAsync(ex.Message, ephemeral: true);
    }
  }

  public async Task PostTwitchAnnouncementAsync(ulong guildID, ulong channelID, DiscordTwitchEmbedData twitchData, CancellationToken cancellationToken)
  {
    if (cancellationToken.IsCancellationRequested)
      return;
    try
    {
      if (twitchData is null) return;

      var guildConfig = guildConfigs.FirstOrDefault(gc => gc.GuildID == guildID);
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
        .WithThumbnailUrl(AttachCacheBuster(twitchData.UserImage))
        .WithImageUrl(AttachCacheBuster(thumbnailURL))
        .WithUrl($"https://twitch.tv/{twitchData.UserLogin}")
        .WithFooter(new EmbedFooterBuilder()
          .WithIconUrl(twitchData.UserImage)
          .WithText(guildConfig.FooterText)
        )
        .WithTimestamp(twitchData.StartedAt)
        .Build();

      await channel.SendMessageAsync(text: guildConfig.Text, embed: embed);
    }
    catch (Exception ex)
    {
      WriteLog(new LogMessage(LogSeverity.Error, "SendTwitchMessage", ex.Message, ex));
    }
  }

  private string AttachCacheBuster(string uri)
  {
    if (string.IsNullOrWhiteSpace(uri))
      return string.Empty;

    try
    {
      var uriBuilder = new UriBuilder(uri);
      var queryParams = HttpUtility.ParseQueryString(uriBuilder.Query);
      queryParams.Add("v", DateTime.Now.Ticks.ToString());
      uriBuilder.Query = queryParams.ToString();
      return uriBuilder.ToString();
    }
    catch (Exception ex)
    {
      WriteLog(new LogMessage(LogSeverity.Error, "AttachCacheBuster", ex.Message, ex));
    }
    return string.Empty;
  }

  public async Task<OneOf<Result<string>, NotFound>> GetChannelNameAsync(ulong channelID, CancellationToken cancellationToken)
  {
    if (cancellationToken.IsCancellationRequested)
      return new NotFound();

    var channel = await discordSocketClient.GetChannelAsync(channelID);
    var channelName = channel?.Name;
    if (channelName is null)
      return new NotFound();
    else
      return new Result<string>(channelName);
  }

  public OneOf<Result<string>, NotFound> GetGuildName(ulong guildID)
  {
    var guild = discordSocketClient.GetGuild(guildID);
    var guildName = guild?.Name;
    if (guildName is null)
      return new NotFound();
    else
      return new Result<string>(guildName);
  }

  public OneOf<Result<string>, NotFound> GetGuildIconUrl(ulong guildID)
  {
    var guild = discordSocketClient.GetGuild(guildID);
    var guildIconUrl = guild?.IconUrl;
    if (guildIconUrl is null)
      return new NotFound();
    else
      return new Result<string>(guildIconUrl);
  }

  public Task<DiscordConnectionState> HealthCheckAsync(CancellationToken cancellationToken)
  {
    if (cancellationToken.IsCancellationRequested)
      return Task.FromCanceled<DiscordConnectionState>(cancellationToken);

    DiscordConnectionState result =
      discordSocketClient.ConnectionState switch
      {
        ConnectionState.Disconnected => DiscordConnectionState.Disconnected,
        ConnectionState.Connecting => DiscordConnectionState.Connecting,
        ConnectionState.Connected => DiscordConnectionState.Connected,
        ConnectionState.Disconnecting => DiscordConnectionState.Disconnecting,
        _ => default,
      };
    return Task.FromResult(result);
  }

  public OneOf<Result<DiscordClientData>, Error<Exception>> GetDiscordClientData()
  {
    try
    {
      return new Result<DiscordClientData>(new()
      {
        GuildData = discordSocketClient.Guilds?.Select(g => new DiscordClientGuildData()
        {
          GuildID = g.Id,
          GuildName = g.Name,
          GuildIconUrl = g.IconUrl,
          DiscordChannelData = g.TextChannels?.Select(tch => new DiscordClientChannelData()
          {
            ChannelID = tch.Id,
            ChannelName = tch.Name,
            ChannelPosition = tch.Position,
            CategoryID = tch.Category?.Id ?? default,
            CategoryName = tch.Category?.Name ?? string.Empty,
            CategoryPosition = tch.Category?.Position ?? default,
          }) ?? []
        }) ?? []
      });

    }
    catch (Exception ex)
    {
      WriteLog(new LogMessage(LogSeverity.Error, "GetDiscordClientData", ex.Message, ex));
      return new Error<Exception>(ex);
    }
  }
}
