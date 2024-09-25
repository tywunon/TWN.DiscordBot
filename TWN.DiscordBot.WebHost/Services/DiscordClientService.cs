using Microsoft.AspNetCore.Http;

using TWN.DiscordBot.Interfaces;

namespace TWN.DiscordBot.WebHost.Services;
internal class DiscordClientService(IDiscordClientAsync discordClient) : IDiscordClientServiceAsync
{
  public async Task<IResult> GetChannelNameAsync(ulong channelID, CancellationToken cancellationToken)
  {
    var result = await discordClient.GetChannelNameAsync(channelID, cancellationToken);
    return result.Match(
      s => Results.Ok(new ResultMessage()
      {
        Success = true,
        Message = string.Empty,
        Payload = new { channelID = channelID, channelName = s.Value },
      }),
      nf => Results.Ok(new ResultMessage()
      {
        Success = false,
        Message = "Channel Not Found",
        Payload = new { }
      }));
  }

  public IResult GetGuildName(ulong guildID)
  {
    var result = discordClient.GetGuildName(guildID);
    return result.Match(
      s => Results.Ok(new ResultMessage()
      {
        Success = true,
        Message = string.Empty,
        Payload = new { guildID = guildID, guildName = s.Value },
      }),
      nf => Results.Ok(new ResultMessage()
      {
        Success = false,
        Message = "Guild Not Found",
        Payload = new { }
      }));
  }
}


