﻿using Microsoft.Extensions.Diagnostics.HealthChecks;

using System.Net.Http.Json;

using TWN.DiscordBot.Interfaces;

namespace TWN.DiscordBot.WebHost.HealthChecks;
internal class TwitchAPIHealthCheck(IHttpClientFactory clientFactory) : AtlassianStatuspageHealthCheckBase(clientFactory)
{
  public override string GetStatusPageURI() => "https://status.twitch.com/api/v2/status.json";
}
