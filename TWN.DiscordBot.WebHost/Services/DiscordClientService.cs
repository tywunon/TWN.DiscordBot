using Microsoft.AspNetCore.Http;

using TWN.DiscordBot.Interfaces;

namespace TWN.DiscordBot.WebHost.Services;
internal class DiscordClientService(IDiscordClientAsync discordClient) : IDiscordClientServiceAsync
{
  public async Task<IResult> GetChannelNameAsync(ulong channelID, CancellationToken cancellationToken)
  {
    var result = await discordClient.GetChannelNameAsync(channelID, cancellationToken);
    return result.Match(
      s => Results.Ok(new ResultMessage<Payloads.ChannelNamePayload>()
      {
        Success = true,
        Message = string.Empty,
        Payload = new Payloads.ChannelNamePayload(channelID, s.Value),
      }),
      nf => Results.Ok(new ResultMessage<Payloads.ChannelNamePayload>()
      {
        Success = false,
        Message = "Channel Not Found",
        Payload = new Payloads.ChannelNamePayload(channelID, string.Empty),
      }));
  }

  public IResult GetGuildName(ulong guildID)
  {
    var result = discordClient.GetGuildName(guildID);
    return result.Match(
      s => Results.Ok(new ResultMessage<Payloads.GuildNamePayload>()
      {
        Success = true,
        Message = string.Empty,
        Payload = new Payloads.GuildNamePayload(guildID, s.Value),
      }),
      nf => Results.Ok(new ResultMessage<Payloads.GuildNamePayload>()
      {
        Success = false,
        Message = "Guild Not Found",
        Payload = new Payloads.GuildNamePayload(guildID, string.Empty),
      }));
  }
}


