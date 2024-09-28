using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using TWN.DiscordBot.Interfaces.Types;

namespace TWN.DiscordBot.WebHost.Services;
internal static class Payloads
{
  internal record EmptyPayload() { }
  internal record AnnouncementsPayload(IEnumerable<Announcement> Announcements) { }
  internal record AnnouncementPayload(Announcement Announcement) { }
  internal record ChannelNamePayload(ulong ChannelID, string ChannelName);
  internal record GuildNamePayload(ulong GuildID, string GuildName);
  internal record GuildIconUrlPayload(ulong GuildID, string GuildIconUrl);
  internal record DiscordClientDataPayload([AllowNull] DiscordClientData? DiscordClientData);
  internal record StreamDataPayload([AllowNull] StreamsResponseData? StreamsResponse, bool IsOnline);
  internal record UserDataPayload([AllowNull]UsersResponseData? UsersResponse);
  internal record HealthCheckPayload(string Status, TimeSpan TotalDuration, IDictionary<string, HealthCheckPayloadEntry> Entries);
  internal record HealthCheckPayloadEntry(IReadOnlyDictionary<string, object> Data, string? Description, TimeSpan Duration, string Status, IEnumerable<string> Tags);
}
