# Getting Started with HERE SDK Explore for .NET MAUI

## Installation

Add the cross-platform MAUI package to your project:

```xml
<PackageReference Include="HereSdk.Explore.Maui" Version="4.25.5.0" />
```

This automatically pulls in the correct platform binding:
- **Android**: `HereSdk.Explore.Android.Binding`
- **iOS**: `HereSdk.Explore.iOS.Binding`

## Prerequisites

- .NET 10 SDK
- .NET MAUI workload (`dotnet workload install maui`)
- Android SDK API 34+ (for Android)
- macOS + Xcode 15+ (for iOS)
- HERE SDK credentials (Access Key ID + Secret)

## Platform Setup

### Android

No additional setup required. The AAR is bundled in the Android binding package.

### iOS

The iOS binding includes the NativeBridge xcframework. No manual framework linking is required when using the NuGet package.

## Initialization

In `MauiProgram.cs`:

```csharp
using Here.Explore.Maui;

builder.UseHereSdkExplore(new HereSdkOptions
{
    AccessKeyId = "YOUR_ACCESS_KEY_ID",
    AccessKeySecret = "YOUR_ACCESS_KEY_SECRET"
});
```

## Using the Map

In XAML:

```xml
<ContentPage xmlns:here="clr-namespace:Here.Explore.Maui.Controls;assembly=HereSdk.Explore.Maui">
    <here:HereMapView x:Name="MapView" MapScheme="NormalDay" />
</ContentPage>
```

In code-behind:

```csharp
public partial class MyPage : ContentPage
{
    public MyPage()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            var mapService = MapView.Map;
            await mapService.LoadSceneAsync(MapScheme.NormalDay);
            await mapService.SetCameraTargetAsync(new GeoCoordinates(52.5, 13.4), 12);
        };
    }
}
```

## Services

All services are registered as singletons in DI and can be injected:

```csharp
public class MyViewModel
{
    private readonly ISearchService _searchService;
    private readonly IRoutingService _routingService;
    private readonly ILocationService _locationService;
    private readonly ITrafficService _trafficService;

    public MyViewModel(
        ISearchService searchService,
        IRoutingService routingService,
        ILocationService locationService,
        ITrafficService trafficService)
    {
        _searchService = searchService;
        _routingService = routingService;
        _locationService = locationService;
        _trafficService = trafficService;
    }
}
```

### Search

```csharp
var result = await _searchService.SuggestAsync(
    new TextQuery("Berlin"),
    new SearchOptions { MaxItems = 10 });

foreach (var suggestion in result.Suggestions ?? new List<Suggestion>())
{
    Console.WriteLine($"{suggestion.Title} ({suggestion.Type})");
}
```

### Routing

```csharp
var waypoints = new List<Waypoint>
{
    new(new GeoCoordinates(52.5, 13.4), WaypointType.Start),
    new(new GeoCoordinates(48.1, 11.5), WaypointType.Stop)
};

var result = await _routingService.CalculateRouteAsync(
    waypoints,
    new RoutingOptions());

if (result.Error == RoutingError.None)
{
    var route = result.Routes[0];
    Console.WriteLine($"Distance: {route.LengthInMeters}m, Duration: {route.DurationInSeconds}s");
}
```

### Location

```csharp
var location = await _locationService.GetCurrentLocationAsync();
if (location is not null)
{
    Console.WriteLine($"Lat: {location.Coordinates.Latitude}, Lon: {location.Coordinates.Longitude}");
}
```

### Map Items

```csharp
// Marker
var marker = new MapMarker(new GeoCoordinates(52.5, 13.4), Text: "Hello");
mapService.AddMapMarker(marker);

// Polyline
var polyline = new MapPolyline(
    new List<GeoCoordinates> { new(52.5, 13.4), new(52.6, 13.5) },
    Color: 0xFF0000FF,
    WidthInPixels: 5);
mapService.AddMapPolyline(polyline);

// Polygon
var polygon = new MapPolygon(
    new List<GeoCoordinates> { new(52.5, 13.4), new(52.6, 13.5), new(52.55, 13.3) },
    FillColor: 0x44FF0000);
mapService.AddMapPolygon(polygon);

// Circle (approximated as polygon)
var circle = new MapCircle(new GeoCoordinates(52.5, 13.4), 500, FillColor: 0x3300FF00);
mapService.AddMapCircle(circle);
```

## Package Contents

| Package | Contents |
|---------|----------|
| `HereSdk.Explore.Maui` | Unified C# API, controls, handlers, service abstractions |
| `HereSdk.Explore.Android.Binding` | Android AAR binding, Metadata.xml transforms |
| `HereSdk.Explore.iOS.Binding` | iOS NativeBridge binding, ApiDefinition.cs |

## Known Limitations

- **iOS binary size**: ~831 MB (stripped for release)
- **Map circles**: Approximated as polygons (HERE SDK has no native circle primitive)
- **iOS positioning**: Uses MAUI Geolocation fallback (HERE native positioning not yet in NativeBridge)
- **Map pick**: Returns null on both platforms (not yet fully implemented)
