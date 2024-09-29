using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using TWN.DiscordBot.Utils;

using TWN.DiscordBot.Settings;

namespace TWN.DiscordBot.Bot.BackgroundServices;
internal class TCPProbeProvider(TCPProbeSettings tcpProbeSettings, ILogger<TCPProbeProvider> logger) : PeriodicBackgroundService(logger)
{
  Socket? listener;
  protected async override Task InitAsync(CancellationToken cancellationToken)
  {
    listener = new(SocketType.Stream, ProtocolType.Tcp);
    listener.Bind(new IPEndPoint(IPAddress.Any, tcpProbeSettings.Port));
    logger.LogInformation("Probe listening on {LocalEndPoint}", listener.LocalEndPoint);
    listener.Listen(100);

    await base.InitAsync(cancellationToken);
  }

  protected override Task<TimeSpan> GetInterval(CancellationToken cancellationToken) 
    => Task.FromResult(TimeSpan.FromMilliseconds(20));
  protected async override Task ExecutePeriodicAsync(CancellationToken cancellationToken)
  {
    if (listener is null)
      return;

    try
    {
      var socket = await listener.AcceptAsync(cancellationToken);
      if (socket == null) 
        return;
      socket.Close();
    }
    catch (Exception ex)
    {
      logger.LogException(ex, "AcceptAsync");
    }
  }

  public override void Dispose()
  {
    base.Dispose();
    listener?.Dispose();
  }
}
