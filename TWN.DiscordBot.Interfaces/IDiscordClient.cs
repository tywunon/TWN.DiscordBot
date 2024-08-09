using TWN.DiscordBot.Interfaces.Types;

namespace TWN.DiscordBot.Interfaces;
public interface IDiscordClient
{
  Task SendTwitchMessage(ulong guildID, ulong channelID, DiscordTwitchEmbedData twitchData);
  Task StartAsync();
  Task<DiscordConnectionState> HealthCheck();
}
