using Microsoft.AspNetCore.Http;

namespace TWN.DiscordBot.WebHost.Services;
internal interface IDiscordClientServiceAsync
{
  IResult GetGuildName(ulong guildID);
  Task<IResult> GetChannelNameAsync(ulong channelID, CancellationToken cancellationToken);
}
