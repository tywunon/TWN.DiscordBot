using TWN.DiscordBot.Interfaces.Types;

namespace TWN.DiscordBot.Interfaces;

public interface IDiscordClientAsync : IHealthCheckProviderAsync<DiscordConnectionState>
{
  Task SendTwitchMessageAsync(ulong guildID, ulong channelID, DiscordTwitchEmbedData twitchData, CancellationToken cancellationToken);
}

