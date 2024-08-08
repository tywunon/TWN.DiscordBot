
using OneOf;
using OneOf.Types;
using TWN.DiscordBot.Interfaces.Types;

namespace TWN.DiscordBot.Interfaces;
public interface ITwitchClient
{
  Task<OneOf<string, Error<Exception>>> GetOAuthToken();
  Task<OneOf<StreamsResponse, Error<Exception>>> GetStreams(IEnumerable<string> userLogins, CancellationToken cancellationToken);
  Task<OneOf<UsersResponse, Error<Exception>>> GetUsers(IEnumerable<string> userLogins, CancellationToken cancellationToken);
}