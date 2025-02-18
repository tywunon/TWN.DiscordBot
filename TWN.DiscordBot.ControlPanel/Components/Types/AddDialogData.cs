using TWN.DiscordBot.ControlPanel.Controller;

namespace TWN.DiscordBot.ControlPanel.Components.Types;
public class AddDialogData(DiscordClientGuildData guildData, DiscordClientChannelData channelData)
{
  public long GuildID { get; set; } = guildData.GuildID;
  public string GuildName { get; set; } = guildData.GuildName;
  public string GuildIconURL { get; set; } = guildData.GuildIconUrl;
  public long CategoryID { get; set; } = channelData.CategoryID;
  public string CategoryName { get; set; } = channelData.CategoryName;
  public int CategoryPosition { get; set; } = channelData.CategoryPosition;
  public long ChannelID { get; set; } = channelData.ChannelID;
  public string ChannelName { get; set; } = channelData.ChannelName;
  public int ChannelPosition { get; set; } = channelData.ChannelPosition;
}
