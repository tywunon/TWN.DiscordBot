using TWN.DiscordBot.Interfaces.Types;

namespace TWN.DiscordBot.Interfaces;

public interface IDiscordClient : IHealthCheckProvider<DiscordConnectionState>
{
  Task SendTwitchMessageAsync(ulong guildID, ulong channelID, DiscordTwitchEmbedData twitchData);
}

