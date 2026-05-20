# How to Draw on the Map

This guide follows the pattern used in `ToolsPage` in the reference app.

## Drawing Tools Overview

The ref app ToolsPage provides these drawing tools:

| Tool | Result |
|------|--------|
| Marker | Places a `MapMarker` at the tapped location |
| Polyline | Adds consecutive points; double-tap to finish |
| Polygon | Adds consecutive points; double-tap to close and fill |
| Circle | Places a `MapCircle` (polygon-approximated) at the tapped location |
| Clear | Removes all map objects |

## Adding a Marker

```csharp
mapService.MapTapped += OnMapTappedForMarker;

private void OnMapTappedForMarker(object? sender, MapTappedEventArgs e)
{
    if (_currentTool != DrawingTool.Marker) return;

    var marker = new MapMarker(e.Coordinates, Text: "Point");
    _mapService.AddMapMarker(marker);
}
```

## Drawing a Polyline

```csharp
private List<GeoCoordinates> _polylinePoints = new();

mapService.MapTapped += OnPolylinePoint;
mapService.MapDoubleTapped += OnPolylineFinished;

private void OnPolylinePoint(object? sender, MapTappedEventArgs e)
{
    if (_currentTool != DrawingTool.Polyline) return;

    _polylinePoints.Add(e.Coordinates);
    // Add temporary marker to show the vertex
    _mapService.AddMapMarker(new MapMarker(e.Coordinates));
}

private void OnPolylineFinished(object? sender, MapTappedEventArgs e)
{
    if (_currentTool != DrawingTool.Polyline || _polylinePoints.Count < 2) return;

    var polyline = new MapPolyline(_polylinePoints, 0xFFFF4444, WidthInPixels: 4);
    _mapService.AddMapPolyline(polyline);
    _polylinePoints.Clear();
}
```

## Drawing a Polygon

```csharp
private List<GeoCoordinates> _polygonPoints = new();

private void OnPolygonFinished(object? sender, MapTappedEventArgs e)
{
    if (_currentTool != DrawingTool.Polygon || _polygonPoints.Count < 3) return;

    // Close the polygon (first point = last point)
    var vertices = new List<GeoCoordinates>(_polygonPoints)
    {
        _polygonPoints[0]
    };

    var polygon = new MapPolygon(
        vertices,
        FillColor: 0x4444FF44,
        StrokeColor: 0xFF44FF44,
        StrokeWidthInPixels: 2);
    _mapService.AddMapPolygon(polygon);
    _polygonPoints.Clear();
}
```

## Adding a Circle

```csharp
mapService.MapTapped += OnCirclePlaced;

private void OnCirclePlaced(object? sender, MapTappedEventArgs e)
{
    if (_currentTool != DrawingTool.Circle) return;

    var circle = new MapCircle(
        Center: e.Coordinates,
        RadiusInMeters: 500,
        FillColor: 0x444444FF,
        StrokeColor: 0xFF4444FF);
    _mapService.AddMapCircle(circle);
}
```

## Clearing All Objects

```csharp
[RelayCommand]
private void ClearAll()
{
    _mapService.ClearMap();
    _polylinePoints.Clear();
    _polygonPoints.Clear();
}
```

## Tool Selection Pattern

The ref app uses an enum-based tool selector:

```csharp
public enum DrawingTool { None, Marker, Polyline, Polygon, Circle }

private DrawingTool _currentTool;

[RelayCommand]
private void SetDrawingTool(string toolName)
{
    _currentTool = toolName switch
    {
        "Marker" => DrawingTool.Marker,
        "Polyline" => DrawingTool.Polyline,
        "Polygon" => DrawingTool.Polygon,
        "Circle" => DrawingTool.Circle,
        _ => DrawingTool.None
    };
}
```

The UI shows the selected tool as active and changes the map gesture behavior accordingly.
