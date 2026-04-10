using System.Windows.Input;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;
using Here.Explore.Maui.Services;

namespace Here.Explore.Maui.RefApp.ViewModels;

public class MapItemsViewModel
{
    private readonly IMapService? _mapService;

    public string StatusMessage { get; private set; } = string.Empty;

    public MapItemsViewModel() { }

    public MapItemsViewModel(IMapService mapService)
    {
        _mapService = mapService;
    }

    public ICommand AddPolylineCommand => new Command(async () => await AddPolylineAsync());
    public ICommand AddPolygonCommand => new Command(async () => await AddPolygonAsync());
    public ICommand AddArrowCommand => new Command(async () => await AddArrowAsync());
    public ICommand AddMarkerCommand => new Command(async () => await AddMarkerAsync());
    public ICommand ClearAllCommand => new Command(() => ClearAll());

    private MapPolyline? _lastPolyline;
    private MapPolygon? _lastPolygon;
    private MapArrow? _lastArrow;
    private MapMarker? _lastMarker;

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
            _lastPolyline = polyline;
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
            _lastPolygon = polygon;
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
            _lastArrow = arrow;
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
            _lastMarker = marker;
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
            if (_lastPolyline is not null) _mapService.RemoveMapPolyline(_lastPolyline);
            if (_lastPolygon is not null) _mapService.RemoveMapPolygon(_lastPolygon);
            if (_lastArrow is not null) _mapService.RemoveMapArrow(_lastArrow);
            if (_lastMarker is not null) _mapService.RemoveMapMarker(_lastMarker);

            _lastPolyline = null;
            _lastPolygon = null;
            _lastArrow = null;
            _lastMarker = null;
            StatusMessage = "Cleared all map items";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }
}