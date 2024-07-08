namespace TWN.LinhBot.App.Discord;
internal interface IDiscordClient
{
  Task SendTwitchMessage(ulong guildID, ulong channelID, TwitchEmbedData twitchData);
  Task StartAsync();
}
