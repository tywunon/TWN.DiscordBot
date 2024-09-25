namespace TWN.DiscordBot.WebHost;
internal readonly struct ResultMessage
{
  public bool Success { get; init; }
  public string Message { get; init; }
  public object Payload { get; init; }
}
