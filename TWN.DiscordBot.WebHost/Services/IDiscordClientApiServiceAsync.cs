using Microsoft.AspNetCore.Http;

namespace TWN.DiscordBot.WebHost.Services;
internal interface IDiscordClientApiServiceAsync
{
  IResult GetClientData();
  IResult GetGuildIconUrl(ulong guildID);
  IResult GetGuildName(ulong guildID);
  Task<IResult> GetChannelNameAsync(ulong channelID, CancellationToken cancellationToken);
}
