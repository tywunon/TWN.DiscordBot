using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Collections.Generic;
public static class SystemCollectionsGenericExtension
{
  public static void Upsert<K,V>(this IDictionary<K,V> self, K key, V value) where K : notnull
  {
    if (self == null) return;
    if (!self.TryAdd(key, value))
      self[key] = value;
  }
}
