using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TWN.LinhBot.App;
public sealed class Settings
{
  public required WatcherSettings Watcher { get; set; }
  public required DiscordSettings Discord { get; set; }
  public required TwitchSettings Twitch { get; set; }
  public required IEnumerable<GuildConfig> GuildConfig {  get; set; }
  public required DataStoreSettings DataStore { get; set; }
  public required TCPProbeSettings TCPProbe { get; set; }
}
public sealed class WatcherSettings
{
  public required int Delay { get; set; }
}

public sealed class DiscordSettings
{
  public required string Status { get; set; }
  public required string AppToken { get; set; }
}

public sealed class TwitchSettings
{
  public required string OAuthURL { get; set; }
  public required string BaseURL { get; set; }
  public required string ClientID { get; set; }
  public required string ClientSecret { get; set; }
}

public sealed class GuildConfig
{
  public required ulong GuildID { get; set; }
  public required string Color { get; set; }
  public required string Text { get; set; }
  public required string FooterText { get; set; }
  public required int ThumbnailWidth { get; set; }
  public required int ThumbnailHeight { get; set; }
}

public sealed class DataStoreSettings
{
  public required string FilePath { get; set; }
}

public sealed class TCPProbeSettings
{
  public required short Port { get; set; }
}