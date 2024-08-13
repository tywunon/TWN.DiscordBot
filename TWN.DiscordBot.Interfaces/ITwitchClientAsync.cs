using TWN.DiscordBot.Interfaces.Types;

namespace TWN.DiscordBot.Interfaces;
public interface ITwitchClientAsync : IHealthCheckProviderAsync<bool>
{
  Task<TwitchOAuthResult> GetOAuthTokenAsync(CancellationToken cancellationToken);
  Task<TwitchStreamsResult> GetStreamsAsync(IEnumerable<string> userLogins, CancellationToken cancellationToken);
  Task<TwitchUsersResult> GetUsersAsync(IEnumerable<string> userLogins, CancellationToken cancellationToken);
}
