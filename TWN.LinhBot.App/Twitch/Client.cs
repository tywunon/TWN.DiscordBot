using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using LanguageExt;
using LanguageExt.Common;

namespace TWN.LinhBot.App.Twitch;
internal class Client(IHttpClientFactory httpClientFactory, TwitchAPISettings twitchAPISettings)
{
  readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
  readonly TwitchAPISettings _twitchAPISettings = twitchAPISettings;

  public async Task<string?> GetOAuthTocken()
  {
    var client = _httpClientFactory.CreateClient("TwitchOAuth");
    var response = await client.PostAsync(string.Empty, new OAuthContent(_twitchAPISettings.ClientID, _twitchAPISettings.ClientSecret));
    var result = await response.Content.ReadFromJsonAsync<OAuthResponse>();
    return (result?.access_token);
  }

  public async Task<StreamsResponse?> GetStreams(IEnumerable<string> userLogins)
  {
    var oAuthToken = await GetOAuthTocken();

    var client = _httpClientFactory.CreateClient("TwitchAPI");
    var queryParameter = userLogins.Any() ? $"?{string.Join("&",userLogins.Select(ul => $"user_login={ul}"))}" : string.Empty;
    var request = new HttpRequestMessage(HttpMethod.Get, $"streams{queryParameter}");
    request.Headers.Authorization = new("Bearer", oAuthToken);
    request.Headers.Add("client-id", "2zswyembrowcn69z52y9ogc5q9ks4i");
    var response = await client.SendAsync(request);
    return await response.Content.ReadFromJsonAsync<StreamsResponse>();
  }
}

class OAuthContent(string clientID, string clientSecret) : FormUrlEncodedContent
  ([
        KeyValuePair.Create("client_id", clientID),
        KeyValuePair.Create("client_secret", clientSecret),
        KeyValuePair.Create("grant_type", "client_credentials"),
  ])
{ }

public record OAuthResponse(
  string? access_token = null,
  int? expires_in = null,
  string? token_type = null,

  int? status = null,
  string? message = null
);

public record StreamsResponse(StreamsResponseData[] data, StreamsResponsePagination pagination);

public record StreamsResponsePagination(string cursor);

public record StreamsResponseData(
  string id,
  string user_id,
  string user_login,
  string user_name,
  string game_id,
  string game_name,
  string type,
  string title,
  int viewer_count,
  DateTime started_at,
  string language,
  string thumbnail_url,
  object[] tag_ids,
  string[] tags,
  bool is_mature);
