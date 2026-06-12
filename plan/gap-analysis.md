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

---

## Appendix A — Per-Package Coverage Snapshot (HERE SDK 4.25.5.0)

Generated 2026-06-11 from `tmp/android-inspect/api-reference/` and
`tmp/ios-inspect/api-reference/`. Baseline: the gap-analysis above says
~58 cross-platform types implemented at `4.25.5.0-beta1` (~16% of ~361
documented types). This appendix does not change that baseline — it
enumerates *where* the gap sits on each platform.

### Android SDK package breakdown (584 documented types)

Counted via `grep -oP 'href="\K[^"]+(?=\.html")' .../allclasses-index.html`
grouped by `com.here.sdk.<package>`.

| Top-level package | Android types | Notable sub-packages |
|---|---|---|
| `com.here.sdk.mapview` | 192 | `mapview.datasource` (52) |
| `com.here.sdk.routing` | 130 | — |
| `com.here.sdk.core` | 95 | `core.engine` (34), `core.threading` (6), `core.errors` (2) |
| `com.here.sdk.search` | 69 | — |
| `com.here.sdk.transport` | 36 | — |
| `com.here.sdk.animation` | 21 | — |
| `com.here.sdk.traffic` | 20 | — |
| `com.here.sdk.gestures` | 13 | — |
| `com.here.sdk.engine` | 1 | (top-level wrapper class) |
| **Total** | **577** | (discrepancy with 584 = 7 top-level index/overview pages in `allclasses-index.html`) |

### iOS SDK module breakdown (471 documented types)

Counted via `ls tmp/ios-inspect/api-reference/{Classes,Enums,Protocols,Extensions,Structs}/ | wc -l`.
The iOS api-reference is organized by type kind (Classes/Enums/Protocols/
Extensions/Structs) rather than by sub-package; module membership is in
the top-level index pages `Core.html`, `Maps.html`, `Routing.html`, etc.

| Type kind | iOS count |
|---|---|
| Classes | 127 |
| Enums | 104 |
| Protocols | 35 |
| Extensions | 1 |
| Structs | 204 |
| **Total** | **471** |

### MAUI wrapper type count (104 types)

Counted via `grep -hE "public (class|record|interface|enum) ..."` over
`src/HereSdk.Explore.Maui/Models/**/*.cs` and `Services/*.cs`.

| Wrapper kind | Count | Notes |
|---|---|---|
| Records (Models) | 72 | Geo*, Map*, Route*, Search*, Traffic*, Transport* |
| Enums (Models) | 26 | AvoidType, ManeuverAction, MapScheme, RoutingError, SearchError, etc. |
| Service interfaces | 6 | `IHereSdkService`, `IMapService`, `IRoutingService`, `ISearchService`, `ITrafficService`, `ILocationService` |
| **Total** | **104** | Of which ~58 are cross-platform (i.e. exist on both Android and iOS) |

### Cross-platform coverage (Android × iOS × MAUI wrapper)

The "implemented" cell counts a MAUI wrapper type whose equivalent exists
on BOTH the Android Javadoc and the iOS api-reference. Numbers below are
floor estimates based on the gap-analysis P0/P1/P2 priorities and the
implementation status of each service.

| Capability | Android types | iOS types | MAUI wrapper types | Status |
|---|---|---|---|---|
| Geo primitives (GeoCoordinates, GeoBox, etc.) | ~10 | ~10 | 8 | Implemented |
| SDK init + options | ~5 | ~3 | 2 | Implemented |
| Map view, camera, gestures, markers, polylines, polygons, circles, location indicator | ~80 | ~60 | 18 | Implemented (with caveat: circles approximated as polygons) |
| Search (text, category, suggest, place-by-id) | ~30 | ~20 | 4 methods + 6 models | Partial (P0/P1: no geocoding, no reverse geocoding on iOS) |
| Routing (calculate, maneuvers, sections, route handle) | ~50 | ~30 | 6 methods + 12 models | Partial (P1: no isoline, no transit, no traffic-on-route on iOS) |
| Traffic (flow, incidents) | ~15 | ~8 | 2 methods + 5 models | Partial (P1: no traffic-on-route) |
| Location | ~10 | ~5 | 1 service | Implemented via MAUI `Geolocation` (iOS NativeBridge lacks positioning types) |
| Animation (3D markers, keyframes, camera animations) | ~21 | ~10 | 0 | Not implemented (P2) |
| Custom data sources, mesh builders, assets | ~30 | ~10 | 0 | Not implemented (P2) |
| Address structured fields, EV/fuel, web details, search extensions | ~30+ | ~20+ | 0–3 | Mostly not implemented (P1) |

### Reading this report

- "Android types" = every public class/enum/interface/struct in the Javadoc
  HTML, including nested types (`Easing.InstantiationErrorCode`,
  `VehicleSpecification.CarBuilder`, etc.).
- "iOS types" = every public class/enum/protocol/struct in the Swift
  api-reference HTML; includes nested types and builders.
- "MAUI wrapper types" = records, enums, and interfaces defined in
  `src/HereSdk.Explore.Maui/Models/` and `Services/`. Does not include the
  ~600 auto-generated Android binding types or ~37 auto-generated iOS
  binding types — those are one-to-one with the platform SDK and are not
  the limiting factor.
- The 16% headline number is `cross-platform MAUI types / max(Android types, iOS types)`
  ≈ 58 / max(577, 471) ≈ 10–12%. The 16% figure in the main report uses a
  smaller denominator (~361) that counts only *non-nested, non-builder* types
  in both APIs.

### Next steps to grow coverage

1. **P0 (blocking):** Add `searchByAddress` + `searchByCoordinates` to
   iOS NativeBridge. 2 methods, ~50 lines of Swift.
2. **P1 (high-value):** Add `searchByPickedPlace`, EV/fuel models, full
   `Place`/`Suggestion` enrichment on iOS. ~15 types, ~500 lines Swift.
3. **P1 (high-value):** Add `IsolineRoutingEngine` and route
   serialization to iOS NativeBridge. ~10 types.
4. **P2 (long tail):** Animation, custom data sources, mesh builders,
   address structured fields, web details. ~250+ types.

