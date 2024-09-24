using Microsoft.AspNetCore.Http;

using OneOf;
using OneOf.Types;

using System;
using System.Linq;

namespace TWN.DiscordBot.WebHost.Services;
internal interface IDiscordClientServiceAsync
{
  IResult GetGuildName(ulong guildID);
  Task<IResult> GetChannelNameAsync(ulong channelID, CancellationToken cancellationToken);
}
