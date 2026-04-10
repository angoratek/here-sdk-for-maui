# Phased Delivery Plan

## Phase 0: Foundation

**Goal**: Solution scaffolding, build infrastructure, Android binding compiles, iOS NativeBridge project compiles.

### Tasks

- [x] **0.1** Initialize git repo with `.gitignore` (exclude `tmp/`, `bin/`, `obj/`, `.vs/`, `*.user`) ✅
- [x] **0.2** Create `global.json` pinning .NET 9 SDK version ✅
- [x] **0.3** Create `Directory.Build.props` with shared settings ✅
- [x] **0.4** Create solution file `HereSdk.Explore.Maui.sln` ✅
- [x] **0.5** Create `HereSdk.Explore.Android.Binding` project ✅
  - Copy AAR to `Jars/` ✅
  - Add `AndroidNamespaceReplacement` items ✅
  - Add empty `Transforms/Metadata.xml` ✅
  - Build and fix initial binding warnings ⏳ (needs dotnet SDK)
- [x] **0.6** Create `HereSdk.Explore.iOS.NativeBridge` Xcode project ✅
  - Create Swift wrapper stubs (HereSdkOptions, HereSdkEngine, HereGeoCoordinates, HereGeoBox, HereMapCamera) ✅
  - Build script created (`build-ios-native.sh`) ✅
  - Xcode project needs to be created via Xcode (requires GUI) ⏳
- [x] **0.7** Create `HereSdk.Explore.iOS.Binding` project ✅
  - Initial `ApiDefinition.cs` and `StructsAndEnums.cs` created (stubs) ✅
  - Sharpie run + fixups deferred to Phase 1 (requires xcframework) ⏳
- [x] **0.8** Create `HereSdk.Explore.Maui` project (multi-targeted class library) ✅
  - Core models, services (stubs), controls, handlers, extension method ✅
- [x] **0.9** Create `HereSdk.Explore.Maui.Tests` xUnit project ✅
  - GeoCoordinatesTests created ✅
- [x] **0.9b** Create `HereSdk.Explore.Maui.DeviceTests` project ✅
- [x] **0.10** Create `scripts/build.sh`, `scripts/clean.sh`, and other scripts ✅
- [x] **0.11** Create `CLAUDE.md` with project conventions ✅
- [x] **0.12** Create `README.md` with project overview ✅
- [x] **0.13** Create GitHub Actions skeleton (build.yml, publish.yml) ✅

### Acceptance Criteria

- [ ] `dotnet build` succeeds for Android binding project ⏳ (needs dotnet SDK installed)
- [ ] `dotnet build` succeeds for iOS binding project ⏳ (needs xcframework built first)
- [ ] Xcode project builds xcframework for device + simulator ⏳ (needs Xcode project creation)
- [ ] Solution builds end-to-end on macOS ⏳ (needs dotnet SDK)
- [ ] `dotnet test` runs (0 tests pass — infrastructure is ready) ⏳ (needs dotnet SDK)
- [x] Git repo initialized with clean history ✅
- [x] All project scaffolding files created ✅
- [x] AAR and mock JAR in place ✅
- [x] iOS NativeBridge Swift wrapper stubs created ✅
- [x] Build/test/pack scripts created ✅

### Estimated Duration: 1 week

---

## Phase 1: MapView + SDK Init

**Goal**: Display a map on both Android and iOS via the MAUI library. Initialize the HERE SDK. Camera control, basic gestures, map markers.

### Tasks

- [x] **1.1** Android Binding — Core module ✅ (scaffolded, Metadata.xml with internal class removals)
  - Bind `SDKNativeEngine`, `SDKOptions`, `Authentication`, `AuthenticationMode` ✅ (via AAR auto-binding)
  - Bind `GeoCoordinates`, `GeoBox`, `Point2D`, `Size2D` ✅ (via AAR auto-binding)
  - Bind `Location`, `LocationTime` ✅ (via AAR auto-binding)
  - Bind `SDKBuildInformation`, `SDKVersion`, `SDKLogger` ✅ (via AAR auto-binding)
  - Bind `InstantiationErrorCode`, `InstantiationErrorException` ✅ (via AAR auto-binding)
  - Add Metadata.xml entries for all above ✅ (comprehensive removals + event mappings)
  - Build verification ⏳ (needs dotnet SDK)
- [x] **1.2** Android Binding — Maps module (core) ✅ (scaffolded, will auto-bind from AAR)
  - Bind `MapView`, `MapViewBase`, `MapCamera`, `MapScene`, `MapScheme` ✅
  - Bind `MapCameraAnimationFactory`, `MapCameraUpdateFactory` ✅
  - Bind `MapCameraLimits`, `MapCameraAnimation` ✅
  - Bind `Gestures` + all gesture listeners ✅ (event mappings in Metadata.xml)
  - Bind `MapMarker`, `MapImage` ✅
  - Bind `LocationIndicator` ✅
  - Bind `MapPickResult`, `PickMapItemsResult` ✅
  - Bind `MapContentSettings`, `MapContext` ✅
  - Build verification ⏳ (needs dotnet SDK)
- [x] **1.3** iOS NativeBridge — Core module ✅
  - Wrap `SDKNativeEngine`, `SDKOptions` → `HereSdkEngine`, `HereSdkOptions` ✅
  - Wrap `GeoCoordinates`, `GeoBox` → ObjC classes ✅
  - Wrap `Point2D`, `Size2D`, `Location` → ObjC classes ✅
  - Wrap `InstantiationErrorCode` → NSInteger enum ✅
  - Xcode project creation + xcframework build ⏳ (needs XcodeGen or manual creation)
- [x] **1.4** iOS NativeBridge — Maps module (core) ✅
  - `MapView` is already ObjC-visible — referenced via `HereMapBridgeView` factory ✅
  - Wrap `MapCamera` → ObjC class with delegate ✅
  - Wrap `MapScene` → ObjC class with load callback ✅
  - Wrap `MapScheme` → NSInteger enum ✅ (all 15 values mapped)
  - Wrap `Gestures` + delegate protocols ✅ (tap, doubleTap, longPress)
  - Wrap `MapMarker` → ObjC class ✅
  - Sharpie + fix bindings ⏳ (needs xcframework build first)
- [x] **1.5** MAUI — Core models ✅
  - Define `HereSdkOptions`, `GeoCoordinates`, `GeoBox`, `Point2D`, `Size2D`, `Location` ✅
  - Define `GeoCoordinatesUpdate`, `GeoOrientationUpdate` ✅
  - Define `MapScheme`, `InstantiationErrorCode`, `LogLevel`, `UnitSystem`, `CachePolicy` enums ✅
  - Define `MapMarker`, `MapPolyline`, `MapPolygon`, `MapArrow` models ✅
  - Write unit tests ✅ (GeoCoordinates, GeoBox, GeoCircle, HereSdkOptions)
- [x] **1.6** MAUI — SDK Initialization ✅
  - Implement `HereSdk.Initialize(HereSdkOptions)` with platform dispatch ✅
  - Implement `HereSdk.Shutdown()` ✅
  - Implement `HereSdkExtensions.UseHereSdkExplore()` ✅
  - Write integration tests (requires device/emulator) ⏳
- [x] **1.7** MAUI — HereMapView control ✅
  - Define `HereMapView` virtual view with bindable properties ✅
  - Implement `HereMapViewHandler.Android.cs` ✅ (scaffold)
  - Implement `HereMapViewHandler.iOS.cs` ✅ (scaffold)
  - Register handler in library extension method ✅
  - Wire up: CameraTarget, MapScheme, MapTapped event ✅ (property mappers defined)
- [x] **1.8** MAUI — IMapService ✅
  - Define `IMapService` interface ✅
  - Implement `MapService.Android.cs` and `MapService.iOS.cs` ✅ (scaffolds)
  - Camera control, scene loading, markers, picking — method stubs in place ⏳ (actual impl needs build verification)
  - Write unit tests with mocked platform layer ✅ (MapServiceTests with NSubstitute)
- [x] **1.9** Ref App — HelloMap ✅
  - Create `HereSdk.Explore.Maui.RefApp` MAUI app ✅
  - Initialize HERE SDK in `MauiProgram.cs` ✅
  - `MainPage` with `HereMapView` ✅
  - Zoom to a default location ✅ (Berlin default in MapViewModel)
  - Add a map marker ⏳ (needs runtime verification)
  - Test on Android emulator + iOS simulator ⏳ (needs dotnet SDK)
- [x] **1.10** API Validation ✅
  - Verify every Phase 1 type against Android Javadoc and iOS Swift interface ✅
  - Cross-reference method signatures between platforms ✅ (key types verified)
  - Document any platform-specific behavior differences ✅ (iOS AuthenticationMode pattern documented)

### Acceptance Criteria

- [ ] Ref App displays a map on Android
- [ ] Ref App displays a map on iOS
- [ ] Camera can be moved programmatically
- [ ] Map scheme can be changed (day/night)
- [ ] Map markers can be added/removed
- [ ] Tap gesture is detected
- [ ] SDK initializes with credentials on both platforms
- [ ] All Phase 1 unit tests pass
- [ ] All Phase 1 API entries in catalog are validated ☑

### Estimated Duration: 2 weeks

---

## Phase 2: Search + Routing

**Goal**: Full search and routing functionality across both platforms.

### Tasks

- [x] **2.1** Android Binding — Search module ✅ (auto-bound from AAR + Metadata.xml entries)
- [x] **2.2** Android Binding — Routing module ✅ (auto-bound from AAR + Metadata.xml section/class renames)
- [x] **2.3** Android Binding — Transport module ✅ (auto-bound from AAR)
- [x] **2.4** iOS NativeBridge — Search module ✅
  - Wrap `SearchEngine` → `HereSearchEngine` ✅
  - Wrap `Place` → `HerePlace` ✅
  - Wrap `Suggestion` → `HereSuggestion` ✅
- [x] **2.5** iOS NativeBridge — Routing module ✅
  - Wrap `RoutingEngine` → `HereRoutingEngine` ✅
  - Wrap `Waypoint` → `HereWaypoint` ✅
  - Wrap `Route` → `HereRoute` ✅
  - Wrap `RoutingOptions` → `HereRoutingOptions` ✅
- [x] **2.6** MAUI — Search models + service ✅
  - Define `TextQuery`, `SearchOptions`, `PlaceFilter`, etc. ✅
  - Define `Place`, `Suggestion`, `Address`, etc. ✅
  - Define `ISearchService` with platform dispatch ✅
  - Write unit tests with mock platform layer ✅
- [x] **2.7** MAUI — Routing models + service ✅
  - Define `Waypoint`, `RoutingOptions`, etc. ✅
  - Define `Route`, `Section`, `Maneuver`, etc. ✅
  - Define `IRoutingService` with platform dispatch ✅
  - Write unit tests with mock platform layer ✅
- [x] **2.8** Ref App — Search page ✅
  - SearchViewModel + SearchPage XAML ✅
  - Text search integration ⏳ (needs runtime)
- [x] **2.9** Ref App — Routing page ✅
  - RoutingViewModel + RoutingPage XAML ✅
  - Calculate route integration ⏳ (needs runtime)
- [x] **2.10** API Validation ✅
  - Search and Routing types verified against both API references ✅

### Acceptance Criteria

- [ ] Text search returns results on both platforms
- [ ] Auto-suggest works
- [ ] Route calculation works for car/pedestrian/truck
- [ ] Route polyline is displayed on map
- [ ] Maneuver instructions are accessible
- [ ] Isoline calculation works
- [ ] Transit routing works
- [ ] All Phase 2 unit tests pass
- [ ] All Phase 2 API entries in catalog are validated ☑

### Estimated Duration: 2 weeks

---

## Phase 3: Traffic + Advanced Map Features

**Goal**: Traffic visualization, advanced map items, custom layers, animations, gestures.

### Tasks

- [x] **3.1** Android Binding — Traffic module ✅
  - Traffic module types auto-bound from AAR
  - Metadata.xml entries for TrafficFlow/TrafficIncidents callbacks and event mappings ✅
  - TrafficFlow → TrafficFlowData rename to avoid collision ✅
  - Build verification ⏳ (needs dotnet SDK)
- [x] **3.2** Android Binding — Maps module (advanced) ✅ (partially scaffolded)
  - `MapPolyline`, `MapPolygon`, `MapArrow` auto-bound from AAR ✅
  - Metadata.xml entries for Representation inner class renames ✅
  - `MapMarker3D`, `MapMarkerCluster`, animations — deferred to Phase 4 (advanced features)
  - Custom layer types — deferred to Phase 4
- [x] **3.3** iOS NativeBridge — Traffic module ✅
  - `HereTrafficEngine`, `HereTrafficFlow`, `HereTrafficIncident` wrappers ✅
  - Build, Sharpie, fix ⏳ (needs xcframework build)
- [x] **3.4** iOS NativeBridge — Maps module (advanced) ✅ (partially scaffolded)
  - `HereMapPolyline`, `HereMapPolygon`, `HereMapArrow` wrappers ✅
  - `HereGeoPolyline`, `HereGeoPolygon` primitives ✅
  - MapMarker3D, animations — deferred to Phase 4
  - Build, Sharpie, fix ⏳ (needs xcframework build)
- [x] **3.5** MAUI — Traffic service + models ✅
  - `ITrafficService` interface ✅
  - `TrafficService` with platform dispatch (Android + iOS) ✅
  - Traffic models: `TrafficFlow`, `TrafficIncident`, `TrafficFlowResult`, `TrafficIncidentsResult` ✅
  - Traffic query options ✅
  - Unit tests ✅
  - Type consolidation — moved from service interfaces to model files ✅
- [x] **3.6** MAUI — Advanced map features ✅ (partially scaffolded)
  - `MapPolyline`, `MapPolygon`, `MapArrow` in `IMapService` ✅
  - Model types moved to `Models/Maps/MapModels.cs` ✅
  - Android `MapService` implementation with item tracking dictionaries ✅
  - iOS `MapService` implementation with NativeBridge wrappers ✅
  - `MapMarker3D`, `MapMarkerCluster` — deferred to Phase 4
  - Custom layer support — deferred to Phase 4
  - Animation APIs — deferred to Phase 4
- [x] **3.7** Ref App — Traffic page ✅
  - Traffic flow querying and display ✅
  - Traffic incident querying and display ✅
- [x] **3.8** Ref App — Advanced map features ✅ (partially scaffolded)
  - MapItemsPage with polyline/polygon/arrow/marker buttons ✅
  - MapItemsViewModel ✅
  - 3D markers, clustering, animations — deferred to Phase 4
- [ ] **3.9** API Validation
  - Verify every Phase 3 type against both API references ⏳
  - Cross-reference all traffic types
  - Verify all advanced map types match between platforms

### Acceptance Criteria

- [ ] Traffic flow query returns results
- [ ] Traffic incidents are displayed on map
- [ ] Custom polylines and polygons render
- [ ] 3D markers display
- [ ] Custom map styles load
- [ ] Camera animations play
- [ ] Custom tile sources render
- [ ] All Phase 3 unit tests pass
- [ ] All Phase 3 API entries in catalog are validated ☑

### Estimated Duration: 2 weeks

---

## Phase 4: Polish + NuGet + CI/CD

**Goal**: Complete API coverage, NuGet packaging, CI/CD, documentation.

### Tasks

- [ ] **4.1** Full API coverage audit
  - Cross-reference every type in `05-api-surface-catalog.md` against implementation
  - Identify any missing types or methods
  - Fill gaps
- [x] **4.2** Performance optimization
  - Platform service implementations use item tracking dictionaries for O(1) add/remove ✅
  - Async patterns use TaskCompletionSource (no allocations in hot paths) ✅
  - Profile memory usage on both platforms ⏳ (needs runtime)
  - Test with large route calculations and many map markers ⏳ (needs runtime)
- [x] **4.3** Memory management ✅
  - `IDisposable` on all service interfaces and implementations ✅
  - `IHereSdkService` extends `IDisposable` ✅
  - All shared service partials implement `Dispose(bool)` pattern ✅
  - Ensure proper cleanup of native resources ⏳ (needs runtime)
  - Test for retain cycles (especially iOS) ⏳ (needs runtime)
  - Verify `SDKNativeEngine` cleanup ⏳ (needs runtime)
- [x] **4.4** NuGet packaging ✅
  - `PackageId`, `Version`, `Description` in all `.csproj` files ✅
  - `Directory.Build.props` defines `HereSdkVersion` and `PackageVersion` ✅
  - MAUI library has `GenerateDocumentationFile`, `PackageReadmeFile`, `PackageTags` ✅
  - Test `dotnet pack` for all 3 library projects ⏳ (needs runtime)
  - Verify packages contain correct platform-specific libs ⏳ (needs runtime)
- [ ] **4.5** XML documentation
  - Add `<summary>` docs to all public API surface ✅ (interfaces and models already have docs)
  - Add `<remarks>` for platform-specific behavior ⏳
  - Add `<example>` for key scenarios ⏳
  - Generate docs with `dotnet docfx` ⏳
- [x] **4.6** CI/CD finalization ✅
  - GitHub Actions: build + test on PR ✅ (build.yml with unit-tests, Android, iOS jobs)
  - GitHub Actions: pack + publish on tag push ✅ (publish.yml)
  - iOS binding regeneration workflow ⏳ (needs Sharpie setup)
  - Android binding regeneration workflow ⏳ (needs AAR update automation)
  - Test CI on a fresh clone ⏳ (needs dotnet SDK)
- [ ] **4.7** Final validation
  - Complete every checkbox in `05-api-surface-catalog.md` ⏳
  - Run all tests on physical devices (Android + iOS) ⏳
  - Test on multiple OS versions (Android API 24-35, iOS 15-18) ⏳
  - Performance benchmarks ⏳

### Acceptance Criteria

- [ ] All types in catalog are implemented and validated
- [ ] NuGet packages can be consumed in a fresh project
- [ ] CI/CD pipeline is green
- [ ] XML docs cover 100% of public API
- [ ] No memory leaks detected
- [ ] Tests pass on physical devices

### Estimated Duration: 1-2 weeks