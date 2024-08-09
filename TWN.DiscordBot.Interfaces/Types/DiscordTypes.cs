using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TWN.DiscordBot.Interfaces.Types;
public sealed class DiscordTwitchEmbedData
{
    public required string Title { get; init; }
    public required string UserLogin { get; init; }
    public required string UserName { get; init; }
    public required string GameName { get; init; }
    public required string UserImage { get; init; }
    public required string ThumbnailURL { get; init; }
    public required DateTime StartedAt { get; init; }
}

public enum DiscordConnectionState : byte
{
  Disconnected,
  Connecting,
  Connected,
  Disconnecting
}