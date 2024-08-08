using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TWN.DiscordBot.Interfaces.Types;

namespace TWN.DiscordBot.API.Services;
internal interface IDataStoreService
{
  Task DeleteAnnouncementAsync(string twitchUser, ulong guildID, ulong? channelID);
  Task<Announcement> AddAnnouncementAsync(string twitchUser, ulong guildID, ulong channelID);
  Task<IEnumerable<Announcement>> GetAnnouncementsAsync();
}