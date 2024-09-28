using TWN.DiscordBot.WebClient;

namespace TWN.DiscordBot.ControlPanel.Provider;

public interface IBotDataController
{
  Task<DiscordClientData> GetDiscordClientDataAsync(string? botID, CancellationToken cancellationToken);
  Task<string?> GetBotNameAsync(string? botID, CancellationToken cancellationToken);
  Task<IEnumerable<AnnouncementData>> GetBotAnnouncementsAsync(string? botID, CancellationToken cancellationToken);
  Task<bool> IsBotConfiguredAsync(string? botID, CancellationToken cancellationToken);
  Task<IEnumerable<BotMetaData>> GetBotMetaDataAsync(CancellationToken cancellationToken);
}