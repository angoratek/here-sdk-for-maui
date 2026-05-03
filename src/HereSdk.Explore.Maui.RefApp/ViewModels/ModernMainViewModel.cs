using System.Collections.ObjectModel;
using System.Windows.Input;
using Here.Explore.Maui.Helpers;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;
using Here.Explore.Maui.Models.Routing;
using Here.Explore.Maui.Models.Search;
using Here.Explore.Maui.Services;

namespace Here.Explore.Maui.RefApp.ViewModels;

public class ModernMainViewModel : ViewModelBase, IDisposable
{
    private IMapService _mapService = null!;
    private readonly ISearchService _searchService;
    private readonly IRoutingService _routingService;
    private readonly ILocationService _locationService;

    // Default location: San Francisco
    private static readonly GeoCoordinates DefaultLocation = new(37.7749, -122.4194);
    private const double DefaultZoom = 12;

    // Search debounce
    private CancellationTokenSource? _originSearchCts;
    private CancellationTokenSource? _destinationSearchCts;

    // Search state
    private string _originQuery = string.Empty;
    private string _destinationQuery = string.Empty;
    private IReadOnlyList<Suggestion>? _originSuggestions;
    private IReadOnlyList<Suggestion>? _destinationSuggestions;
    private Place? _originPlace;
    private Place? _destinationPlace;

    // Route state
    private Route? _currentRoute;
    private MapPolyline? _routePolyline;

    // Map state
    private MapScheme _selectedScheme = MapScheme.NormalDay;
    private double _currentZoom = DefaultZoom;
    private bool _isLoading;
    private bool _isMapObjectsPanelVisible;

    // Location tracking state
    private bool _isTrackingLocation;
    private LocationIndicator? _locationIndicator;

    // Drawing mode state
    private DrawingMode _currentDrawingMode;
    private readonly List<GeoCoordinates> _drawingPoints = new();
    private MapPolyline? _drawingPreviewPolyline;
    private MapPolygon? _drawingPreviewPolygon;

    // Map objects
    private readonly List<MapMarker> _demoMarkers = new();
    private readonly List<MapCircle> _demoCircles = new();
    private readonly List<MapPolyline> _demoPolylines = new();
    private readonly List<MapPolygon> _demoPolygons = new();
    private bool _markersVisible;
    private bool _circlesVisible;
    private bool _polylinesVisible;
    private bool _polygonsVisible;

    public string OriginQuery
    {
        get => _originQuery;
        set => SetProperty(ref _originQuery, value);
    }

    public string DestinationQuery
    {
        get => _destinationQuery;
        set => SetProperty(ref _destinationQuery, value);
    }

    public IReadOnlyList<Suggestion>? OriginSuggestions
    {
        get => _originSuggestions;
        private set => SetProperty(ref _originSuggestions, value);
    }

    public IReadOnlyList<Suggestion>? DestinationSuggestions
    {
        get => _destinationSuggestions;
        private set => SetProperty(ref _destinationSuggestions, value);
    }

    public Place? OriginPlace
    {
        get => _originPlace;
        private set
        {
            if (SetProperty(ref _originPlace, value))
            {
                OnPropertyChanged(nameof(CanCalculateRoute));
                ((Command)CalculateRouteCommand).ChangeCanExecute();
            }
        }
    }

    public Place? DestinationPlace
    {
        get => _destinationPlace;
        private set
        {
            if (SetProperty(ref _destinationPlace, value))
            {
                OnPropertyChanged(nameof(CanCalculateRoute));
                ((Command)CalculateRouteCommand).ChangeCanExecute();
            }
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public Route? CurrentRoute
    {
        get => _currentRoute;
        private set => SetProperty(ref _currentRoute, value);
    }

    public MapScheme SelectedScheme
    {
        get => _selectedScheme;
        set => SetProperty(ref _selectedScheme, value);
    }

    public bool IsMapObjectsPanelVisible
    {
        get => _isMapObjectsPanelVisible;
        set => SetProperty(ref _isMapObjectsPanelVisible, value);
    }

    public bool HasRoute => CurrentRoute != null;

    public bool CanCalculateRoute => OriginPlace != null && DestinationPlace != null;

    public bool IsTrackingLocation
    {
        get => _isTrackingLocation;
        private set => SetProperty(ref _isTrackingLocation, value);
    }

    public DrawingMode CurrentDrawingMode
    {
        get => _currentDrawingMode;
        private set => SetProperty(ref _currentDrawingMode, value);
    }

    public string DistanceText => CurrentRoute != null ? $"{CurrentRoute.LengthInMeters / 1000.0:F1} km" : "--";

    public string DurationText => CurrentRoute != null ? $"{CurrentRoute.DurationInSeconds / 60:F0} min" : "--";

    public IReadOnlyList<Maneuver> Maneuvers =>
        CurrentRoute?.Sections.SelectMany(s => s.Maneuvers).ToList() ?? new List<Maneuver>();

    // Commands
    public ICommand SearchOriginCommand { get; }
    public ICommand SearchDestinationCommand { get; }
    public ICommand SelectOriginCommand { get; }
    public ICommand SelectDestinationCommand { get; }
    public ICommand ClearOriginCommand { get; }
    public ICommand ClearDestinationCommand { get; }
    public ICommand SwapLocationsCommand { get; }
    public ICommand CalculateRouteCommand { get; }
    public ICommand ClearRouteCommand { get; }
    public ICommand CenterOnLocationCommand { get; }
    public ICommand ZoomInCommand { get; }
    public ICommand ZoomOutCommand { get; }
    public ICommand ResetMapOrientationCommand { get; }
    public ICommand ChangeMapSchemeCommand { get; }
    public ICommand ToggleMapObjectsPanelCommand { get; }
    public ICommand ToggleMarkersCommand { get; }
    public ICommand ToggleCirclesCommand { get; }
    public ICommand TogglePolylinesCommand { get; }
    public ICommand TogglePolygonsCommand { get; }
    public ICommand ClearMapObjectsCommand { get; }
    public ICommand SetDrawingModeCommand { get; }
    public ICommand FinishDrawingCommand { get; }
    public ICommand CancelDrawingCommand { get; }

    public ModernMainViewModel(
        ISearchService searchService,
        IRoutingService routingService,
        ILocationService locationService)
    {
        _searchService = searchService;
        _routingService = routingService;
        _locationService = locationService;

        SearchOriginCommand = new Command(async () => await SearchAsync(isOrigin: true));
        SearchDestinationCommand = new Command(async () => await SearchAsync(isOrigin: false));
        SelectOriginCommand = new Command<Suggestion>(async s => await SelectSuggestionAsync(s, isOrigin: true));
        SelectDestinationCommand = new Command<Suggestion>(async s => await SelectSuggestionAsync(s, isOrigin: false));
        ClearOriginCommand = new Command(() => ClearSearch(isOrigin: true));
        ClearDestinationCommand = new Command(() => ClearSearch(isOrigin: false));
        SwapLocationsCommand = new Command(SwapLocations);
        CalculateRouteCommand = new Command(async () => await CalculateRouteAsync(), () => CanCalculateRoute);
        ClearRouteCommand = new Command(ClearRoute);
        CenterOnLocationCommand = new Command(async () => await ToggleLocationTrackingAsync());
        ZoomInCommand = new Command(() => ZoomBy(1));
        ZoomOutCommand = new Command(() => ZoomBy(-1));
        ResetMapOrientationCommand = new Command(ResetMapOrientation);
        ChangeMapSchemeCommand = new Command<MapScheme>(async s => await ChangeMapSchemeAsync(s));
        ToggleMapObjectsPanelCommand = new Command(() => IsMapObjectsPanelVisible = !IsMapObjectsPanelVisible);
        ToggleMarkersCommand = new Command(async () => await ToggleMarkersAsync());
        ToggleCirclesCommand = new Command(async () => await ToggleCirclesAsync());
        TogglePolylinesCommand = new Command(async () => await TogglePolylinesAsync());
        TogglePolygonsCommand = new Command(async () => await TogglePolygonsAsync());
        ClearMapObjectsCommand = new Command(ClearMapObjects);
        SetDrawingModeCommand = new Command<DrawingMode>(SetDrawingMode);
        FinishDrawingCommand = new Command(async () => await FinishDrawingAsync());
        CancelDrawingCommand = new Command(CancelDrawing);
    }

    public async void InitializeMapView(Here.Explore.Maui.Controls.HereMapView mapView)
    {
        _mapService = mapView.Map;
        System.Diagnostics.Debug.WriteLine($"InitializeMapView: _mapService initialized, isNull={_mapService is null}");

        if (_mapService is null) return;

        // Set default camera position to San Francisco
        try
        {
            await _mapService.SetCameraTargetAsync(DefaultLocation, DefaultZoom);
            System.Diagnostics.Debug.WriteLine($"Map initialized at San Francisco ({DefaultLocation.Latitude}, {DefaultLocation.Longitude}), zoom {DefaultZoom}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to set default location: {ex}");
        }
    }

    private async Task SearchAsync(bool isOrigin)
    {
        var query = isOrigin ? OriginQuery : DestinationQuery;
        System.Diagnostics.Debug.WriteLine($"SearchAsync({isOrigin}): query='{query}'");

        // Cancel previous search
        if (isOrigin)
        {
            _originSearchCts?.Cancel();
            _originSearchCts?.Dispose();
            _originSearchCts = new CancellationTokenSource();
        }
        else
        {
            _destinationSearchCts?.Cancel();
            _destinationSearchCts?.Dispose();
            _destinationSearchCts = new CancellationTokenSource();
        }

        // Require minimum 2 characters
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
        {
            if (isOrigin)
                OriginSuggestions = null;
            else
                DestinationSuggestions = null;
            return;
        }

        var cts = isOrigin ? _originSearchCts : _destinationSearchCts;

        IsLoading = true;
        try
        {
            // Debounce: wait 300ms after last keystroke
            await Task.Delay(300, cts!.Token);

            var result = await _searchService.SuggestAsync(
                new TextQuery(query),
                new SearchOptions { MaxItems = 8 });
            System.Diagnostics.Debug.WriteLine($"SearchAsync: got {result.Suggestions?.Count ?? 0} suggestions");

            if (isOrigin)
                OriginSuggestions = result.Suggestions ?? new List<Suggestion>();
            else
                DestinationSuggestions = result.Suggestions ?? new List<Suggestion>();
        }
        catch (OperationCanceledException)
        {
            System.Diagnostics.Debug.WriteLine($"SearchAsync({isOrigin}): cancelled");
            // Ignored - search was superseded by newer search
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Search error: {ex}");
            if (isOrigin)
                OriginSuggestions = new List<Suggestion>();
            else
                DestinationSuggestions = new List<Suggestion>();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task SelectSuggestionAsync(Suggestion suggestion, bool isOrigin)
    {
        System.Diagnostics.Debug.WriteLine($"SelectSuggestionAsync({isOrigin}): id={suggestion.Id}, title='{suggestion.Title}'");
        if (string.IsNullOrEmpty(suggestion.Id)) return;

        IsLoading = true;
        try
        {
            var place = await _searchService.GetPlaceByIdAsync(suggestion.Id);
            System.Diagnostics.Debug.WriteLine($"SelectSuggestionAsync: place={(place is null ? "null" : $"'{place.Title}' @ {place.Coordinates.Latitude},{place.Coordinates.Longitude}")}");
            if (place is null) return;

            if (isOrigin)
            {
                OriginQuery = place.Title;
                OriginSuggestions = null;
                OriginPlace = place;
            }
            else
            {
                DestinationQuery = place.Title;
                DestinationSuggestions = null;
                DestinationPlace = place;
            }

            // Add marker for selected place
            var marker = new MapMarker(place.Coordinates, Text: place.Title);
            _mapService.AddMapMarker(marker);

            // Center map on selected place
            await _mapService.SetCameraTargetAsync(place.Coordinates, 15);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Select suggestion error: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ClearSearch(bool isOrigin)
    {
        if (isOrigin)
        {
            OriginQuery = string.Empty;
            OriginSuggestions = null;
            OriginPlace = null;
        }
        else
        {
            DestinationQuery = string.Empty;
            DestinationSuggestions = null;
            DestinationPlace = null;
        }
    }

    private void SwapLocations()
    {
        var tempQuery = OriginQuery;
        var tempPlace = OriginPlace;

        OriginQuery = DestinationQuery;
        OriginPlace = DestinationPlace;
        DestinationQuery = tempQuery;
        DestinationPlace = tempPlace;

        OnPropertyChanged(nameof(CanCalculateRoute));
        ((Command)CalculateRouteCommand).ChangeCanExecute();
    }

    private async Task CalculateRouteAsync()
    {
        System.Diagnostics.Debug.WriteLine("CalculateRouteAsync called");
        if (OriginPlace is null || DestinationPlace is null) return;

        IsLoading = true;
        try
        {
            var waypoints = new List<Waypoint>
            {
                new(OriginPlace.Coordinates, WaypointType.Start, Name: OriginPlace.Title),
                new(DestinationPlace.Coordinates, WaypointType.Stop, Name: DestinationPlace.Title)
            };

            var result = await _routingService.CalculateRouteAsync(waypoints, new RoutingOptions());
            System.Diagnostics.Debug.WriteLine($"CalculateRouteAsync: result.Error={result.Error}, RoutesCount={result.Routes?.Count ?? 0}");

            if (result.Error == RoutingError.None && result.Routes?.Count > 0)
            {
                CurrentRoute = result.Routes[0];
                OnPropertyChanged(nameof(HasRoute));
                OnPropertyChanged(nameof(DistanceText));
                OnPropertyChanged(nameof(DurationText));
                OnPropertyChanged(nameof(Maneuvers));
                System.Diagnostics.Debug.WriteLine($"CalculateRouteAsync: route set, {CurrentRoute.Sections.Count} sections, {Maneuvers.Count} maneuvers");

                // Draw route polyline
                var geometry = RouteGeometryHelper.ExtractGeometry(CurrentRoute);
                if (geometry.Count > 0)
                {
                    _routePolyline = RouteGeometryHelper.CreatePolyline(geometry);
                    _mapService.AddMapPolyline(_routePolyline);

                    var midIndex = geometry.Count / 2;
                    await _mapService.SetCameraTargetAsync(geometry[midIndex], 10);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Route calculation failed: {result.Error}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Route calculation error: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ClearRoute()
    {
        if (_routePolyline != null)
        {
            _mapService.RemoveMapPolyline(_routePolyline);
            _routePolyline = null;
        }

        CurrentRoute = null;
        OnPropertyChanged(nameof(HasRoute));
        OnPropertyChanged(nameof(DistanceText));
        OnPropertyChanged(nameof(DurationText));
        OnPropertyChanged(nameof(Maneuvers));
    }

    private async Task ToggleLocationTrackingAsync()
    {
        if (_isTrackingLocation)
        {
            // Stop tracking
            await _locationService.StopListeningAsync();
            _locationService.LocationChanged -= OnLocationChanged;

            if (_locationIndicator != null)
            {
                _mapService.RemoveLocationIndicator();
                _locationIndicator = null;
            }

            IsTrackingLocation = false;
            System.Diagnostics.Debug.WriteLine("Location tracking stopped");
        }
        else
        {
            // Start tracking
            IsLoading = true;
            try
            {
                var location = await _locationService.GetCurrentLocationAsync();
                if (location is null)
                {
                    System.Diagnostics.Debug.WriteLine("Location not available");
                    return;
                }

                // Center map on current location
                _currentZoom = 15;
                await _mapService.SetCameraTargetAsync(location.Coordinates, (int)_currentZoom);

                // Add location indicator (navigation puck)
                _locationIndicator = new LocationIndicator(
                    location.Coordinates,
                    location.BearingInDegrees ?? 0,
                    LocationIndicatorStyle.Navigation,
                    true);
                _mapService.AddLocationIndicator(_locationIndicator);

                // Start continuous listening
                await _locationService.StartListeningAsync();
                _locationService.LocationChanged += OnLocationChanged;

                IsTrackingLocation = true;
                System.Diagnostics.Debug.WriteLine($"Location tracking started at {location.Coordinates.Latitude}, {location.Coordinates.Longitude}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Location tracking error: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    private void OnLocationChanged(object? sender, Models.Location e)
    {
        if (_isTrackingLocation && _locationIndicator != null)
        {
            _mapService.UpdateLocationIndicator(e.Coordinates, e.BearingInDegrees);
            System.Diagnostics.Debug.WriteLine($"Location updated: {e.Coordinates.Latitude}, {e.Coordinates.Longitude}, bearing: {e.BearingInDegrees}");
        }
    }

    private void ZoomBy(int delta)
    {
        _currentZoom = Math.Max(1, Math.Min(20, _currentZoom + delta));
        _ = _mapService.SetCameraTargetAsync(_mapService.GetCameraTargetAsync().Result, (int)_currentZoom);
    }

    private void ResetMapOrientation()
    {
        _ = _mapService.SetCameraTargetAsync(_mapService.GetCameraTargetAsync().Result, (int)_currentZoom);
    }

    private async Task ChangeMapSchemeAsync(MapScheme scheme)
    {
        try
        {
            await _mapService.LoadSceneAsync(scheme);
            SelectedScheme = scheme;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Map scheme change error: {ex}");
        }
    }

    // Map objects
    private async Task ToggleMarkersAsync()
    {
        System.Diagnostics.Debug.WriteLine("ToggleMarkersAsync called");
        try
        {
            if (_markersVisible)
            {
                foreach (var m in _demoMarkers) _mapService.RemoveMapMarker(m);
                _demoMarkers.Clear();
                _markersVisible = false;
                System.Diagnostics.Debug.WriteLine("ToggleMarkersAsync: markers removed");
                return;
            }

            var center = await _mapService.GetCameraTargetAsync();
            var marker = new MapMarker(center, Text: "Demo Marker");
            _mapService.AddMapMarker(marker);
            _demoMarkers.Add(marker);
            _markersVisible = true;
            System.Diagnostics.Debug.WriteLine("ToggleMarkersAsync: marker added");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ToggleMarkersAsync ERROR: {ex}");
        }
    }

    private async Task ToggleCirclesAsync()
    {
        System.Diagnostics.Debug.WriteLine("ToggleCirclesAsync called");
        try
        {
            if (_circlesVisible)
            {
                foreach (var c in _demoCircles) _mapService.RemoveMapCircle(c);
                _demoCircles.Clear();
                _circlesVisible = false;
                System.Diagnostics.Debug.WriteLine("ToggleCirclesAsync: circles removed");
                return;
            }

            var center = await _mapService.GetCameraTargetAsync();
            var circle = new MapCircle(center, 500, FillColor: 0x3300FF00, StrokeColor: 0xFF00FF00);
            _mapService.AddMapCircle(circle);
            _demoCircles.Add(circle);
            _circlesVisible = true;
            System.Diagnostics.Debug.WriteLine("ToggleCirclesAsync: circle added");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ToggleCirclesAsync ERROR: {ex}");
        }
    }

    private async Task TogglePolylinesAsync()
    {
        System.Diagnostics.Debug.WriteLine("TogglePolylinesAsync called");
        try
        {
            if (_polylinesVisible)
            {
                foreach (var p in _demoPolylines) _mapService.RemoveMapPolyline(p);
                _demoPolylines.Clear();
                _polylinesVisible = false;
                System.Diagnostics.Debug.WriteLine("TogglePolylinesAsync: polylines removed");
                return;
            }

            var center = await _mapService.GetCameraTargetAsync();
            var polyline = new MapPolyline(new List<GeoCoordinates>
            {
                new(center.Latitude - 0.01, center.Longitude - 0.01),
                new(center.Latitude + 0.005, center.Longitude),
                new(center.Latitude + 0.01, center.Longitude + 0.01),
            }, Color: 0xFF0000FF, WidthInPixels: 5);
            _mapService.AddMapPolyline(polyline);
            _demoPolylines.Add(polyline);
            _polylinesVisible = true;
            System.Diagnostics.Debug.WriteLine("TogglePolylinesAsync: polyline added");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"TogglePolylinesAsync ERROR: {ex}");
        }
    }

    private async Task TogglePolygonsAsync()
    {
        System.Diagnostics.Debug.WriteLine("TogglePolygonsAsync called");
        try
        {
            if (_polygonsVisible)
            {
                foreach (var p in _demoPolygons) _mapService.RemoveMapPolygon(p);
                _demoPolygons.Clear();
                _polygonsVisible = false;
                System.Diagnostics.Debug.WriteLine("TogglePolygonsAsync: polygons removed");
                return;
            }

            var center = await _mapService.GetCameraTargetAsync();
            var polygon = new MapPolygon(new List<GeoCoordinates>
            {
                new(center.Latitude + 0.005, center.Longitude),
                new(center.Latitude - 0.003, center.Longitude + 0.008),
                new(center.Latitude - 0.003, center.Longitude - 0.008),
            }, FillColor: 0x44FF0000);
            _mapService.AddMapPolygon(polygon);
            _demoPolygons.Add(polygon);
            _polygonsVisible = true;
            System.Diagnostics.Debug.WriteLine("TogglePolygonsAsync: polygon added");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"TogglePolygonsAsync ERROR: {ex}");
        }
    }

    private void ClearMapObjects()
    {
        foreach (var m in _demoMarkers) _mapService.RemoveMapMarker(m);
        foreach (var c in _demoCircles) _mapService.RemoveMapCircle(c);
        foreach (var p in _demoPolylines) _mapService.RemoveMapPolyline(p);
        foreach (var p in _demoPolygons) _mapService.RemoveMapPolygon(p);
        _demoMarkers.Clear();
        _demoCircles.Clear();
        _demoPolylines.Clear();
        _demoPolygons.Clear();
        _markersVisible = false;
        _circlesVisible = false;
        _polylinesVisible = false;
        _polygonsVisible = false;
    }

    // Drawing mode methods
    private void SetDrawingMode(DrawingMode mode)
    {
        if (_currentDrawingMode == mode)
        {
            // Toggle off if same mode selected
            _currentDrawingMode = DrawingMode.None;
        }
        else
        {
            _currentDrawingMode = mode;
            _drawingPoints.Clear();
        }
        OnPropertyChanged(nameof(CurrentDrawingMode));
        System.Diagnostics.Debug.WriteLine($"Drawing mode set to: {_currentDrawingMode}");
    }

    public void OnMapTapped(GeoCoordinates coordinates)
    {
        if (_currentDrawingMode == DrawingMode.None) return;

        _drawingPoints.Add(coordinates);
        System.Diagnostics.Debug.WriteLine($"Drawing point added: {coordinates.Latitude}, {coordinates.Longitude}. Total points: {_drawingPoints.Count}");

        switch (_currentDrawingMode)
        {
            case DrawingMode.Marker:
                _mapService.AddMapMarker(new MapMarker(coordinates, Text: "Marker"));
                _currentDrawingMode = DrawingMode.None;
                _drawingPoints.Clear();
                OnPropertyChanged(nameof(CurrentDrawingMode));
                break;

            case DrawingMode.Polyline:
            case DrawingMode.Polygon:
                UpdatePreviewShape();
                break;
        }
    }

    public void OnMapDoubleTapped(GeoCoordinates coordinates)
    {
        if (_currentDrawingMode is DrawingMode.Polyline or DrawingMode.Polygon)
        {
            FinishDrawingAsync().Wait();
        }
    }

    private void UpdatePreviewShape()
    {
        if (_drawingPoints.Count < 2) return;

        // Remove previous preview
        if (_drawingPreviewPolyline != null)
            _mapService.RemoveMapPolyline(_drawingPreviewPolyline);
        if (_drawingPreviewPolygon != null)
            _mapService.RemoveMapPolygon(_drawingPreviewPolygon);

        if (_currentDrawingMode == DrawingMode.Polyline)
        {
            _drawingPreviewPolyline = new MapPolyline(
                new List<GeoCoordinates>(_drawingPoints),
                Color: 0xFFFF0000,
                WidthInPixels: 3);
            _mapService.AddMapPolyline(_drawingPreviewPolyline);
        }
        else if (_currentDrawingMode == DrawingMode.Polygon)
        {
            _drawingPreviewPolygon = new MapPolygon(
                new List<GeoCoordinates>(_drawingPoints),
                FillColor: 0x44FF0000);
            _mapService.AddMapPolygon(_drawingPreviewPolygon);
        }
    }

    private async Task FinishDrawingAsync()
    {
        if (_drawingPoints.Count < 2)
        {
            CancelDrawing();
            return;
        }

        // Remove preview
        if (_drawingPreviewPolyline != null)
            _mapService.RemoveMapPolyline(_drawingPreviewPolyline);
        if (_drawingPreviewPolygon != null)
            _mapService.RemoveMapPolygon(_drawingPreviewPolygon);

        // Add final shape
        if (_currentDrawingMode == DrawingMode.Polyline)
        {
            var polyline = new MapPolyline(
                new List<GeoCoordinates>(_drawingPoints),
                Color: 0xFFFF0000,
                WidthInPixels: 5);
            _mapService.AddMapPolyline(polyline);
            _demoPolylines.Add(polyline);
        }
        else if (_currentDrawingMode == DrawingMode.Polygon)
        {
            var polygon = new MapPolygon(
                new List<GeoCoordinates>(_drawingPoints),
                FillColor: 0x66FF0000);
            _mapService.AddMapPolygon(polygon);
            _demoPolygons.Add(polygon);
        }

        System.Diagnostics.Debug.WriteLine($"Drawing finished: {_currentDrawingMode} with {_drawingPoints.Count} points");
        _currentDrawingMode = DrawingMode.None;
        _drawingPoints.Clear();
        OnPropertyChanged(nameof(CurrentDrawingMode));
    }

    private void CancelDrawing()
    {
        // Remove preview
        if (_drawingPreviewPolyline != null)
            _mapService.RemoveMapPolyline(_drawingPreviewPolyline);
        if (_drawingPreviewPolygon != null)
            _mapService.RemoveMapPolygon(_drawingPreviewPolygon);

        _currentDrawingMode = DrawingMode.None;
        _drawingPoints.Clear();
        OnPropertyChanged(nameof(CurrentDrawingMode));
        System.Diagnostics.Debug.WriteLine("Drawing cancelled");
    }

    public void Dispose()
    {
        // Services are singletons in DI — don't dispose here
    }
}

/// <summary>Drawing mode for interactive shape creation.</summary>
public enum DrawingMode
{
    None,
    Marker,
    Polyline,
    Polygon,
    Circle
}
