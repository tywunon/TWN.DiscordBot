using BlazorBootstrap;

using Microsoft.AspNetCore.Components;

using OneOf;
using OneOf.Types;

using TWN.DiscordBot.ControlPanel.Components.Types;
using TWN.DiscordBot.ControlPanel.Controller;

namespace TWN.DiscordBot.ControlPanel.Components;
public partial class AddAnnouncmentDialog
{
  [Parameter] public required Action<string, IEnumerable<AddDialogData>> ResultCallback { get; set; }
  [Parameter] public required Func<Task<IEnumerable<AddDialogData>>> DataSelector { get; set; }

  private string twitchUser = default!;
  HashSet<AddDialogData> selectedAddDialogItems = [];

  private Task HandleSelectedItemsChanged(HashSet<AddDialogData> selection)
  {
    selectedAddDialogItems =
      selection is not null && selection.Count > 0
        ? new(selection)
        : selectedAddDialogItems;
    return Task.CompletedTask;
  }

  private async Task HandleDialogAdd()
  {
    await addDialog.HideAsync();

    if (selectedAddDialogItems is null)
      return;

    ResultCallback(twitchUser.Trim(), selectedAddDialogItems);
  }

  private async Task<GridDataProviderResult<AddDialogData>> AddDialogDataDataProvider(GridDataProviderRequest<AddDialogData> request)
  => await Task.FromResult(request.ApplyTo(await DataSelector()));

  public async Task ShowAsync() => await addDialog.ShowAsync();
}