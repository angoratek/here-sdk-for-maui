# Changelog

All notable changes to the HERE SDK for MAUI project.

## [4.25.5.0] — 2026-06-11

### Added
- Automated Android-emulator UI tests (Appium + UIAutomator2) covering the 4
  ref-app tabs (Explore, Directions, Traffic, Tools). Runs in CI on
  `ubuntu-latest` with the Android SDK.
- Per-package API coverage report (see `plan/gap-analysis.md` Appendix A).

### Changed
- Version bump to `4.25.5.0` (GA). `VersionSuffix` removed.

### Fixed
- `scripts/pack.sh`: replace non-portable `grep -oP` with portable `sed -n`
  so the script works on macOS without GNU grep.
- `DeviceTests` csproj: enable `<UseMaui>` / `<SingleProject>` and set
  `SupportedOSPlatformVersion=24.0` on Android so the test project links
  the MAUI wrapper assembly and matches the HERE SDK AAR's `minSdkVersion 24`.
- `DeviceTests`: add type-alias `using` directives to disambiguate the
  MAUI `Location`/`Contact` records from the generated binding types.

## [4.25.5.0-beta1] — 2026-05-20

### Added

- **Ref App**: 5-page reference app (Explore, Directions, Traffic, Tools, Settings) with dual-input search, route calculation with maneuvers, traffic flow/incidents, map object drawing tools, and dark mode support.
- **Design System**: Floating glass-morphic UI, `ErrorBanner` and `EmptyStateView` controls, `ConnectivityService` for offline detection, `PermissionsService` for location permission UX.
- **SDK Version Info**: `SdkInfo` static class reading from assembly metadata; version, build config, and platform displayed in Settings.
- **Configurable Defaults**: `MapDefaultsService` for configurable default map center via `appsettings.json`.
- **Unit Tests**: 225 tests covering models, converters, and service logic.
- **UI Tests**: 201 tests covering ViewModel commands, state transitions, error/empty states, control behavior, and integration flows.
- **Device Tests**: ~180 tests across 10 files (Android builds clean; iOS blocked by AOT compilation environment issue). Covers MapService, RoutingService, SearchService, TrafficService models and lifecycle, plus cross-platform parity.
- **CI/CD**: GitHub Actions workflows for build+test on push/PR (`build.yml`) and NuGet publish on version tag (`publish.yml`).

### Changed

- ViewModels decoupled from `IHereMapView`; map operations use `IMapService` directly.
- `BoolToColorConverter.ConvertBack` throws `NotSupportedException` (was `NotImplementedException`).
- `Route` record now includes `DurationText` computed property.

### Removed

- Orphaned `MapViewModel.cs` — no page or DI registration referenced it.
- Unused `SearchLocationInput` control — replaced by inline search in Explore and Directions pages.

### Fixed

- iOS protocol bindings: added `[Model]` attribute to prevent SIGSEGV in `objc_msgSendSuper`.
- Polyline/polygon drawing, panel toggle, style picker dropdown, and iOS screen-to-geo conversion in ref app.
- Removed stale `BoolToColorConverter` XAML reference that caused Android parse error.
- `ToggleDarkMode` no longer called twice on settings page load.

### Infrastructure

- Version bump to `4.25.5.0-beta1` with `VersionSuffix` property for CI injection.
- Build scripts updated from `net9.0` → `net10.0`.
- `pack.sh` reads version from `Version.props`, supports `--suffix`, outputs to clean `artifacts/`.
- `release.sh` one-shot pipeline: clean → build → test → pack → validate.
- `validate-nupkg.sh` verifies package structure, README, XML docs, and nuspec metadata.

---

## [4.25.5.0-alpha1] — 2026-04-26

### Added

- **Android Binding**: Full AAR binding with namespace remapping via `Metadata.xml`.
- **iOS NativeBridge**: Swift wrapper framework re-exposing 4 engines (Map, Search, Routing, Traffic) via `@objc` annotations.
- **iOS Binding**: Objective-Sharpie binding with `ApiDefinition.cs` and `StructsAndEnums.cs`.
- **MAUI Library**: Unified C# API with 58 of 361 types (~16%): `IMapService`, `IRoutingService`, `ISearchService`, `ITrafficService`, `ILocationService`.
- **MapView Control**: `HereMapView` with handler pattern, camera, gestures (tap, long-press, double-tap).
- **Map Items**: Markers (including 3D and clusters), polylines, polygons, arrows, circles (polygon-approximated via `CircleGeometryHelper`), location indicator.
- **Search**: Text search, category search, auto-suggest, place details with `Place`, `Address`, `Contact`, `OpeningHours` models.
- **Routing**: Car, truck, pedestrian, bicycle, scooter, bus, taxi, transit routing with waypoints, maneuvers, sections, and isolines.
- **Traffic**: Real-time flow and incident queries.
- **Location Service**: Cross-platform device location via `Microsoft.Maui.Devices.Sensors.Geolocation`.
- **Modern Ref App UI**: Floating search, routing form, map style picker, zoom and compass controls.
- Architecture: handler pattern, service pattern with `#if ANDROID` / `#if IOS` partial classes, platform converters, `IDisposable` native resource management.

### Infrastructure

- Centralized version management via `Version.props` and `Directory.Build.props`.
- `build.sh`, `test.sh`, `pack.sh`, `clean.sh` scripts.
- Objective-Sharpie pipeline for iOS binding generation.
- Mock JAR for Android testing.
