
using OneOf;
using OneOf.Types;

namespace TWN.LinhBot.App.Twitch;
internal interface ITwitchClient
{
  Task<OneOf<string, Error<Exception>>> GetOAuthToken();
  Task<OneOf<StreamsResponse, Error<Exception>>> GetStreams(IEnumerable<string> userLogins, CancellationToken cancellationToken);
  Task<OneOf<UsersResponse, Error<Exception>>> GetUsers(IEnumerable<string> userLogins, CancellationToken cancellationToken);
}