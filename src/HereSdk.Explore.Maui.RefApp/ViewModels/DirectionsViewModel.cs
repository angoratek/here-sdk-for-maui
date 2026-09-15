using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;
using Here.Explore.Maui.Models.Routing;
using Here.Explore.Maui.Models.Search;
using Here.Explore.Maui.RefApp.Services;
using Here.Explore.Maui.Services;

namespace Here.Explore.Maui.RefApp.ViewModels;

public partial class DirectionsViewModel : ViewModelBase
{
    private readonly IRoutingService _routingService;
    private readonly ISearchService _searchService;
    private readonly ILocationService? _locationService;
    private readonly IPermissionsService? _permissionsService;
    private IMapService? _mapService;

    [ObservableProperty] private string _originQuery = "";
    [ObservableProperty] private string _destinationQuery = "";
    [ObservableProperty] private IReadOnlyList<Suggestion> _originSuggestions = Array.Empty<Suggestion>();
    [ObservableProperty] private IReadOnlyList<Suggestion> _destinationSuggestions = Array.Empty<Suggestion>();
    [ObservableProperty] private bool _hasOriginSuggestions;
    [ObservableProperty] private bool _hasDestinationSuggestions;
    [ObservableProperty] private Place? _originPlace;
    [ObservableProperty] private Place? _destinationPlace;
    [ObservableProperty] private int _selectedTransportMode;
    /// <summary>Truck-spec editor is only relevant in truck mode (index 1).</summary>
    [ObservableProperty] private bool _isTruckMode;
    [ObservableProperty] private bool _isCalculating;
    [ObservableProperty] private Route? _currentRoute;
    [ObservableProperty] private IReadOnlyList<Route> _alternativeRoutes = Array.Empty<Route>();
    [ObservableProperty] private Section[] _maneuvers = Array.Empty<Section>();
    [ObservableProperty] private string _routeSummary = "";
    /// <summary>Route duration alone (e.g. "32 min") for the ETA hero display.</summary>
    [ObservableProperty] private string _routeEta = "";
    /// <summary>Route length alone (e.g. "14.2 km") for the ETA hero display.</summary>
    [ObservableProperty] private string _routeDistance = "";
    [ObservableProperty] private string _maneuverItems = "";
    [ObservableProperty] private bool _isRouteVisible;
    [ObservableProperty] private bool _isIsolineMode;
    /// <summary>Material glyph for the isoline toggle (circle → place when active).</summary>
    public string IsolineButtonGlyph => IsIsolineMode ? "\ue55f" : "\ue39e";
    partial void OnIsIsolineModeChanged(bool value) => OnPropertyChanged(nameof(IsolineButtonGlyph));
    partial void OnSelectedTransportModeChanged(int value) => IsTruckMode = value == 1;
    [ObservableProperty] private bool _hasTrafficOnRoute;
    [ObservableProperty] private string _routeError = "";
    [ObservableProperty] private string? _emptyStateTitle;
    [ObservableProperty] private string? _emptyStateSubtitle;

    // Truck vehicle specification editor (truck mode only; empty = SDK default).
    [ObservableProperty] private string _truckHeightCm = "";
    [ObservableProperty] private string _truckWidthCm = "";
    [ObservableProperty] private string _truckLengthCm = "";
    [ObservableProperty] private string _truckGrossWeightKg = "";
    [ObservableProperty] private string _truckAxles = "";

    private TruckVehicleSpecifications? ParseTruckSpecs() => IsTruckMode
        ? new TruckVehicleSpecifications(
            GrossWeightInKilograms: ParseIntOrNull(TruckGrossWeightKg),
            HeightInCentimeters: ParseIntOrNull(TruckHeightCm),
            WidthInCentimeters: ParseIntOrNull(TruckWidthCm),
            LengthInCentimeters: ParseIntOrNull(TruckLengthCm),
            AxleCount: ParseIntOrNull(TruckAxles))
        : null;

    private static int? ParseIntOrNull(string? text) =>
        int.TryParse(text?.Trim(), out var value) ? value : null;

    private CancellationTokenSource? _originDebounce;
    private CancellationTokenSource? _destDebounce;
    private MapPolyline? _routePolyline;
    private readonly List<MapPolyline> _altRoutePolylines = new();
    private readonly List<MapPolygon> _isolinePolygons = new();
    private MapMarker? _originMarker;
    private MapMarker? _destMarker;
    private MapMarker? _isolineCenterMarker;
    private const int DebounceMs = 300;
    // True while Origin/DestinationQuery is set programmatically so the
    // suggestion popup is not triggered for an already-resolved place.
    private bool _suppressOriginSuggestions;
    private bool _suppressDestinationSuggestions;

    public DirectionsViewModel(IRoutingService routingService, ISearchService searchService,
        ILocationService? locationService = null, IPermissionsService? permissionsService = null)
    {
        _routingService = routingService;
        _searchService = searchService;
        _locationService = locationService;
        _permissionsService = permissionsService;
    }

    /// <summary>
    /// Whether this VM's tab panel is the active one over the shared map.
    /// All panel VMs receive the same map events from the single map; the
    /// tap handler early-returns when inactive. Defaults to true so unit
    /// tests that never switch tabs behave as before.
    /// </summary>
    public bool IsActive { get; set; } = true;

    public void SetMapService(IMapService mapService)
    {
        _mapService = mapService;
        _mapService.MapTapped += OnMapTappedDismissSuggestions;
    }

    private void OnMapTappedDismissSuggestions(object? sender, MapTappedEventArgs e)
    {
        if (!IsActive) return;
        DismissSuggestions();
    }

    /// <summary>
    /// Closes both suggestion dropdowns and cancels any in-flight debounced
    /// suggest call — used after a suggestion is picked and when the map is
    /// tapped, so the popups never linger over the map.
    /// </summary>
    public void DismissSuggestions()
    {
        _originDebounce?.Cancel();
        _destDebounce?.Cancel();
        OriginSuggestions = Array.Empty<Suggestion>();
        DestinationSuggestions = Array.Empty<Suggestion>();
        HasOriginSuggestions = false;
        HasDestinationSuggestions = false;
    }

    partial void OnOriginQueryChanged(string value)
    {
        if (_suppressOriginSuggestions) return;
        if (value.Length >= 2) _ = DebouncedSuggestAsync(value, true);
        else { OriginSuggestions = Array.Empty<Suggestion>(); HasOriginSuggestions = false; }
    }

    partial void OnDestinationQueryChanged(string value)
    {
        if (_suppressDestinationSuggestions) return;
        if (value.Length >= 2) _ = DebouncedSuggestAsync(value, false);
        else { DestinationSuggestions = Array.Empty<Suggestion>(); HasDestinationSuggestions = false; }
    }

    private async Task DebouncedSuggestAsync(string query, bool isOrigin)
    {
        if (isOrigin)
        {
            _originDebounce?.Cancel();
            _originDebounce = new CancellationTokenSource();
            var token = _originDebounce.Token;
            try
            {
                await Task.Delay(DebounceMs, token);
                if (token.IsCancellationRequested) return;
                var area = new GeoCoordinates(37.7749, -122.4194);
                var result = await _searchService.SuggestAsync(new TextQuery(query, area), new SearchOptions { MaxItems = 5 });
                if (!token.IsCancellationRequested)
                {
                    OriginSuggestions = result.Suggestions ?? Array.Empty<Suggestion>();
                    HasOriginSuggestions = OriginSuggestions.Count > 0;
                    System.Diagnostics.Debug.WriteLine($"[REFAPP_DIAG] Origin suggest '{query}': {OriginSuggestions.Count} results");
                }
            }
            catch (TaskCanceledException) { }
        }
        else
        {
            _destDebounce?.Cancel();
            _destDebounce = new CancellationTokenSource();
            var token = _destDebounce.Token;
            try
            {
                await Task.Delay(DebounceMs, token);
                if (token.IsCancellationRequested) return;
                var area = OriginPlace?.Coordinates ?? new GeoCoordinates(37.7749, -122.4194);
                var result = await _searchService.SuggestAsync(new TextQuery(query, area), new SearchOptions { MaxItems = 5 });
                if (!token.IsCancellationRequested)
                {
                    DestinationSuggestions = result.Suggestions ?? Array.Empty<Suggestion>();
                    HasDestinationSuggestions = DestinationSuggestions.Count > 0;
                    System.Diagnostics.Debug.WriteLine($"[REFAPP_DIAG] Destination suggest '{query}': {DestinationSuggestions.Count} results");
                }
            }
            catch (TaskCanceledException) { }
        }
    }

    [RelayCommand]
    private async Task SelectOriginSuggestion(Suggestion suggestion)
    {
        // Suppress the query-change suggest trigger AND cancel any pending
        // debounce: otherwise the debounced call for the selected title
        // completes after the clear below and re-opens the dropdown.
        _originDebounce?.Cancel();
        _suppressOriginSuggestions = true;
        try { OriginQuery = suggestion.Title; }
        finally { _suppressOriginSuggestions = false; }
        OriginSuggestions = Array.Empty<Suggestion>();
        HasOriginSuggestions = false;
        var place = await _searchService.GetPlaceByIdAsync(suggestion.Id);
        if (place is not null)
        {
            OriginPlace = place;
            UpdateOriginMarker(place);
        }
    }

    [RelayCommand]
    private async Task SelectDestinationSuggestion(Suggestion suggestion)
    {
        // Same suppression as SelectOriginSuggestion — see comment there.
        _destDebounce?.Cancel();
        _suppressDestinationSuggestions = true;
        try { DestinationQuery = suggestion.Title; }
        finally { _suppressDestinationSuggestions = false; }
        DestinationSuggestions = Array.Empty<Suggestion>();
        HasDestinationSuggestions = false;
        var place = await _searchService.GetPlaceByIdAsync(suggestion.Id);
        if (place is not null)
        {
            DestinationPlace = place;
            UpdateDestMarker(place);
        }
    }

    [RelayCommand]
    private void SwapLocations()
    {
        (OriginQuery, DestinationQuery) = (DestinationQuery, OriginQuery);
        (OriginPlace, DestinationPlace) = (DestinationPlace, OriginPlace);
        if (OriginPlace is not null) UpdateOriginMarker(OriginPlace);
        if (DestinationPlace is not null) UpdateDestMarker(DestinationPlace);
    }

    /// <summary>
    /// Sets a POI selected on the Explore page as the destination, uses the
    /// current device location as the origin, and calculates the route.
    /// </summary>
    public async Task SetPoiDestinationAsync(Place destination)
    {
        System.Diagnostics.Debug.WriteLine($"[REFAPP_DIAG] SetPoiDestinationAsync: {destination.Title} ({destination.Coordinates.Latitude},{destination.Coordinates.Longitude})");

        DestinationPlace = destination;
        _suppressDestinationSuggestions = true;
        try { DestinationQuery = destination.Title; }
        finally { _suppressDestinationSuggestions = false; }

        if (_locationService is null)
        {
            RouteError = "Location service unavailable. Set an origin to calculate a route.";
            return;
        }

        try
        {
            if (_permissionsService is not null && !await _permissionsService.RequestLocationPermissionAsync())
            {
                RouteError = "Location permission denied. Set an origin to calculate a route.";
                return;
            }

            var loc = await _locationService.GetCurrentLocationAsync();
            if (loc is null)
            {
                RouteError = "Could not determine your location. Set an origin to calculate a route.";
                return;
            }

            var origin = new Place("current-location", "Current location", loc.Coordinates);
            OriginPlace = origin;
            _suppressOriginSuggestions = true;
            try { OriginQuery = $"{loc.Coordinates.Latitude:F5}, {loc.Coordinates.Longitude:F5}"; }
            finally { _suppressOriginSuggestions = false; }

            if (_mapService is not null)
            {
                UpdateOriginMarker(origin);
                UpdateDestMarker(destination);
                await CalculateRouteCommand.ExecuteAsync(null);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[REFAPP_DIAG] SetPoiDestinationAsync: map service not ready — set destination only");
            }
        }
        catch (Exception ex)
        {
            RouteError = $"Location error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task CalculateRoute()
    {
        if (_mapService is null || OriginPlace is null || DestinationPlace is null)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[REFAPP_DIAG] CalculateRoute skipped: mapService={_mapService is not null} " +
                $"originSet={OriginPlace is not null} destinationSet={DestinationPlace is not null}");
            return;
        }

        System.Diagnostics.Debug.WriteLine($"[REFAPP_DIAG] CalculateRoute: {OriginPlace.Title} -> {DestinationPlace.Title}");
        IsCalculating = true;
        RouteError = "";
        EmptyStateTitle = null;
        ClearRoute();
        // ClearRoute drops the A/B pins too — restore them so the freshly
        // calculated route still shows its endpoints (camera re-fits below).
        var originPlace = OriginPlace;
        var destinationPlace = DestinationPlace;
        if (originPlace is not null) UpdateOriginMarker(originPlace);
        if (destinationPlace is not null) UpdateDestMarker(destinationPlace);

        try
        {
            var waypoints = new List<Waypoint>
            {
                new(originPlace!.Coordinates),
                new(destinationPlace!.Coordinates)
            };

            var mode = TransportModeFromInt(SelectedTransportMode);
            var options = new RoutingOptions
            {
                TransportMode = mode,
                MaxAlternatives = 2,
                Truck = ParseTruckSpecs()
            };
            if (options.Truck is not null)
                System.Diagnostics.Debug.WriteLine($"[REFAPP_DIAG] Truck specs: {options.Truck}");

            var result = await _routingService.CalculateRouteAsync(waypoints, options);

            if (result.Error != RoutingError.None)
            {
                System.Diagnostics.Debug.WriteLine($"[REFAPP_DIAG] CalculateRoute failed: {result.Error}");
                RouteError = $"Routing error: {result.Error}";
                IsCalculating = false;
                IsRouteVisible = false;
                return;
            }

            CurrentRoute = result.Routes?.FirstOrDefault();
            if (CurrentRoute is null)
            {
                EmptyStateTitle = "No route found";
                EmptyStateSubtitle = "Try different locations or transport mode";
                IsCalculating = false;
                IsRouteVisible = false;
                return;
            }

            AlternativeRoutes = result.Routes!.Skip(1).ToList();

            // Draw route on map
            var geometry = CurrentRoute.Sections?.SelectMany(s => s.Geometry ?? Array.Empty<GeoCoordinates>())
                .ToList() ?? new List<GeoCoordinates>();

            if (geometry.Count >= 2)
            {
                _routePolyline = new MapPolyline(geometry, Color: 0xFFFF385C, WidthInPixels: 8);
                _mapService.AddMapPolyline(_routePolyline);
            }

            // Draw alternative routes thinner
            foreach (var alt in AlternativeRoutes)
            {
                var altGeom = alt.Sections?.SelectMany(s => s.Geometry ?? Array.Empty<GeoCoordinates>())
                    .ToList() ?? new List<GeoCoordinates>();
                if (altGeom.Count >= 2)
                {
                    var altLine = new MapPolyline(altGeom, Color: 0xFF8E8E93, WidthInPixels: 4);
                    _mapService.AddMapPolyline(altLine);
                    _altRoutePolylines.Add(altLine);
                }
            }

            RouteSummary = FormatRouteSummary(CurrentRoute);
            (RouteEta, RouteDistance) = FormatEtaAndDistance(CurrentRoute);
            Maneuvers = CurrentRoute.Sections?.ToArray() ?? Array.Empty<Section>();
            IsRouteVisible = true;
            System.Diagnostics.Debug.WriteLine($"[REFAPP_DIAG] CalculateRoute ok: eta={RouteEta} distance={RouteDistance}");

            // Fit camera to route
            if (geometry.Count >= 2)
            {
                var mid = geometry[geometry.Count / 2];
                await _mapService.SetCameraTargetAsync(mid, 12);
            }
        }
        catch (Exception ex)
        {
            RouteError = ex.Message;
            IsRouteVisible = false;
        }
        finally
        {
            IsCalculating = false;
        }
    }

    [RelayCommand]
    private async Task ToggleIsolineMode()
    {
        IsIsolineMode = !IsIsolineMode;
        if (!IsIsolineMode)
        {
            ClearIsolines();
            return;
        }

        if (_mapService is null) return;
        var center = OriginPlace?.Coordinates ?? DestinationPlace?.Coordinates ?? new GeoCoordinates(37.7749, -122.4194);

        // Mark center
        _isolineCenterMarker = new MapMarker(center, Color: MarkerVisuals.Neutral, Glyph: MarkerVisuals.GlyphPlace);
        _mapService.AddMapMarker(_isolineCenterMarker);

        try
        {
            var isoOptions = new IsolineOptions
            {
                TransportMode = TransportModeFromInt(SelectedTransportMode),
                RangeInMeters = 5000
            };
            var result = await _routingService.CalculateIsolineAsync(center, isoOptions);
            if (result.Isolines is not null)
            {
                foreach (var isoline in result.Isolines)
                {
                    var polygon = new MapPolygon(isoline.Polygon, FillColor: 0x33FF385C);
                    _mapService.AddMapPolygon(polygon);
                    _isolinePolygons.Add(polygon);
                }
            }
        }
        catch (Exception ex)
        {
            RouteError = $"Isoline error: {ex.Message}";
            IsIsolineMode = false;
        }
    }

    [RelayCommand]
    private async Task ToggleTrafficOnRoute()
    {
        HasTrafficOnRoute = !HasTrafficOnRoute;
        if (!HasTrafficOnRoute || CurrentRoute is null) return;

        try
        {
            var result = await _routingService.GetTrafficOnRouteAsync(CurrentRoute);
            if (result.TrafficOnRoute is null)
            {
                HasTrafficOnRoute = false;
                return;
            }

            var totalDelay = result.TrafficOnRoute.TrafficSections
                .SelectMany(s => s.TrafficSpans)
                .Sum(s => s.TrafficDelayInSeconds);
            RouteSummary = FormatRouteSummary(CurrentRoute) + $" | Traffic delay: {(int)totalDelay}s";
        }
        catch { HasTrafficOnRoute = false; }
    }

    [RelayCommand]
    private void ClearRoute()
    {
        ClearRouteInternal();
        // Explicit clear also drops the A/B pins (Reset Map relies on this);
        // CalculateRoute uses ClearRouteInternal only, so its pins survive.
        if (_mapService is not null)
        {
            if (_originMarker is not null) _mapService.RemoveMapMarker(_originMarker);
            if (_destMarker is not null) _mapService.RemoveMapMarker(_destMarker);
        }
        _originMarker = null;
        _destMarker = null;
        IsRouteVisible = false;
        RouteSummary = "";
        RouteEta = "";
        RouteDistance = "";
        Maneuvers = Array.Empty<Section>();
        CurrentRoute = null;
        AlternativeRoutes = Array.Empty<Route>();
        RouteError = "";
        EmptyStateTitle = null;
    }

    private void ClearRouteInternal()
    {
        if (_mapService is null) return;
        if (_routePolyline is not null) _mapService.RemoveMapPolyline(_routePolyline);
        _routePolyline = null;
        foreach (var p in _altRoutePolylines) _mapService.RemoveMapPolyline(p);
        _altRoutePolylines.Clear();
    }

    private void ClearIsolines()
    {
        if (_mapService is null) return;
        foreach (var p in _isolinePolygons) _mapService.RemoveMapPolygon(p);
        _isolinePolygons.Clear();
        if (_isolineCenterMarker is not null) _mapService.RemoveMapMarker(_isolineCenterMarker);
        _isolineCenterMarker = null;
    }

    private void UpdateOriginMarker(Place place)
    {
        if (_mapService is null) return;
        if (_originMarker is not null) _mapService.RemoveMapMarker(_originMarker);
        _originMarker = new MapMarker(place.Coordinates, Color: MarkerVisuals.Origin, Glyph: MarkerVisuals.GlyphOrigin);
        _mapService.AddMapMarker(_originMarker);
        _ = _mapService.SetCameraTargetAsync(place.Coordinates, 14);
    }

    private void UpdateDestMarker(Place place)
    {
        if (_mapService is null) return;
        if (_destMarker is not null) _mapService.RemoveMapMarker(_destMarker);
        _destMarker = new MapMarker(place.Coordinates, Color: MarkerVisuals.Accent, Glyph: MarkerVisuals.GlyphDestination);
        _mapService.AddMapMarker(_destMarker);
    }

    private static string FormatRouteSummary(Route route)
    {
        var km = route.LengthInMeters / 1000.0;
        var mins = (int)(route.DurationInSeconds / 60);
        if (mins >= 60)
            return $"{km:F1} km — {mins / 60}h {mins % 60}min";
        return $"{km:F1} km — {mins}min";
    }

    /// <summary>Duration and length split for the ETA hero (e.g. "32 min", "14.2 km").</summary>
    private static (string Eta, string Distance) FormatEtaAndDistance(Route route)
    {
        var km = route.LengthInMeters / 1000.0;
        var mins = (int)(route.DurationInSeconds / 60);
        var eta = mins >= 60 ? $"{mins / 60}h {mins % 60}min" : $"{mins} min";
        return (eta, $"{km:F1} km");
    }

    private static SectionTransportMode TransportModeFromInt(int mode) => mode switch
    {
        0 => SectionTransportMode.Car,
        1 => SectionTransportMode.Truck,
        2 => SectionTransportMode.Pedestrian,
        3 => SectionTransportMode.Bicycle,
        4 => SectionTransportMode.Scooter,
        5 => SectionTransportMode.Bus,
        6 => SectionTransportMode.Taxi,
        7 => SectionTransportMode.Transit,
        _ => SectionTransportMode.Car,
    };
}
