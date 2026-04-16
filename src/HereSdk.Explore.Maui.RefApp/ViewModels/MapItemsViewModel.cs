using System.Windows.Input;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;
using Here.Explore.Maui.Services;

namespace Here.Explore.Maui.RefApp.ViewModels;

public class MapItemsViewModel : ViewModelBase
{
    private IMapService? _mapService;
    private string _statusMessage = string.Empty;

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public MapItemsViewModel() { }

    public MapItemsViewModel(IMapService mapService)
    {
        _mapService = mapService;
    }

    /// <summary>
    /// Sets the map service. Called by the page after the handler creates the MapService.
    /// </summary>
    internal void SetMapService(IMapService mapService) => _mapService = mapService;

    private ICommand? _addPolylineCommand;
    public ICommand AddPolylineCommand => _addPolylineCommand ??= new Command(async () => await AddPolylineAsync());

    private ICommand? _addPolygonCommand;
    public ICommand AddPolygonCommand => _addPolygonCommand ??= new Command(async () => await AddPolygonAsync());

    private ICommand? _addArrowCommand;
    public ICommand AddArrowCommand => _addArrowCommand ??= new Command(async () => await AddArrowAsync());

    private ICommand? _addMarkerCommand;
    public ICommand AddMarkerCommand => _addMarkerCommand ??= new Command(async () => await AddMarkerAsync());

    private ICommand? _clearAllCommand;
    public ICommand ClearAllCommand => _clearAllCommand ??= new Command(() => ClearAll());

    private readonly List<MapPolyline> _polylines = new();
    private readonly List<MapPolygon> _polygons = new();
    private readonly List<MapArrow> _arrows = new();
    private readonly List<MapMarker> _markers = new();

    private async Task AddPolylineAsync()
    {
        if (_mapService is null) return;

        try
        {
            var center = await _mapService.GetCameraTargetAsync();
            var polyline = new MapPolyline(
                new List<GeoCoordinates>
                {
                    new(center.Latitude - 0.01, center.Longitude - 0.01),
                    new(center.Latitude + 0.005, center.Longitude),
                    new(center.Latitude + 0.01, center.Longitude + 0.01),
                },
                Color: 0xFF0000FF, // Blue
                WidthInPixels: 5
            );
            _mapService.AddMapPolyline(polyline);
            _polylines.Add(polyline);
            StatusMessage = "Polyline added";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private async Task AddPolygonAsync()
    {
        if (_mapService is null) return;

        try
        {
            var center = await _mapService.GetCameraTargetAsync();
            var polygon = new MapPolygon(
                new List<GeoCoordinates>
                {
                    new(center.Latitude + 0.005, center.Longitude),
                    new(center.Latitude - 0.003, center.Longitude + 0.008),
                    new(center.Latitude - 0.003, center.Longitude - 0.008),
                },
                FillColor: 0x44FF0000 // Semi-transparent red
            );
            _mapService.AddMapPolygon(polygon);
            _polygons.Add(polygon);
            StatusMessage = "Polygon added";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private async Task AddArrowAsync()
    {
        if (_mapService is null) return;

        try
        {
            var center = await _mapService.GetCameraTargetAsync();
            var arrow = new MapArrow(
                new List<GeoCoordinates>
                {
                    new(center.Latitude - 0.005, center.Longitude - 0.005),
                    new(center.Latitude, center.Longitude),
                    new(center.Latitude + 0.005, center.Longitude + 0.005),
                },
                Color: 0xFF00FF00, // Green
                WidthInPixels: 8
            );
            _mapService.AddMapArrow(arrow);
            _arrows.Add(arrow);
            StatusMessage = "Arrow added";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private async Task AddMarkerAsync()
    {
        if (_mapService is null) return;

        try
        {
            var center = await _mapService.GetCameraTargetAsync();
            var marker = new MapMarker(center);
            _mapService.AddMapMarker(marker);
            _markers.Add(marker);
            StatusMessage = "Marker added";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private void ClearAll()
    {
        if (_mapService is null) return;

        try
        {
            foreach (var p in _polylines) _mapService.RemoveMapPolyline(p);
            foreach (var p in _polygons) _mapService.RemoveMapPolygon(p);
            foreach (var a in _arrows) _mapService.RemoveMapArrow(a);
            foreach (var m in _markers) _mapService.RemoveMapMarker(m);

            _polylines.Clear();
            _polygons.Clear();
            _arrows.Clear();
            _markers.Clear();
            StatusMessage = "Cleared all map items";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }
}