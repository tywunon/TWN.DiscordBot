
using LanguageExt;
using LanguageExt.Common;

using Microsoft.Extensions.Logging;

using OneOf;
using OneOf.Types;

using System.Net.Http.Json;

using TWN.DiscordBot.Interfaces;
using TWN.DiscordBot.Interfaces.Types;
using TWN.DiscordBot.Settings;

namespace TWN.DiscordBot.Twitch;
public class TwitchClient(IHttpClientFactory httpClientFactory,
                          TwitchSettings twitchAPISettings,
                          ILogger<TwitchClient> logger)
: ITwitchClient
{
  public async Task<OneOf<string, Error<Exception>>> GetOAuthToken()
  {
    try
    {
      var client = httpClientFactory.CreateTwitchOAuthClient();
      var response = await PostOAuthAsync();
      var result = await response.Content.ReadFromJsonAsync<OAuthResponse>();
      if (result is null)
      {
        var parseException = new Exception($"Response:\n{response?.Content}\ncouldn't be parsed");
        logger.LogError(parseException, "{Message}", parseException.Message);
        return new Error<Exception>();
      }
      return result.Access_Token ?? string.Empty;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "{Message}", ex.Message);
      return new Error<Exception>(ex);
    }
  }

  public async Task<OneOf<StreamsResponse, Error<Exception>>> GetStreams(IEnumerable<string> userLogins, CancellationToken cancellationToken)
  {
    try
    {
      var oAuthTokenResult = await GetOAuthToken();
      return await oAuthTokenResult.Match(
        async oAuthToken =>
        {
          var client = httpClientFactory.CreateTwitchAPIClient();
          var queryParameter = userLogins.Any() ? $"?{string.Join("&", userLogins.Select(ul => $"user_login={ul}"))}" : string.Empty;
          var request = new HttpRequestMessage(HttpMethod.Get, $"streams{queryParameter}");
          request.Headers.Authorization = new("Bearer", oAuthToken);
          request.Headers.Add("client-id", twitchAPISettings.ClientID);
          var response = await client.SendAsync(request, cancellationToken);
          var result = await response.Content.ReadFromJsonAsync<StreamsResponse>(cancellationToken);
          if (result is null)
          {
            var parseException = new Exception($"Response:\n{response?.Content}\ncouldn't be parsed");
            logger.LogError(parseException, "{Message}", parseException.Message);
            return new Error<Exception>();
          }
          return result;
        },
        error => Task.FromResult<OneOf<StreamsResponse, Error<Exception>>>(error)
      ); ;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "{Message}", ex.Message);
      return new Error<Exception>(ex);
    }
  }

  public async Task<OneOf<UsersResponse, Error<Exception>>> GetUsers(IEnumerable<string> userLogins, CancellationToken cancellationToken)
  {
    try
    {
      var oAuthTokenResult = await GetOAuthToken();
      return await oAuthTokenResult.Match(
        async oAuthToken =>
        {
          var client = httpClientFactory.CreateTwitchAPIClient();
          var queryParameter = userLogins.Any() ? $"?{string.Join("&", userLogins.Select(ul => $"login={ul}"))}" : string.Empty;
          var request = new HttpRequestMessage(HttpMethod.Get, $"users{queryParameter}");
          request.Headers.Authorization = new("Bearer", oAuthToken);
          request.Headers.Add("client-id", twitchAPISettings.ClientID);
          var response = await client.SendAsync(request, cancellationToken);
          var result = await response.Content.ReadFromJsonAsync<UsersResponse>(cancellationToken) ?? new([]);
          if (result is null)
          {
            var parseException = new Exception($"Response:\n{response?.Content}\ncouldn't be parsed");
            logger.LogError(parseException, "{Message}", parseException.Message);
            return new Error<Exception>();
          }
          return result;
        },
        error => Task.FromResult<OneOf<UsersResponse, Error<Exception>>>(error)
      );
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "{Message}", ex.Message);
      return new Error<Exception>(ex);
    }
  }

  private async Task<HttpResponseMessage> PostOAuthAsync() => await httpClientFactory.CreateTwitchOAuthClient().PostAsync(string.Empty, new OAuthContent(twitchAPISettings.ClientID, twitchAPISettings.ClientSecret));

  public async Task<bool> HealthCheck()
  {
    var response = await PostOAuthAsync();
    return response.IsSuccessStatusCode;
  }
}

internal class OAuthContent(string clientID, string clientSecret)
: FormUrlEncodedContent
  ([
    KeyValuePair.Create("client_id", clientID),
    KeyValuePair.Create("client_secret", clientSecret),
    KeyValuePair.Create("grant_type", "client_credentials"),
  ])
{ }

internal record OAuthResponse(
  string? Access_Token = null,
  int? Expires_In = null,
  string? Token_Type = null,

  int? Status = null,
  string? Message = null
);