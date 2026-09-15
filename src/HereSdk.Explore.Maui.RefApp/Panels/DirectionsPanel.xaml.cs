using Here.Explore.Maui.RefApp.Controls;
using Here.Explore.Maui.RefApp.ViewModels;

namespace Here.Explore.Maui.RefApp.Panels;

/// <summary>
/// Directions overlays (origin/destination inputs, suggestions, transport
/// mode picker, route sheet) shown over the shared map — the HereMapView
/// itself lives in MapHomePage.
/// </summary>
public partial class DirectionsPanel : ContentView
{
    private readonly DirectionsViewModel _viewModel;

    public DirectionsPanel(DirectionsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;

        ModePicker.ModeSelected += (_, mode) =>
        {
            _viewModel.SelectedTransportMode = mode;
            // A visible route recalculates immediately so the effect of the
            // mode change (e.g. car ↔ truck restrictions) is visible.
            if (_viewModel.IsRouteVisible)
                _viewModel.CalculateRouteCommand.Execute(null);
        };

        // Editing a truck spec re-routes on completion (return key) so the
        // route change from the new restrictions is visible without extra taps.
        foreach (var entry in new[] { TruckHeightEntry, TruckWidthEntry, TruckLengthEntry, TruckWeightEntry, TruckAxlesEntry })
        {
            entry.Completed += (_, _) =>
            {
                if (_viewModel.IsRouteVisible)
                    _viewModel.CalculateRouteCommand.Execute(null);
            };
        }

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
}