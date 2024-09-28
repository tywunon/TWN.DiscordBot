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

public sealed class DiscordClientData
{
  public IEnumerable<DiscordClientGuildData>? GuildData { get; init; }
}

public sealed class DiscordClientGuildData
{
  public ulong GuildID { get; set; }
  public string? GuildName { get; set; }
  public string? GuildIconUrl { get; set; }
  public IEnumerable<DiscordClientChannelData>? DiscordChannelData { get; set; }
}

public sealed class DiscordClientChannelData
{
  public ulong ChannelID { get; set; }
  public string? ChannelName { get; set; }
  public int ChannelPosition { get; set; }
  public ulong CategoryID { get; set; }
  public string? CategoryName { get; set; }
  public int CategoryPosition { get; set; }
}

public enum DiscordConnectionState : byte
{
  Disconnected,
  Connecting,
  Connected,
  Disconnecting
}