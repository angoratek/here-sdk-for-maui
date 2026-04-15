# API Surface Catalog — Complete Type Inventory

> This document is THE validation reference. Every type listed here MUST be bound and verified
> against both the Android Javadoc and iOS Swift interface.

## Validation Sources

| Platform | Location |
|---|---|
| Android Javadoc | `tmp/android-inspect/api-reference/` |
| Android classes.jar | `tmp/android-inspect/aar-contents/classes.jar` |
| iOS Swift Interface | `tmp/ios-inspect/heresdk-explore-ios-4.25.5.0.274356/heresdk/frameworks/heresdk.xcframework/ios-arm64/heresdk.framework/Modules/heresdk.swiftmodule/arm64-apple-ios.swiftinterface` |
| iOS Jazzy Docs | `tmp/ios-inspect/api-reference/` |

## Statistics

| Metric | Android | iOS |
|---|---|---|
| Total classes (public) | ~450+ (incl. inner/builder) | 128 sealed + 1 open + 1 public (IconProvider) + 2 @objc bridge = 132 |
| Structs | N/A (Java uses classes) | 189 |
| Enums | ~30 named + ~80 Java enum classes | 103 top-level + 39 nested = 142 |
| Interfaces/Protocols | ~67 | 35 |
| Type aliases | N/A (Java uses functional interfaces) | 21 |

### Corrections Log (from verification against actual SDK data)

- `CollectionOf<T>` exists ONLY in iOS Swift interface — NOT in Android Javadoc. Android uses `CollectionOf` as a regular class with type parameter erasure.
- `RefreshRouteOptions` on iOS is `@_hasMissingDesignatedInitializers` (sealed) AND deprecated ("Will be removed in v4.28.0. Use `RoutingOptions` instead"). Do NOT bind — use `RoutingOptions` only.
- `IconProvider` is the ONLY non-sealed, non-@objc public class besides `MapView` on iOS.
- Android package counts (verified from Javadoc): core=36 classes+12 enums+4 interfaces, core.engine=21 classes+9 enums+3 interfaces, mapview=76 classes+38 enums+14 interfaces, mapview.datasource=32 classes+3 enums+17 interfaces, routing=85 classes+41 enums+4 interfaces, search=50 classes+11 enums+7 interfaces, traffic=9 classes+6 enums+5 interfaces, transport=25 classes+11 enums, gestures=4 classes+2 enums+7 interfaces, animation=9 classes+7 enums+4 exceptions+1 interface
- `com.here.sdk.core.utilities` is EMPTY — no types. Omit from namespace remapping.
- `FuelType` belongs to `com.here.sdk.transport` (NOT search).
- `AuthenticationMode` belongs to `com.here.sdk.core.engine` (NOT core).
- `LogControl`, `SDKBuildInformation`, `SDKLogger` belong to `com.here.sdk.core.engine` (NOT core).
- `Threading` belongs to `com.here.sdk.core.threading` (NOT core).
- 16 type aliases are missing from the original catalog (all completion handlers) — added below.
- 39 nested enums on iOS (e.g., `Easing.InstantiationErrorCode`, `MapMarker.TextStyle.Placement`) need wrapping.
- `JunctionsTraversability` is in `com.here.sdk.traffic` on Android, not core.
- **[2026-04-14]** `TrafficIncidentOnRoute` is in `com.here.sdk.routing`, NOT `com.here.sdk.traffic` on both platforms. Its `id` is `String?` (optional) vs `TrafficIncident.id` which is `String` (non-null).
- **[2026-04-14]** `TrafficOptimizationMode` is in `com.here.sdk.routing`, NOT `com.here.sdk.traffic` on Android.
- **[2026-04-14]** `TrafficEngine` method names: Android uses `queryForFlow`/`queryForIncidents` (not `queryFlow`/`queryIncidents`).
- **[2026-04-14]** `MapCameraAnimation` is in `com.here.sdk.mapview`, NOT `com.here.sdk.animation` on Android.
- **[2026-04-14]** `MapItemKeyFrameTrack` naming: iOS uses capital F (`MapItemKeyFrameTrack`), Android uses lowercase f (`MapItemKeyframeTrack`).
- **[2026-04-14]** `EasingFunction` is a top-level enum on iOS (38 cases), NOT nested inside `Easing`.
- **[2026-04-14]** Missing from original catalog: 3 traffic callback interfaces, 2 traffic base interfaces, 2 TrafficIncident nested types (RestrictedVehicleCategory, VehicleRestriction).
- **[2026-04-14]** Missing from original catalog: MapCameraAnimationFactory, MapItemRepresentation, DataSource builders (Point/Line/PolygonDataSourceBuilder), MapLayerPriority/Builder, keyframe structs.
- **[2026-04-14]** Deprecated: `MapCameraKeyframeTrack.lookAtDistance(keyframes:easing:interpolationMode:)` — removed in v4.27.0; use `lookAtDistance(ofKind:keyframes:easing:interpolationMode:)`.
- **[2026-04-14]** `TrafficDataProvider` has no public API surface — opaque handle class on both platforms.

## Module: Core

### Android Package: `com.here.sdk.core` (36 classes, 12 enums, 4 interfaces)

### Classes

| Android Class | iOS Equivalent | MAUI Unified Type | Phase | Validated |
|---|---|---|---|---|
| `Angle` | `Angle` (sealed class) | `Angle` | 1 | ☐ |
| `Authentication` | `Authentication` (sealed class) | `Authentication` | 1 | ☐ |
| `CatalogVersionHint` | `CatalogVersionHint` (sealed class) | `CatalogVersionHint` | 2 | ☐ |
| `CollectionOf` | `CollectionOf<T>` (sealed class, iOS only — iOS has generics; Android erases to Object) | `CollectionOf` | 2 | ☐ |
| `Metadata` | `Metadata` (sealed class) | `Metadata` | 2 | ☐ |
| `PolylineSimplifier` | `PolylineSimplifier` (sealed class) | `PolylineSimplifier` | 3 | ☐ |
| `TimeRule` | `TimeRule` (sealed class) | `TimeRule` | 3 | ☐ |

> **Note**: `AuthenticationMode`, `LogControl`, `SDKBuildInformation`, `SDKLogger` belong to `com.here.sdk.core.engine` (see Engine module below). `Threading` belongs to `com.here.sdk.core.threading`. `SDKOptions`, `SDKNativeEngine`, etc. are in the Engine module.

### Structs (iOS only — Android uses classes)

| iOS Struct | MAUI Unified Type | Phase | Validated |
|---|---|---|---|
| `Anchor2D` | `Anchor2D` | 2 | ☐ |
| `Anchor2DKeyframe` | `Anchor2DKeyframe` | 3 | ☐ |
| `AngleRange` | `AngleRange` | 2 | ☐ |
| `CatalogConfiguration` | `CatalogConfiguration` | 2 | ☐ |
| `CatalogIdentifier` | `CatalogIdentifier` | 2 | ☐ |
| `DesiredCatalog` | `DesiredCatalog` | 2 | ☐ |
| `EngineOptions` | `EngineOptions` | 1 | ☐ |
| `ExternalID` | `ExternalID` | 2 | ☐ |
| `GeoBox` | `GeoBox` | 1 | ☐ |
| `GeoCircle` | `GeoCircle` | 1 | ☐ |
| `GeoCoordinates` | `GeoCoordinates` | 1 | ☐ |
| `GeoCoordinatesUpdate` | `GeoCoordinatesUpdate` | 1 | ☐ |
| `GeoCorridor` | `GeoCorridor` | 1 | ☐ |
| `GeoOrientation` | `GeoOrientation` | 1 | ☐ |
| `GeoOrientationUpdate` | `GeoOrientationUpdate` | 1 | ☐ |
| `GeoPolygon` | `GeoPolygon` | 2 | ☐ |
| `GeoPolyline` | `GeoPolyline` | 2 | ☐ |
| `IntegerRange` | `IntegerRange` | 2 | ☐ |
| `LayerConfiguration` | `LayerConfiguration` | 3 | ☐ |
| `LocalizedRoadNumber` | `LocalizedRoadNumber` | 2 | ☐ |
| `LocalizedRoadNumbers` | `LocalizedRoadNumbers` | 2 | ☐ |
| `LocalizedText` | `LocalizedText` | 2 | ☐ |
| `LocalizedTexts` | `LocalizedTexts` | 2 | ☐ |
| `Location` | `Location` | 1 | ☐ |
| `LocationTime` | `LocationTime` | 1 | ☐ |
| `NameID` | `NameID` | 2 | ☐ |
| `NetworkEndpoint` | `NetworkEndpoint` | 2 | ☐ |
| `NetworkSettings` | `NetworkSettings` | 1 | ☐ |
| `ParameterConfiguration` | `ParameterConfiguration` | 3 | ☐ |
| `PedestrianProfile` | `PedestrianProfile` | 2 | ☐ |
| `PickedPlace` | `PickedPlace` | 2 | ☐ |
| `Point2D` | `Point2D` | 1 | ☐ |
| `Point3D` | `Point3D` | 2 | ☐ |
| `ProxySettings` | `ProxySettings` | 2 | ☐ |
| `Rectangle2D` | `Rectangle2D` | 2 | ☐ |
| `SDKOptions` | `SDKOptions` | 1 | ☐ |
| `SDKVersion` | `SDKVersion` | 1 | ☐ |
| `Size2D` | `Size2D` | 1 | ☐ |
| `TransportProfile` | `TransportProfile` | 2 | ☐ |
| `UsageStats` | `UsageStats` | 2 | ☐ |

### Enums (both platforms)

| Enum | Values | MAUI Type | Phase | Validated |
|---|---|---|---|---|
| `CardinalDirection` | 8 | `CardinalDirection` | 2 | ☐ |
| `CountryCode` | 224 | `CountryCode` | 2 | ☐ |
| `CurrentType` | — | `CurrentType` | 2 | ☐ |
| `EngineBaseURL` | — | `EngineBaseURL` | 1 | ☐ |
| `GeoPolylineDirection` | — | `GeoPolylineDirection` | 2 | ☐ |
| `InstantiationErrorCode` | — | `InstantiationErrorCode` | 1 | ☐ |
| `JunctionsTraversability` | — | `JunctionsTraversability` | 2 | ☐ |
| `LanguageCode` | — | `LanguageCode` | 2 | ☐ |
| `LocationSource` | — | `LocationSource` | 1 | ☐ |
| `LocationTechnology` | — | `LocationTechnology` | 1 | ☐ |
| `LogLevel` | — | `LogLevel` | 1 | ☐ |
| `MetadataType` | — | `MetadataType` | 2 | ☐ |
| `PassThroughFeature` | — | `PassThroughFeature` | 2 | ☐ |
| `PolylineSimplificationError` | — | `PolylineSimplificationError` | 3 | ☐ |
| `RouteType` | — | `RouteType` | 2 | ☐ |
| `TaskOutcome` | — | `TaskOutcome` | 2 | ☐ |
| `UnitSystem` | — | `UnitSystem` | 1 | ☐ |

### Protocols/Interfaces (both platforms)

| Protocol | MAUI Type | Phase | Validated |
|---|---|---|---|
| `CustomMetadataValue` | `ICustomMetadataValue` | 2 | ☐ |
| `LocationDelegate` | `ILocationDelegate` | 1 | ☐ |
| `LogAppender` | `ILogAppender` | 1 | ☐ |
| `PlatformThreading` | `IPlatformThreading` | 2 | ☐ |
| `Runnable` | `IRunnable` | 2 | ☐ |
| `TaskHandle` | `ITaskHandle` | 2 | ☐ |

### Type Aliases (iOS — 21 total)

All type aliases are completion handler closures or error aliases. They map to C# events or exception types.

| Type Alias | MAUI Type | Phase | Validated |
|---|---|---|---|
| `AuthenticationCompletionHandler` | `EventHandler<AuthenticationEventArgs>` | 1 | ☐ |
| `AuthenticationException` | `AuthenticationException` | 1 | ☐ |
| `CalculateIsolineCompletionHandler` | `EventHandler<IsolineCalculatedEventArgs>` | 2 | ☐ |
| `CalculateRouteCompletionHandler` | `EventHandler<RouteCalculatedEventArgs>` | 2 | ☐ |
| `CalculateTrafficOnRouteCompletionHandler` | `EventHandler<TrafficOnRouteCalculatedEventArgs>` | 3 | ☐ |
| `DeviceIdHandle` | `DeviceIdHandle` | 1 | ☐ |
| `IconProviderCallback` | `EventHandler<IconProvidedEventArgs>` | 3 | ☐ |
| `InstantiationError` | `InstantiationError` | 1 | ☐ |
| `PlaceIdSearchCompletionHandler` | `EventHandler<PlaceIdSearchEventArgs>` | 2 | ☐ |
| `PlaceIdSearchExtendedCompletionHandler` | `EventHandler<PlaceIdSearchExtendedEventArgs>` | 2 | ☐ |
| `PlaceSerializationException` | `PlaceSerializationException` | 2 | ☐ |
| `PolylineSimplificationCompletionHandler` | `EventHandler<PolylineSimplificationEventArgs>` | 3 | ☐ |
| `SearchCompletionHandler` | `EventHandler<SearchCompletedEventArgs>` | 2 | ☐ |
| `SearchExtendedCompletionHandler` | `EventHandler<SearchExtendedCompletedEventArgs>` | 2 | ☐ |
| `SuggestCompletionHandler` | `EventHandler<SuggestCompletedEventArgs>` | 2 | ☐ |
| `SuggestExtendedCompletionHandler` | `EventHandler<SuggestExtendedCompletedEventArgs>` | 2 | ☐ |
| `TaskCompletionHandler` | `EventHandler<TaskEventArgs>` | 2 | ☐ |
| `TileUrlRequestHandler` | `Func<int,int,int,string>` | 3 | ☐ |
| `TrafficFlowQueryCompletionHandler` | `EventHandler<TrafficFlowQueryEventArgs>` | 3 | ☐ |
| `TrafficIncidentCompletionHandler` | `EventHandler<TrafficIncidentLookupEventArgs>` | 3 | ☐ |
| `TrafficIncidentsQueryCompletionHandler` | `EventHandler<TrafficIncidentsQueryEventArgs>` | 3 | ☐ |

---

## Module: Engine

### Android: `com.here.sdk.core.engine` (21 classes, 9 enums, 3 interfaces) + `com.here.sdk.engine` (1 class)

### Classes

| Android Class | iOS Equivalent | MAUI Type | Phase | Validated |
|---|---|---|---|---|
| `SDKNativeEngine` | `SDKNativeEngine` (sealed) | `SDKNativeEngine` | 1 | ☐ |
| `Authentication` | `Authentication` (sealed) | `Authentication` | 1 | ☐ |
| `AuthenticationMode` | `AuthenticationMode` (sealed) | `AuthenticationMode` | 1 | ☐ |
| `LogControl` | `LogControl` (sealed) | `LogControl` | 1 | ☐ |
| `SDKBuildInformation` | `SDKBuildInformation` (sealed) | `SDKBuildInformation` | 1 | ☐ |
| `SDKLogger` | `SDKLogger` (sealed) | `SDKLogger` | 1 | ☐ |
| `InitProvider` | — (Android only) | N/A | 1 | ☐ |
| `HereMap` | `HereMap` (sealed) | `HereMap` | 1 | ☐ |

### Key Structs (iOS)

| iOS Struct | MAUI Type | Phase | Validated |
|---|---|---|---|
| `EngineOptions` | `EngineOptions` | 1 | ☐ |
| `SDKOptions` | `HereSdkOptions` | 1 | ☐ |
| `AuthenticationData` | `AuthenticationData` | 1 | ☐ |

### Key Enums (both platforms)

| Enum | MAUI Type | Phase | Validated |
|---|---|---|---|
| `EngineBaseURL` | `EngineBaseURL` | 1 | ☐ |
| `InstantiationErrorCode` | `InstantiationErrorCode` | 1 | ☐ |
| `LogLevel` | `LogLevel` | 1 | ☐ |

---

## Module: Maps (MapView)

### Android: `com.here.sdk.mapview` (122 classes, 14 interfaces)
### Android: `com.here.sdk.mapview.datasource` (32 classes, 3 enums, 17 interfaces)

### Critical Classes — Phase 1

| Android Class | iOS Equivalent | MAUI Type | Phase | Validated |
|---|---|---|---|---|
| `MapView` | `MapView` (open, ObjC-visible) | `HereMapView` | 1 | ☐ |
| `MapCamera` | `MapCamera` (sealed) | `MapCamera` | 1 | ☐ |
| `MapScene` | `MapScene` (sealed) | `MapScene` | 1 | ☐ |
| `MapScheme` | `MapScheme` (enum) | `MapScheme` | 1 | ☐ |
| `MapCameraAnimationFactory` | `MapCameraAnimationFactory` (sealed) | `MapCameraAnimationFactory` | 1 | ☐ |
| `MapCameraUpdateFactory` | `MapCameraUpdateFactory` (sealed) | `MapCameraUpdateFactory` | 1 | ☐ |
| `Gestures` | `Gestures` (sealed) | `Gestures` | 1 | ☐ |
| `LocationIndicator` | `LocationIndicator` (sealed) | `LocationIndicator` | 1 | ☐ |
| `MapMarker` | `MapMarker` (sealed) | `MapMarker` | 1 | ☐ |
| `MapImage` | `MapImage` (sealed) | `MapImage` | 1 | ☐ |
| `MapViewBase` | `MapViewBase` (protocol) | `IHereMapView` | 1 | ☐ |
| `SDKMapViewInitializer` | `SDKMapViewInitializer` | — (internal) | 1 | ☐ |

### Map Items — Phase 3

> **Validated 2026-04-14** against Android Javadoc and iOS Swift interface (v4.25.5.0).

| Android Class | iOS Equivalent | MAUI Type | Phase | Validated |
|---|---|---|---|---|
| `MapMarker3D` | `MapMarker3D` (sealed) | `MapMarker3D` | 3 | ☑ |
| `MapMarker3DModel` | `MapMarker3DModel` (sealed) | `MapMarker3DModel` | 3 | ☑ |
| `MapMarkerCluster` | `MapMarkerCluster` (sealed) | `MapMarkerCluster` | 3 | ☑ |
| `MapMarkerAnimation` | `MapMarkerAnimation` (sealed) | `MapMarkerAnimation` | 3 | ☑ |
| `MapPolyline` | `MapPolyline` (sealed) | `MapPolyline` | 2 | ☑ |
| `MapPolygon` | `MapPolygon` (sealed) | `MapPolygon` | 2 | ☑ |
| `MapArrow` | `MapArrow` (sealed) | `MapArrow` | 3 | ☑ |
| `MapImageOverlay` | `MapImageOverlay` (sealed) | `MapImageOverlay` | 3 | ☑ |

### Custom Layers — Phase 3

| Android Class | iOS Equivalent | MAUI Type | Phase | Validated |
|---|---|---|---|---|
| `MapLayer` | `MapLayer` (sealed) | `MapLayer` | 3 | ☑ |
| `MapLayerBuilder` | `MapLayerBuilder` (sealed) | `MapLayerBuilder` | 3 | ☑ |
| `RasterDataSource` | `RasterDataSource` (sealed) | `RasterDataSource` | 3 | ☑ |
| `PointDataSource` | `PointDataSource` (sealed) | `PointDataSource` | 3 | ☑ |
| `LineDataSource` | `LineDataSource` (sealed) | `LineDataSource` | 3 | ☑ |
| `PolygonDataSource` | `PolygonDataSource` (sealed) | `PolygonDataSource` | 3 | ☑ |
| `TileUrlProviderFactory` | `TileUrlProviderFactory` (sealed) | `TileUrlProviderFactory` | 3 | ☑ |
| `JsonStyleFactory` | `JsonStyleFactory` (sealed) | `JsonStyleFactory` | 3 | ☑ |
| `Style` | `Style` (sealed) | `Style` | 3 | ☑ |
| `Mesh` / `MeshBuilder` | `Mesh` / `MeshBuilder` (sealed) | `Mesh` / `MeshBuilder` | 3 | ☑ |

### Camera + Animation — Phase 1-3

| Android Class | iOS Equivalent | MAUI Type | Phase | Validated |
|---|---|---|---|---|
| `MapCameraLimits` | `MapCameraLimits` (sealed) | `MapCameraLimits` | 2 | ☑ |
| `MapCameraAnimation` | `MapCameraAnimation` (sealed) | `MapCameraAnimation` | 1 | ☑ |
| `MapCameraKeyframeTrack` | `MapCameraKeyframeTrack` (sealed) | `MapCameraKeyframeTrack` | 3 | ☑ |
| `MapItemKeyFrameTrack` | `MapItemKeyFrameTrack` (sealed) | `MapItemKeyFrameTrack` | 3 | ☑ ⚠️ |
| `MapSceneLights` | `MapSceneLights` (sealed) | `MapSceneLights` | 3 | ☑ |
| `MapContentSettings` | `MapContentSettings` (sealed) | `MapContentSettings` | 3 | ☑ |
| `MapContext` | `MapContext` (sealed) | `MapContext` | 3 | ☑ |
| `AssetsManager` | `AssetsManager` (sealed) | `AssetsManager` | 3 | ☑ |

> **⚠️ MapItemKeyFrameTrack**: iOS uses capital F (`MapItemKeyFrameTrack`) while Android uses lowercase f (`MapItemKeyframeTrack`). NativeBridge must use the iOS casing.

### Gesture Delegates — Phase 1

| Android Interface | iOS Protocol | MAUI Event | Phase | Validated |
|---|---|---|---|---|
| `TapListener` | `TapDelegate` | `MapTapped` | 1 | ☐ |
| `DoubleTapListener` | `DoubleTapDelegate` | `MapDoubleTapped` | 1 | ☐ |
| `LongPressListener` | `LongPressDelegate` | `MapLongPressed` | 1 | ☐ |
| `PanListener` | `PanDelegate` | `MapPanned` | 2 | ☐ |
| `PinchRotateListener` | `PinchRotateDelegate` | `MapPinchRotated` | 2 | ☐ |
| `TwoFingerPanListener` | `TwoFingerPanDelegate` | `MapTwoFingerPanned` | 2 | ☐ |
| `TwoFingerTapListener` | `TwoFingerTapDelegate` | `MapTwoFingerTapped` | 2 | ☐ |

---

## Module: Routing

### Android: `com.here.sdk.routing` (94 classes, 4 interfaces)

### Engine Classes — Phase 2

| Android Class | iOS Equivalent | MAUI Type | Phase | Validated |
|---|---|---|---|---|
| `RoutingEngine` | `RoutingEngine` (sealed) | `RoutingEngine` | 2 | ☐ |
| `IsolineRoutingEngine` | `IsolineRoutingEngine` (sealed) | `IsolineRoutingEngine` | 2 | ☐ |
| `TransitRoutingEngine` | `TransitRoutingEngine` (sealed) | `TransitRoutingEngine` | 2 | ☐ |

### Route Result Types — Phase 2

| Android Class | iOS Equivalent | MAUI Type | Phase | Validated |
|---|---|---|---|---|
| `Route` | `Route` (sealed) | `Route` | 2 | ☐ |
| `RouteHandle` | `RouteHandle` (struct) | `RouteHandle` | 2 | ☐ |
| `Section` | `Section` (sealed) | `Section` | 2 | ☐ |
| `Span` | `Span` (sealed) | `Span` | 2 | ☐ |
| `Maneuver` | `Maneuver` (sealed) | `Maneuver` | 2 | ☐ |
| `Waypoint` | `Waypoint` (struct) | `Waypoint` | 2 | ☐ |
| `Isoline` | `Isoline` (sealed) | `Isoline` | 2 | ☐ |

### Route Options — Phase 2

| Android Class | iOS Struct | MAUI Type | Phase | Validated |
|---|---|---|---|---|
| `RoutingOptions` | `RoutingOptions` (struct) | `RoutingOptions` | 2 | ☐ |
| `RouteOptions` | `RouteOptions` (struct) | `RouteOptions` | 2 | ☐ |
| `CarOptions` | `CarOptions` (struct) | `CarOptions` | 2 | ☐ |
| `TruckOptions` | `TruckOptions` (struct) | `TruckOptions` | 2 | ☐ |
| `PedestrianOptions` | `PedestrianOptions` (struct) | `PedestrianOptions` | 2 | ☐ |
| `BicycleOptions` | `BicycleOptions` (struct) | `BicycleOptions` | 2 | ☐ |
| `ScooterOptions` | `ScooterOptions` (struct) | `ScooterOptions` | 2 | ☐ |
| `EVCarOptions` | `EVCarOptions` (struct) | `EVCarOptions` | 2 | ☐ |
| `EVTruckOptions` | `EVTruckOptions` (struct) | `EVTruckOptions` | 2 | ☐ |
| `TaxiOptions` | `TaxiOptions` (struct) | `TaxiOptions` | 2 | ☐ |
| `BusOptions` | `BusOptions` (struct) | `BusOptions` | 2 | ☐ |
| `PrivateBusOptions` | `PrivateBusOptions` (struct) | `PrivateBusOptions` | 2 | ☐ |
| `TransitRouteOptions` | `TransitRouteOptions` (struct) | `TransitRouteOptions` | 2 | ☐ |
| `AvoidanceOptions` | `AvoidanceOptions` (struct) | `AvoidanceOptions` | 2 | ☐ |
| `RefreshRouteOptions` | `RefreshRouteOptions` (class) | `RefreshRouteOptions` | 2 | ☐ |
| `IsolineOptions` | `IsolineOptions` (struct) | `IsolineOptions` | 2 | ☐ |

### Route Details — Phase 2

| Android Class/Struct | iOS Struct | MAUI Type | Phase | Validated |
|---|---|---|---|---|
| `TrafficOnRoute` | `TrafficOnRoute` (struct) | `TrafficOnRoute` | 2 | ☐ |
| `TrafficOnSection` | `TrafficOnSection` (struct) | `TrafficOnSection` | 2 | ☐ |
| `TrafficOnSpan` | `TrafficOnSpan` (struct) | `TrafficOnSpan` | 2 | ☐ |
| `Toll` | `Toll` (struct) | `Toll` | 2 | ☐ |
| `TollFare` | `TollFare` (struct) | `TollFare` | 2 | ☐ |
| `Signpost` | `Signpost` (struct) | `Signpost` | 2 | ☐ |
| `SectionNotice` | `SectionNotice` (struct) | `SectionNotice` | 2 | ☐ |
| `MapMatchedCoordinates` | `MapMatchedCoordinates` (struct) | `MapMatchedCoordinates` | 2 | ☐ |
| `DynamicSpeedInfo` | `DynamicSpeedInfo` (struct) | `DynamicSpeedInfo` | 2 | ☐ |
| `MaxSpeedOnSegment` | `MaxSpeedOnSegment` (struct) | `MaxSpeedOnSegment` | 2 | ☐ |
| `RouteLabel` | `RouteLabel` (struct) | `RouteLabel` | 2 | ☐ |
| `RoutePlace` | `RoutePlace` (struct) | `RoutePlace` | 2 | ☐ |
| `RouteRailwayCrossing` | `RouteRailwayCrossing` (struct) | `RouteRailwayCrossing` | 2 | ☐ |
| `ViolatedRestriction` | `ViolatedRestriction` (struct) | `ViolatedRestriction` | 2 | ☐ |
| `ChargingStation` | `ChargingStation` (struct) | `ChargingStation` | 2 | ☐ |
| `ChargingStop` | `ChargingStop` (struct) | `ChargingStop` | 2 | ☐ |
| `ChargingActionDetails` | `ChargingActionDetails` (struct) | `ChargingActionDetails` | 2 | ☐ |
| `EVChargingPool` | `EVChargingPool` (struct) | `EVChargingPool` | 2 | ☐ |
| `EVChargingStation` | `EVChargingStation` (struct) | `EVChargingStation` | 2 | ☐ |
| `PostAction` | `PostAction` (struct) | `PostAction` | 2 | ☐ |
| `PreAction` | `PreAction` (struct) | `PreAction` | 2 | ☐ |

### Vehicle Specifications — Phase 2

| Android Class | iOS Struct | MAUI Type | Phase | Validated |
|---|---|---|---|---|
| `VehicleSpecification` + Builders | `VehicleSpecification` + Builders | `VehicleSpecification` | 2 | ☐ |
| `TransportSpecification` + Builders | `TransportSpecification` + Builders | `TransportSpecification` | 2 | ☐ |
| `CarSpecifications` | `CarSpecifications` (struct) | `CarSpecifications` | 2 | ☐ |
| `TruckSpecifications` | `TruckSpecifications` (struct) | `TruckSpecifications` | 2 | ☐ |
| `BusSpecifications` | `BusSpecifications` (struct) | `BusSpecifications` | 2 | ☐ |
| `ScooterSpecification` | `ScooterSpecification` (struct) | `ScooterSpecification` | 2 | ☐ |
| `TaxiSpecification` | `TaxiSpecification` (struct) | `TaxiSpecification` | 2 | ☐ |
| `PedestrianSpecification` | `PedestrianSpecification` (struct) | `PedestrianSpecification` | 2 | ☐ |
| `WeightPerAxleGroup` | `WeightPerAxleGroup` (struct) | `WeightPerAxleGroup` | 2 | ☐ |

### Routing Enums — Phase 2

| Enum | MAUI Type | Phase | Validated |
|---|---|---|---|
| `RoutingError` | `RoutingError` | 2 | ☐ |
| `ManeuverAction` | `ManeuverAction` | 2 | ☐ |
| `OptimizationMode` | `OptimizationMode` | 2 | ☐ |
| `IsolineCalculationMode` | `IsolineCalculationMode` | 2 | ☐ |
| `IsolineRangeType` | `IsolineRangeType` | 2 | ☐ |
| `SectionTransportMode` | `SectionTransportMode` | 2 | ☐ |
| `SectionNoticeCode` | `SectionNoticeCode` | 2 | ☐ |
| `NoticeSeverity` | `NoticeSeverity` | 2 | ☐ |
| `FunctionalRoadClass` | `FunctionalRoadClass` | 2 | ☐ |
| `RoadFeatures` | `RoadFeatures` | 2 | ☐ |
| `TunnelCategory` | `TunnelCategory` | 2 | ☐ |
| `HazardousMaterial` | `HazardousMaterial` | 2 | ☐ |
| `ChargingConnectorType` | `ChargingConnectorType` | 2 | ☐ |
| `ChargingSupplyType` | `ChargingSupplyType` | 2 | ☐ |
| `TransitMode` | `TransitMode` | 2 | ☐ |
| `TransitDepartureStatus` | `TransitDepartureStatus` | 2 | ☐ |
| `WaypointType` | `WaypointType` | 2 | ☐ |
| `AccessAttributes` | `AccessAttributes` | 2 | ☐ |
| `StreetAttributes` | `StreetAttributes` | 2 | ☐ |
| `TravelDirection` | `TravelDirection` | 2 | ☐ |
| `SideOfDestination` | `SideOfDestination` | 2 | ☐ |
| `MatchSideOfStreet` | `MatchSideOfStreet` | 2 | ☐ |
| `FarePriceType` | `FarePriceType` | 2 | ☐ |
| `FareReason` | `FareReason` | 2 | ☐ |
| `PaymentMethod` | `PaymentMethod` | 2 | ☐ |
| `ZoneCategory` | `ZoneCategory` | 2 | ☐ |
| `TruckType` | `TruckType` | 2 | ☐ |
| `VehicleType` | `VehicleType` | 2 | ☐ |

---

## Module: Search

### Android: `com.here.sdk.search` (51 classes, 7 interfaces)

### Key Classes — Phase 2

| Android Class | iOS Equivalent | MAUI Type | Phase | Validated |
|---|---|---|---|---|
| `SearchEngine` | `SearchEngine` (sealed) | `SearchEngine` | 2 | ☐ |
| `Place` | `Place` (sealed) | `Place` | 2 | ☐ |
| `PlaceCategory` | `PlaceCategory` (sealed) | `PlaceCategory` | 2 | ☐ |
| `Suggestion` | `Suggestion` (sealed) | `Suggestion` | 2 | ☐ |
| `IndexRange` | `IndexRange` (sealed) | `IndexRange` | 2 | ☐ |

### Query Types — Phase 2

| Android Class | iOS Struct | MAUI Type | Phase | Validated |
|---|---|---|---|---|
| `TextQuery` | `TextQuery` (struct) | `TextQuery` | 2 | ☐ |
| `CategoryQuery` | `CategoryQuery` (struct) | `CategoryQuery` | 2 | ☐ |
| `AddressQuery` | `AddressQuery` (struct) | `AddressQuery` | 2 | ☐ |
| `StructuredQuery` | `StructuredQuery` (struct) | `StructuredQuery` | 2 | ☐ |
| `PlaceIdQuery` | `PlaceIdQuery` (struct) | `PlaceIdQuery` | 2 | ☐ |
| `SearchOptions` | `SearchOptions` (struct) | `SearchOptions` | 2 | ☐ |
| `PlaceFilter` | `PlaceFilter` (struct) | `PlaceFilter` | 2 | ☐ |

### Place Details — Phase 2

| Android Class | iOS Struct | MAUI Type | Phase | Validated |
|---|---|---|---|---|
| `Address` | `Address` (struct) | `Address` | 2 | ☐ |
| `Details` | `Details` (struct) | `Details` | 2 | ☐ |
| `Contact` | `Contact` (struct) | `Contact` | 2 | ☐ |
| `BusinessDetails` | `BusinessDetails` (struct) | `BusinessDetails` | 2 | ☐ |
| `LocationDetails` | `LocationDetails` (struct) | `LocationDetails` | 2 | ☐ |
| `GeoPlace` | `GeoPlace` (struct) | `GeoPlace` | 2 | ☐ |
| `OpeningHours` | `OpeningHours` (struct) | `OpeningHours` | 2 | ☐ |
| `ResponseDetails` | `ResponseDetails` (struct) | `ResponseDetails` | 2 | ☐ |
| `SupplierReference` | `SupplierReference` (struct) | `SupplierReference` | 2 | ☐ |
| `WebDetails` | `WebDetails` (struct) | `WebDetails` | 2 | ☐ |
| `EVChargingPoolDetails` | `EVChargingPoolDetails` (struct) | `EVChargingPoolDetails` | 2 | ☐ |
| `FuelStation` | `FuelStation` (struct) | `FuelStation` | 2 | ☐ |

### Search Enums — Phase 2

| Enum | MAUI Type | Phase | Validated |
|---|---|---|---|
| `SearchError` | `SearchError` | 2 | ☐ |
| `SuggestionType` | `SuggestionType` | 2 | ☐ |
| `PlaceType` | `PlaceType` | 2 | ☐ |
| `AddressType` | `AddressType` | 2 | ☐ |
| `AreaType` | `AreaType` | 2 | ☐ |
| `HighlightType` | `HighlightType` | 2 | ☐ |
| `EVSEStatus` | `EVSEStatus` | 2 | ☐ |
| `EVAccessRestrictionReason` | `EVAccessRestrictionReason` | 2 | ☐ |
| `EVAccessType` | `EVAccessType` | 2 | ☐ |
| `PlaceSerializationError` | `PlaceSerializationError` | 2 | ☐ |
| `FuelAdditiveType` | `FuelAdditiveType` | 2 | ☐ |

> **Note**: `FuelType` belongs to the Transport module (`com.here.sdk.transport`), NOT Search.

---

## Module: Traffic

### Android: `com.here.sdk.traffic` (15 classes, 5 interfaces)

> **Validated 2026-04-14** against Android Javadoc and iOS Swift interface (v4.25.5.0).
> See Corrections Log below for discrepancies.

### Key Classes — Phase 3

| Android Class | iOS Equivalent | MAUI Type | Phase | Validated |
|---|---|---|---|---|
| `TrafficEngine` | `TrafficEngine` (sealed) | `TrafficEngine` | 3 | ☑ |
| `TrafficFlow` | `TrafficFlow` (sealed) | `TrafficFlow` | 3 | ☑ |
| `TrafficIncident` | `TrafficIncident` (sealed) | `TrafficIncident` | 3 | ☑ |
| `TrafficIncidentOnRoute` | `TrafficIncidentOnRoute` (sealed) | `TrafficIncidentOnRoute` | 3 | ☑ ⚠️ |
| `TrafficDataProvider` | `TrafficDataProvider` (sealed) | `TrafficDataProvider` | 3 | ☑ |

> **⚠️ TrafficIncidentOnRoute**: Android places this in `com.here.sdk.routing`, NOT `com.here.sdk.traffic`. iOS also has it under Routing. It conforms to `TrafficIncidentBase` (traffic) but lives in the routing package. The `id` property is `String?` (optional) on iOS vs `String` (non-null) on `TrafficIncident`.

### Traffic Structs — Phase 3

| iOS Struct | MAUI Type | Phase | Validated | NSObject Wrapper |
|---|---|---|---|---|
| `TrafficFlowQueryOptions` | `TrafficFlowQueryOptions` | 3 | ☑ | Yes |
| `TrafficIncidentsQueryOptions` | `TrafficIncidentsQueryOptions` | 3 | ☑ | Yes |
| `TrafficIncidentLookupOptions` | `TrafficIncidentLookupOptions` | 3 | ☑ | Yes |
| `TrafficLocation` | `TrafficLocation` | 3 | ☑ | Yes (contains GeoPolyline) |

### Traffic Enums — Phase 3

| Enum | MAUI Type | Phase | Validated | Values |
|---|---|---|---|---|
| `TrafficQueryError` | `TrafficQueryError` | 3 | ☑ | 19 |
| `TrafficIncidentImpact` | `TrafficIncidentImpact` | 3 | ☑ | 5 (Critical, Major, Minor, Low, Unknown) |
| `TrafficIncidentType` | `TrafficIncidentType` | 3 | ☑ | 12 |
| `Traversability` | `Traversability` | 3 | ☑ | 3 (Open, Closed, ReversibleNotRoutable) |
| `JunctionsTraversability` | `JunctionsTraversability` | 3 | ☑ | 5 (AllOpen, AllClosed, IntermediateClosedEdgeOpen, StartOpenOthersClosed, EndOpenOthersClosed) |
| `TrafficOptimizationMode` | `TrafficOptimizationMode` | 3 | ☑ ⚠️ | 3 (TimeDependent, LongTermClosuresOnly, Disabled) |

> **⚠️ TrafficOptimizationMode**: Android places this in `com.here.sdk.routing`, NOT `com.here.sdk.traffic`. It belongs with routing options, not the traffic engine.

### Traffic Callbacks — Phase 3 (MISSING from original catalog)

| Android Interface | iOS Type Alias | MAUI Type | Phase | Validated |
|---|---|---|---|---|
| `TrafficFlowQueryCallback` | `TrafficFlowQueryCompletionHandler` | `EventHandler<TrafficFlowQueryEventArgs>` | 3 | ☑ |
| `TrafficIncidentsQueryCallback` | `TrafficIncidentsQueryCompletionHandler` | `EventHandler<TrafficIncidentsQueryEventArgs>` | 3 | ☑ |
| `TrafficIncidentLookupCallback` | `TrafficIncidentCompletionHandler` | `EventHandler<TrafficIncidentLookupEventArgs>` | 3 | ☑ |

### Traffic Base Interfaces — Phase 3 (MISSING from original catalog)

| Android Interface | iOS Protocol | MAUI Type | Phase | Validated |
|---|---|---|---|---|
| `TrafficFlowBase` | `TrafficFlowBase` (AnyObject) | `ITrafficFlowBase` | 3 | ☑ |
| `TrafficIncidentBase` | `TrafficIncidentBase` (AnyObject) | `ITrafficIncidentBase` | 3 | ☑ |

### TrafficIncident Nested Types — Phase 3 (MISSING from original catalog)

| Type | Kind | MAUI Type | Phase | Validated | iOS Wrapper |
|---|---|---|---|---|---|
| `TrafficIncident.RestrictedVehicleCategory` | Enum (13 values) | `RestrictedVehicleCategory` | 3 | ☑ | NSInteger wrapper |
| `TrafficIncident.VehicleRestriction` | Class (27 fields) | `VehicleRestriction` | 3 | ☑ | NSObject wrapper |

### Traffic Validation Corrections

1. **Method naming**: Android `TrafficEngine` uses `queryForFlow`/`queryForIncidents` (not `queryFlow`/`queryIncidents`)
2. **TrafficIncidentOnRoute** is in `com.here.sdk.routing` on both platforms, not traffic
3. **TrafficOptimizationMode** is in `com.here.sdk.routing` on Android, not traffic
4. **TrafficDataProvider** has no public API surface — it is an opaque handle class
5. **TrafficFlow** extends `TrafficFlowBase` (provides `freeFlowSpeedInMetersPerSecond`, `jamFactor`); **TrafficIncident** extends `TrafficIncidentBase` (provides `impact`, `type`, `description`, `startTime`, `endTime`)
6. **TrafficFlow** properties on both platforms: `location`, `speedInMetersPerSecond?`, `speedUncappedInMetersPerSecond?`, `jamTendency?`, `confidence?`, `traversability?`, `junctionsTraversability?`, `freeFlowSpeedInMetersPerSecond`, `jamFactor`
7. **TrafficIncident** additional properties beyond base: `id`, `originalId`, `parentId?`, `junctionsTraversability`, `isRoadClosed`, `codes`, `summary`, `entryTime?`, `location`, `vehicleRestrictions`

---

## Module: Transport

### Android: `com.here.sdk.transport` (39 classes)

### Key Structs — Phase 2

| iOS Struct | MAUI Type | Phase | Validated |
|---|---|---|---|
| `TransportSpecification` + Builders | `TransportSpecification` | 2 | ☐ |
| `VehicleSpecification` + Builders | `VehicleSpecification` | 2 | ☐ |
| `CarSpecifications` | `CarSpecifications` | 2 | ☐ |
| `TruckSpecifications` | `TruckSpecifications` | 2 | ☐ |
| `BusSpecifications` | `BusSpecifications` | 2 | ☐ |
| `ScooterSpecification` | `ScooterSpecification` | 2 | ☐ |
| `TaxiSpecification` | `TaxiSpecification` | 2 | ☐ |
| `PedestrianSpecification` | `PedestrianSpecification` | 2 | ☐ |
| `GeneralVehicleSpeedLimits` | `GeneralVehicleSpeedLimits` | 2 | ☐ |
| `VehicleProfile` | `VehicleProfile` | 2 | ☐ |
| `WeightPerAxleGroup` | `WeightPerAxleGroup` | 2 | ☐ |

### Transport Enums — Phase 2

| Enum | MAUI Type | Phase | Validated |
|---|---|---|---|
| `TransportMode` | `TransportMode` | 2 | ☐ |
| `VehicleType` | `VehicleType` | 2 | ☐ |
| `TruckCategory` | `TruckCategory` | 2 | ☐ |
| `TruckClass` | `TruckClass` | 2 | ☐ |
| `TruckRoadType` | `TruckRoadType` | 2 | ☐ |
| `TruckFuelType` | `TruckFuelType` | 2 | ☐ |
| `TunnelCategory` | `TunnelCategory` | 2 | ☐ |
| `FuelType` | `FuelType` | 2 | ☐ |

---

## Module: Animation

### Android: `com.here.sdk.animation` (23 classes, 1 interface)

> **Validated 2026-04-14** against Android Javadoc and iOS Swift interface.

### Key Classes — Phase 1-3

| Android Class | iOS Equivalent | MAUI Type | Phase | Validated |
|---|---|---|---|---|
| `Easing` | `Easing` (sealed) | `Easing` | 3 | ☑ |
| `MapCameraAnimation` | `MapCameraAnimation` (sealed) | `MapCameraAnimation` | 1 | ☑ ⚠️ |
| `MapCameraKeyframeTrack` | `MapCameraKeyframeTrack` (sealed) | `MapCameraKeyframeTrack` | 3 | ☑ |
| `MapMarkerAnimation` | `MapMarkerAnimation` (sealed) | `MapMarkerAnimation` | 3 | ☑ |
| `MapPolylineAnimation` | `MapPolylineAnimation` (sealed) | `MapPolylineAnimation` | 3 | ☑ |

> **⚠️ MapCameraAnimation** is in `com.here.sdk.mapview` on Android, NOT `com.here.sdk.animation`. Only `MapMarkerAnimation` and `MapPolylineAnimation` are in the animation package.

### Phase 3 Validation — iOS Nested Types Requiring Wrappers

**Enums needing NSInteger wrappers (all UInt32-backed):**

| Nested Enum | Parent Type | Cases | MAUI Type |
|---|---|---|---|
| `Easing.InstantiationErrorCode` | Easing | 5 | `EasingInstantiationError` |
| `EasingFunction` (top-level) | — | 38 | `EasingFunction` |
| `KeyframeInterpolationMode` (top-level) | — | 3 (Step, Linear, Smooth) | `KeyframeInterpolationMode` |
| `MapMarker3DModel.InstantiationErrorCode` | MapMarker3DModel | 1 | `MapMarker3DModelError` |
| `MapMarkerAnimation.InstantiationErrorCode` | MapMarkerAnimation | 1 (IncompatibleTrack) | `MapMarkerAnimationError` |
| `MapPolylineAnimation.InstantiationErrorCode` | MapPolylineAnimation | 1 (IncompatibleTrack) | `MapPolylineAnimationError` |
| `MapItemKeyFrameTrack.InstantiationErrorCode` | MapItemKeyFrameTrack | 2 | `MapItemKeyFrameTrackError` |
| `MapCameraKeyframeTrack.InstantiationErrorCode` | MapCameraKeyframeTrack | 2 | `MapCameraKeyframeTrackError` |
| `MapPolyline.Representation.InstantiationErrorCode` | MapPolyline.Representation | 1 | `MapPolylineRepresentationError` |
| `MapSceneLights.Category` | MapSceneLights | 3 (Main, Back, Rim) | `MapSceneLightCategory` |
| `MapSceneLights.AttributeSettingError` | MapSceneLights | 1 (NoLights) | `MapSceneLightSettingError` |
| `MapContentSettings.TrafficRefreshPeriodErrorCode` | MapContentSettings | 2 | `TrafficRefreshPeriodError` |
| `MapContext.MemoryManagementStrategy` | MapContext | 2 (Fixed, Dynamic) | `MemoryManagementStrategy` |
| `MapContext.MemoryManagementResultCode` | MapContext | 5 | `MemoryManagementResultCode` |
| `MapContext.ResourceType` | MapContext | 1 (Memory) | `MapContextResourceType` |
| `MapContext.FreeResourceSeverity` | MapContext | 2 (Moderate, Critical) | `FreeResourceSeverity` |
| `MapLayerBuilder.InstantiationErrorCode` | MapLayerBuilder | 2 | `MapLayerBuilderError` |
| `JsonStyleFactory.InstantiationErrorCode` | JsonStyleFactory | 2 | `JsonStyleFactoryError` |
| `MapContentType` (top-level) | — | 4 | `MapContentType` |
| `LineCap` (top-level) | — | 3 (Round, Square, Butt) | `LineCap` |
| `DrawOrderType` (top-level) | — | 2 | `DrawOrderType` |
| `AnimationState` (top-level) | — | 3 (Started, Completed, Cancelled) | `AnimationState` |
| `RenderSize.Unit` (nested) | RenderSize | 3 (Pixels, DIP, Meters) | `RenderSizeUnit` |
| `MapMeasureDependentRenderSize.InstantiationErrorCode` | MapMeasureDependentRenderSize | 3 | — |

**Structs needing NSObject wrappers:**

| Nested Struct | Parent Type | Key Properties |
|---|---|---|
| `MapMarkerCluster.Grouping` | MapMarkerCluster | `markers`, `parent` |
| `MapMarkerCluster.ImageStyle` | MapMarkerCluster | `image`, `anchor` |
| `MapMarkerCluster.CounterStyle` | MapMarkerCluster | `textColor`, `fontSize`, `textAnchor`, `maxCountNumber`, `aboveMaxText` |
| `MapSceneLights.Direction` | MapSceneLights | `azimuth`, `altitude` |
| `MapContext.MemoryManagementResult` | MapContext | `diffBetweenVideoMemoryLimitAndRequirementInKiB?`, `resultCode` |
| `MapContext.MemoryManagementOptions` | MapContext | `memoryManagementStrategy`, `tileCacheMemoryLimitInKiB?`, `videoMemoryLimitInKiB?` |
| `MapLayerBuilder.InstantiationErrorDetails` | MapLayerBuilder | `errorCode`, `errorDescription?` |
| `JsonStyleFactory.InstantiationErrorDetails` | JsonStyleFactory | `errorCode`, `errorDescription?` |
| `ScalarKeyframe` | — | `value` (Double), `duration` (TimeInterval) |
| `GeoCoordinatesKeyframe` | — | `value` (GeoCoordinates), `duration` |
| `GeoOrientationKeyframe` | — | `value` (GeoOrientation), `duration` |
| `Point2DKeyframe` | — | `value` (Point2D), `duration` |
| `Anchor2DKeyframe` | — | `value` (Anchor2D), `duration` |

### Phase 3 Validation — Additional Types NOT in Original Catalog

These types were discovered during validation and need to be added for complete API coverage:

| Type | Kind | Package | Notes |
|---|---|---|---|
| `MapCameraAnimationFactory` | class | mapview | Static factory for `createAnimation` and `flyTo` — essential for camera animations |
| `MapItemRepresentation` | class | mapview | Abstract base for `MapPolyline.Representation` |
| `PointDataSourceBuilder` | class | mapview.datasource | Required to construct `PointDataSource` |
| `LineDataSourceBuilder` | class | mapview.datasource | Required to construct `LineDataSource` |
| `PolygonDataSourceBuilder` | class | mapview.datasource | Required to construct `PolygonDataSource` |
| `MapLayerPriority` | class | mapview | Used by `MapLayer`/`MapLayerBuilder` |
| `MapLayerPriorityBuilder` | class | mapview | Builder for `MapLayerPriority` |
| `MapLayerVisibilityRange` | class | mapview | Used by `MapLayerBuilder.withVisibilityRange` |
| `MapLayerMapMeasureDependentStorageLevels` | class | mapview | Storage level configuration |
| `QuadMeshBuilder` | class | mapview | Extends `MeshBuilder` for quad geometry |
| `TriangleMeshBuilder` | class | mapview | Extends `MeshBuilder` for triangle geometry |
| `MapMeasureRange` | struct | — | Used by visibility ranges |
| `MapMeasureDependentRenderSize` | struct | — | Used for line widths etc. |

### Phase 3 Validation — Deprecated APIs

| API | Deprecated | Replacement | Notes |
|---|---|---|---|
| `MapCameraKeyframeTrack.lookAtDistance(keyframes:easing:interpolationMode:)` | v4.27.0 | `lookAtDistance(ofKind:keyframes:easing:interpolationMode:)` | New version takes `MapMeasure.Kind` param |

### Phase 3 Validation — Beta APIs

These types/methods are marked BETA in the SDK and may change:
- `JsonStyleFactory`, `Style` — custom map styling
- `PointDataSource`, `LineDataSource`, `PolygonDataSource` — custom data layers
- `AssetsManager` — font registration
- `MapContext` memory management methods
- `MapLayer.setStyle()`, `MapLayerBuilder.withStyle()`
- `MapArrow.setMeasureDependentTailWidth()`
- `TrafficEngine.queryForFlow()` — traffic flow querying

---

## Module: Gestures

### Android: `com.here.sdk.gestures` (4 classes, 7 interfaces)

Covered in Maps module gesture delegates above.

---

## Platform-Specific Types

These types exist on one platform only and should be documented but may not appear in the unified API:

| Type | Platform | Notes |
|---|---|---|
| `AndroidManifest.xml` entries | Android only | Permissions, activities |
| `InitProvider` | Android only | ContentProvider for auto-init |
| `MapSurface` / `MapView` (Surface variant) | Android only | SurfaceView-based map |
| `HelloMapAndroidAuto` | Android only | Android Auto support |
| `HelloMapCarPlay` | iOS only | CarPlay support |
| `TransitRoutingEngine` | Both | But CarPlay/Android Auto are platform-specific UI |
| `Duration` (com.here.time) | Android only | Utility class |

## Validation Checklist

For each type in this catalog, the following must be verified BEFORE implementation:

1. [ ] Type exists in Android Javadoc (`tmp/android-inspect/api-reference/`)
2. [ ] Type exists in iOS Swift interface (`arm64-apple-ios.swiftinterface`)
3. [ ] Method signatures are equivalent between platforms
4. [ ] Return types map to the same unified type
5. [ ] Error/exception handling is consistent
6. [ ] Async patterns map correctly
7. [ ] Type is added to the correct phase milestone
8. [ ] iOS: `@objc` wrapper is defined in NativeBridge
9. [ ] Android: `Metadata.xml` entry is defined
10. [ ] MAUI: Unified model type is defined in `HereSdk.Explore.Maui`