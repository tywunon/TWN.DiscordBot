namespace TWN.DiscordBot.Interfaces;
public interface IHealthCheckProviderAsync<T>
{
  Task<T> HealthCheckAsync(CancellationToken cancellationToken);
}
