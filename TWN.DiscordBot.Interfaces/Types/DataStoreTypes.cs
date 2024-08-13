namespace TWN.DiscordBot.Interfaces.Types;
public sealed record Data(ICollection<Announcement> Announcements) { }

public sealed record Announcement(string TwitchUser, ulong GuildID, ulong ChannelID) { }
