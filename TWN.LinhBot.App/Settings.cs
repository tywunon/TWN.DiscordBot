using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TWN.LinhBot.App;
public sealed class Settings
{
  public required string DiscordAppToken { get; set; }
  public required IEnumerable<StreamObserverSettingsItem> StreamObserverSettings { get; set; }
}

public sealed class StreamObserverSettingsItem
{
  public required ulong GuildID { get; set; }
  public required double TimerInterval { get; set; }
  public required IEnumerable<CheckSettingsItem> CheckSettings { get; set; }
}

public sealed class CheckSettingsItem
{
  public required ulong RoleID { get;set; }
  public required ulong ChannelID { get;set; }
}