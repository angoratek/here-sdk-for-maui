using System.ComponentModel;

using Here.Explore.Maui.RefApp.Controls;
using Here.Explore.Maui.RefApp.ViewModels;

namespace Here.Explore.Maui.RefApp.Panels;

/// <summary>
/// Tools &amp; Settings overlays (floating drawing toolbar, hint pill,
/// reset/clear buttons, object-count badge, tools bottom sheet) shown over
/// the shared map — the HereMapView itself lives in MapHomePage.
/// </summary>
public partial class ToolsPanel : ContentView
{
    private BottomSheet.SheetState _sheetStateBeforeDrawing;

    public ToolsPanel(ToolsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        // Picking a tool collapses the sheet so the map being drawn on is
        // visible; ending the session (Done/✕) restores the previous state.
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
        ToolsSheet.PropertyChanged += OnSheetPropertyChanged;
        UpdateMapControlsVisibility();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is not nameof(ToolsViewModel.IsDrawingActive))
        {
            return;
        }

        var vm = (ToolsViewModel)sender!;
        if (vm.IsDrawingActive)
        {
            if (ToolsSheet.CurrentState != BottomSheet.SheetState.Collapsed)
            {
                _sheetStateBeforeDrawing = ToolsSheet.CurrentState;
                ToolsSheet.CurrentState = BottomSheet.SheetState.Collapsed;
            }
        }
        else if (ToolsSheet.CurrentState == BottomSheet.SheetState.Collapsed)
        {
            ToolsSheet.CurrentState = _sheetStateBeforeDrawing;
        }
    }

    private void OnSheetPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(BottomSheet.CurrentState))
        {
            UpdateMapControlsVisibility();
        }
    }

    private void UpdateMapControlsVisibility()
    {
        // Map mode: the floating controls (drawing toolbar, reset/clear,
        // object-count badge) live over the map. While the sheet is expanded
        // they would sit on top of its Settings / Demo Gallery rows and
        // swallow their taps, so hide them — the sheet header shows the
        // object count instead.
        MapControlsLayer.IsVisible =
            ToolsSheet.CurrentState == BottomSheet.SheetState.Collapsed ||
            ((ToolsViewModel)BindingContext).IsDrawingActive;
    }

    private async void OnOpenSettingsClicked(object? sender, EventArgs e)
    {
        var settingsPage = Handler?.MauiContext?.Services.GetService<Pages.SettingsPage>();
        if (settingsPage is not null)
            await Shell.Current.Navigation.PushModalAsync(settingsPage);
    }
}