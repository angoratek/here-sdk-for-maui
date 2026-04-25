using System.Collections.ObjectModel;
using System.Windows.Input;
using Here.Explore.Maui.Helpers;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;
using Here.Explore.Maui.Models.Routing;
using Here.Explore.Maui.Models.Search;
using Here.Explore.Maui.Services;

namespace Here.Explore.Maui.RefApp.ViewModels;

/// <summary>
/// Unified ViewModel for the modern main page with search, routing, and map functionality.
/// </summary>
public class ModernMainViewModel : ViewModelBase, IDisposable
{
    private readonly IMapService _mapService;
    private readonly ISearchService _searchService;
    private readonly IRoutingService _routingService;

    // Search state
    private string _searchQuery = string.Empty;
    private IReadOnlyList<Suggestion>? _suggestions;
    private Place? _selectedPlace;

    // Route state
    private readonly ObservableCollection<Waypoint> _waypoints = new();
    private Route? _currentRoute;
    private MapPolyline? _routePolyline;

    // Map state
    private GeoCoordinates _mapCenter = new(52.531268, 13.387659); // Berlin
    private MapScheme _selectedScheme = MapScheme.NormalDay;

    // Properties
    public string SearchQuery
    {
        get => _searchQuery;
        set => SetProperty(ref _searchQuery, value);
    }

    public IReadOnlyList<Suggestion>? Suggestions
    {
        get => _suggestions;
        private set => SetProperty(ref _suggestions, value);
    }

    public Place? SelectedPlace
    {
        get => _selectedPlace;
        private set => SetProperty(ref _selectedPlace, value);
    }

    public ObservableCollection<Waypoint> Waypoints => _waypoints;

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

    public GeoCoordinates MapCenter
    {
        get => _mapCenter;
        set => SetProperty(ref _mapCenter, value);
    }

    // Computed properties
    public bool HasResults => Suggestions?.Count > 0;

    public bool CanCalculateRoute => _waypoints.Count >= 2;

    public string DistanceText => CurrentRoute != null ? $"{CurrentRoute.LengthInMeters / 1000.0:F1} km" : "--";

    public string DurationText => CurrentRoute != null ? $"{CurrentRoute.DurationInSeconds / 60:F0} min" : "--";

    public IReadOnlyList<Maneuver> Maneuvers =>
        CurrentRoute?.Sections.SelectMany(s => s.Maneuvers).ToList() ?? new List<Maneuver>();

    // Commands
    public ICommand SearchCommand { get; }
    public ICommand SelectSuggestionCommand { get; }
    public ICommand SetAsStartCommand { get; }
    public ICommand SetAsDestinationCommand { get; }
    public ICommand AddViaPointCommand { get; }
    public ICommand CalculateRouteCommand { get; }
    public ICommand ClearRouteCommand { get; }
    public ICommand ChangeMapSchemeCommand { get; }
    public ICommand ClearSearchCommand { get; }

    public ModernMainViewModel(IMapService mapService, ISearchService searchService, IRoutingService routingService)
    {
        _mapService = mapService;
        _searchService = searchService;
        _routingService = routingService;

        SearchCommand = new Command(async () => await SearchAsync());
        SelectSuggestionCommand = new Command<Place>(async p => await SelectPlaceAsync(p));
        SetAsStartCommand = new Command<Place>(p => SetWaypoint(0, p, WaypointType.Start));
        SetAsDestinationCommand = new Command<Place>(p => SetWaypoint(1, p, WaypointType.Stop));
        AddViaPointCommand = new Command<Place>(p => InsertWaypoint(p));
        CalculateRouteCommand = new Command(async () => await CalculateRouteAsync(), () => CanCalculateRoute);
        ClearRouteCommand = new Command(ClearRoute);
        ChangeMapSchemeCommand = new Command<MapScheme>(async s => await ChangeMapSchemeAsync(s));
        ClearSearchCommand = new Command(ClearSearch);
    }

    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
            return;

        try
        {
            var result = await _searchService.SuggestAsync(
                new TextQuery(SearchQuery),
                new SearchOptions { MaxItems = 10 });

            Suggestions = result.Suggestions ?? new List<Suggestion>();
            OnPropertyChanged(nameof(HasResults));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Search error: {ex}");
            Suggestions = new List<Suggestion>();
        }
    }

    private async Task SelectPlaceAsync(Place place)
    {
        SelectedPlace = place;

        // Add marker for selected place
        var marker = new MapMarker(place.Coordinates, Text: place.Title);
        _mapService.AddMapMarker(marker);

        // Center map on selected place
        await _mapService.SetCameraTargetAsync(place.Coordinates, 15);
    }

    private void SetWaypoint(int index, Place place, WaypointType type)
    {
        var waypoint = new Waypoint(place.Coordinates, type, Name: place.Title);

        while (_waypoints.Count <= index)
            _waypoints.Add(new Waypoint(new GeoCoordinates(0, 0), WaypointType.Through));

        _waypoints[index] = waypoint;
        ((Command)CalculateRouteCommand).ChangeCanExecute();
    }

    private void InsertWaypoint(Place place)
    {
        // Insert before the last waypoint (destination)
        var insertIndex = Math.Max(0, _waypoints.Count - 1);
        _waypoints.Insert(insertIndex, new Waypoint(place.Coordinates, WaypointType.Through, Name: place.Title));
        ((Command)CalculateRouteCommand).ChangeCanExecute();
    }

    private async Task CalculateRouteAsync()
    {
        if (_waypoints.Count < 2)
            return;

        try
        {
            var result = await _routingService.CalculateRouteAsync(_waypoints.ToList(), new RoutingOptions());

            if (result.Error == RoutingError.None && result.Routes?.Count > 0)
            {
                CurrentRoute = result.Routes[0];

                // Extract geometry and draw polyline
                var geometry = RouteGeometryHelper.ExtractGeometry(CurrentRoute);
                if (geometry.Count > 0)
                {
                    _routePolyline = RouteGeometryHelper.CreatePolyline(geometry);
                    _mapService.AddMapPolyline(_routePolyline);

                    // Center map on route midpoint
                    var midIndex = geometry.Count / 2;
                    await _mapService.SetCameraTargetAsync(geometry[midIndex], 10);
                }

                ((Command)ClearRouteCommand).ChangeCanExecute();
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
    }

    private void ClearRoute()
    {
        if (_routePolyline != null)
        {
            _mapService.RemoveMapPolyline(_routePolyline);
            _routePolyline = null;
        }

        CurrentRoute = null;
        ((Command)ClearRouteCommand).ChangeCanExecute();
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

    private void ClearSearch()
    {
        SearchQuery = string.Empty;
        Suggestions = new List<Suggestion>();
        OnPropertyChanged(nameof(HasResults));
    }

    public void Dispose()
    {
        // Services are registered as singletons in DI - don't dispose here
    }
}
