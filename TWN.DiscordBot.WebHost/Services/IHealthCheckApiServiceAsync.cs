using Microsoft.AspNetCore.Http;

namespace TWN.DiscordBot.WebHost.Services;
public interface IHealthCheckApiServiceAsync
{
  Task<IResult> CheckHealthAsync(HttpContext context, CancellationToken cancellationToken);
}