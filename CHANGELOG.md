# Changelog

All notable changes to the HERE SDK for MAUI project.

## [Unreleased]

### Added
- `LICENSE` (MIT) at repo root so the GitHub license detector and NuGet packages resolve `<PackageLicenseExpression>MIT</PackageLicenseExpression>`.
- `SECURITY.md` with responsible-disclosure process (`security@angoratek.com`, 5-day acknowledgement, 90-day disclosure window).
- `CONTRIBUTING.md` covering setup, test matrix, commit style, PR workflow.
- `.editorconfig` enforcing 4-space indent, 120-char line length, file-scoped C# namespaces.
- `.github/CODEOWNERS` auto-assigning review to `@angoratek/maintainers`.
- `.github/workflows/changelog.yml` + `scripts/update-changelog.js` — auto-update `## [Unreleased]` on merged PRs by parsing conventional-commit prefixes.
- `.github/workflows/release-changelog.yml` + `scripts/release-changelog.js` — promote `## [Unreleased]` to a dated release entry on `v*` tag push.
- `bool IsInitialized` property on `SearchService`, `RoutingService`, `TrafficService` (read by integration tests to assert the DI factory wired the native engine).
- `tests/HereSdk.Explore.Maui.Tests/Services/SearchServiceLifecycleTests.cs` — locks the "not initialized" error message and idempotent disposal contract on the no-device stub.
- `tests/HereSdk.Explore.Maui.Tests/Services/SearchServiceIntegrationTests.cs` + `RoutingServiceIntegrationTests.cs` + `TrafficServiceIntegrationTests.cs` — verify `UseHereSdkExplore` registers initialized services as singletons.
- `tests/HereSdk.Explore.Maui.UITests/PageObjects/ExplorePageSearchTests.cs` (4 tests), `DirectionsPageRouteTests.cs` (1), `TrafficPageFlowTests.cs` (1) — end-to-end Appium regression net for the "service not initialized" bug on the real RefApp.

### Changed
- README NuGet badge bumped from `4.25.5.0-beta1` to `4.25.5.0` (GA).
- `UseHereSdkExplore` now registers `IRoutingService`, `ISearchService`, `ITrafficService` via a `Func<IServiceProvider, TService>` factory lambda that calls the partial `Initialize()` at first resolution; the previous direct `AddSingleton<I, T>()` left the native engine field null and every operation threw `InvalidOperationException("XxxService not initialized.")`.
- RefApp `MauiProgram.cs` no longer re-registers the three services with `AddSingleton<I, T>()` (which would have overridden the SDK's factory with one that skipped `Initialize()`).

### Fixed
- iOS RefApp `SupportedOSPlatformVersion` aligned with `Info.plist` `MinimumOSVersion=15.2` (was implicitly inheriting 24, causing `MT5210` warnings).
- `src/HereSdk.Explore.Maui.RefApp/appsettings.json` now committed with placeholder credentials; real credentials live only in gitignored `appsettings.Local.json`.
- `HereSdk.Explore.Maui.sln`: removed orphaned `UITests.Android.csproj` reference; added the actual `HereSdk.Explore.Maui.UITests.csproj`.
- `build.yml` MAUI Android job moved from `windows-latest` to `ubuntu-latest` (Windows runners cannot install the `maui-android` workload).
- `build.yml` and `publish.yml` now invoke `./scripts/build-ios-native.sh` instead of the non-existent `src/HereSdk.Explore.iOS.NativeBridge/build-xcframework.sh`.
- Appium UI tests connect to the helper server and detect the xcframework as a directory.
- Android UI tests now run green against a local emulator (9/9 passing).
- `scripts/release.sh` no longer silently swallows the `clean.sh` failure with `2>/dev/null`; the script now reports if `clean.sh` is missing instead of masking its real exit code.
- `ConfigLoadingTests` updated: the committed `appsettings.json` ships with placeholder credentials, so the test now asserts the placeholder values are present (previously asserted they were *not* present, which contradicted the security fix in `6d9398b`).
- `Controls/CategoryChipBar.xaml.cs` — replaced placeholder category IDs (`"restaurant"`, `"hotel"`, …) with real HERE Place Category taxonomy codes (`"100-1000"`, `"500-5000"`, …). The HERE Places API rejected the friendly names with `400 Illegal input for parameter 'categories'`, causing all category chip searches to fail with empty results even after the "not initialized" fix.

- docs site sidebar navigation (toc.json for all pages) (@angoratek) [#5](https://github.com/angoratek/here-sdk-for-maui/pull/5)
## [4.25.5.0] — 2026-06-11

### Added
- Automated Android-emulator UI tests (Appium + UIAutomator2) covering the 4
  ref-app tabs (Explore, Directions, Traffic, Tools). Runs in CI on
  `ubuntu-latest` with the Android SDK.
- Per-package API coverage report (see the PLAN.md backlog — `plan/gap-analysis.md` Appendix A was consolidated into PLAN.md).

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
