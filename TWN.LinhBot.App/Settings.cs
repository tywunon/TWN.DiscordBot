using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TWN.LinhBot.App;
public sealed class Settings
{
  public required DiscordAPISettings DiscordAPI { get; set; }
  public required TwitchAPISettings TwitchAPI { get; set; }
  public required IEnumerable<StreamObserverSettingsItem> StreamObserverSettings { get; set; }
}

public sealed class DiscordAPISettings
{
  public required string AppToken { get; set; }
}

public sealed class TwitchAPISettings
{
  public required string OAuthURL { get; set; }
  public required string BaseURL { get; set; }
  public required string ClientID { get; set; }
  public required string ClientSecret { get; set; }
}

public sealed class StreamObserverSettingsItem
{
  public required ulong GuildID { get; set; }
  public required double TimerInterval { get; set; }
  public required IEnumerable<CheckSettingsItem> CheckSettings { get; set; }

  public override string ToString() => $"{GuildID} | {base.ToString()}";
}

public sealed class CheckSettingsItem
{
  public required ulong RoleID { get;set; }
  public required ulong ChannelID { get;set; }

  public override string ToString() => $"{RoleID}-{ChannelID} | {base.ToString()}";
}