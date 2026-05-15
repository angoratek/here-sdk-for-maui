using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;
using Here.Explore.Maui.Models.Routing;
using Here.Explore.Maui.Models.Search;
using Here.Explore.Maui.Services;

namespace Here.Explore.Maui.RefApp.ViewModels;

public partial class DirectionsViewModel : ViewModelBase
{
    private readonly IRoutingService _routingService;
    private readonly ISearchService _searchService;
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
    [ObservableProperty] private bool _isCalculating;
    [ObservableProperty] private Route? _currentRoute;
    [ObservableProperty] private IReadOnlyList<Route> _alternativeRoutes = Array.Empty<Route>();
    [ObservableProperty] private Section[] _maneuvers = Array.Empty<Section>();
    [ObservableProperty] private string _routeSummary = "";
    [ObservableProperty] private string _maneuverItems = "";
    [ObservableProperty] private bool _isRouteVisible;
    [ObservableProperty] private bool _isIsolineMode;
    [ObservableProperty] private bool _hasTrafficOnRoute;
    [ObservableProperty] private string _routeError = "";
    [ObservableProperty] private string? _emptyStateTitle;
    [ObservableProperty] private string? _emptyStateSubtitle;

    private CancellationTokenSource? _originDebounce;
    private CancellationTokenSource? _destDebounce;
    private MapPolyline? _routePolyline;
    private readonly List<MapPolyline> _altRoutePolylines = new();
    private readonly List<MapPolygon> _isolinePolygons = new();
    private MapMarker? _originMarker;
    private MapMarker? _destMarker;
    private MapMarker? _isolineCenterMarker;
    private const int DebounceMs = 300;

    public DirectionsViewModel(IRoutingService routingService, ISearchService searchService)
    {
        _routingService = routingService;
        _searchService = searchService;
    }

    public void SetMapService(IMapService mapService)
    {
        _mapService = mapService;
    }

    partial void OnOriginQueryChanged(string value)
    {
        if (value.Length >= 2) _ = DebouncedSuggestAsync(value, true);
        else { OriginSuggestions = Array.Empty<Suggestion>(); HasOriginSuggestions = false; }
    }

    partial void OnDestinationQueryChanged(string value)
    {
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
                }
            }
            catch (TaskCanceledException) { }
        }
    }

    [RelayCommand]
    private async Task SelectOriginSuggestion(Suggestion suggestion)
    {
        OriginQuery = suggestion.Title;
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
        DestinationQuery = suggestion.Title;
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

    [RelayCommand]
    private async Task CalculateRoute()
    {
        if (_mapService is null || OriginPlace is null || DestinationPlace is null) return;

        IsCalculating = true;
        RouteError = "";
        EmptyStateTitle = null;
        ClearRoute();

        try
        {
            var waypoints = new List<Waypoint>
            {
                new(OriginPlace.Coordinates),
                new(DestinationPlace.Coordinates)
            };

            var mode = TransportModeFromInt(SelectedTransportMode);
            var options = new RoutingOptions
            {
                TransportMode = mode,
                MaxAlternatives = 2
            };

            var result = await _routingService.CalculateRouteAsync(waypoints, options);

            if (result.Error != RoutingError.None)
            {
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
                _routePolyline = new MapPolyline(geometry, Color: 0xFF007AFF, WidthInPixels: 8);
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
            Maneuvers = CurrentRoute.Sections?.ToArray() ?? Array.Empty<Section>();
            IsRouteVisible = true;

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
        _isolineCenterMarker = new MapMarker(center);
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
                    var polygon = new MapPolygon(isoline.Polygon, FillColor: 0x44007AFF);
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
            var traffic = await _routingService.GetTrafficOnRouteAsync(CurrentRoute);
            RouteSummary = FormatRouteSummary(CurrentRoute) + $" | Traffic delay: {traffic.DelayInSeconds ?? 0}s";
        }
        catch { HasTrafficOnRoute = false; }
    }

    [RelayCommand]
    private void ClearRoute()
    {
        ClearRouteInternal();
        IsRouteVisible = false;
        RouteSummary = "";
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
        _originMarker = new MapMarker(place.Coordinates);
        _mapService.AddMapMarker(_originMarker);
        _ = _mapService.SetCameraTargetAsync(place.Coordinates, 14);
    }

    private void UpdateDestMarker(Place place)
    {
        if (_mapService is null) return;
        if (_destMarker is not null) _mapService.RemoveMapMarker(_destMarker);
        _destMarker = new MapMarker(place.Coordinates);
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
