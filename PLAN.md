# HERE SDK for MAUI — Release Plan

> Generated 2026-05-13 from full codebase audit.
> Previous plan archived as historical reference; this is the active release plan.

## Current State

| Area | Status |
|------|--------|
| Android binding | Full AAR binding, all services functional |
| iOS NativeBridge | xcframework built, 4 engines exposed (Map, Search, Routing, Traffic) |
| MAUI library | 58 of 361 API types (~16%), core services complete |
| Ref app | 5 pages, 6 VMs, 7 controls, 7 converters — functional, lacking error/empty states |
| Unit tests | 225 passing |
| UI tests | 52 passing (ViewModel commands + state transitions) |
| Device tests | 13 placeholder skeletons (`Assert.True(true)`) |
| Version | 4.25.5.0-alpha1 (generated nuspecs show beta2 — stale mismatch) |
| Docs | README.md, docs/getting-started.md, XML doc comments on public API |
| CI/CD | None |
| Build scripts | build.sh/test.sh reference net9.0 (stale) |

---

## Phase 1: Ref App Polish & Richness → alpha2

### 1.1 App Identity
- [x] Add `MauiIcon` — simple HERE pin SVG in `Resources/AppIcon/`
- [x] Add `MauiSplashScreen` — HERE branding SVG in `Resources/Splash/`
- [x] Set `ApplicationTitle` and `ApplicationId` consistently in csproj

### 1.2 Error & Empty States
- [x] Create `ErrorBanner` control: icon + message + optional retry button, bindable `ErrorMessage` and `RetryCommand`
- [x] Create `EmptyStateView` control: icon + title + subtitle, bindable properties
- [x] Wire error states into:
  - ExplorePage — search failure banner
  - DirectionsPage — route calculation failure, isoline failure
  - TrafficPage — flow/incidents query failure
- [x] Wire empty states into:
  - ExplorePage — "No places found" after search
  - DirectionsPage — "No routes found"
  - TrafficPage — "No traffic data for this area"
- [x] Add `ConnectivityService` wrapping `Microsoft.Maui.Networking.Connectivity` — show offline banner in all pages
- [x] **UI tests**: ErrorBanner visibility on service throw, EmptyState visibility on null results, offline banner on no connectivity

### 1.3 Dead Code Removal
- [x] Delete orphaned `MapViewModel.cs` — no page uses it, not registered in DI
- [x] Delete unused `SearchLocationInput` control — both ExplorePage and DirectionsPage use inline search
- [x] Fix `BoolToColorConverter.ConvertBack` — `NotImplementedException` → `NotSupportedException`
- [x] Align all ConvertBack methods to throw `NotSupportedException` consistently

### 1.4 Hardcoded Values → Configuration
- [x] Create `SdkInfo` static class in MAUI library reading from assembly metadata
- [x] Reference `SdkInfo.Version` in both `ToolsViewModel:74` and `SettingsViewModel:42`
- [x] Make default map center configurable via `appsettings.json` (`DefaultCenter` Lat/Lng)

### 1.5 Location Permissions UX
- [x] Create `IPermissionsService` — wraps `Microsoft.Maui.ApplicationModel.Permissions`
- [x] Show rationale dialog before `CheckAndRequestAsync<Permissions.LocationWhenInUse>` — in PermissionsService.RequestLocationPermissionAsync
- [x] Show non-blocking banner when location permission is denied — ExploreViewModel sets SearchErrorMessage when denied
- [x] Added location permission declarations to both iOS Info.plist and Android AndroidManifest.xml (fixes geocoder crash)
- [x] Updated ExploreViewModel.CenterOnLocation to request permissions before calling LocationService

### 1.6 Settings Page Completion
- [x] Settings accessible via ToolsPage toolbar (existing pattern)
- [x] Display SDK version from `SdkInfo` (not hardcoded literal)
- [x] Show build config (Debug/Release) and platform in SettingsPage
- [x] Add "Send Feedback" link (mailto:support@here.com)
- [x] Added `BuildConfig`, `Platform`, `SendFeedbackCommand` to SettingsViewModel

---

## Phase 2: UI Test Coverage Expansion → beta1

### 2.1 Control Tests (new files)
- [ ] `BottomSheetTests` — Collapsed→HalfExpanded→FullyExpanded transitions, snap thresholds, drag gesture
- [ ] `CategoryChipBarTests` — chip selection fires event, scrollable layout
- [ ] `TransportModePickerTests` — mode selection, default state, all 8 modes present
- [ ] `PlaceCardTests` — title/address/phone/web visibility, open/closed color, distance formatting
- [ ] `MapStylePickerTests` — scheme change fires event, dropdown open/close toggle

### 2.2 Integration Flow Tests (new files)
- [ ] `ExploreFlowTests` — SearchQuery → suggestions populate → SelectSuggestion → PlaceCard visible → NavigateToDirections
- [ ] `DirectionsFlowTests` — Origin/Destination set → CalculateRoute → Route sheet populated with maneuvers
- [ ] `TrafficFlowTests` — ToggleFlow → polylines rendered → ToggleIncidents → markers placed → SelectIncident → detail shown
- [ ] `ToolsFlowTests` — SetDrawingTool(Marker) → tap → marker placed → ClearAll → count zero

### 2.3 Edge Case Tests
- [ ] Rapid search input — debounce cancellation, only last query executes
- [ ] Route calculation with zero results — error message set, no crash
- [ ] Traffic query with empty response — empty state visible
- [ ] Map service null during command — command handles gracefully, no NRE
- [ ] Concurrent SubmitSearch calls — only latest result displayed

---

## Phase 3: Device Tests → beta1

### 3.1 Android Device Tests
- [ ] `MapViewAndroidTests` — real `MapView` instantiation, camera target set/get roundtrip, zoom level
- [ ] `MapServiceAndroidTests` — scene load `NormalDay`/`SatelliteDay`, marker add/remove, polyline add/remove, polygon add/remove, circle add/remove
- [ ] `RoutingServiceAndroidTests` — real route SF→Oakland, verify sections/maneuvers populated, isoline generation, traffic-on-route
- [ ] `SearchServiceAndroidTests` — text search "restaurant", category search, suggest, getPlaceById
- [ ] `TrafficServiceAndroidTests` — query flow, query incidents, lookup incident detail

### 3.2 iOS Device Tests
- [ ] `MapViewiOSTests` — map view creation via NativeBridge, camera target, zoom
- [ ] `MapServiceiOSTests` — scene loading, marker/polyline/polygon/circle add/remove
- [ ] `RoutingServiceiOSTests` — route calculation (isolines marked `[Fact(Skip=...)]` until NativeBridge updated)
- [ ] `SearchServiceiOSTests` — text search, category search, suggest, getPlaceById

### 3.3 Cross-Platform Parity Tests
- [ ] Same route query produces consistent section count across Android/iOS
- [ ] Same search query produces consistent result count across Android/iOS
- [ ] Map object rendering parity (marker anchor, polyline width, polygon fill, circle radius)

---

## Phase 4: NuGet Packaging & Release Infrastructure → beta2

### 4.1 Version Normalization
- [ ] Bump `PackageVersion` in `Version.props` to `4.25.5.0-beta1`
- [ ] Add `VersionSuffix` property for CI injection
- [ ] Verify all 3 csprojs read `$(PackageVersion)` correctly

### 4.2 Package Metadata
- [ ] Add `PackageLicenseExpression` — `MIT` — to all 3 library csprojs
- [ ] Add 128×128 `PackageIcon` PNG to each project
- [ ] Add `<Description>` taglines: "HERE SDK Explore v4.25.5.0 — Android Binding / iOS Binding / Cross-Platform MAUI API"
- [ ] Add `<PackageReleaseNotes>` with link to CHANGELOG.md
- [ ] Verify `PackageReadmeFile` = `README.md` in all packages
- [ ] Add `<PackageTags>` for NuGet.org discoverability

### 4.3 Build Scripts Overhaul
- [ ] Update `build.sh` — replace net9.0 → net10.0, add error handling, add artifact output summary
- [ ] Update `test.sh` — replace net9.0 → net10.0, run all 3 test projects, fail on any failure
- [ ] Update `pack.sh` — read version from Version.props, accept `--suffix` flag, output clean `artifacts/` directory
- [ ] Add `scripts/release.sh` — one-shot: clean → build → test → pack → validate
- [ ] Add `scripts/validate-nupkg.sh` — verify package structure, dependencies, XML doc inclusion, signing

### 4.4 CI/CD
- [ ] Create `.github/workflows/ci.yml` — build + test on push/PR (ubuntu + macOS runners)
- [ ] Create `.github/workflows/release.yml` — pack + publish on version tag
- [ ] Create `NuGet.config` with source feeds
- [ ] Add CI/CD status badges to README.md

### 4.5 CHANGELOG
- [ ] Create `CHANGELOG.md` — semantic versioning, breaking changes, additions, fixes
- [ ] Write entries for all versions since project inception
- [ ] Write `4.25.5.0-beta1` entry

---

## Phase 5: API Documentation Generation → rc1

### 5.1 XML Doc Audit & Fix
- [ ] Remove `<NoWarn>CS1591</NoWarn>` from MAUI library csproj
- [ ] Fix all missing XML doc warnings — every public type, method, property, event
- [ ] Add `<exception>` docs to all async methods
- [ ] Add `<remarks>` with code snippets to key service methods (`SearchAsync`, `CalculateRouteAsync`, `QueryFlowAsync`)
- [ ] Verify XML doc file is produced in all package output directories

### 5.2 DocFX Pipeline
- [ ] Add `docfx.json` at repo root — source the 3 csproj XML doc outputs + conceptual markdown
- [ ] Create `docs/toc.yml` — structured navigation: Getting Started → Architecture → Services → Map Objects → API Reference
- [ ] Create conceptual docs:
  - `docs/architecture.md` — handler pattern, service pattern, platform converters, memory model
  - `docs/services.md` — IMapService, IRoutingService, ISearchService, ITrafficService, ILocationService
  - `docs/map-objects.md` — markers, polylines, polygons, circles, arrows, 3D markers, clusters, location indicator
  - `docs/platform-differences.md` — Android-only vs iOS-only features, workaround guidance
  - `docs/initialization.md` — HereSdk.Initialize(), credentials, logging, options
- [ ] Create `scripts/generate-docs.sh` — runs DocFX build, outputs to `artifacts/docs/`
- [ ] Add GitHub Actions step to publish docs to GitHub Pages on release

### 5.3 Living Ref App Documentation
- [ ] Create how-to guides extracted from ref app patterns:
  - `docs/how-to-search.md` — from ExplorePage flow
  - `docs/how-to-route.md` — from DirectionsPage flow
  - `docs/how-to-traffic.md` — from TrafficPage flow
  - `docs/how-to-draw.md` — from ToolsPage drawing tools
- [ ] Add inline comments in ref app XAML highlighting key SDK integration points

### 5.4 API Reference Structure
- [ ] Namespace index pages: `Here.Explore.Maui`, `Here.Explore.Maui.Models`, `Here.Explore.Maui.Models.Maps`, `.Routing`, `.Search`, `.Traffic`, `Here.Explore.Maui.Services`, `Here.Explore.Maui.Controls`
- [ ] Per-type pages — inheritance, properties, methods, events, remarks
- [ ] Platform compatibility badges: Both / Android-only / iOS-only
- [ ] Cross-reference links between interfaces and implementations

---

## Phase 6: Final Release → 4.25.5.0 GA

### 6.1 Pre-Release Gate
- [ ] All tests green: 225+ unit, 80+ UI, 30+ device
- [ ] Zero-error build on clean checkout for all platforms
- [ ] Smoke test ref app on Android emulator + iOS simulator — all 4 tabs functional
- [ ] Smoke test on physical Android device + iPhone if available
- [ ] API surface diff vs HERE SDK 4.25.5.0 official docs — no missing documented types
- [ ] All breaking changes documented in CHANGELOG.md
- [ ] README.md reviewed end-to-end for accuracy
- [ ] docs/ fully built and browsable locally

### 6.2 Release Artifacts
- [ ] `HereSdk.Explore.Android.Binding.4.25.5.0.nupkg`
- [ ] `HereSdk.Explore.iOS.Binding.4.25.5.0.nupkg`
- [ ] `HereSdk.Explore.Maui.4.25.5.0.nupkg`
- [ ] Static HTML API documentation
- [ ] Ref app source + APK/IPA builds for dogfooding

### 6.3 Post-Release
- [ ] Git tag `v4.25.5.0`
- [ ] Push packages to NuGet.org (or HERE internal feed, per legal guidance)
- [ ] Publish docs to GitHub Pages
- [ ] Draft release notes / announcement
- [ ] Create `develop` branch for post-GA iteration

---

## Risk Register

| Risk | Impact | Mitigation |
|------|--------|------------|
| iOS NativeBridge missing geocoding, isoline, traffic-on-route | High | Document as v1 limitations; scope NativeBridge v2 |
| Only 16% API surface (58/361 types) | Medium | Position as "SDK Essentials" release; catalog remainder for roadmap |
| HERE SDK redistribution terms | High | Verify EULA with HERE legal before public NuGet.org push |
| Device tests need CI emulator/simulator runners | Medium | macOS GitHub Actions runner for iOS; ubuntu + Android SDK for Android |
| HERE SDK 4.26+ breaking changes | Low | Pin to 4.25.5.0 for GA; plan migration separately |

---

## Execution Order

```
Phase 1 (Ref App Polish)
    ↓
Phase 2 (UI Test Coverage)
    ↓
Phase 3 (Device Tests) ──→ Phase 4 (Packaging/CI)
                              ↓
                          Phase 5 (Documentation)
                              ↓
                          Phase 6 (GA Release)
```

## Effort Estimate

| Phase | Est. Days | Milestone |
|-------|-----------|-----------|
| 1: Ref App Polish | 2-3 | alpha2 |
| 2: UI Test Coverage | 2-3 | beta1 |
| 3: Device Tests | 3-4 | beta1 |
| 4: Packaging & CI | 3-4 | beta2 |
| 5: Documentation | 4-5 | rc1 |
| 6: Final Release | 2-3 | 4.25.5.0 GA |
| **Total** | **16-22 days** | |
