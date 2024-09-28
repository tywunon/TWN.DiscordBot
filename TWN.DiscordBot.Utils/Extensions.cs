using Microsoft.Extensions.Logging;

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

    public static void LogException(this ILogger self, Exception exception, string source) 
      => self.LogError(exception, "[{source}] {Message}", source, exception.Message);
  }
}
