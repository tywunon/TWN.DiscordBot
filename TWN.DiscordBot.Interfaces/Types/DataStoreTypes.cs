using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TWN.DiscordBot.Interfaces.Types;
public sealed record Data(ICollection<Announcement> Announcements) { }

public sealed record Announcement(string TwitchUser, ulong GuildID, ulong ChannelID) { }
