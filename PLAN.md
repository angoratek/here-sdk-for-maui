# HERE SDK Explore for .NET MAUI — Master Plan

> This plan has been verified against both the Android Javadoc and iOS Swift interface.
> See [Corrections Log](plan/05-api-surface-catalog.md) for discrepancies found and fixed.

## Quick Reference

| What | Value |
|---|---|
| SDK | HERE Explore SDK v4.25.5.0 |
| .NET | .NET 10 |
| Platforms | Android (API 24+), iOS (15.2+) |
| iOS Binding | Native Library Interop (Swift wrapper → ObjC → Sharpie) |
| API Style | Unified idiomatic C# |
| Packages | `HereSdk.Explore.Android.Binding`, `HereSdk.Explore.iOS.Binding`, `HereSdk.Explore.Maui` |
| Delivery | Phased (4 phases) |
| Testing | TDD — xUnit + device runners |

## Plan Documents

| Doc | Content |
|---|---|
| [00-overview.md](plan/00-overview.md) | Decisions, critical findings, sub-document index |
| [01-project-structure.md](plan/01-project-structure.md) | Solution layout, .csproj details, build props |
| [02-android-binding.md](plan/02-android-binding.md) | AAR binding, Metadata.xml, namespace mapping (verified counts) |
| [03-ios-binding.md](plan/03-ios-binding.md) | Native Library Interop deep-dive, Swift wrapper patterns, deprecation notes |
| [04-cross-platform-api.md](plan/04-cross-platform-api.md) | Unified MAUI API, services, handlers, models |
| [05-api-surface-catalog.md](plan/05-api-surface-catalog.md) | Complete type catalog with validation checkboxes (corrected) |
| [06-phased-delivery.md](plan/06-phased-delivery.md) | Phase-by-phase tasks and acceptance criteria |
| [07-testing-strategy.md](plan/07-testing-strategy.md) | TDD, mock patterns, device tests |
| [08-nuget-and-ci.md](plan/08-nuget-and-ci.md) | Packaging, GitHub Actions, build scripts |
| [09-risks-and-mitigations.md](plan/09-risks-and-mitigations.md) | Risk register |

## Key Discrepancies Found During Verification

These were found by cross-referencing plan claims against actual SDK data:

1. **`CollectionOf<T>`** — exists ONLY in iOS Swift interface, NOT in Android Javadoc
2. **`RefreshRouteOptions`** — iOS type is sealed AND deprecated (v4.28.0); do NOT bind
3. **iOS struct count** — actual 189, not ~160 as originally estimated
4. **iOS type aliases** — 21 total, 16 were missing from catalog (all completion handlers)
5. **iOS nested enums** — 39 nested enums need NSInteger wrappers (e.g., `Easing.InstantiationErrorCode`)
6. **Android package counts** were inaccurate — corrected to Javadoc-verified numbers
7. **`FuelType`** belongs to Transport module, NOT Search
8. **`AuthenticationMode`, `LogControl`, `SDKBuildInformation`, `SDKLogger`** belong to `core.engine`, not `core`
9. **`com.here.sdk.core.utilities`** is empty — removed from namespace mapping
10. **`AndroidLibrayInclusion`** was a typo in csproj example — removed
11. **`SearchError`/`RoutingError` are Java.Lang.Enum** — cannot use in C# switch, must use if/else if with .Equals()
12. **`ManeuverAction` naming**: binding uses PascalCase (LeftTurn, SharpLeftTurn, LeftUTurn) not the simplified names
13. **`ManeuverAction` has no Ferry** — Explore SDK doesn't expose Ferry as a maneuver action
14. **`Address.HouseNumOrName`** (not HouseNumber), **`Address.Country`** (not CountryName)
15. **`RouteHandle.Handle`** (string field, not Id)
16. **`RouteSection.DeparturePlace`/`ArrivalPlace`** (RoutePlace type, not Departure/Arrival)
17. **`Isoline.RangeValue`** (double, not RangeInMeters) and **`Isoline.Polygons`** (IList\<GeoPolygon\>)
18. **`RoadTexts.Names.DefaultValue`** (for road name), **`RoadTexts.NumbersWithDirection.DefaultValue`** (for road number)
19. **`TransportSpecification`** uses builder pattern (CarBuilder, TruckBuilder, etc.)
20. **`SearchEngine.Search(TextQuery, ...)`** uses `ISearchCallbackExtended`, while **`SearchByText(TextQuery, ...)`** uses `SearchCompletedHandler`

## Phase Summary

| Phase | Scope | Status |
|---|---|---|
| 0 | Foundation: scaffolding, Android binding, iOS NativeBridge skeleton | ✅ Complete |
| 1 | MapView + SDK Init: map display, camera, gestures, markers | ✅ Complete (Android ✅, iOS ✅ — NativeBridge xcframework built, real API bindings) |
| 2 | Search + Routing: full search & routing across both platforms | ✅ Complete (Android ✅, iOS ✅ — NativeBridge HereSearchEngine/HereRoutingEngine functional) |
| 3 | Traffic + Advanced: traffic, map items, advanced features | ✅ Complete (Android ✅, iOS ✅ — NativeBridge HereTrafficEngine functional) |
| 4 | Polish + NuGet: coverage audit, packaging, CI/CD, docs | 🔨 In Progress (4.1 ✅, 4.2 ✅, 4.3 ✅, 4.4 ✅, 4.5 ✅, 4.6 ✅; 4.7 remaining) |

> **Note**: All phases are scaffolded with code structure, models, services, NativeBridge wrappers, and tests.
> **Current status (2026-04-15)**: Android + iOS binding + MAUI library all build. 198 unit tests pass.
> Phase 4 progress: API coverage audit complete (4.1), NuGet metadata (4.4), XML docs (4.5), CI/CD .NET 10 (4.6).
> iOS NativeBridge xcframework: **BUILT** — real Swift wrapper compiled for arm64 + simulator, ObjC headers generated.
> iOS binding ApiDefinition.cs: **Matches actual xcframework headers** — all 30+ types correctly bound.
> iOS service implementations: SearchService, RoutingService, TrafficService **functional** (using NativeBridge completion handlers). MapService functional (camera, gestures, map items, scene loading).
> iOS HereMapViewHandler: Uses real HereMapBridgeView.Create() + PlatformView.
> Added: Transport models (TruckSpecifications, CarSpecifications, TransportSpecification, AvoidanceOptions).
> Added: Gesture events (MapDoubleTapped, MapLongPressed, MapPanned, MapPinchRotated).
> Added: Route detail models (RouteHandle, Span, Toll, SectionNotice, Signpost, RoutePlace).
> Added: Version.props — centralized version file for HereSdkVersion, PackageVersion.
> Added: appsettings.json credential management for RefApp.
> Remaining Phase 4: final validation on physical devices (4.7). iOS RefApp build requires Xcode version alignment with .NET SDK.

## Validation Rule

**Every feature MUST be validated against both API references before marking complete:**
- Android: `tmp/android-inspect/api-reference/`
- iOS: `tmp/ios-inspect/heresdk-explore-ios-4.25.5.0.274356/heresdk/frameworks/heresdk.xcframework/ios-arm64/heresdk.framework/Modules/heresdk.swiftmodule/arm64-apple-ios.swiftinterface`