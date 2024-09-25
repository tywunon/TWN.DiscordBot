using Microsoft.AspNetCore.Http;

using TWN.DiscordBot.Interfaces;

namespace TWN.DiscordBot.WebHost.Services;
internal class TwitchClientService(ITwitchClientAsync twitchClient)
: ITwitchClientServiceAsync
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
          not null => Results.Ok(new ResultMessage()
          {
            Success = true,
            Message = string.Empty,
            Payload = new
            {
              streamsResponseData = streamsResponseData,
              isOnline = true,
            },
          }),
          _ => Results.Ok(new ResultMessage()
          {
            Success = true,
            Message = $"User {username} not found",
            Payload = new
            {
              streamsResponseData = new { },
              isOnline = false,
            },
          }),
        };
      },
      err => Results.Json(data: new ResultMessage()
      {
        Success = false,
        Message = err.Value.Message,
        Payload = new
        {
          streamsResponseData = new { },
          isOnline = false,
        },
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
          not null => Results.Ok(new ResultMessage()
          {
            Success = true,
            Message = string.Empty,
            Payload = new { usersResponse = usersResponseData },
          }),
          _ => Results.NotFound(new ResultMessage()
          {
            Success = false,
            Message = $"User {username} not found",
            Payload = new { usersResponse = new { } },
          }),
        };
      },
      err => Results.Json(data: new ResultMessage()
      {
        Success = false,
        Message = err.Value.Message,
        Payload = new
        {
          usersResponse = new { },
          isOnline = false,
        },
      },
      statusCode: StatusCodes.Status500InternalServerError)
    );
  }
}
