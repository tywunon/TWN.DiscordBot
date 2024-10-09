using OneOf;
using OneOf.Types;

using TWN.DiscordBot.WebClient;

namespace TWN.DiscordBot.ControlPanel.Controller;

public interface IBotDataController
{
  Task<OneOf<Success, Error<string>>> PostTwitchAnnouncementAsync(string? botID, string twitchUser, long guildID, long channelID, CancellationToken cancellationToken);
  Task<OneOf<Success, Error<string>>> DeleteAnnouncementAsync(string? botID, string twitchUser, long guildID, long? channelID, CancellationToken cancellationToken);
  Task<OneOf<Success, Error<string>>> AddAnnouncementAsync(string? botID, string twitchUser, long guildID, long channelID, CancellationToken cancellationToken);
  Task<DiscordClientData> GetDiscordClientDataAsync(string? botID, CancellationToken cancellationToken);
  Task<string?> GetBotNameAsync(string? botID, CancellationToken cancellationToken);
  Task<IEnumerable<AnnouncementData>> GetBotAnnouncementsAsync(string? botID, CancellationToken cancellationToken);
  Task<bool> IsBotConfiguredAsync(string? botID, CancellationToken cancellationToken);
  Task<IEnumerable<BotMetaData>> GetBotMetaDataAsync(CancellationToken cancellationToken);
}