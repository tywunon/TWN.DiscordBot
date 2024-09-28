using Microsoft.AspNetCore.Http;

using TWN.DiscordBot.Interfaces;

namespace TWN.DiscordBot.WebHost.Services;
internal class DiscordClientApiService(IDiscordClientAsync discordClient) : IDiscordClientApiServiceAsync
{
  public async Task<IResult> GetChannelNameAsync(ulong channelID, CancellationToken cancellationToken)
  {
    var result = await discordClient.GetChannelNameAsync(channelID, cancellationToken);
    return result.Match(
      s => Results.Ok(new ResultMessage<Payloads.ChannelNamePayload>()
      {
        Success = true,
        Message = string.Empty,
        Payload = new (channelID, s.Value),
      }),
      nf => Results.Ok(new ResultMessage<Payloads.ChannelNamePayload>()
      {
        Success = false,
        Message = "Channel Not Found",
        Payload = new (channelID, string.Empty),
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
        Payload = new (guildID, s.Value),
      }),
      nf => Results.Ok(new ResultMessage<Payloads.GuildNamePayload>()
      {
        Success = false,
        Message = "Guild Not Found",
        Payload = new (guildID, string.Empty),
      }));
  }

  public IResult GetGuildIconUrl(ulong guildID)
  {
    var result = discordClient.GetGuildIconUrl(guildID);
    return result.Match(
      s => Results.Ok(new ResultMessage<Payloads.GuildIconUrlPayload>()
      {
        Success = true,
        Message = string.Empty,
        Payload = new(guildID, s.Value),
      }),
      nf => Results.Ok(new ResultMessage<Payloads.GuildIconUrlPayload>()
      {
        Success = false,
        Message = "Guild Not Found",
        Payload = new(guildID, string.Empty),
      }));
  }

  public IResult GetClientData()
  {
    var result = discordClient.GetDiscordClientData();
    return result.Match(
      s => Results.Ok(new ResultMessage<Payloads.DiscordClientDataPayload>()
      {
        Success = true,
        Message = string.Empty,
        Payload = new(s.Value),
      }),
      nf => Results.Json(new ResultMessage<Payloads.DiscordClientDataPayload>()
      {
        Success = false,
        Message = nf.Value.Message,
        Payload = new(new() { GuildData = [] }),
      }, statusCode: StatusCodes.Status500InternalServerError));
  }
}


