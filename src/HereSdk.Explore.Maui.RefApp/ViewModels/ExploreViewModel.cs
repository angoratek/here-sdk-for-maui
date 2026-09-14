using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;
using Here.Explore.Maui.Models.Search;
using Here.Explore.Maui.RefApp.Services;
using Here.Explore.Maui.Services;

namespace Here.Explore.Maui.RefApp.ViewModels;

public partial class ExploreViewModel : ViewModelBase
{
    private readonly ISearchService _searchService;
    private readonly ILocationService _locationService;
    private readonly IConnectivityService _connectivityService;
    private readonly IPermissionsService _permissionsService;
    private IMapService? _mapService;

    /// <summary>A place search result prepared for the results carousel.</summary>
    public record SearchResultItem(
        Place Place,
        string Title,
        string? DistanceText,
        string CategoryGlyph,
        Color? CategoryColor);

    [ObservableProperty] private string _searchQuery = "";
    [ObservableProperty] private IReadOnlyList<Suggestion> _suggestions = Array.Empty<Suggestion>();
    [ObservableProperty] private bool _hasSuggestions;
    [ObservableProperty] private bool _isSearching;
    [ObservableProperty] private Place? _selectedPlace;
    [ObservableProperty] private string? _placeDistance;
    [ObservableProperty] private double? _selectedPlaceDistanceKm;
    [ObservableProperty] private bool _isPlaceCardVisible;
    [ObservableProperty] private MapScheme _currentScheme = MapScheme.NormalDay;
    [ObservableProperty] private double _currentZoom = 14;
    [ObservableProperty] private bool _isLocationTracking;
    [ObservableProperty] private string? _searchErrorMessage;
    [ObservableProperty] private string? _emptyStateTitle;
    [ObservableProperty] private string? _emptyStateSubtitle;
    [ObservableProperty] private bool _isOffline;
    [ObservableProperty] private IReadOnlyList<SearchResultItem> _searchResults = Array.Empty<SearchResultItem>();
    [ObservableProperty] private bool _hasSearchResults;

    private CancellationTokenSource? _debounceCts;
    private bool _suppressSuggestions;
    private readonly PanelNavigationService? _panelNavigation;
    private readonly List<MapMarker> _placeMarkers = new();
    private MapMarker? _selectedMarker;
    private MapMarker? _longPressMarker;
    private MapMarker? _tapMarker;
    private const int DebounceMs = 300;
    private const int MinQueryLength = 2;

    /// <summary>
    /// Whether this VM's tab panel is the active one over the shared map.
    /// With a single map all VMs receive the same map events; tap handlers
    /// early-return when inactive so e.g. tapping the map on the Tools panel
    /// does not drop a reverse-geocode pin for Explore. Defaults to true so
    /// unit tests that never switch tabs behave as before.
    /// </summary>
    public bool IsActive { get; set; } = true;

    public ExploreViewModel(ISearchService searchService, ILocationService locationService, IConnectivityService connectivityService, IPermissionsService permissionsService, PanelNavigationService? panelNavigation = null)
    {
        _searchService = searchService;
        _locationService = locationService;
        _connectivityService = connectivityService;
        _permissionsService = permissionsService;
        _panelNavigation = panelNavigation;

        _connectivityService.ConnectivityChanged += OnConnectivityChanged;
        IsOffline = !_connectivityService.IsConnected;
    }

    private void OnConnectivityChanged(object? sender, bool connected)
    {
        IsOffline = !connected;
        if (!connected)
            SearchErrorMessage = "You are offline. Search results may be unavailable.";
        else if (SearchErrorMessage is string msg && msg.StartsWith("You are offline"))
            SearchErrorMessage = null;
    }

    public void SetMapService(IMapService mapService)
    {
        _mapService = mapService;

        _mapService.MapTapped += OnMapTapped;
        _mapService.MapLongPressed += OnMapLongPressed;
        _mapService.CameraStateChanged += OnCameraChanged;
    }

    partial void OnSearchQueryChanged(string value)
    {
        // True while SearchQuery is set programmatically (suggestion picked)
        // so the change does not schedule a suggest call for the already-
        // resolved place — its result would re-open the suggestions panel.
        if (_suppressSuggestions) return;
        if (value.Length >= MinQueryLength)
            _ = DebouncedSuggestAsync(value);
        else
        {
            Suggestions = Array.Empty<Suggestion>();
            HasSuggestions = false;
        }
    }

    private async Task DebouncedSuggestAsync(string query)
    {
        _debounceCts?.Cancel();
        _debounceCts = new CancellationTokenSource();
        var token = _debounceCts.Token;

        try
        {
            await Task.Delay(DebounceMs, token);
            if (token.IsCancellationRequested) return;

            IsSearching = true;
            var area = await GetSearchCenter();
            var result = await _searchService.SuggestAsync(
                new TextQuery(query, area),
                new SearchOptions { MaxItems = 6 });

            if (!token.IsCancellationRequested)
            {
                Suggestions = result.Suggestions ?? Array.Empty<Suggestion>();
                HasSuggestions = Suggestions.Count > 0;
            }
        }
        catch (TaskCanceledException) { }
        finally
        {
            if (!token.IsCancellationRequested)
                IsSearching = false;
        }
    }

    [RelayCommand]
    private async Task SelectSuggestion(Suggestion suggestion)
    {
        if (_mapService is null) return;

        // Cancel any pending debounced suggest, then set the query with the
        // suggest trigger suppressed — otherwise the debounce for the
        // selected title re-opens the suggestions panel right after the
        // clear below (same fix as SubmitSearch).
        _debounceCts?.Cancel();
        _suppressSuggestions = true;
        try { SearchQuery = suggestion.Title; }
        finally { _suppressSuggestions = false; }
        Suggestions = Array.Empty<Suggestion>();
        HasSuggestions = false;
        IsSearching = true;

        try
        {
            var place = await _searchService.GetPlaceByIdAsync(suggestion.Id);
            if (place is null) return;

            await ShowPlaceOnMap(place);
        }
        finally
        {
            IsSearching = false;
        }
    }

    /// <summary>
    /// Closes the suggestions dropdown and cancels any in-flight debounced
    /// suggest call — used when the Explore panel is deactivated, so the
    /// dropdown never lingers over the shared map while another panel is
    /// active (same pattern as DirectionsViewModel.DismissSuggestions).
    /// </summary>
    public void DismissSuggestions()
    {
        _debounceCts?.Cancel();
        Suggestions = Array.Empty<Suggestion>();
        HasSuggestions = false;
    }

    [RelayCommand]
    private async Task SubmitSearch()
    {
        if (_mapService is null || string.IsNullOrWhiteSpace(SearchQuery) || SearchQuery.Length < 2) return;

        // Cancel any in-flight debounced suggest — its result must not
        // re-open the suggestions panel after the submit cleared it.
        _debounceCts?.Cancel();

        Suggestions = Array.Empty<Suggestion>();
        HasSuggestions = false;
        IsSearching = true;
        ClearPlaceMarkers();
        SearchResults = Array.Empty<SearchResultItem>();
        HasSearchResults = false;
        SearchErrorMessage = null;
        EmptyStateTitle = null;

        try
        {
            var area = await GetSearchCenter();
            var result = await _searchService.SearchAsync(
                new TextQuery(SearchQuery, area),
                new SearchOptions { MaxItems = 20 });

            if (result.Error != SearchError.None)
            {
                SearchErrorMessage = $"Search failed: {result.Error}";
                return;
            }

            if (result.Places is { Count: > 0 })
            {
                foreach (var place in result.Places)
                {
                    var marker = new MapMarker(place.Coordinates);
                    _mapService.AddMapMarker(marker);
                    _placeMarkers.Add(marker);
                }

                SearchResults = BuildResultItems(result.Places, area);
                HasSearchResults = SearchResults.Count > 0;
                await FitCameraToCoordinates(_placeMarkers.Select(m => m.Coordinates).ToList());
            }
            else
            {
                EmptyStateTitle = "No places found";
                EmptyStateSubtitle = "Try a different search term or area";
            }
        }
        catch (Exception ex)
        {
            SearchErrorMessage = $"Search error: {ex.Message}";
        }
        finally
        {
            IsSearching = false;
        }
    }

    [RelayCommand]
    private async Task SearchCategory(string categoryId)
    {
        if (_mapService is null) return;

        IsSearching = true;
        ClearPlaceMarkers();
        SearchResults = Array.Empty<SearchResultItem>();
        HasSearchResults = false;
        SearchErrorMessage = null;
        EmptyStateTitle = null;

        try
        {
            var area = await GetSearchCenter();
            var result = await _searchService.SearchAsync(
                new CategoryQuery(categoryId, area),
                new SearchOptions { MaxItems = 20 });

            if (result.Error != SearchError.None)
            {
                SearchErrorMessage = $"Category search failed: {result.Error}";
                return;
            }

            if (result.Places is { Count: > 0 })
            {
                foreach (var place in result.Places)
                {
                    var marker = new MapMarker(place.Coordinates);
                    _mapService.AddMapMarker(marker);
                    _placeMarkers.Add(marker);
                }

                SearchResults = BuildResultItems(result.Places, area);
                HasSearchResults = SearchResults.Count > 0;

                if (_placeMarkers.Count > 0)
                {
                    var coords = _placeMarkers.Select(m => m.Coordinates).ToList();
                    await FitCameraToCoordinates(coords);
                }
                EmptyStateTitle = null;
            }
            else
            {
                EmptyStateTitle = "No places found";
                EmptyStateSubtitle = $"No results for category \"{categoryId}\" in this area";
            }
        }
        catch (Exception ex)
        {
            SearchErrorMessage = $"Search error: {ex.Message}";
        }
        finally
        {
            IsSearching = false;
        }
    }

    private async Task ShowPlaceOnMap(Place place)
    {
        if (_mapService is null) return;

        if (_selectedMarker is not null)
            _mapService.RemoveMapMarker(_selectedMarker);

        ClearPlaceMarkers();

        _selectedMarker = new MapMarker(place.Coordinates);
        _mapService.AddMapMarker(_selectedMarker);

        SelectedPlace = place;

        // Calculate distance from map center
        try
        {
            var center = await _mapService.GetCameraTargetAsync();
            var distKm = ComputeDistanceKm(center, place.Coordinates);
            SelectedPlaceDistanceKm = distKm;
            PlaceDistance = distKm < 1 ? $"{distKm * 1000:F0}m away" : $"{distKm:F1}km away";
        }
        catch { PlaceDistance = null; }

        IsPlaceCardVisible = true;
        await _mapService.SetCameraTargetAsync(place.Coordinates, 15);
    }

    /// <summary>
    /// Shows a place from the results carousel on the map and opens its
    /// place card. Distances are recomputed for the tapped place.
    /// </summary>
    [RelayCommand]
    private async Task SelectResult(SearchResultItem item)
    {
        if (_mapService is null) return;
        await ShowPlaceOnMap(item.Place);
    }

    private IReadOnlyList<SearchResultItem> BuildResultItems(IReadOnlyList<Place> places, GeoCoordinates center)
    {
        var items = new List<SearchResultItem>();
        foreach (var place in places)
        {
            var cat = place.PrimaryCategory ?? place.Categories?.FirstOrDefault();
            var catId = cat?.Id ?? cat?.Name ?? "";
            string? distance = null;
            try
            {
                var distKm = ComputeDistanceKm(center, place.Coordinates);
                distance = distKm < 1 ? $"{distKm * 1000:F0}m" : $"{distKm:F1}km";
            }
            catch { /* distance is decorative */ }
            items.Add(new SearchResultItem(
                place,
                place.Title,
                distance,
                Services.CategoryVisuals.GlyphFor(catId),
                Services.CategoryVisuals.ColorFor(catId)));
        }
        return items;
    }

    [RelayCommand]
    private async Task ZoomIn()
    {
        if (_mapService is null) return;
        var target = await _mapService.GetCameraTargetAsync();
        await _mapService.AnimateCameraAsync(new CameraAnimation(target, ZoomLevel: CurrentZoom + 1, DurationInSeconds: 0.3));
    }

    [RelayCommand]
    private async Task ZoomOut()
    {
        if (_mapService is null) return;
        var target = await _mapService.GetCameraTargetAsync();
        await _mapService.AnimateCameraAsync(new CameraAnimation(target, ZoomLevel: CurrentZoom - 1, DurationInSeconds: 0.3));
    }

    [RelayCommand]
    private async Task CenterOnLocation()
    {
        if (_mapService is null) return;
        IsLocationTracking = true;

        try
        {
            var allowed = await _permissionsService.RequestLocationPermissionAsync();
            if (!allowed)
            {
                SearchErrorMessage = "Location permission denied. Enable in Settings to use this feature.";
                IsLocationTracking = false;
                return;
            }

            var loc = await _locationService.GetCurrentLocationAsync();
            if (loc is not null)
            {
                await _mapService.SetCameraTargetAsync(loc.Coordinates, 16);
                _mapService.AddLocationIndicator(new LocationIndicator(loc.Coordinates, Bearing: loc.BearingInDegrees ?? 0));
            }
            else
            {
                SearchErrorMessage = "Could not determine your location. Check GPS signal.";
            }
        }
        catch (Exception ex)
        {
            SearchErrorMessage = $"Location error: {ex.Message}";
            IsLocationTracking = false;
        }
    }

    [RelayCommand]
    private async Task ChangeScheme(string schemeName)
    {
        if (_mapService is null) return;
        var scheme = schemeName switch
        {
            "HybridDay" => MapScheme.HybridDay,
            "SatelliteDay" => MapScheme.SatelliteDay,
            "NormalNight" => MapScheme.NormalNight,
            "TerrainDay" => MapScheme.TerrainDay,
            _ => MapScheme.NormalDay
        };
        CurrentScheme = scheme;
        await _mapService.LoadSceneAsync(scheme);
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchQuery = "";
        Suggestions = Array.Empty<Suggestion>();
        HasSuggestions = false;
        SearchResults = Array.Empty<SearchResultItem>();
        HasSearchResults = false;
        SelectedPlace = null;
        IsPlaceCardVisible = false;
        SearchErrorMessage = null;
        EmptyStateTitle = null;
        ClearPlaceMarkers();
        if (_selectedMarker is not null && _mapService is not null)
        {
            _mapService.RemoveMapMarker(_selectedMarker);
            _selectedMarker = null;
        }
        if (_tapMarker is not null && _mapService is not null)
        {
            _mapService.RemoveMapMarker(_tapMarker);
            _tapMarker = null;
        }
    }

    [RelayCommand]
    private void NavigateToDirections()
    {
        if (SelectedPlace is null) return;

        // One shared map, no Shell routes: ask the home page to activate the
        // Directions panel and pass the place along.
        if (_panelNavigation is not null)
            _panelNavigation.RequestDirections(SelectedPlace);
    }

    private async void OnMapTapped(object? sender, MapTappedEventArgs e)
    {
        if (!IsActive) return;

        // Tap while the place card is showing: dismiss it
        if (IsPlaceCardVisible)
        {
            ClearSearchCommand.Execute(null);
            return;
        }

        // Tap on empty map: drop a pin and reverse-geocode the address
        if (_mapService is null) return;

        if (_tapMarker is not null)
            _mapService.RemoveMapMarker(_tapMarker);
        _tapMarker = new MapMarker(e.Coordinates);
        _mapService.AddMapMarker(_tapMarker);

        try
        {
            var result = await _searchService.SearchAsync(
                e.Coordinates,
                new SearchOptions { MaxItems = 1 });

            if (result.Places is { Count: > 0 })
                await ShowPlaceOnMap(result.Places[0]);
            else
                System.Diagnostics.Debug.WriteLine("[REFAPP_DIAG] reverse geocode returned no places");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[REFAPP_DIAG] reverse geocode failed: {ex.Message}");
        }
    }

    private async void OnMapLongPressed(object? sender, MapLongPressedEventArgs e)
    {
        if (!IsActive || _mapService is null) return;

        if (_longPressMarker is not null)
            _mapService.RemoveMapMarker(_longPressMarker);

        _longPressMarker = new MapMarker(e.Coordinates);
        _mapService.AddMapMarker(_longPressMarker);

        // Try to find nearby places
        try
        {
            var result = await _searchService.SearchAsync(
                new TextQuery("", e.Coordinates),
                new SearchOptions { MaxItems = 1 });

            if (result.Places is { Count: > 0 })
                await ShowPlaceOnMap(result.Places[0]);
        }
        catch { /* ignore */ }
    }

    private void OnCameraChanged(object? sender, CameraStateChangedEventArgs e)
    {
        CurrentZoom = e.ZoomLevel;
    }

    private async Task<GeoCoordinates> GetSearchCenter()
    {
        if (_mapService is not null)
            return await _mapService.GetCameraTargetAsync();
        return new GeoCoordinates(37.7749, -122.4194);
    }

    private void ClearPlaceMarkers()
    {
        if (_mapService is null) return;
        foreach (var m in _placeMarkers)
            _mapService.RemoveMapMarker(m);
        _placeMarkers.Clear();
    }

    private async Task FitCameraToCoordinates(List<GeoCoordinates> coordinates)
    {
        if (_mapService is null || coordinates.Count == 0) return;

        var minLat = coordinates.Min(c => c.Latitude);
        var maxLat = coordinates.Max(c => c.Latitude);
        var minLng = coordinates.Min(c => c.Longitude);
        var maxLng = coordinates.Max(c => c.Longitude);
        var center = new GeoCoordinates((minLat + maxLat) / 2, (minLng + maxLng) / 2);

        var latDiff = maxLat - minLat;
        var lngDiff = maxLng - minLng;
        var maxDiff = Math.Max(latDiff, lngDiff);
        var zoom = maxDiff switch
        {
            > 1.0 => 10,
            > 0.5 => 11,
            > 0.1 => 12,
            > 0.05 => 13,
            > 0.01 => 14,
            _ => 15
        };

        await _mapService.SetCameraTargetAsync(center, zoom);
    }

    private static double ComputeDistanceKm(GeoCoordinates a, GeoCoordinates b)
    {
        const double r = 6371.0;
        var dLat = ToRad(b.Latitude - a.Latitude);
        var dLon = ToRad(b.Longitude - a.Longitude);
        var lat1 = ToRad(a.Latitude);
        var lat2 = ToRad(b.Latitude);

        var aa = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
               Math.Cos(lat1) * Math.Cos(lat2) *
               Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(aa), Math.Sqrt(1 - aa));
        return r * c;
    }

    private static double ToRad(double deg) => deg * Math.PI / 180.0;
}
