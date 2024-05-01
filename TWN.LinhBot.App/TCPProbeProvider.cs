using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TWN.LinhBot.App;
internal class TCPProbeProvider(TCPProbeSettings tcpProbeSettings, ILogger<TCPProbeProvider> logger) 
  : BackgroundService
{
  private readonly TCPProbeSettings _tcpProbeSettings = tcpProbeSettings;
  private readonly ILogger<TCPProbeProvider> _logger = logger;

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    try
    {
      using Socket listener = new(SocketType.Stream, ProtocolType.Tcp);
      listener.Bind(new IPEndPoint(IPAddress.Any, _tcpProbeSettings.Port));
      _logger.LogInformation("Probe listening on {LocalEndPoint}", listener.LocalEndPoint);
      listener.Listen(100);

      PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromMilliseconds(20));

      while (await timer.WaitForNextTickAsync(stoppingToken))
      {
        try
        {
          var socket = await listener.AcceptAsync(stoppingToken);
          if (socket == null) continue;
          socket.Close();
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "{Message}", ex.Message);
        }
      }
    }
    catch (Exception ex)
    {
      _logger.LogCritical(ex, "{Message}", ex.Message);
    }
  }
}
