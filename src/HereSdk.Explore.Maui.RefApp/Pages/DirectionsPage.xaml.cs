using System.Globalization;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Search;
using Here.Explore.Maui.RefApp.Controls;
using Here.Explore.Maui.RefApp.ViewModels;

namespace Here.Explore.Maui.RefApp.Pages;

public partial class DirectionsPage : ContentPage, IQueryAttributable
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

        // Auto-expand the route sheet when a route lands: it starts
        // Collapsed (height 0), so without this the ETA and maneuver
        // timeline are invisible until the user drags the sheet up.
        _viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName != nameof(DirectionsViewModel.IsRouteVisible)) return;
            RouteSheet.CurrentState = _viewModel.IsRouteVisible
                ? BottomSheet.SheetState.HalfExpanded
                : BottomSheet.SheetState.Collapsed;
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

    /// <summary>
    /// Receives the POI passed from the Explore place card
    /// (placeId, placeName, lat, lng) and starts route calculation
    /// from the current location.
    /// </summary>
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.TryGetValue("lat", out var latObj) ||
            !query.TryGetValue("lng", out var lngObj) ||
            !double.TryParse(latObj?.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var lat) ||
            !double.TryParse(lngObj?.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var lng))
        {
            return;
        }

        query.TryGetValue("placeName", out var nameObj);
        query.TryGetValue("placeId", out var idObj);
        var name = nameObj?.ToString();
        var place = new Place(
            idObj?.ToString() ?? "poi",
            string.IsNullOrWhiteSpace(name) ? "Destination" : name!,
            new GeoCoordinates(lat, lng));

        _ = _viewModel.SetPoiDestinationAsync(place);
    }
}