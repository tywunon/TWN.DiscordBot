using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TWN.DiscordBot.Interfaces.Types;

namespace TWN.DiscordBot.WebHost.Services;
internal static class Payloads
{
  internal record EmptyPayload() { }
  internal record AnnouncementsPayload(IEnumerable<Announcement> Announcements) { }
  internal record AnnouncementPayload(Announcement Announcement) { }
  internal record ChannelNamePayload(ulong ChannelID, string ChannelName);
  internal record GuildNamePayload(ulong GuildID, string GuildName);
  internal record StreamDataPayload(StreamsResponseData? StreamsResponse, bool IsOnline);
  internal record UserDataPayload(UsersResponseData? UsersResponse);
}
