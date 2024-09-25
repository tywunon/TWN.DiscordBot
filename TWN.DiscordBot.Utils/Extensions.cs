namespace TWN.DiscordBot.Utils
{
  public static class SystemCollectionsGenericExtension
  {
    public static void Upsert<K, V>(this IDictionary<K, V> self, K key, V value) where K : notnull
    {
      if (self == null) return;
      if (!self.TryAdd(key, value))
        self[key] = value;
    }

    public static bool RemoveAll<K, V>(this IDictionary<K, V> self, Func<KeyValuePair<K, V>, bool> predicate)
      => self.Filter(predicate).ToList().All(self.Remove);
  }

  public static class SystemNetHttpExtension
  {
    public static HttpClient CreateTwitchOAuthClient(this IHttpClientFactory httpClientFactory)
      => httpClientFactory.CreateClient("TwitchOAuth");
    public static HttpClient CreateTwitchAPIClient(this IHttpClientFactory httpClientFactory)
      => httpClientFactory.CreateClient("TwitchAPI");
  }
}
