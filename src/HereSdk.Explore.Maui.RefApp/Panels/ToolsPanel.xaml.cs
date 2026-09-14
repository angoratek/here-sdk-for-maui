using Here.Explore.Maui.RefApp.ViewModels;

namespace Here.Explore.Maui.RefApp.Panels;

/// <summary>
/// Tools &amp; Settings overlays (drawing banner, reset/clear buttons,
/// object-count badge, tools bottom sheet) shown over the shared map —
/// the HereMapView itself lives in MapHomePage.
/// </summary>
public partial class ToolsPanel : ContentView
{
    public ToolsPanel(ToolsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnOpenSettingsClicked(object? sender, EventArgs e)
    {
        var settingsPage = Handler?.MauiContext?.Services.GetService<Pages.SettingsPage>();
        if (settingsPage is not null)
            await Shell.Current.Navigation.PushModalAsync(settingsPage);
    }
}