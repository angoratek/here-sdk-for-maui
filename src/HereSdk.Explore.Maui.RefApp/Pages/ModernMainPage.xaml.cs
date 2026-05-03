using Here.Explore.Maui.RefApp.ViewModels;
using Here.Explore.Maui.Services;
using Here.Explore.Maui.Models.Maps;

namespace Here.Explore.Maui.RefApp.Pages;

public partial class ModernMainPage : ContentPage
{
    private readonly ModernMainViewModel _viewModel;
    private readonly SettingsViewModel _settingsViewModel;

    public ModernMainPage(ModernMainViewModel viewModel, SettingsViewModel settingsViewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _settingsViewModel = settingsViewModel;
        BindingContext = _viewModel;

        Loaded += OnPageLoaded;
    }

    private void OnPageLoaded(object? sender, EventArgs e)
    {
        Loaded -= OnPageLoaded;
        _viewModel.InitializeMapView(MapView);

        // Wire up map events for drawing mode
        if (MapView.Map is IMapService mapService)
        {
            mapService.MapTapped += OnMapTapped;
            mapService.MapDoubleTapped += OnMapDoubleTapped;
        }
    }

    private void OnMapTapped(object? sender, Models.Maps.MapTappedEventArgs e)
    {
        _viewModel.OnMapTapped(e.Coordinates);
    }

    private void OnMapDoubleTapped(object? sender, Models.Maps.MapTappedEventArgs e)
    {
        _viewModel.OnMapDoubleTapped(e.Coordinates);
    }

    private async void OnSettingsClicked(object? sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new NavigationPage(new SettingsPage(_settingsViewModel)));
    }
}
