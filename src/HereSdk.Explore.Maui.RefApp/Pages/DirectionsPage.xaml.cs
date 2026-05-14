using Here.Explore.Maui.RefApp.ViewModels;

namespace Here.Explore.Maui.RefApp.Pages;

public partial class DirectionsPage : ContentPage
{
    private readonly DirectionsViewModel _viewModel;

    public DirectionsPage(DirectionsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;

        MapView.HandlerChanged += OnMapViewHandlerChanged;

        ModePicker.ModeSelected += (_, mode) =>
        {
            _viewModel.SelectedTransportMode = mode;
        };
    }

    private void OnMapViewHandlerChanged(object? sender, EventArgs e)
    {
        if (MapView.Handler is not null && MapView.Map is not null)
        {
            MapView.HandlerChanged -= OnMapViewHandlerChanged;
            _viewModel.SetMapService(MapView.Map);
        }
    }
}
