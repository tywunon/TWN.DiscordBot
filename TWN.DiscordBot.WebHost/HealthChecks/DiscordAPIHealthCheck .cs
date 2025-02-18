namespace TWN.DiscordBot.WebHost.HealthChecks;
internal class DiscordAPIHealthCheck(IHttpClientFactory clientFactory) : AtlassianStatuspageHealthCheckBase(clientFactory)
{
  public override string GetStatusPageURI() => "https://discordstatus.com/api/v2/status.json";
}
