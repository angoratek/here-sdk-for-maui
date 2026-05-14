using Here.Explore.Maui.RefApp.ViewModels;

namespace Here.Explore.Maui.RefApp.Pages;

public partial class ToolsPage : ContentPage
{
    private readonly ToolsViewModel _viewModel;

    public ToolsPage(ToolsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;

        MapView.HandlerChanged += OnMapViewHandlerChanged;
    }

    private void OnMapViewHandlerChanged(object? sender, EventArgs e)
    {
        if (MapView.Handler is not null && MapView.Map is not null)
        {
            MapView.HandlerChanged -= OnMapViewHandlerChanged;
            _viewModel.SetMapService(MapView.Map);
        }
    }

    private async void OnOpenSettingsClicked(object? sender, EventArgs e)
    {
        var settingsPage = Handler?.MauiContext?.Services.GetService<SettingsPage>();
        if (settingsPage is not null)
            await Navigation.PushModalAsync(settingsPage);
    }
}
