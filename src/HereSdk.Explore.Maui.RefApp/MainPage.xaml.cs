using Here.Explore.Maui.Controls;
using Here.Explore.Maui.RefApp.ViewModels;

namespace Here.Explore.Maui.RefApp;

public partial class MainPage : ContentPage
{
    private MapViewModel _viewModel;

    public MainPage()
    {
        InitializeComponent();
        _viewModel = new MapViewModel();
        BindingContext = _viewModel;
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        // Wire the MapViewModel with the MapService once the handler creates it
        if (MapView.Map is { } mapService)
            _viewModel.SetMapService(mapService);
    }
}