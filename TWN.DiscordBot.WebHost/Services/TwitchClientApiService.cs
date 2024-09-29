using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using TWN.DiscordBot.Interfaces;
using TWN.DiscordBot.Interfaces.Types;

namespace TWN.DiscordBot.WebHost.Services;
internal class TwitchClientApiService(ITwitchClientAsync twitchClient)
: ITwitchClientApiServiceAsync
{
  public async Task<IResult> GetStreamDataAsync(string username, CancellationToken cancellationToken)
  {
    var result = await twitchClient.GetStreamsAsync([username], cancellationToken);
    return result.Match(
      s =>
      {
        var streamsResponse = s.Value;
        var streamsResponseData = streamsResponse.Data?.FirstOrDefault(srd => srd.User_Login == username);
        return streamsResponseData switch
        {
          not null => Results.Ok(new ResultMessage<Payloads.StreamDataPayload>()
          {
            Success = true,
            Message = string.Empty,
            Payload = new (streamsResponseData, true),
          }),
          _ => Results.Ok(new ResultMessage<Payloads.StreamDataPayload>()
          {
            Success = false,
            Message = $"Stream Data {username} not found",
            Payload = new (streamsResponseData, false),
          }),
        };
      },
      err => Results.Json(data: new ResultMessage<Payloads.StreamDataPayload>()
      {
        Success = false,
        Message = err.Value.Message,
        Payload = new (null, false),
      },
      statusCode: StatusCodes.Status500InternalServerError)
    );
  }

  public async Task<IResult> GetUserDataAsync(string username, CancellationToken cancellationToken)
  {
    var result = await twitchClient.GetUsersAsync([username], cancellationToken);
    return result.Match(
      s =>
      {
        var usersResponse = s.Value;
        var usersResponseData = usersResponse.Data.FirstOrDefault(urd => urd.Login == username);
        return usersResponseData switch
        {
          not null => Results.Ok(new ResultMessage<Payloads.UserDataPayload>()
          {
            Success = true,
            Message = string.Empty,
            Payload = new Payloads.UserDataPayload(usersResponseData),
          }),
          _ => Results.NotFound(new ResultMessage<Payloads.UserDataPayload>()
          {
            Success = false,
            Message = $"User {username} not found",
            Payload = new Payloads.UserDataPayload(usersResponseData),
          }),
        };
      },
      err => Results.Json(data: new ResultMessage<Payloads.UserDataPayload>()
      {
        Success = false,
        Message = err.Value.Message,
        Payload = new Payloads.UserDataPayload(null),
      },
      statusCode: StatusCodes.Status500InternalServerError)
    );
  }
}
