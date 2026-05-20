# How to Route

This guide follows the pattern used in `DirectionsPage` in the reference app.

## Basic Route Calculation

```csharp
var waypoints = new[]
{
    new Waypoint(new GeoCoordinates(52.5200, 13.4050), WaypointType.Start),
    new Waypoint(new GeoCoordinates(48.1351, 11.5820), WaypointType.Stop)
};

var result = await _routingService.CalculateRouteAsync(
    waypoints,
    new RoutingOptions { TransportMode = SectionTransportMode.Car });
```

## Route Results

```csharp
if (result.Error != RoutingError.None)
{
    ErrorMessage = result.Error switch
    {
        RoutingError.NoRouteFound => "No route found between these locations.",
        RoutingError.NetworkError => "Check your connection and try again.",
        _ => $"Routing failed: {result.Error}"
    };
    return;
}

var route = result.Routes![0];
Console.WriteLine($"Total: {route.DurationText} ({route.LengthInMeters}m)");

foreach (var section in route.Sections)
{
    Console.WriteLine($"Section {section.SectionIndex}: {section.LengthInMeters}m");
    foreach (var maneuver in section.Maneuvers)
    {
        Console.WriteLine($"  {maneuver.Action}: {maneuver.Instruction} ({maneuver.DistanceText})");
    }
}
```

## Transport Modes

```csharp
// Car (default)
new RoutingOptions(TransportMode: SectionTransportMode.Car)

// Truck
new RoutingOptions(TransportMode: SectionTransportMode.Truck)

// Pedestrian
new RoutingOptions(TransportMode: SectionTransportMode.Pedestrian)

// Bicycle
new RoutingOptions(TransportMode: SectionTransportMode.Bicycle)

// Scooter
new RoutingOptions(TransportMode: SectionTransportMode.Scooter)

// Public transit
new RoutingOptions(TransportMode: SectionTransportMode.Transit)

// Bus
new RoutingOptions(TransportMode: SectionTransportMode.Bus)
```

## Route Optimization

```csharp
// Fastest route (default)
new RoutingOptions(Optimization: OptimizationMode.Fastest)

// Shortest distance
new RoutingOptions(Optimization: OptimizationMode.Shortest)

// Multiple alternatives
new RoutingOptions(MaxAlternatives: 3)
// result.Routes will contain up to 3 routes
```

## Isoline (Reachable Area)

Calculate the area reachable within a given time/distance:

```csharp
var result = await _routingService.CalculateIsolineAsync(
    center: new GeoCoordinates(52.5200, 13.4050),
    options: new IsolineOptions(
        TransportMode: SectionTransportMode.Car,
        RangeInMeters: 5000,
        MaxPoints: 500));

if (result.Error == RoutingError.None)
{
    foreach (var isoline in result.Isolines!)
    {
        Console.WriteLine($"Reachable area: {isoline.Vertices.Count} vertices");
    }
}
```

## Visualizing Routes on the Map

```csharp
// Draw route as polyline
foreach (var section in route.Sections)
{
    if (section.Geometry is not null)
    {
        var polyline = new MapPolyline(
            section.Geometry,
            Color: 0xFF0099FF,   // blue
            WidthInPixels: 6);
        _mapService.AddMapPolyline(polyline);
    }
}

// Draw waypoint markers
_mapService.AddMapMarker(new MapMarker(origin, Text: "Start"));
_mapService.AddMapMarker(new MapMarker(destination, Text: "End"));
```

## Error States

The ref app `DirectionsViewModel` handles these error scenarios:

1. **Empty origin/destination** — validation before calling the service
2. **No route found** — `RoutingError.NoRouteFound`
3. **Network failure** — `RoutingError.NetworkError` / thrown exception
4. **Service not initialized** — `InvalidOperationException`

All are surfaced through a bound `RouteErrorMessage` property displayed in an `ErrorBanner`.
