# HERE SDK Explore for .NET MAUI

Cross-platform .NET MAUI bindings for the [HERE SDK](https://www.here.com/) Explore Edition v4.25.5.0, supporting Android (API 24+) and iOS (15.2+).

## Features

- **Unified C# API** — One idiomatic interface for both Android and iOS
- **Map display** — MapView control with camera, gestures, schemes
- **Search** — Text search, category search, auto-suggest, place details
- **Routing** — Car, truck, pedestrian, bicycle, scooter, EV routing with maneuvers
- **Traffic** — Real-time traffic flow and incident queries
- **Map items** — Markers, polylines, polygons, 3D markers, clusters
- **Custom layers** — Raster and vector tile sources, custom map styles

## Prerequisites

- **.NET 9 SDK** — [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
- **.NET MAUI workloads** — `dotnet workload install maui`
- **macOS + Xcode 15+** — Required for iOS binding (Native Library Interop)
- **Android SDK** — API 34+ recommended
- **HERE SDK credentials** — Access Key ID and Secret from [HERE platform](https://developer.here.com/)

## Quick Start

### 1. Install the NuGet package

```xml
<ItemGroup>
  <PackageReference Include="HereSdk.Explore.Maui" Version="4.25.5.0" />
</ItemGroup>
```

### 2. Initialize in `MauiProgram.cs`

```csharp
using Here.Explore.Maui;

builder.UseHereSdkExplore(new HereSdkOptions
{
    AccessKeyId = "YOUR_ACCESS_KEY_ID",
    AccessKeySecret = "YOUR_ACCESS_KEY_SECRET"
});
```

### 3. Add the map to your XAML

```xml
<xmlns:here="clr-namespace:Here.Explore.Maui.Controls;assembly=HereSdk.Explore.Maui"

<here:HereMapView
    x:Name="Map"
    CameraTarget="{Binding Center}"
    MapScheme="NormalDay" />
```

### 4. Use services

```csharp
// Search
var results = await searchService.SearchAsync(
    new TextQuery("coffee nearby"),
    new SearchOptions { SearchArea = new GeoCircle(center, 1000) });

// Routing
var route = await routingService.CalculateRouteAsync(
    new[] { new Waypoint(start), new Waypoint(end) },
    new CarOptions());
```

## Architecture

```
┌─────────────────────────────────────────────┐
│           HereSdk.Explore.Maui              │
│     Unified idiomatic C# API               │
│  (Models, Services, Controls, Handlers)     │
└──────────┬──────────────────┬───────────────┘
           │                  │
  ┌────────▼────────┐  ┌─────▼──────────────┐
  │  Android Binding │  │  iOS Binding         │
  │  (AAR → C#)     │  │  (NativeBridge →    │
  │                  │  │   Sharpie → C#)      │
  └────────┬────────┘  └─────┬───────────────┘
           │                  │
  ┌────────▼────────┐  ┌─────▼──────────────┐
  │  HERE Android    │  │  NativeBridge Swift  │
  │  SDK (AAR)       │  │  wrapper framework   │
  │                  │  └─────┬───────────────┘
  │                  │        │
  │                  │  ┌─────▼──────────────┐
  │                  │  │  HERE iOS SDK        │
  │                  │  │  (Swift-only)        │
  └──────────────────┘  └─────────────────────┘
```

### Why NativeBridge for iOS?

The iOS HERE SDK is **Swift-only**. Its ObjC bridge header exposes only 4 types — insufficient for binding. We create a Swift wrapper framework (`HereSdk.Explore.iOS.NativeBridge`) that re-exposes all APIs via `@objc` annotations, then bind that with Objective-Sharpie.

This is the standard pattern recommended by the MAUI Community Toolkit for Swift-only SDKs.

## Building from Source

### Android

```bash
# Extract AAR (first time only)
cp tmp/heresdk-explore-android-*/heresdk-explore-android-*.aar src/HereSdk.Explore.Android.Binding/Jars/

# Build Android binding
dotnet build src/HereSdk.Explore.Android.Binding -c Release
```

### iOS

```bash
# Build NativeBridge xcframework (requires Xcode)
cd src/HereSdk.Explore.iOS.NativeBridge
./build-xcframework.sh
cd ../..

# Run Objective-Sharpie + fix bindings
./scripts/bind-ios.sh

# Build iOS binding
dotnet build src/HereSdk.Explore.iOS.Binding -c Release
```

### MAUI Library

```bash
# Build for both platforms
dotnet build src/HereSdk.Explore.Maui -f net9.0-android -c Release
dotnet build src/HereSdk.Explore.Maui -f net9.0-ios -c Release

# Or use the build script
./scripts/build.sh
```

### Tests

```bash
# Unit tests (no device needed)
dotnet test tests/HereSdk.Explore.Maui.Tests -c Release

# Device tests (requires emulator/simulator)
dotnet test tests/HereSdk.Explore.Maui.DeviceTests -f net9.0-android -c Release
dotnet test tests/HereSdk.Explore.Maui.DeviceTests -f net9.0-ios -c Release
```

## Project Structure

```
here-sdk-for-maui/
├── src/
│   ├── HereSdk.Explore.Android.Binding/       # Android AAR binding
│   ├── HereSdk.Explore.iOS.NativeBridge/       # Swift wrapper (Xcode project)
│   ├── HereSdk.Explore.iOS.Binding/            # iOS C# binding
│   ├── HereSdk.Explore.Maui/                  # Cross-platform MAUI library
│   └── HereSdk.Explore.Maui.RefApp/           # Demo app
├── tests/
│   ├── HereSdk.Explore.Maui.Tests/            # xUnit unit tests
│   └── HereSdk.Explore.Maui.DeviceTests/      # Platform device tests
├── scripts/                                    # Build, test, pack scripts
├── plan/                                       # Design documents
└── tmp/                                        # SDK archives (gitignored)
```

## NuGet Packages

| Package | Description |
|---------|-------------|
| `HereSdk.Explore.Maui` | Cross-platform MAUI library (depends on platform bindings) |
| `HereSdk.Explore.Android.Binding` | Android AAR binding |
| `HereSdk.Explore.iOS.Binding` | iOS NativeBridge binding |

Most consumers should reference only `HereSdk.Explore.Maui`.

## Roadmap

| Phase | Scope | Status |
|-------|-------|--------|
| 0 | Foundation: scaffolding, bindings, build infrastructure | Complete |
| 1 | MapView + SDK Init: map display, camera, gestures, markers | Scaffolded |
| 2 | Search + Routing: full search & routing on both platforms | Scaffolded |
| 3 | Traffic + Advanced: traffic, map items, advanced features | Scaffolded |
| 4 | Polish + NuGet: coverage, packaging, CI/CD, docs | Scaffolded |

See [plan/06-phased-delivery.md](plan/06-phased-delivery.md) for detailed task breakdowns.

## Key Design Decisions

- **Unified API over platform exposure** — Consumers use one C# interface; platform details are hidden
- **Native Library Interop for iOS** — Swift wrapper framework bridges the Swift-only gap
- **MAUI Handler pattern** — For `HereMapView`, not legacy custom renderers
- **Record types for models** — Immutable value types (GeoCoordinates, Route, Waypoint, etc.)
- **Interface-driven services** — `IMapService`, `IRoutingService`, etc. for testability
- **TDD** — Write tests first, then implement

## Known Limitations

- **No Windows/macOS Catalyst support** — HERE SDK is Android + iOS only
- **No CarPlay/Android Auto** in unified API — platform-specific only
- **iOS `RefreshRouteOptions`** is deprecated and will NOT be bound
- **iOS binary size** — xcframework is ~831 MB (stripped for release)
- **macOS required** for building iOS bindings (Xcode dependency)

## License

MIT