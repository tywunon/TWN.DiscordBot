using LanguageExt;

using Microsoft.Extensions.Diagnostics.HealthChecks;

using System.Net.Http.Json;

using TWN.DiscordBot.Interfaces.Types;

namespace TWN.DiscordBot.WebHost.HealthChecks;
internal abstract class AtlassianStatuspageHealthCheckBase(IHttpClientFactory clientFactory) : IHealthCheck
{
  public virtual async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
  {
    try
    {
      var statusPageURI = GetStatusPageURI();
      var client = clientFactory.CreateClient("HealthCheck");
      
      var response = await client.GetFromJsonAsync<AtlassianStatuspageResponse>(statusPageURI, cancellationToken);
      if (response is null)
        return HealthCheckResult.Degraded($"response failed from {statusPageURI}", new ResultIsNullException());
      
      var data = new Dictionary<string, object>()
      {
        ["response"] = response,
      };
      return response.Status.Indicator switch
      {
        "none" => HealthCheckResult.Healthy(response.Status.Description, data: data),
        _ => HealthCheckResult.Unhealthy(response.Status.Description, data: data),
      };
    }
    catch (Exception ex)
    {
      return HealthCheckResult.Unhealthy(exception: ex);
    }
  }

  public abstract string GetStatusPageURI();

}

internal record AtlassianStatuspageResponsePage(string Id, string Name, string Url, DateTime UpdatedAt);
internal record AtlassianStatuspageResponseStatus(string Description, string Indicator);
internal record AtlassianStatuspageResponse(AtlassianStatuspageResponsePage Page, AtlassianStatuspageResponseStatus Status);
