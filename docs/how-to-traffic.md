# How to Use Traffic

This guide follows the pattern used in `TrafficPage` in the reference app.

## Traffic Flow (Congestion)

Query traffic flow for an area:

```csharp
var area = new GeoCircle(
    center: mapCenter,
    radiusInMeters: 5000);

var result = await _trafficService.QueryFlowAsync(
    area,
    new TrafficFlowQueryOptions());

if (result.Error == TrafficError.None)
{
    foreach (var flow in result.Flows!)
    {
        // flow.SpeedRatio: 0.0 = free flow, 1.0 = jammed
        // flow.LengthInMeters: segment length
        // flow.Geometry: GeoPolyline with coordinate vertices

        uint color = flow.SpeedRatio switch
        {
            < 0.25 => 0xFF00AA00,  // green — free flowing
            < 0.5  => 0xFFFFAA00,  // yellow — slow
            < 0.75 => 0xFFFF6600,  // orange — heavy
            _      => 0xFFFF0000,  // red — jammed
        };

        var polyline = new MapPolyline(flow.Geometry.Vertices, color, WidthInPixels: 4);
        mapService.AddMapPolyline(polyline);
    }
}
```

## Traffic Incidents

Query incidents (accidents, closures, construction):

```csharp
var result = await _trafficService.QueryIncidentsAsync(
    new GeoCircle(mapCenter, 5000),
    new TrafficIncidentsQueryOptions());

foreach (var incident in result.Incidents!)
{
    Console.WriteLine($"{incident.Type}: {incident.Description}");

    // Incident types
    if (incident.Type == TrafficIncidentType.Accident)
        ShowAccidentMarker(incident);

    // Severity
    if (incident.Impact == TrafficIncidentImpact.Major && incident.RoadClosed)
        ShowClosureBanner(incident);

    // Start/end times
    if (incident.StartTime > 0)
        Console.WriteLine($"Expected end: {DateTimeOffset.FromUnixTimeSeconds(incident.EndTime)}");
}
```

## Incident Details

Look up a specific incident for more information:

```csharp
var detail = await _trafficService.LookupIncidentAsync(
    incidentId: "inc-123",
    options: new TrafficIncidentLookupOptions());

if (detail is not null)
{
    Console.WriteLine($"Incident: {detail.Description}");
    Console.WriteLine($"Road closed: {detail.RoadClosed}");
}
```

## Visualizing Traffic on the Map

```csharp
// Toggle flow layer on/off
private bool _showFlow;
private async Task ToggleFlow()
{
    _showFlow = !_showFlow;
    if (_showFlow)
    {
        var flowResult = await _trafficService.QueryFlowAsync(area, options);
        foreach (var flow in flowResult.Flows!)
            mapService.AddMapPolyline(new MapPolyline(
                flow.Geometry.Vertices, FlowColor(flow.SpeedRatio), 4));
    }
    else
    {
        mapService.ClearMap();
    }
}

// Toggle incident markers
private async Task ToggleIncidents()
{
    var result = await _trafficService.QueryIncidentsAsync(area, options);
    foreach (var incident in result.Incidents!)
    {
        mapService.AddMapMarker(new MapMarker(
            incident.Coordinates,
            ImagePath: IncidentIcon(incident.Type),
            Text: incident.Description));
    }
}
```

## Error Handling

```csharp
try
{
    var result = await _trafficService.QueryFlowAsync(area, options);
    if (result.Error != TrafficError.None)
    {
        ErrorMessage = $"Traffic data unavailable: {result.Error}";
        return;
    }

    if (result.Flows is null || result.Flows.Count == 0)
    {
        ShowEmptyState("No traffic data for this area");
        return;
    }
}
catch (InvalidOperationException)
{
    ErrorMessage = "Traffic service not initialized.";
}
catch (Exception ex)
{
    ErrorMessage = $"Traffic query failed: {ex.Message}";
}
```

## Traffic Model Types

| Type | Key Properties |
|------|---------------|
| `TrafficFlow` | `SpeedRatio`, `LengthInMeters`, `Geometry` |
| `TrafficIncident` | `Id`, `Description`, `Type`, `Impact`, `RoadClosed`, `StartTime`, `EndTime`, `Coordinates` |
| `GeoPolyline` | `Vertices` (list of GeoCoordinates) |
| `GeoCircle` | `Center`, `RadiusInMeters` |
