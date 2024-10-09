using LanguageExt;

using Microsoft.AspNetCore.Http;

using OneOf.Types;

using TWN.DiscordBot.Interfaces;

namespace TWN.DiscordBot.WebHost.Services;
internal class DiscordClientApiService(IDiscordClientAsync discordClient, ITwitchClientAsync twitchClient) : IDiscordClientApiServiceAsync
{
  public async Task<IResult> GetChannelNameAsync(ulong channelID, CancellationToken cancellationToken)
  {
    var result = await discordClient.GetChannelNameAsync(channelID, cancellationToken);
    return result.Match(
      s => Results.Ok(new ResultMessage<Payloads.ChannelNamePayload>()
      {
        Success = true,
        Message = string.Empty,
        Payload = new(channelID, s.Value),
      }),
      nf => Results.Ok(new ResultMessage<Payloads.ChannelNamePayload>()
      {
        Success = false,
        Message = "Channel Not Found",
        Payload = new(channelID, string.Empty),
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
        Payload = new(guildID, s.Value),
      }),
      nf => Results.Ok(new ResultMessage<Payloads.GuildNamePayload>()
      {
        Success = false,
        Message = "Guild Not Found",
        Payload = new(guildID, string.Empty),
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

  public async Task<IResult> PostTwitchAnnouncementAsync(string twitchUser, ulong guildID, ulong channelID, CancellationToken cancellationToken)
  {
    var userDataResult = await twitchClient.GetUsersAsync([twitchUser], cancellationToken);
    if (userDataResult is null)
      return Results.Ok(new ResultMessage<Payloads.EmptyPayload>()
      {
        Success = false,
        Message = "user data was null",
        Payload = new(),
      });


    return await userDataResult.Match(
      async userS =>
      {
        var userData = userS.Value.Data.FirstOrDefault(d => d.Login == twitchUser);
        if (userData is null)
          return Results.Ok(new ResultMessage<Payloads.EmptyPayload>()
          {
            Success = false,
            Message = $"user data for {twitchUser} was not found",
            Payload = new(),
          });

        var streamDataResult = await twitchClient.GetStreamsAsync([twitchUser], cancellationToken);
        if (streamDataResult is null)
        {

          return Results.Ok(new ResultMessage<Payloads.EmptyPayload>()
          {
            Success = false,
            Message = $"stream data was null",
            Payload = new(),
          });
        }

        return await streamDataResult.Match(
          async streamS =>
          {
            var streamData = streamS.Value.Data.FirstOrDefault(d => d.User_Login == twitchUser);
            if (streamData is null)
            {
              await discordClient.PostTwitchAnnouncementAsync(guildID, channelID, new()
              {
                Title = "Offline",
                UserLogin = userData.Login,
                UserName = userData.Display_Name,
                GameName = "{{GameName}}",
                UserImage = userData.Profile_Image_Url,
                ThumbnailURL = userData.Offline_Image_Url,
                StartedAt = DateTime.Now,
              }, cancellationToken);

              return Results.Ok(new ResultMessage<Payloads.EmptyPayload>()
              {
                Success = true,
                Message = string.Empty,
                Payload = new(),
              });
            }

            await discordClient.PostTwitchAnnouncementAsync(guildID, channelID, new()
            {
              Title = streamData.Title,
              UserLogin = streamData.User_Login,
              UserName = streamData.User_Name,
              GameName = streamData.Game_Name,
              UserImage = userData.Profile_Image_Url,
              ThumbnailURL = streamData.Thumbnail_Url,
              StartedAt = streamData.Started_At,
            }, cancellationToken);
            return Results.Ok(new ResultMessage<Payloads.EmptyPayload>()
            {
              Success = true,
              Message = string.Empty,
              Payload = new(),
            });
          },
          async streamErr => await Task.FromResult(Results.Json(new ResultMessage<Payloads.EmptyPayload>()
          {
            Success = false,
            Message = streamErr.Value.Message,
            Payload = new(),
          }, statusCode: StatusCodes.Status500InternalServerError)));
      },
      async userErr => await Task.FromResult(Results.Json(new ResultMessage<Payloads.EmptyPayload>()
      {
        Success = false,
        Message = userErr.Value.Message,
        Payload = new Payloads.EmptyPayload(),
      }, statusCode: StatusCodes.Status500InternalServerError)));


  }
}


