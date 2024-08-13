
using OneOf;
using OneOf.Types;

namespace TWN.DiscordBot.Interfaces.Types;

[GenerateOneOf]
public partial class TwitchUsersResult : OneOfBase<Success<UsersResponse>, Error<Exception>>
{ }