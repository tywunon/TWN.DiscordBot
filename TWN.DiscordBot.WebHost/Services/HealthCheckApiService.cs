using System.Text.Json;
using System.Text.Json.Serialization;

using HealthChecks.UI.Core;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TWN.DiscordBot.WebHost.Services;
internal class HealthCheckApiService(HealthCheckService healthCheckService) : IHealthCheckApiServiceAsync
{
  public async Task<IResult> CheckHealthAsync(HttpContext httpContext, CancellationToken cancellationToken)
  {
    var report = await healthCheckService.CheckHealthAsync(cancellationToken);
    return await Task.FromResult(Results.Ok(
      new Payloads.HealthCheckPayload(httpContext.Request.Host.Value,report.Status.ToString(), 
                                      report.TotalDuration, 
                                      report.Entries
                                        .ToDictionary(
                                          kvp => kvp.Key, 
                                          kvp => new Payloads.HealthCheckPayloadEntry(kvp.Value.Data, 
                                                                                      kvp.Value.Description, 
                                                                                      kvp.Value.Duration, 
                                                                                      kvp.Value.Status.ToString(), 
                                                                                      kvp.Value.Tags)))));
  }
}
