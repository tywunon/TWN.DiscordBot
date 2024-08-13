
using OneOf;
using OneOf.Types;

namespace TWN.DiscordBot.Interfaces.Types;

[GenerateOneOf]
public partial class TwitchOAuthResult : OneOfBase<Success<string>, Error<Exception>> { }
