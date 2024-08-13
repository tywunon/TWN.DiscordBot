
using OneOf;
using OneOf.Types;

namespace TWN.DiscordBot.Interfaces.Types;

[GenerateOneOf]
public partial class TwitchStreamsResult : OneOfBase<Success<StreamsResponse>, Error<Exception>> { }
