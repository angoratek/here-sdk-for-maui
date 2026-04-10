# Cross-Platform MAUI API Design

## Principles

1. **Unified API** — One set of types/services for both platforms
2. **Idiomatic C#** — PascalCase, async/await, events, IDisposable
3. **Thin platform layer** — Platform implementations delegate to bindings; no business logic
4. **Interface-driven** — All services exposed as interfaces for testability
5. **Value types for models** — GeoCoordinates, Route, etc. are immutable structs or records

## SDK Initialization

```csharp
// HereSdk.cs — the entry point
namespace Here.Explore.Maui;

public static class HereSdk
{
    /// <summary>
    /// Initialize the HERE SDK for Explore edition.
    /// Call this once at app startup before using any other API.
    /// </summary>
    public static void Initialize(HereSdkOptions options)
    {
        // Delegates to platform-specific init
        InternalInitialize(options);
    }

#if ANDROID
    private static void InternalInitialize(HereSdkOptions options)
    {
        var androidOptions = new Com.Here.Sdk.Core.Engine.SDKOptions
        {
            AccessKeyId = options.AccessKeyId,
            AccessKeySecret = options.AccessKeySecret,
            // ... map fields
        };
        Com.Here.Sdk.Core.Engine.SDKNativeEngine.CreateInstance(androidOptions);
    }
#elif IOS
    private static void InternalInitialize(HereSdkOptions options)
    {
        var iosOptions = new HereSdkOptions /* iOS binding type */
        {
            AccessKeyId = options.AccessKeyId,
            AccessKeySecret = options.AccessKeySecret,
        };
        HereSdkEngine.Initialize(iosOptions);  // our NativeBridge wrapper
    }
#endif
}

public record HereSdkOptions
{
    public required string AccessKeyId { get; init; }
    public required string AccessKeySecret { get; init; }
    public string? CachePath { get; init; }
    public HereSdkCachePolicy CachePolicy { get; init; } = HereSdkCachePolicy.Default;
}
```

## Core Models (Platform-Agnostic)

These are shared types that exist in the cross-platform library. Platform implementations convert to/from platform binding types.

```csharp
namespace Here.Explore.Maui.Models;

// Immutable record types — no platform coupling
public record GeoCoordinates(double Latitude, double Longitude)
{
    public bool IsValid => Latitude is >= -90 and <= 90 && Longitude is >= -180 and <= 180;
}

public record GeoBox(GeoCoordinates SouthWest, GeoCoordinates NorthEast);
public record GeoCircle(GeoCoordinates Center, double RadiusInMeters);
public record GeoCorridor(GeoPolyline Polyline, double RadiusInMeters);
public record GeoOrientation(double Bearing, double Tilt);
public record GeoPolygon(IReadOnlyList<GeoCoordinates> Vertices);
public record GeoPolyline(IReadOnlyList<GeoCoordinates> Vertices);
public record Point2D(double X, double Y);
public record Size2D(double Width, double Height);
public record Rectangle2D(Point2D Origin, Size2D Size);

public record Location(
    GeoCoordinates Coordinates,
    double? Altitude = null,
    double? SpeedInMetersPerSecond = null,
    double? BearingInDegrees = null,
    LocationSource Source = LocationSource.Unknown
);

public enum LocationSource
{
    Unknown,
    Gps,
    Network,
    Passive
}
```

## Platform Converter Pattern

Each platform implements converters between shared models and binding types:

```csharp
// Platforms/Android/PlatformConverters/GeoCoordinatesConverter.Android.cs
#if ANDROID
namespace Here.Explore.Maui.PlatformConverters;

internal static class GeoCoordinatesConverter
{
    public static GeoCoordinates ToShared(this Com.Here.Sdk.Core.GeoCoordinates android)
        => new(android.Latitude, android.Longitude);

    public static Com.Here.Sdk.Core.GeoCoordinates ToAndroid(this GeoCoordinates shared)
        => new(shared.Latitude, shared.Longitude);
}
#endif
```

```csharp
// Platforms/iOS/PlatformConverters/GeoCoordinatesConverter.iOS.cs
#if IOS
namespace Here.Explore.Maui.PlatformConverters;

internal static class GeoCoordinatesConverter
{
    public static GeoCoordinates ToShared(this HereGeoCoordinates ios)
        => new(ios.Latitude, ios.Longitude);

    public static HereGeoCoordinates ToiOS(this GeoCoordinates shared)
        => new(shared.Latitude, shared.Longitude);
}
#endif
```

## Service Interfaces

### IMapService

```csharp
namespace Here.Explore.Maui.Services;

public interface IMapService
{
    // Camera control
    Task<GeoCoordinates> GetCameraTargetAsync();
    Task SetCameraTargetAsync(GeoCoordinates target, double? zoomLevel = null);
    Task AnimateCameraAsync(CameraAnimation animation);

    // Scene
    Task LoadSceneAsync(MapScheme scheme);
    MapScheme CurrentScheme { get; }

    // Map items
    void AddMapMarker(MapMarker marker);
    void RemoveMapMarker(MapMarker marker);
    void AddMapPolyline(MapPolyline polyline);
    void RemoveMapPolyline(MapPolyline polyline);
    void AddMapPolygon(MapPolygon polygon);
    void RemoveMapPolygon(MapPolygon polygon);
    void AddMapArrow(MapArrow arrow);
    void RemoveMapArrow(MapArrow arrow);

    // Picking
    Task<MapPickResult> PickAsync(Point2D screenPoint);

    // Events
    event EventHandler<CameraStateChangedEventArgs>? CameraStateChanged;
    event EventHandler? MapIdle;
}
```

### IRoutingService

```csharp
namespace Here.Explore.Maui.Services;

public interface IRoutingService
{
    Task<RoutingResult> CalculateRouteAsync(
        IReadOnlyList<Waypoint> waypoints,
        RoutingOptions options);

    Task<IsolineResult> CalculateIsolineAsync(
        GeoCoordinates center,
        IsolineOptions options);

    Task<RoutingResult> RefreshRouteAsync(
        Route route,
        RefreshRouteOptions options);

    Task<TrafficOnRoute> GetTrafficOnRouteAsync(Route route);
}

public record RoutingResult(
    RoutingError Error,
    IReadOnlyList<Route>? Routes
);

public record Route(
    RouteHandle Handle,
    IReadOnlyList<Section> Sections,
    double LengthInMeters,
    long DurationInSeconds,
    // ... key properties
);
```

### ISearchService

```csharp
namespace Here.Explore.Maui.Services;

public interface ISearchService
{
    Task<SearchResult> SearchAsync(TextQuery query, SearchOptions options);
    Task<SearchResult> SearchAsync(CategoryQuery query, SearchOptions options);
    Task<SuggestResult> SuggestAsync(TextQuery query, SearchOptions options);
    Task<Place?> GetPlaceByIdAsync(PlaceIdQuery query);
}
```

### ITrafficService

```csharp
namespace Here.Explore.Maui.Services;

public interface ITrafficService
{
    Task<TrafficFlowResult> QueryFlowAsync(GeoCircle area, TrafficFlowQueryOptions options);
    Task<TrafficIncidentsResult> QueryIncidentsAsync(GeoCircle area, TrafficIncidentsQueryOptions options);
    Task<TrafficIncident?> LookupIncidentAsync(string incidentId, TrafficIncidentLookupOptions options);
}
```

## HereMapView Control (MAUI Handler)

The map view is the primary UI control. We use the MAUI handler pattern.

### Virtual View (Cross-Platform)

```csharp
namespace Here.Explore.Maui.Controls;

public class HereMapView : View, IHereMapView
{
    // Bindable properties
    public static readonly BindableProperty CameraTargetProperty =
        BindableProperty.Create(nameof(CameraTarget), typeof(GeoCoordinates), typeof(HereMapView));

    public static readonly BindableProperty MapSchemeProperty =
        BindableProperty.Create(nameof(MapScheme), typeof(MapScheme), typeof(HereMapView),
            MapScheme.NormalDay);

    public GeoCoordinates CameraTarget
    {
        get => (GeoCoordinates)GetValue(CameraTargetProperty);
        set => SetValue(CameraTargetProperty, value);
    }

    public MapScheme MapScheme
    {
        get => (MapScheme)GetValue(MapSchemeProperty);
        set => SetValue(MapSchemeProperty, value);
    }

    // Services exposed on the view
    public IMapService Map => _mapService.Value;
    public IRoutingService Routing => _routingService.Value;

    private readonly Lazy<IMapService> _mapService;
    private readonly Lazy<IRoutingService> _routingService;
}
```

### Handler (Platform-Specific)

```csharp
// Handlers/HereMapViewHandler.Android.cs
#if ANDROID
using HereMapViewPlatform = Com.Here.Sdk.Mapview.MapView;

public partial class HereMapViewHandler : ViewHandler<IHereMapView, HereMapViewPlatform>
{
    protected override HereMapViewPlatform CreatePlatformView()
    {
        return new HereMapViewPlatform(Context);
    }

    protected override void ConnectHandler(HereMapViewPlatform platformView)
    {
        base.ConnectHandler(platformView);
        // Wire up MapView events, SDK init, etc.
    }

    // Property mappers
    public static void MapCameraTarget(IHereMapView handler, HereMapView view)
    {
        var target = view.CameraTarget;
        var camera = handler.PlatformView.Camera;
        camera.Target = target.ToAndroid();
    }

    public static void MapMapScheme(IHereMapView handler, HereMapView view)
    {
        var scene = handler.PlatformView.MapScene;
        scene.LoadScene(view.MapScheme.ToAndroidMapScheme());
    }
}
#endif
```

```csharp
// Handlers/HereMapViewHandler.iOS.cs
#if IOS
using HereMapViewPlatform = HereMapView; // our ObjC-exposed MapView from the SDK

public partial class HereMapViewHandler : ViewHandler<IHereMapView, HereMapViewPlatform>
{
    protected override HereMapViewPlatform CreatePlatformView()
    {
        return new HereMapViewPlatform();
    }

    // Similar mapping pattern as Android
}
#endif
```

### Registration in MauiProgram.cs

```csharp
// In the Ref App's MauiProgram.cs
builder.Services.AddSingleton<IMapService, MapService>();
builder.Services.AddSingleton<IRoutingService, RoutingService>();
builder.Services.AddSingleton<ISearchService, SearchService>();
builder.Services.AddSingleton<ITrafficService, TrafficService>();

// Handler registration is done via AddMauiControls or in the library's Extension method:
builder.UseHereSdkExplore();
```

## Extension Method

The library provides a clean DI registration:

```csharp
namespace Here.Explore.Maui;

public static class HereSdkExtensions
{
    public static MauiAppBuilder UseHereSdkExplore(this MauiAppBuilder builder, HereSdkOptions options)
    {
        HereSdk.Initialize(options);
        builder.Services.AddSingleton<IMapService, MapService>();
        builder.Services.AddSingleton<IRoutingService, RoutingService>();
        builder.Services.AddSingleton<ISearchService, SearchService>();
        builder.Services.AddSingleton<ITrafficService, TrafficService>();
        return builder;
    }
}
```

## Error Handling Pattern

Both Android and iOS SDKs use error codes (not exceptions). Our unified API converts these:

```csharp
namespace Here.Explore.Maui;

// Unified error types per module
public enum RoutingError
{
    None,
    NetworkError,
    HttpError,
    NoRouteFound,
    InvalidParameters,
    // ... exhaustive list from both platforms
}

public enum SearchError
{
    None,
    NetworkError,
    NoResults,
    InvalidQuery,
    // ...
}

// Platform converters map native error codes → unified enums
```

## Async Pattern

Java callbacks and Swift closures → C# TaskCompletionSource:

```csharp
// Platform implementation example (Android)
public async Task<RoutingResult> CalculateRouteAsync(
    IReadOnlyList<Waypoint> waypoints, RoutingOptions options)
{
    var tcs = new TaskCompletionSource<RoutingResult>();

    _androidRoutingEngine.CalculateRoute(
        waypoints.Select(w => w.ToAndroid()).ToList(),
        options.ToAndroid(),
        new RouteCalculateCallback(
            onSuccess: routes => tcs.SetResult(new RoutingError.None, routes.ToShared()),
            onError: error => tcs.SetResult(new RoutingResult(error.ToShared(), null))
        ));

    return await tcs.Task;
}
```