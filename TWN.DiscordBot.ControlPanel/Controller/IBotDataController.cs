using OneOf;
using OneOf.Types;

using TWN.DiscordBot.WebClient;

namespace TWN.DiscordBot.ControlPanel.Controller;

public interface IBotDataController
{
  Task<OneOf<Success, Error<string>>> DeleteAnnouncement(string? botID, string twitchUser, long guildID, long? channelID);
  Task<OneOf<OneOf.Types.Success, OneOf.Types.Error<string>>> AddAnnouncement(string? botID, string twitchUser, long guildID, long channelID);
  Task<DiscordClientData> GetDiscordClientDataAsync(string? botID, CancellationToken cancellationToken);
  Task<string?> GetBotNameAsync(string? botID, CancellationToken cancellationToken);
  Task<IEnumerable<AnnouncementData>> GetBotAnnouncementsAsync(string? botID, CancellationToken cancellationToken);
  Task<bool> IsBotConfiguredAsync(string? botID, CancellationToken cancellationToken);
  Task<IEnumerable<BotMetaData>> GetBotMetaDataAsync(CancellationToken cancellationToken);
}