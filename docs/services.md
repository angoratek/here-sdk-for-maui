# Services

## IMapService

The map service manages the map view: camera, map items, gestures, and display.

```csharp
IMapService map = mapView.Map;

// Camera
await map.SetCameraTargetAsync(center, zoomLevel);
await map.LoadSceneAsync(MapScheme.NormalDay);
var target = await map.GetCameraTargetAsync();

// Map items
map.AddMapMarker(marker);
map.AddMapPolyline(polyline);
map.AddMapPolygon(polygon);
map.AddMapCircle(circle);
map.ClearMap();

// Current state
MapScheme scheme = map.CurrentScheme;  // NormalDay, HybridDay, SatelliteDay
double zoom = map.ZoomLevel;
double bearing = map.Bearing;
double tilt = map.Tilt;
```

### Events

| Event | Args | Trigger |
|-------|------|---------|
| `CameraStateChanged` | `CameraStateChangedEventArgs` | Camera moved (pan, zoom, tilt, bearing) |
| `MapIdle` | `EventArgs` | Map becomes idle after interaction |
| `MapTapped` | `MapTappedEventArgs` | Single tap on map |
| `MapLongPressed` | `MapLongPressedEventArgs` | Long press on map |
| `MapDoubleTapped` | `MapTappedEventArgs` | Double tap on map |
| `MapPinched` | `MapPinchedEventArgs` | Pinch gesture |

## IRoutingService

Calculates routes and isolines between waypoints.

```csharp
// Route calculation
var waypoints = new[] {
    new Waypoint(origin, WaypointType.Start),
    new Waypoint(destination, WaypointType.Stop)
};
var result = await routingService.CalculateRouteAsync(waypoints, new RoutingOptions());
if (result.Error == RoutingError.None) {
    var route = result.Routes![0];
    Console.WriteLine($"{route.LengthInMeters}m, {route.DurationText}");
    foreach (var section in route.Sections) {
        foreach (var maneuver in section.Maneuvers) {
            Console.WriteLine(maneuver.Instruction);
        }
    }
}

// Isoline (reachable area)
var isolineResult = await routingService.CalculateIsolineAsync(
    center, new IsolineOptions(SectionTransportMode.Car, 5000));
```

### RoutingOptions

| Property | Default | Description |
|----------|---------|-------------|
| `TransportMode` | `Car` | Car, Truck, Pedestrian, Bicycle, Scooter, Bus, Taxi, Transit |
| `Optimization` | `Fastest` | Fastest or Shortest route |
| `MaxAlternatives` | `null` | Maximum number of alternative routes |

## ISearchService

Text search, category search, auto-suggest, and place details.

```csharp
// Text search
var result = await searchService.SearchAsync(
    new TextQuery("coffee near Alexanderplatz"),
    new SearchOptions { MaxItems = 20 });

// Category search
var categoryResult = await searchService.SearchAsync(
    new CategoryQuery(PlaceCategory.Restaurant),
    new SearchOptions());

// Auto-suggest
var suggestions = await searchService.SuggestAsync(
    new TextQuery("Brandenburger Tor"),
    new SearchOptions());

// Place details
var place = await searchService.GetPlaceByIdAsync("here:pds:place:...");
```

### SearchOptions

| Property | Default | Description |
|----------|---------|-------------|
| `MaxItems` | `10` | Maximum results to return |
| `SearchArea` | `null` | `GeoCircle` to restrict search |
| `LanguageCode` | `null` | BCP-47 language code for results |

## ITrafficService

Real-time traffic flow and incident data.

```csharp
// Traffic flow (congestion levels on roads)
var flow = await trafficService.QueryFlowAsync(
    new GeoCircle(center, 5000),
    new TrafficFlowQueryOptions());

// Traffic incidents (accidents, closures, construction)
var incidents = await trafficService.QueryIncidentsAsync(
    new GeoCircle(center, 5000),
    new TrafficIncidentsQueryOptions());

// Incident details
var detail = await trafficService.LookupIncidentAsync(
    incidentId, new TrafficIncidentLookupOptions());
```

## ILocationService

Cross-platform device location using `Microsoft.Maui.Devices.Sensors.Geolocation`.

```csharp
// One-shot location
var location = await locationService.GetCurrentLocationAsync();

// Continuous updates
await locationService.StartListeningAsync();
locationService.LocationChanged += (_, loc) =>
    Console.WriteLine($"{loc.Coordinates.Latitude}, {loc.Coordinates.Longitude}");
await locationService.StopListeningAsync();
```
