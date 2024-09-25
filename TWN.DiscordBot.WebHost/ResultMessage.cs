namespace TWN.DiscordBot.WebHost;
internal readonly struct ResultMessage<T>
{
  public bool Success { get; init; }
  public string Message { get; init; }
  public T Payload { get; init; }
}
