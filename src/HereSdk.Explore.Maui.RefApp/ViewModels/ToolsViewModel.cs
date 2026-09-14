using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Here.Explore.Maui.Helpers;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;
using Here.Explore.Maui.Services;
using Here.Explore.Maui.RefApp.Services;

namespace Here.Explore.Maui.RefApp.ViewModels;

public partial class ToolsViewModel : ViewModelBase
{
    private readonly IThemeService _themeService;
    private IMapService? _mapService;

    // --- Drawing state ---
    [ObservableProperty] private DrawingTool _currentDrawingTool = DrawingTool.None;
    [ObservableProperty] private string _drawingHint = "";
    [ObservableProperty] private int _drawingPointCount;

    public bool IsDrawingActive => CurrentDrawingTool != DrawingTool.None;

    private readonly List<GeoCoordinates> _drawingPoints = new();
    private MapPolyline? _drawingPreviewPolyline;
    private MapPolygon? _drawingPreviewPolygon;
    private MapCircle? _drawingPreviewCircle;
    private MapMarker? _drawingCenterMarker;

    // --- Map objects ---
    private readonly List<MapMarker> _userMarkers = new();
    private readonly List<MapPolyline> _userPolylines = new();
    private readonly List<MapPolygon> _userPolygons = new();
    private readonly List<MapCircle> _userCircles = new();

    // --- Demo gallery ---
    public ObservableCollection<DemoPreset> DemoPresets { get; } = new();

    // --- Settings ---
    [ObservableProperty] private bool _isDarkMode;
    [ObservableProperty] private string _selectedSchemeName = "NormalDay";
    [ObservableProperty] private string _sdkVersion = "";
    [ObservableProperty] private int _totalObjectCount;

    // --- Section state ---
    [ObservableProperty] private bool _isDrawingExpanded = true;
    [ObservableProperty] private bool _isGalleryExpanded;
    [ObservableProperty] private bool _isSettingsExpanded;

    /// <summary>
    /// Whether this VM's tab panel is the active one over the shared map.
    /// All panel VMs receive the same map events from the single map; the
    /// drawing tap handlers early-return when inactive so tapping the map on
    /// another panel does not add drawing points. Defaults to true so unit
    /// tests that never switch tabs behave as before.
    /// </summary>
    public bool IsActive { get; set; } = true;

    public ToolsViewModel(IThemeService themeService)
    {
        _themeService = themeService;
        IsDarkMode = _themeService.IsDarkMode;

        DemoPresets.Add(new DemoPreset("SF Landmarks", "10 iconic San Francisco locations as markers",
            () => _ = AddSfLandmarksAsync()));
        DemoPresets.Add(new DemoPreset("Route Network", "Multiple routes across downtown SF",
            () => _ = AddRouteNetworkAsync()));
        DemoPresets.Add(new DemoPreset("District Boundaries", "Neighborhood polygons with fill",
            () => _ = AddDistrictsAsync()));
        DemoPresets.Add(new DemoPreset("Concentric Circles", "Overlapping circles radiating outward",
            () => _ = AddConcentricCirclesAsync()));
        DemoPresets.Add(new DemoPreset("Marker Cluster", "Dense markers to demo clustering",
            () => _ = AddMarkerClusterAsync()));
    }

    public void SetMapService(IMapService mapService)
    {
        _mapService = mapService;

        _mapService.MapTapped += OnMapTapped;
        _mapService.MapDoubleTapped += OnMapDoubleTapped;

        SdkVersion = Here.Explore.Maui.SdkInfo.Version;
    }

    // =========================================================================
    // Section Toggles
    // =========================================================================

    [RelayCommand]
    private void ToggleDrawingExpanded() => IsDrawingExpanded = !IsDrawingExpanded;

    [RelayCommand]
    private void ToggleGalleryExpanded() => IsGalleryExpanded = !IsGalleryExpanded;

    [RelayCommand]
    private void ToggleSettingsExpanded() => IsSettingsExpanded = !IsSettingsExpanded;

    partial void OnCurrentDrawingToolChanged(DrawingTool value)
    {
        OnPropertyChanged(nameof(IsDrawingActive));
    }

    partial void OnIsDarkModeChanged(bool value)
    {
        _themeService.SetDarkMode(value);
    }

    // =========================================================================
    // Drawing Commands
    // =========================================================================

    [RelayCommand]
    private void SetDrawingTool(string toolName)
    {
        if (Enum.TryParse<DrawingTool>(toolName, out var tool))
        {
            if (CurrentDrawingTool == tool)
            {
                CancelDrawing();
                return;
            }

            CancelDrawing();
            CurrentDrawingTool = tool;
            _drawingPoints.Clear();
            DrawingPointCount = 0;

            DrawingHint = tool switch
            {
                DrawingTool.Marker => "Tap anywhere to place a marker",
                DrawingTool.Polyline => "Tap to add vertices. Double-tap to finish",
                DrawingTool.Polygon => "Tap to add vertices. Double-tap to finish",
                DrawingTool.Circle => "Tap to set center, then tap again for radius",
                _ => ""
            };
        }
    }

    [RelayCommand]
    private void FinishDrawing()
    {
        var minPoints = CurrentDrawingTool == DrawingTool.Polygon ? 3 : 2;

        if (CurrentDrawingTool == DrawingTool.Marker && _drawingPoints.Count >= 1)
        {
            var marker = new MapMarker(_drawingPoints[0]);
            _mapService?.AddMapMarker(marker);
            _userMarkers.Add(marker);
        }
        else if (CurrentDrawingTool == DrawingTool.Polyline && _drawingPoints.Count >= minPoints)
        {
            ClearPreview();
            var polyline = new MapPolyline(new List<GeoCoordinates>(_drawingPoints), Color: 0xFF007AFF, WidthInPixels: 4);
            _mapService?.AddMapPolyline(polyline);
            _userPolylines.Add(polyline);
        }
        else if (CurrentDrawingTool == DrawingTool.Polygon && _drawingPoints.Count >= minPoints)
        {
            ClearPreview();
            var polygon = new MapPolygon(new List<GeoCoordinates>(_drawingPoints), FillColor: 0x44007AFF);
            _mapService?.AddMapPolygon(polygon);
            _userPolygons.Add(polygon);
        }
        else if (CurrentDrawingTool == DrawingTool.Circle && _drawingPoints.Count >= 2)
        {
            ClearPreview();
            var center = _drawingPoints[0];
            var edge = _drawingPoints[1];
            var radius = ComputeDistanceMeters(center, edge);
            var circle = new MapCircle(center, radius, FillColor: 0x33007AFF, StrokeColor: 0xFF007AFF);
            _mapService?.AddMapCircle(circle);
            _userCircles.Add(circle);
        }

        ResetDrawingState();
    }

    [RelayCommand]
    private void CancelDrawing()
    {
        ClearPreview();
        ResetDrawingState();
    }

    /// <summary>
    /// Invoked by <see cref="ResetMap"/> in addition to ClearAll so the
    /// other overlays on the shared map (route, traffic, search) reset too.
    /// Wired by MapHomePage.
    /// </summary>
    public Action? ResetExtras { get; set; }

    [RelayCommand]
    private void ResetMap()
    {
        ClearAll();
        ResetExtras?.Invoke();
    }

    [RelayCommand]
    private void ClearAll()
    {
        CancelDrawing();

        if (_mapService is null) return;

        foreach (var m in _userMarkers) _mapService.RemoveMapMarker(m);
        foreach (var p in _userPolylines) _mapService.RemoveMapPolyline(p);
        foreach (var p in _userPolygons) _mapService.RemoveMapPolygon(p);
        foreach (var c in _userCircles) _mapService.RemoveMapCircle(c);

        _userMarkers.Clear();
        _userPolylines.Clear();
        _userPolygons.Clear();
        _userCircles.Clear();

        UpdateObjectCount();
    }

    // =========================================================================
    // Settings
    // =========================================================================

    [RelayCommand]
    private void ToggleDarkMode()
    {
        IsDarkMode = !IsDarkMode;
    }

    [RelayCommand]
    private async Task ChangeScheme(string schemeName)
    {
        if (_mapService is null) return;

        var scheme = schemeName switch
        {
            "NormalDay" => MapScheme.NormalDay,
            "NormalNight" => MapScheme.NormalNight,
            "HybridDay" => MapScheme.HybridDay,
            "SatelliteDay" => MapScheme.SatelliteDay,
            "TerrainDay" => MapScheme.TerrainDay,
            _ => MapScheme.NormalDay
        };

        try
        {
            await _mapService.LoadSceneAsync(scheme);
            SelectedSchemeName = schemeName;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[REFAPP_DIAG] Scheme change error: {ex}");
        }
    }

    // =========================================================================
    // Map Event Handlers
    // =========================================================================

    private void OnMapTapped(object? sender, MapTappedEventArgs e)
    {
        if (!IsActive || CurrentDrawingTool == DrawingTool.None) return;

        var coordinates = e.Coordinates;
        _drawingPoints.Add(coordinates);
        DrawingPointCount = _drawingPoints.Count;

        System.Diagnostics.Debug.WriteLine(
            $"[REFAPP_DIAG] Drawing point: {coordinates.Latitude:F6},{coordinates.Longitude:F6} count={_drawingPoints.Count}");

        switch (CurrentDrawingTool)
        {
            case DrawingTool.Marker:
                // Immediately finish — single tap places the marker
                FinishDrawing();
                break;

            case DrawingTool.Polyline:
            case DrawingTool.Polygon:
                if (_drawingPoints.Count >= 2)
                {
                    DrawingHint = $"Tap to add more. {_drawingPoints.Count} points. Double-tap to finish";
                }
                UpdatePreviewShape();
                break;

            case DrawingTool.Circle:
                if (_drawingPoints.Count == 1)
                {
                    // Show center marker
                    _drawingCenterMarker = new MapMarker(coordinates, Text: "Center");
                    _mapService?.AddMapMarker(_drawingCenterMarker);
                    DrawingHint = "Now tap to set the circle radius";
                }
                else if (_drawingPoints.Count >= 2)
                {
                    UpdateCirclePreview();
                    DrawingHint = "Tap to adjust radius, or double-tap to finish";
                }
                break;
        }
    }

    private void OnMapDoubleTapped(object? sender, MapTappedEventArgs e)
    {
        if (!IsActive) return;

        if (CurrentDrawingTool is DrawingTool.Polyline or DrawingTool.Polygon or DrawingTool.Circle
            && _drawingPoints.Count >= 2)
        {
            FinishDrawing();
        }
    }

    private void UpdatePreviewShape()
    {
        ClearPreview();

        if (CurrentDrawingTool == DrawingTool.Polyline && _drawingPoints.Count >= 2)
        {
            _drawingPreviewPolyline = new MapPolyline(
                new List<GeoCoordinates>(_drawingPoints),
                Color: 0x88007AFF, WidthInPixels: 3);
            _mapService?.AddMapPolyline(_drawingPreviewPolyline);
        }
        else if (CurrentDrawingTool == DrawingTool.Polygon && _drawingPoints.Count >= 3)
        {
            _drawingPreviewPolygon = new MapPolygon(
                new List<GeoCoordinates>(_drawingPoints),
                FillColor: 0x22007AFF);
            _mapService?.AddMapPolygon(_drawingPreviewPolygon);
        }
    }

    private void UpdateCirclePreview()
    {
        ClearPreview();

        if (_drawingPoints.Count >= 2)
        {
            var center = _drawingPoints[0];
            var edge = _drawingPoints[1];
            var radius = ComputeDistanceMeters(center, edge);
            _drawingPreviewCircle = new MapCircle(center, radius, FillColor: 0x22007AFF, StrokeColor: 0x88007AFF);
            _mapService?.AddMapCircle(_drawingPreviewCircle);
        }
    }

    private void ClearPreview()
    {
        if (_mapService is null) return;

        if (_drawingPreviewPolyline is not null)
        {
            _mapService.RemoveMapPolyline(_drawingPreviewPolyline);
            _drawingPreviewPolyline = null;
        }
        if (_drawingPreviewPolygon is not null)
        {
            _mapService.RemoveMapPolygon(_drawingPreviewPolygon);
            _drawingPreviewPolygon = null;
        }
        if (_drawingPreviewCircle is not null)
        {
            _mapService.RemoveMapCircle(_drawingPreviewCircle);
            _drawingPreviewCircle = null;
        }
        if (_drawingCenterMarker is not null)
        {
            _mapService.RemoveMapMarker(_drawingCenterMarker);
            _drawingCenterMarker = null;
        }
    }

    private void ResetDrawingState()
    {
        CurrentDrawingTool = DrawingTool.None;
        _drawingPoints.Clear();
        DrawingPointCount = 0;
        DrawingHint = "";
        UpdateObjectCount();
    }

    private void UpdateObjectCount()
    {
        TotalObjectCount = _userMarkers.Count + _userPolylines.Count + _userPolygons.Count + _userCircles.Count;
    }

    // =========================================================================
    // Demo Gallery Presets
    // =========================================================================

    private async Task AddSfLandmarksAsync()
    {
        if (_mapService is null) return;

        var landmarks = new (string Name, double Lat, double Lon)[]
        {
            ("Golden Gate Bridge", 37.8199, -122.4783),
            ("Pier 39", 37.8087, -122.4098),
            ("Coit Tower", 37.8024, -122.4058),
            ("Palace of Fine Arts", 37.8029, -122.4488),
            ("Ferry Building", 37.7956, -122.3936),
            ("Oracle Park", 37.7786, -122.3896),
            ("City Hall", 37.7793, -122.4193),
            ("Chinatown Gate", 37.7905, -122.4062),
            ("Lombard Street", 37.8020, -122.4196),
            ("Painted Ladies", 37.7763, -122.4329),
        };

        foreach (var (name, lat, lon) in landmarks)
        {
            var marker = new MapMarker(new GeoCoordinates(lat, lon), Text: name);
            _mapService.AddMapMarker(marker);
            _userMarkers.Add(marker);
        }

        // Fit camera to show all markers
        await _mapService.SetCameraTargetAsync(new GeoCoordinates(37.79, -122.42), 12);
        UpdateObjectCount();
    }

    private async Task AddRouteNetworkAsync()
    {
        if (_mapService is null) return;

        var hub = new GeoCoordinates(37.787, -122.408);

        var routes = new[]
        {
            new[] { hub, new GeoCoordinates(37.795, -122.394), new GeoCoordinates(37.798, -122.390) },
            new[] { hub, new GeoCoordinates(37.782, -122.415), new GeoCoordinates(37.778, -122.418) },
            new[] { hub, new GeoCoordinates(37.791, -122.402), new GeoCoordinates(37.793, -122.397) },
            new[] { hub, new GeoCoordinates(37.784, -122.411), new GeoCoordinates(37.780, -122.413) },
        };

        var colors = new uint[] { 0xFF007AFF, 0xFFFF9500, 0xFF34C759, 0xFFFF3B30 };

        for (int i = 0; i < routes.Length; i++)
        {
            var polyline = new MapPolyline(new List<GeoCoordinates>(routes[i]),
                Color: colors[i], WidthInPixels: 4);
            _mapService.AddMapPolyline(polyline);
            _userPolylines.Add(polyline);
        }

        var hubMarker = new MapMarker(hub, Text: "Hub");
        _mapService.AddMapMarker(hubMarker);
        _userMarkers.Add(hubMarker);

        await _mapService.SetCameraTargetAsync(hub, 13);
        UpdateObjectCount();
    }

    private async Task AddDistrictsAsync()
    {
        if (_mapService is null) return;

        var financialDistrict = new[]
        {
            new GeoCoordinates(37.795, -122.403),
            new GeoCoordinates(37.795, -122.397),
            new GeoCoordinates(37.791, -122.394),
            new GeoCoordinates(37.788, -122.399),
            new GeoCoordinates(37.789, -122.403),
        };

        var soma = new[]
        {
            new GeoCoordinates(37.789, -122.403),
            new GeoCoordinates(37.788, -122.399),
            new GeoCoordinates(37.784, -122.395),
            new GeoCoordinates(37.779, -122.395),
            new GeoCoordinates(37.778, -122.403),
            new GeoCoordinates(37.782, -122.408),
        };

        var dist1 = new MapPolygon(new List<GeoCoordinates>(financialDistrict), FillColor: 0x33007AFF);
        var dist2 = new MapPolygon(new List<GeoCoordinates>(soma), FillColor: 0x33FF9500);

        _mapService.AddMapPolygon(dist1);
        _mapService.AddMapPolygon(dist2);
        _userPolygons.Add(dist1);
        _userPolygons.Add(dist2);

        await _mapService.SetCameraTargetAsync(new GeoCoordinates(37.787, -122.400), 14);
        UpdateObjectCount();
    }

    private async Task AddConcentricCirclesAsync()
    {
        if (_mapService is null) return;

        var center = new GeoCoordinates(37.787, -122.408);
        double[] radii = { 300, 600, 1000, 1600, 2500 };
        uint[] colors = { 0x33007AFF, 0x335850FF, 0x33FF3B30, 0x33FF9500, 0x3334C759 };

        for (int i = 0; i < radii.Length; i++)
        {
            var circle = new MapCircle(center, radii[i], FillColor: colors[i], StrokeColor: colors[i] | 0xFF000000);
            _mapService.AddMapCircle(circle);
            _userCircles.Add(circle);
        }

        await _mapService.SetCameraTargetAsync(center, 14);
        UpdateObjectCount();
    }

    private async Task AddMarkerClusterAsync()
    {
        if (_mapService is null) return;

        var clusterCenter = new GeoCoordinates(37.787, -122.408);
        var rng = new Random(42);

        for (int i = 0; i < 30; i++)
        {
            var lat = clusterCenter.Latitude + (rng.NextDouble() - 0.5) * 0.03;
            var lon = clusterCenter.Longitude + (rng.NextDouble() - 0.5) * 0.03;
            var marker = new MapMarker(new GeoCoordinates(Math.Round(lat, 6), Math.Round(lon, 6)));
            _mapService.AddMapMarker(marker);
            _userMarkers.Add(marker);
        }

        await _mapService.SetCameraTargetAsync(clusterCenter, 13);
        UpdateObjectCount();
    }

    // =========================================================================
    // Helpers
    // =========================================================================

    private static double ComputeDistanceMeters(GeoCoordinates a, GeoCoordinates b)
    {
        const double r = 6371000;
        var dLat = (b.Latitude - a.Latitude) * Math.PI / 180;
        var dLon = (b.Longitude - a.Longitude) * Math.PI / 180;
        var lat1 = a.Latitude * Math.PI / 180;
        var lat2 = b.Latitude * Math.PI / 180;

        var x = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
        return r * 2 * Math.Atan2(Math.Sqrt(x), Math.Sqrt(1 - x));
    }

    public void Dispose()
    {
        if (_mapService is not null)
        {
            _mapService.MapTapped -= OnMapTapped;
            _mapService.MapDoubleTapped -= OnMapDoubleTapped;
        }
    }
}

// =========================================================================
// Supporting Types
// =========================================================================

public enum DrawingTool
{
    None,
    Marker,
    Polyline,
    Polygon,
    Circle
}

public class DemoPreset
{
    public string Name { get; }
    public string Description { get; }
    public IRelayCommand ActivateCommand { get; }

    public DemoPreset(string name, string description, Action activate)
    {
        Name = name;
        Description = description;
        ActivateCommand = new RelayCommand(activate);
    }
}
