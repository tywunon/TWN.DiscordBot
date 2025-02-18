using BlazorBootstrap;

using LanguageExt;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using TWN.DiscordBot.ControlPanel.Components.Types;
using TWN.DiscordBot.ControlPanel.Controller;

namespace TWN.DiscordBot.ControlPanel.Components.Pages;
public partial class BotOverview
{
  [Parameter] public string? BotID { get; set; } = default!;

  [Inject] IBotDataController BotDataController { get; set; } = default!;
  [Inject] IJSRuntime JSRuntime { get; set; } = default!;

  private string botName = default!;

  private async Task<GridDataProviderResult<AnnouncementData>> AnnouncementsDataProvider(GridDataProviderRequest<AnnouncementData> request)
    => await Task.FromResult(request.ApplyTo(await GetBotAnnouncements(request.CancellationToken)));

  private async Task<IEnumerable<AnnouncementData>> GetBotAnnouncements(CancellationToken cancellationToken)
    => await BotDataController.GetBotAnnouncementsAsync(BotID, cancellationToken);

  protected override async Task OnInitializedAsync()
  {
    await base.OnInitializedAsync();

    botName = await BotDataController.GetBotNameAsync(BotID, CancellationToken.None) ?? "Unknown Bot";
  }

  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    await base.OnAfterRenderAsync(firstRender);
  }

  private async Task HandleReload()
  {
    await dataGrid.RefreshDataAsync();
  }

  private async Task HandleAdd()
  {
    await addAnnouncementDialog.ShowAsync();
  }

  private async void AddAnnouncmentDialogCallback(string twitchUser, IEnumerable<AddDialogData> dialogData)
  {
    if (string.IsNullOrEmpty(twitchUser) || (dialogData == null))
      return;

    foreach(var dialogDate in dialogData)
    {
      var addAnnouncementResult = await BotDataController.AddAnnouncementAsync(BotID, twitchUser, dialogDate.GuildID, dialogDate.ChannelID, CancellationToken.None);
      addAnnouncementResult.Switch(
        s => NotifySuccess(twitchUser, dialogDate),
        err => NotifyError(err, twitchUser, dialogDate)
      );
    }
  }

  private async Task<IEnumerable<AddDialogData>> GetAddDialogData()
  {
    var clientData = await BotDataController.GetDiscordClientDataAsync(BotID, CancellationToken.None);
    return clientData.GuildData
      .SelectMany(gd => gd.DiscordChannelData, (gd, dcd) => new AddDialogData(gd, dcd))
      .OrderBy(dcd => dcd.GuildID)
      .ThenBy(dcd => dcd.CategoryPosition)
      .ThenBy(dcd => dcd.ChannelPosition)
      ;
  }

  private async Task HandleDelete(AnnouncementData announcementData)
  {
    if (announcementData is null)
      return;

    var twitchUser = announcementData.TwitchUser;
    var guildID = announcementData.AnnouncementDiscordData.GuildID;
    var guildName = announcementData.AnnouncementDiscordData.GuildName;
    var channelID = announcementData.AnnouncementDiscordData.ChannelID;
    var channelName = announcementData.AnnouncementDiscordData.ChannelName;

    var confirmationOptions = new ConfirmDialogOptions()
    {
      Size = DialogSize.Large,
      YesButtonText = "Proceed with deletion",
      YesButtonColor = ButtonColor.Danger,
      NoButtonText = "Don't Delete",
      NoButtonColor = ButtonColor.Secondary,
      AutoFocusYesButton = false,
    };
    var confirmation = await deleteConfirmDialog.ShowAsync("Are you sure you want to delete this announcement?", $"{twitchUser} | {guildName}({guildID}) | {channelName}({guildID})", confirmationOptions);
    if (!confirmation)
      return;

    var deleteAnnouncementResult = await BotDataController.DeleteAnnouncementAsync(BotID, twitchUser, guildID, channelID, CancellationToken.None);
    await deleteAnnouncementResult.Match(
      s =>
      {
        ToastService.Notify(new ToastMessage()
        {
          Type = ToastType.Success,
          Title = botName,
          HelpText = $"{DateTime.Now}",
          AutoHide = true,
          Message = $"Announcement for {twitchUser} in Channel {channelName} on {guildName} deleted",
          IconName = IconName.Database,
        });
        return dataGrid.RefreshDataAsync();
      },
      err =>
      {
        ToastService.Notify(new ToastMessage()
        {
          Type = ToastType.Danger,
          Title = botName,
          HelpText = $"{DateTime.Now}",
          AutoHide = true,
          Message = $"Failed to delete Announcement for {twitchUser} in Channel {channelName} on {guildName}: {err.Value}",
          IconName = IconName.Database,
        });
        return Task.CompletedTask;
      }
    );

    await Task.CompletedTask;
  }

  private async Task HandleRun(Controller.AnnouncementData announcementData)
  {
    if (announcementData is null)
      return;

    var twitchUser = announcementData.TwitchUser;
    var guildID = announcementData.AnnouncementDiscordData.GuildID;
    var guildName = announcementData.AnnouncementDiscordData.GuildName;
    var channelID = announcementData.AnnouncementDiscordData.ChannelID;
    var channelName = announcementData.AnnouncementDiscordData.ChannelName;

    var postTwitchAnnouncementResult = await BotDataController.PostTwitchAnnouncementAsync(BotID, twitchUser, guildID, channelID, CancellationToken.None);
    await postTwitchAnnouncementResult.Match(
      s =>
      {
        ToastService.Notify(new ToastMessage()
        {
          Type = ToastType.Success,
          Title = botName,
          HelpText = $"{DateTime.Now}",
          AutoHide = true,
          Message = $"Announcement for {twitchUser} in Channel {channelName} on {guildName} was posted",
          IconName = IconName.Database,
        });
        return Task.CompletedTask;
      },
      err =>
      {
        ToastService.Notify(new ToastMessage()
        {
          Type = ToastType.Danger,
          Title = botName,
          HelpText = $"{DateTime.Now}",
          AutoHide = true,
          Message = $"Failed to post Announcement for {twitchUser} in Channel {channelName} on {guildName}: {err.Value}",
          IconName = IconName.Database,
        });
        return Task.CompletedTask;
      }
    );
    await Task.CompletedTask;
  }

  private async Task GoToWebsiteAsync(string twitchUser)
  {
    var jsTask = JSRuntime?.InvokeVoidAsync("open", $"https://www.twitch.tv/{twitchUser}", "_blank");
    if(jsTask is not null)
      await jsTask.Value;
  }
}