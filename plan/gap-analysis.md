# HERE SDK for MAUI — Gap Analysis

## Overview

HERE SDK Explore Edition v4.25.5.0 binding for .NET MAUI. 361 total API types across both platforms. **58 types implemented (~16%)**, 303 types remaining.

---

## Implemented (What Works)

### Core
- `GeoCoordinates`, `GeoBox`, `GeoCircle`, `GeoPolyline`, `GeoPolygon`
- `LanguageCode`, `MapScheme`, `HereSdkOptions`, `HereSdk.Initialize()`
- Android: `SDKNativeEngine` via binding; iOS: via NativeBridge

### Maps
- `HereMapView` (MAUI handler), `IMapService`
- `MapMarker`, `MapPolyline`, `MapPolygon`, `MapCircle`, `LocationIndicator`
- Camera: `SetCameraTargetAsync`, `GetCameraTargetAsync`
- Map events: `MapTapped`, `MapDoubleTapped`
- Map schemes: NormalDay, HybridDay, SatelliteDay

### Search
- `ISearchService`: `SearchAsync(TextQuery)`, `SearchAsync(CategoryQuery)`, `SuggestAsync`, `GetPlaceByIdAsync`
- Models: `Place`, `Suggestion`, `Address`, `Contact`, `OpeningHours`, `TextQuery`, `CategoryQuery`, `SearchOptions`
- Android: Full binding via `SearchEngine` (all native methods available)
- iOS: NativeBridge wraps 4 core methods (searchByText, searchByCategory, suggest, searchByPlaceId)

### Routing
- `IRoutingService`: `CalculateRouteAsync`
- Models: `Route`, `Section`, `Maneuver`, `Waypoint`, `RouteHandle`, `Toll`, `SectionNotice`, `RoutePlace`
- Android: Full binding (118 generated routing types via AAR)
- iOS: NativeBridge wraps basic `calculateRoute` only

### Traffic
- `ITrafficService`: `QueryTrafficAsync`, `QueryIncidentsAsync`
- Android: Full binding; iOS: basic NativeBridge wrapper

### Location
- `ILocationService`: Uses `Microsoft.Maui.Devices.Sensors.Geolocation` (not HERE native positioning)
- iOS NativeBridge does NOT expose HERE positioning types

---

## Gap Analysis by Priority

### P0 — Blocking (crashes, missing core functionality)

| Gap | Detail | Platforms |
|-----|--------|-----------|
| No `searchByAddress` (geocoding) | Cannot search address text → coordinates | iOS NativeBridge missing |
| No `searchByCoordinates` (reverse geocoding) | Cannot look up address from map tap coordinates | iOS NativeBridge missing |

### P1 — Important (major feature gaps)

| Gap | Detail | Platforms |
|-----|--------|-----------|
| iOS NativeBridge: missing geocoding | `searchByText(query:options:)` exists but `searchByAddress` is not wrapped | iOS |
| iOS NativeBridge: missing reverse geocoding | `searchByCoordinates` is not wrapped | iOS |
| iOS NativeBridge: missing picked place lookup | `searchByPickedPlace` is not wrapped | iOS |
| iOS NativeBridge: missing structured address search | `StructuredQuery` not wrapped | iOS |
| iOS NativeBridge: missing extended callbacks | All `SearchCallbackExtended` / `SuggestCallbackExtended` variants | iOS |
| iOS NativeBridge: missing EV/fuel models | `EVChargingStation`, `FuelStation`, `EVChargingPool`, etc. | iOS |
| iOS NativeBridge: missing WebDetails on Place | `WebImage`, `WebEditorial`, `WebRating`, web content | iOS |
| iOS NativeBridge: Place model too thin | Only `id`, `title`, `latitude`, `longitude` — no address, categories, details | iOS |
| iOS NativeBridge: Suggestion too thin | Only `title`, `id`, `isPlace` — no `SuggestionType.poi` vs `.category` distinction via enum | iOS |
| iOS NativeBridge: no `setCustomOption` | Cannot set TripAdvisor integration or other custom options | iOS |
| iOS NativeBridge: no `sendRequest` | Cannot call raw REST API endpoints | iOS |
| iOS NativeBridge: no advanced routing | Missing `IsolineRoutingEngine`, `TransitRoutingEngine`, route refresh, import, returnToRoute | iOS |
| iOS NativeBridge: no full `RoutingOptions` | Only `transportMode` — missing avoidance, toll, EV, text, allow options | iOS |
| iOS NativeBridge: no route serialization | `Route.serialize()`/`deserialize()` not wrapped | iOS |
| iOS NativeBridge: no `TrafficOnRoute` | Cannot get live traffic on a calculated route | Both (Android partially) |
| MAUI `GetTrafficOnRouteAsync` broken | Android: needs native Route object reference; iOS: throws NotImplementedException | Both |
| MAUI `CalculateIsolineAsync` iOS | Throws NotImplementedException | iOS |
| Shared `Place` model incomplete | Missing `details` (WebDetails), `openingHours`, full `categories`, `chains` | Shared |
| Shared `Suggestion` model incomplete | iOS loses SuggestionType enum precision (chain vs category) | iOS → Shared |
| `ILocationService` not using HERE positioning | Uses MAUI GeoLocation — no HERE native fused positioning | Both |

### P2 — Nice-to-have (advanced, 3D, custom layers)

| Gap | Detail | Platforms |
|-----|--------|-----------|
| 3D markers (`MapMarker3D`) | Deferred from Phase 3 | Both |
| Marker clustering (`MapMarkerCluster`) | Deferred from Phase 3 | Both |
| Custom data sources | `TileUrlProviderFactory`, `RasterDataSource`, `PointDataSource`, etc. | Both |
| Camera animations | `FlyToAnimation`, `KeyframeTracks`, `GeoTrail` | Both |
| Map scene lights | `MapSceneLights` | Both |
| Mesh builders | Terrain mesh, custom mesh layers | Both |
| Assets management | `MapAssets` for custom model/marker bundles | Both |
| 84% of catalog types | ~303 types: animations, custom layers, detailed options models, mesh builders, keyframe tracks, lights, assets, network configuration builders, advanced routing models, EV/fuel detail models, address structured fields, etc. | Both |

---

## Testing Gaps

| Gap | Detail |
|-----|--------|
| No runtime device tests | `DeviceTests` project has skeleton structure, no real tests |
| No code coverage | `coverlet.collector` not added |
| No dedicated converter tests | Conversion logic tested only indirectly |
| No dedicated error mapping tests | Error code mappings untested |
| No dedicated async edge case tests | Cancellation, timeout, error recovery |
| No emulator/simulator CI | No Android emulator CI, no iOS simulator CI |

---

## CI/CD Gaps

| Gap | Detail |
|-----|--------|
| AAR not in git | 61 MB AAR must be manually placed before CI can build |
| No iOS binding regeneration workflow | No automated Objective-Sharpie generation |
| No Android binding regeneration workflow | No CI to regenerate AAR bindings |
| No NuGet publish verification | Pack and push not tested in CI |

---

## iOS NativeBridge Architecture Limitation

The NativeBridge requires manual `@objc` wrapper classes for every Swift struct, class, and enum. To add a type:
1. Create a new `@objc` wrapper class in Swift
2. Manual property-for-property conversion (Swift struct → NSObject subclass)
3. Add `[BaseType]` interface in `ApiDefinition.cs`
4. Rebuild the xcframework via `./scripts/build-ios-native.sh`
5. Update the iOS partial class in `Services/` to use the new wrapper

This is ~10x more labor-intensive than Android (where the binding generator auto-produces all C# wrappers from the AAR).

---

## Key Discrepancies (Platform Differences)

1. `CollectionOf<T>` — iOS only, Android erases type parameter
2. `RefreshRouteOptions` — deprecated, will be removed in v4.28.0
3. iOS has 189 structs (not ~160), 142 enums (103 top-level + 39 nested)
4. `FuelType` belongs to Transport module, NOT Search
5. `AuthenticationMode`, `LogControl`, `SDKBuildInformation`, `SDKLogger` belong to `core.engine`, not `core`
6. `com.here.sdk.core.utilities` — empty package on Android
7. `MapCircles` — no native circle primitive; polygon approximation via `CircleGeometryHelper`
8. iOS NativeBridge lacks positioning types — `ILocationService` falls back to MAUI Geolocation
