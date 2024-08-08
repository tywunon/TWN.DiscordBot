using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace System.Net.Http;
public static class SystemNetHttpExtension
{
  public static HttpClient CreateTwitchOAuthClient(this IHttpClientFactory httpClientFactory) 
    => httpClientFactory.CreateClient("TwitchOAuth");
  public static HttpClient CreateTwitchAPIClient(this IHttpClientFactory httpClientFactory)
    => httpClientFactory.CreateClient("TwitchAPI");
}
