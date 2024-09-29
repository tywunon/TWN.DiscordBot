namespace TWN.DiscordBot.ControlPanel.Controller;

public record BotMetaData(string Name, string ID, BotMetaDataStatus Status);
public enum BotMetaDataStatus
{
  Unhealthy = 0,
  Degraded = 1,
  Healthy = 2,
}

public record AnnouncementData(string TwitchUser,
                               bool IsOnline,
                               AnnouncementDiscordData AnnouncementDiscordData,
                               AnnouncementTwitchUserData AnnouncementTwitchUserData,
                               AnnouncementTwitchStreamData AnnouncementTwitchStreamData);
public record AnnouncementDiscordData(string GuildName,
                                      string GuildIconUrl,
                                      string ChannelName);
public record AnnouncementTwitchUserData(string ID,
                                         string Login,
                                         string DisplayName,
                                         string Type,
                                         string BroadcasterType,
                                         string Description,
                                         string ProfileImageUrl,
                                         string OfflineImageUrl,
                                         int ViewCount,
                                         DateTimeOffset CreatedAt);

public record AnnouncementTwitchStreamData(string ID,
                                           string UserID,
                                           string UserLogin,
                                           string UserName,
                                           string GameID,
                                           string GameName,
                                           string Type,
                                           string Title,
                                           int ViewerCount,
                                           DateTimeOffset StartedAt,
                                           string Language,
                                           string ThumbnailUrl,
                                           IEnumerable<object> TagIDs,
                                           IEnumerable<string> Tags,
                                           bool IsMature);
public record DiscordClientData(IEnumerable<DiscordClientGuildData> GuildData);
public record DiscordClientGuildData(long GuildID,
                                     string GuildName,
                                     string GuildIconUrl,
                                     IEnumerable<DiscordClientChannelData> DiscordChannelData);
public record DiscordClientChannelData(long ChannelID,
                                       string ChannelName,
                                       int ChannelPosition,
                                       long CategoryID,
                                       string CategoryName,
                                       int CategoryPosition);
