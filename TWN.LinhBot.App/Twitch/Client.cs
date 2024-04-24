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

using Microsoft.Extensions.Logging;

namespace TWN.LinhBot.App.Twitch;
internal class Client(IHttpClientFactory httpClientFactory, TwitchSettings twitchAPISettings, ILogger<Client> logger)
{
  readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
  readonly TwitchSettings _twitchAPISettings = twitchAPISettings;
  private readonly ILogger<Client> _logger = logger;

  public async Task<string?> GetOAuthTocken()
  {
    try
    {

      var client = _httpClientFactory.CreateClient("TwitchOAuth");
      var response = await client.PostAsync(string.Empty, new OAuthContent(_twitchAPISettings.ClientID, _twitchAPISettings.ClientSecret));
      var result = await response.Content.ReadFromJsonAsync<OAuthResponse>();
      return (result?.Access_Token);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "{Message}", ex.Message);
      return string.Empty;
    }
  }

  public async Task<StreamsResponse?> GetStreams(IEnumerable<string> userLogins, CancellationToken cancellationToken)
  {
    try
    {
      var oAuthToken = await GetOAuthTocken();

      var client = _httpClientFactory.CreateClient("TwitchAPI");
      var queryParameter = userLogins.Any() ? $"?{string.Join("&", userLogins.Select(ul => $"user_login={ul}"))}" : string.Empty;
      var request = new HttpRequestMessage(HttpMethod.Get, $"streams{queryParameter}");
      request.Headers.Authorization = new("Bearer", oAuthToken);
      request.Headers.Add("client-id", "2zswyembrowcn69z52y9ogc5q9ks4i");
      var response = await client.SendAsync(request, cancellationToken);
      return await response.Content.ReadFromJsonAsync<StreamsResponse>(cancellationToken);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "{Message}", ex.Message);
      return null;
    }
  }

  public async Task<UsersResponse?> GetUsers(IEnumerable<string> userLogins, CancellationToken cancellationToken)
  {
    try
    {
      var oAuthToken = await GetOAuthTocken();

      var client = _httpClientFactory.CreateClient("TwitchAPI");
      var queryParameter = userLogins.Any() ? $"?{string.Join("&", userLogins.Select(ul => $"login={ul}"))}" : string.Empty;
      var request = new HttpRequestMessage(HttpMethod.Get, $"users{queryParameter}");
      request.Headers.Authorization = new("Bearer", oAuthToken);
      request.Headers.Add("client-id", "2zswyembrowcn69z52y9ogc5q9ks4i");
      var response = await client.SendAsync(request, cancellationToken);
      return await response.Content.ReadFromJsonAsync<UsersResponse>(cancellationToken);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "{Message}", ex.Message);
      return null;
    }
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
  string? Access_Token = null,
  int? Expires_In = null,
  string? Token_Type = null,

  int? Status = null,
  string? Message = null
);

#region Streams
public record StreamsResponse(StreamsResponseData[] Data, StreamsResponsePagination Pagination);

public record StreamsResponsePagination(string Cursor);

public record StreamsResponseData(
  string ID,
  string User_Id,
  string User_Login,
  string User_Name,
  string Game_ID,
  string Game_Name,
  string Type,
  string Title,
  int Viewer_Count,
  DateTime Started_At,
  string Language,
  string Thumbnail_Url,
  object[] Tag_IDs,
  string[] Tags,
  bool Is_Mature);
#endregion

#region Users
public record UsersResponse(UsersResponseData[] Data);

public record UsersResponseData(
  string ID,
  string Login,
  string Display_Name,
  string Type,
  string Broadcaster_Type,
  string Description,
  string Profile_Image_Url,
  string Offline_Image_Url,
  int View_Count,
  DateTime Created_At);

#endregion