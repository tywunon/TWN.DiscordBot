using OneOf;
using OneOf.Types;

using TWN.DiscordBot.Interfaces.Types;

namespace TWN.DiscordBot.Interfaces;

public interface IDiscordClientAsync : IHealthCheckProviderAsync<DiscordConnectionState>
{
  OneOf<Result<string>, NotFound> GetGuildIconUrl(ulong guildID);
  OneOf<Result<DiscordClientData>, Error<Exception>> GetDiscordClientData();
  OneOf<Result<string>, NotFound> GetGuildName(ulong guildID);
  Task<OneOf<Result<string>, NotFound>> GetChannelNameAsync(ulong channelID, CancellationToken cancellationToken);
  Task PostTwitchAnnouncementAsync(ulong guildID, ulong channelID, DiscordTwitchEmbedData twitchData, CancellationToken cancellationToken);
}

