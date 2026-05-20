# Architecture

## Layer Model

```
┌──────────────────────────────┐
│   Your App (.NET MAUI)       │
├──────────────────────────────┤
│   HereSdk.Explore.Maui        │  ← Unified C# API (controls, services, models)
├──────────┬───────────────────┤
│ Android  │   iOS Binding      │  ← Platform bindings (AAR → C#, NativeBridge → C#)
│ Binding  │                    │
├──────────┼───────────────────┤
│ HERE     │   NativeBridge     │  ← Swift wrapper (re-exposes API via @objc)
│ Android  ├───────────────────┤
│ SDK      │   HERE iOS SDK     │  ← Swift-only SDK
└──────────┴───────────────────┘
```

## Key Patterns

### Handler Pattern for MapView

`HereMapView` uses the MAUI handler pattern (not legacy custom renderers). The handler creates a platform-specific native view and connects it to the cross-platform `IMapService`.

- **Android**: `HereMapViewHandler.Android.cs` creates an Android `MapView` and bridges it via `MapService.Android.cs`
- **iOS**: `HereMapViewHandler.iOS.cs` creates a `HereMapBridgeView` (UIView subclass from NativeBridge) and bridges it via `MapService.iOS.cs`

### Service Pattern

Each service follows this structure:

| File | Purpose |
|------|---------|
| `IService.cs` | Interface definition (public contract) |
| `Service.cs` | Shared partial class (events, Dispose, stubs) |
| `Service.Android.cs` | Platform implementation with `#pragma warning disable CS1591` |
| `Service.iOS.cs` | Platform implementation with `#pragma warning disable CS1591` |

Platform partials use `#if ANDROID` / `#if IOS` preprocessor directives. The shared partial provides non-platform stubs (under `#if !ANDROID && !IOS`) so unit tests can compile without device targets.

### Platform Converters

Extension methods in `PlatformConverters/` translate between platform types and shared types:

```
GeoCoordinatesConverter.ToShared()      // Android GeoCoordinates   → shared GeoCoordinates
GeoCoordinatesConverter.ToAndroid()     // shared GeoCoordinates   → Android GeoCoordinates
GeoCoordinatesConverter.ToiOS()         // shared GeoCoordinates   → iOS HereGeoCoordinates
```

### Async Bridge

Java callbacks and Swift completion handlers are bridged to C# `async/await` via `TaskCompletionSource<T>`:

```csharp
var tcs = new TaskCompletionSource<RoutingResult>();
nativeEngine.calculateRoute(waypoints, options, (routingError, routes) => {
    tcs.SetResult(new RoutingResult(MapRoutingError(routingError), MapRoutes(routes)));
});
return tcs.Task;
```

### Memory Model

- All platform wrapper types implement `IDisposable`
- Native resources (Android `MapView`, iOS `MapBridgeView`) are created in the handler and disposed in `DisconnectHandler`
- Service instances are DI singletons — they live for the app lifetime
- Model types are C# `record` types (immutable, value semantics)

## Dependency Injection

| Service | Registration | Lifetime |
|---------|-------------|----------|
| `IMapService` | Created by `HereMapViewHandler` | Per MapView |
| `IRoutingService` | `builder.Services.AddSingleton` | Singleton |
| `ISearchService` | `builder.Services.AddSingleton` | Singleton |
| `ITrafficService` | `builder.Services.AddSingleton` | Singleton |
| `ILocationService` | `builder.Services.AddSingleton` | Singleton |

`IMapService` is NOT in DI — each `HereMapView` creates its own instance via the handler.
Access it through `mapView.Map`.
