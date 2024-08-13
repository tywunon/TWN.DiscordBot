
using OneOf;
using OneOf.Types;
using TWN.DiscordBot.Interfaces.Types;

namespace TWN.DiscordBot.Interfaces;
public interface ITwitchClient
{
  Task<TwitchOAuthResult> GetOAuthToken(CancellationToken cancellationToken);
  Task<TwitchStreamsResult> GetStreams(IEnumerable<string> userLogins, CancellationToken cancellationToken);
  Task<TwitchUsersResult> GetUsers(IEnumerable<string> userLogins, CancellationToken cancellationToken);
  Task<bool> HealthCheck(CancellationToken cancellationToken);
}
