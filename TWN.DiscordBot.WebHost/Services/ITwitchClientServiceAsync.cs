using Microsoft.AspNetCore.Http;

namespace TWN.DiscordBot.WebHost.Services;
internal interface ITwitchClientServiceAsync
{
  Task<IResult> GetStreamDataAsync(string username, CancellationToken cancellationToken);
  Task<IResult> GetUserDataAsync(string username, CancellationToken cancellationToken);
}
