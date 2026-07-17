# HERE SDK for MAUI — Release Plan

> Last refreshed 2026-07-17 against `main @ b0f3884`. Phase 6 GA is shipped;
> the active backlog is Phase 7 (RefApp UX fixes) and Phase 8 (UI test coverage
> expansion). Earlier completed phases are summarized, not in full detail.

## Current State

| Area | Status |
|------|--------|
| Android binding | Full AAR binding, all services functional |
| iOS NativeBridge | xcframework built, 4 engines exposed (Map, Search, Routing, Traffic) |
| MAUI library | 58 of 361 cross-platform API types (~16%), core services complete |
| Ref app | 5 pages, 5 VMs (+ViewModelBase), 7 controls, 7 converters — functional but has known UX gaps (Phase 7) |
| Unit tests | 253 passing |
| RefApp UI tests | 201 passing (ViewModel commands + state transitions + error/empty states) |
| Appium smoke | 15 NUnit tests on Android emulator (basic + "not initialized" regression net) |
| Device tests | ~180 tests across 10 files (Android builds clean, iOS blocked by AOT/env) |
| Version | 4.25.5.0 GA (generated nuspecs consistent) |
| Docs | README.md, docs/getting-started.md, CHANGELOG.md, XML doc comments on public API, DocFX site |
| CI/CD | build.yml (push/PR), publish.yml (version tag), ui-tests-android.yml (Appium) + changelog automation |
| Build scripts | build.sh/test.sh/pack.sh target net10.0; release.sh + validate-nupkg.sh + download-sdk.sh added |

### Known structural debt (now in scope of Phase 7)

1. **Traffic flow polylines never render** — `TrafficService.Android.cs:114-116` constructs `new GeoPolyline(new List<GeoCoordinates>())` (empty list). The Android binding exposes `f.Location.Polyline` but the code discards it. Every flow has 0 vertices, so `TrafficViewModel.cs:126` (`if (flow.Geometry is { Vertices.Count: >= 2 })`) is always false.
2. **Traffic incident markers cluster at query center** — `TrafficViewModel.cs:175` uses `Math.Abs(incident.Description.GetHashCode()) % 100 / 10000.0` as a fake coordinate offset. The shared `TrafficIncident` model has no `Location` field; the Android `i.Location.Polyline.Vertices[0]` is discarded at `TrafficService.Android.cs:132-136`.
3. **iOS traffic service has no location data path** — `NativeTrafficEngine.swift` does not expose `Location` on `HereTrafficFlow`/`HereTrafficIncident`. The shared model is empty for iOS traffic. Decision needed (extend bridge vs document as platform gap).
4. **Place card category chip has no icon glyph** — `PlaceCard.xaml:24-28` is text-only; users see "Restaurant" as a colored pill with no icon. The `CategoryChipBar` chips in `ExplorePage` already use emoji (🍽 🏨 ⛽); the card is inconsistent.
5. **Top margin hard-coded to 60 px on all 4 pages** — `Margin="12,60,12,0"` in ExplorePage/TrafficPage/ToolsPage/DirectionsPage. No `SafeAreaEdges`, no platform `Padding`. On iOS MAUI's default safe-area inset is applied on top of the 60.
6. **Tools page lands in a near-empty half-expanded bottom sheet** — `CurrentState="HalfExpanded"` + `CollapsedHeight="80"`. The drawing tool buttons are hidden by default; users can't tell the page supports drawing.
7. **Incident list is too cramped** — `BottomSheet.HalfExpandedHeight="300"` + per-row chrome (~44 px) leaves room for 2-3 incidents. ⚠ glyph is the same for every incident.
8. **Floating button clusters have guessed margins** — `Margin="0,0,12,100"` on Explore floating stack. 100 px is a "clear the sheet" hack; collides with the style picker.
9. **Settings page has zero AutomationIds** — entire page unreachable from Appium.

### Known test coverage debt (Phase 8)

The 15 existing Appium tests cover the "not initialized" regression net plus presence checks. User-facing flows below have **no** Appium coverage and need AutomationIds first:

| Priority | Flow | Why critical |
|----------|------|--------------|
| 1 | Place card → "Get Directions" CTA | End-to-end search-to-route journey |
| 2 | Route sheet maneuvers / alternatives | Post-route interaction |
| 3 | Origin / destination suggestion selection | Real search-driven path |
| 4 | Transport mode selection (Car/Truck/Pedestrian/Bicycle/Scooter) | Different routes per mode |
| 5 | Traffic incidents toggle → list → selection | Mirror of Flow coverage |
| 6 | Traffic Refresh (↻) button | Recovery path |
| 7 | Drawing tools: Polyline / Polygon / Circle | 3 of 4 tools uncovered |
| 8 | Demo Gallery preset activation | "Add" buttons per preset |
| 9 | Map style scheme change (5 chips) | Style switcher |
| 10 | Map floating controls (Zoom ±, Center, Style picker) | 4 untested core controls |
| 11 | Tools MapView presence | No `ToolsMapView` AutomationId |
| 12 | Settings page navigation + back | Entire page untested |

---

## Phase 7: RefApp UX Fixes → patch release

> Goal: ship fixes for the 9 user-visible issues. Each item has a concrete
> root cause from the 2026-07-17 audit; verify by manual smoke + new Appium
> test (added in Phase 8).

### 7.1 Traffic geometry on Android

- [ ] `TrafficService.Android.cs:114-116` — read `f.Location.Polyline.Vertices`, map to `List<GeoCoordinates>`, pass to `TrafficFlow` constructor
- [ ] `TrafficService.Android.cs:132-136` — read `i.Location?.Polyline?.Vertices?.FirstOrDefault()`, pass as new positional `Location: GeoCoordinates?` on `TrafficIncident`
- [ ] `Models/Traffic/TrafficModels.cs:19-28` — add `Location: GeoCoordinates? = null` to `TrafficIncident` record (with default to avoid breaking call sites)
- [ ] `TrafficViewModel.cs:175-180` — replace the hash-offset hack with `incident.Location ?? _lastQueryArea.Center`
- [ ] **iOS gap**: extend `NativeTrafficEngine.swift:99-112` (`HereTrafficFlow`) to expose `Location`/`Polyline` from the underlying `TrafficFlowData`. Extend `HereTrafficIncident` likewise. Update `TrafficService.iOS.cs:32-34,42-61` to populate the shared model
- [ ] **Test**: add `TrafficServiceAndroidTests` cases for `TrafficFlow` and `TrafficIncident` having non-null geometry on a real query (requires HERE credentials in env)

### 7.2 Place card category icon

- [ ] `PlaceCard.xaml:24-28` — wrap `CategoryLabel` in `HorizontalStackLayout` with a new `CategoryIconLabel` (emoji)
- [ ] `PlaceCard.xaml.cs:114-133` — add `CategoryIconFor(categoryId)` mirroring `CategoryColorFor` (restaurants→🍽, hotels→🏨, gas→⛽, parking→🅿, atm→🏧, hospital→🏥, shopping→🛍, attractions→🎯, default→📍)
- [ ] `PlaceCard.xaml.cs:22-25` — set `CategoryIconLabel.Text = CategoryIconFor(...)` when category is non-null

### 7.3 Safe-area top margin

- [ ] `Styles.xaml` — add `TopSafeMargin` `OnPlatform` resource (iOS: `12,60,12,0`; Android: `12,12,12,0`)
- [ ] `ExplorePage.xaml:28`, `TrafficPage.xaml:28`, `ToolsPage.xaml:42`, `DirectionsPage.xaml:29` — replace literal `Margin="12,60,12,0"` with `Margin="{StaticResource TopSafeMargin}"`

### 7.4 Tools page discoverability

- [ ] `ToolsPage.xaml:80` — change `CurrentState="HalfExpanded"` to `CurrentState="Expanded"`
- [ ] `ToolsPage.xaml:44` — bind drawing-mode banner `IsVisible="{Binding IsDrawingExpanded}"` (visible whenever the tools card is expanded, not only after a tool is picked) plus a static "Tip: pick a tool, then tap the map to draw." label

### 7.5 Traffic list browsability

- [ ] `TrafficPage.xaml:97` — bump `HalfExpandedHeight="420"`
- [ ] `TrafficPage.xaml:106` — incident card margin `0,4` → `0,2`
- [ ] `TrafficPage.xaml:113-114` — description `FontSize="14"` → `12`, add `LineBreakMode="TailTruncation" MaxLines="2"`
- [ ] New `Converters/TrafficIncidentTypeToIconConverter.cs` — maps `TrafficIncidentType.Accident→💥, Congestion→🚗, Construction→🚧, RoadClosure→⛔, RoadHazard→⚠, Weather→🌧, DisabledVehicle→🔧, default→⚠`
- [ ] `TrafficPage.xaml:108-110` — bind the icon label via the new converter
- [ ] `MauiProgram.cs` — register the new converter

### 7.6 Floating button margins

- [ ] `ExplorePage.xaml:103` — `Margin="0,0,12,100"` → `Margin="0,0,12,72"` (sits above the style picker)
- [ ] `Styles.xaml:140-160` — `ChipButton` add `VerticalContentAlignment="Center"`, `LineHeight="1.0"`, `Padding="14,4"`

### 7.7 AutomationId audit (delivered alongside the fixes above; consumed by Phase 8)

- [ ] `ExplorePage.xaml` — add `ExploreZoomInButton`, `ExploreZoomOutButton`, `ExploreCenterOnLocationButton`, `ExploreClearSearchButton`, `ExploreMapStyleToggle`
- [ ] `PlaceCard.xaml` — add `PlaceCardDirectionsButton` (or expose `DirectionsButton` AutomationId)
- [ ] `DirectionsPage.xaml` — add ids on `TransportModePicker` chips (Car/Truck/Pedestrian/Bicycle/Scooter) via the underlying control; add `RouteSummary` label id
- [ ] `TrafficPage.xaml` — add `TrafficRefreshButton`
- [ ] `ToolsPage.xaml` — add `ToolsMapView`, `ToolsPolylineButton`, `ToolsPolygonButton`, `ToolsCircleButton`, `ToolsClearAllButton`, `ToolsMoreSettingsButton`, `ToolsNormalDayChip`, `ToolsNightChip`, `ToolsHybridChip`, `ToolsSatelliteChip`, `ToolsTerrainChip`
- [ ] `CategoryChipBar.xaml.cs` — set per-chip `AutomationId` (`ExploreCategoryRestaurants`, `ExploreCategoryHotels`, etc.) on the border or label
- [ ] `SettingsPage.xaml` — add `SettingsBackButton`, ids on Terms / Privacy / Feedback / Clear Cache rows
- [ ] `MapStylePicker.xaml` — add `MapStylePickerToggle`

---

## Phase 8: Appium UI Test Coverage → pre-next-GA gate

> Goal: every user-facing flow in the RefApp has an Appium regression test.
> Tests must pass on Android emulator in CI before merge.

### 8.1 Explore flows

- [ ] `ExplorePageTests.cs` — extend with: zoom in twice, zoom out twice, center on location, map style picker open/close
- [ ] `ExplorePageSearchTests.cs` — extend with: clear search button, suggestion selection (type → tap suggestion → assert entry filled), place card "Get Directions" tap (asserts arrival on Directions tab with destination set)
- [ ] `ExplorePageCategoryChipTests.cs` — new file: per-chip test (Restaurants, Hotels, Gas Stations, Parking, ATMs, Hospitals, Shopping, Attractions), each verifies a search was issued (not just "no error")

### 8.2 Directions flows

- [ ] `DirectionsPageRouteTests.cs` — extend with: transport mode selection (one test per mode × 5 modes), route sheet maneuvers visible after calculation, swap origin/destination
- [ ] `DirectionsPageSuggestionTests.cs` — new file: type partial origin → tap suggestion → assert entry populated; same for destination
- [ ] `DirectionsPageIsolineTests.cs` — new file: enable isoline → assert map receives a circle (or at least no error)
- [ ] `DirectionsPageTrafficOnRouteTests.cs` — new file: enable traffic on route → assert no error

### 8.3 Traffic flows

- [ ] `TrafficPageFlowTests.cs` — extend with: tap Flow → assert legend visible + "X flow segments rendered" status; tap Flow again → assert legend hidden
- [ ] `TrafficPageIncidentTests.cs` — new file: tap Incidents → assert list populated + bottom sheet visible; tap incident card → assert no error (marker placement is not currently observable; assert on StatusMessage / no error banner)
- [ ] `TrafficPageRefreshTests.cs` — new file: tap Refresh → assert no error after re-query
- [ ] `TrafficPageToggleTests.cs` — new file: toggle Flow off, then on, then off — assert polylines/markers cleared

### 8.4 Tools flows

- [ ] `ToolsPageDrawingTests.cs` — new file: select Marker → assert `DrawingHint` text contains "Tap anywhere"; same for Polyline / Polygon / Circle. Assert `Clear All` button is hidden before drawing and visible after preset activation.
- [ ] `ToolsPageGalleryTests.cs` — new file: expand Demo Gallery → tap "Add" on SF Landmarks preset → assert "10 objects" label updates (or `Clear All` becomes visible)
- [ ] `ToolsPageSchemeTests.cs` — new file: tap each of 5 scheme chips (NormalDay / NormalNight / HybridDay / SatelliteDay / TerrainDay) → assert no error / no exception
- [ ] `ToolsPageSettingsTests.cs` — new file: tap "More Settings →" → assert arrival on SettingsPage; tap back → assert return to Tools
- [ ] `ToolsPageDarkModeTests.cs` — fix the existing workaround to actually tap the `Switch` (relies on Phase 7.7 AutomationId)

### 8.5 Settings flows

- [ ] `SettingsPageTests.cs` — new file: assert page reachable, About card content present (App Name, App Version, HERE SDK Version), back button returns to Tools
- [ ] `SettingsPageLegalTests.cs` — new file: tap Terms / Privacy / Send Feedback — assert no crash; the URL launches are out of scope

### 8.6 Helpers / infrastructure

- [ ] `BaseTest.cs` — add `FindByAutomationId(id)` (Android: `MobileBy.Id(...)`, iOS future: `MobileBy.AccessibilityId(...)`); add `WaitForNoElementContaining(substring, timeoutMs)` for "not initialized" pattern reuse
- [ ] `AppiumSetup.cs` — increase default wait timeouts (current 5s sleeps are brittle; use WebDriverWait with ExpectedConditions)
- [ ] `ci.yml` — add a smoke test for RefApp launch on iOS simulator (uses XCUITest driver; the iOS AOT test-build blocker is now resolved enough for the launch path)

### 8.7 CI gate

- [ ] All 4 test projects must pass on PR:
  ```bash
  dotnet test tests/HereSdk.Explore.Maui.Tests -c Release
  dotnet test tests/HereSdk.Explore.Maui.RefApp.UITests -c Release
  dotnet test tests/HereSdk.Explore.Maui.UITests -c Release
  dotnet build tests/HereSdk.Explore.Maui.DeviceTests -f net10.0-android -c Release
  dotnet build tests/HereSdk.Explore.Maui.DeviceTests -f net10.0-ios -c Release
  ```
- [ ] `ui-tests-android.yml` runs the Appium suite and blocks on any failure

---

## Phase 9: Post-GA Polish → 4.25.6.0

> Lower priority; queued after Phase 7/8 ship.

- [ ] MapView parity verification (smoke each platform with all 4 services)
- [ ] Pre-built demos expose map screenshots (regression detection for rendering)
- [ ] HERE SDK 4.26+ migration plan (track upstream breaking changes)
- [ ] CI macOS runner for iOS simulator Appium tests
- [ ] Public release of NativeBridge v2 (geocoding, isoline on-route, traffic-on-route)

---

## Risk Register

| Risk | Impact | Mitigation |
|------|--------|------------|
| iOS NativeBridge missing location data on TrafficFlow/Incident | High | Phase 7.1 extends bridge; ship together with Android fix |
| iOS AOT removes unreachable error strings (good) | Medium | Appium "not initialized" tests must run on real device/simulator, not stub |
| Appium iOS requires XCUITest driver matching server version | Medium | Pin `appium-xcuitest-driver@5.0.0` in CI |
| HERE SDK redistribution terms | High | Verified MIT; EULA review pending for HERE-bundled AAR |
| HERE SDK 4.26+ breaking changes | Low | Pin to 4.25.5.0 for next release; plan migration |

---

## Execution Order

```
Phase 7 (RefApp UX) ──→ Phase 8 (UI Test Coverage)
                              ↓
                          Phase 9 (Polish → 4.25.6.0)
```

## Effort Estimate

| Phase | Est. Days | Milestone |
|-------|-----------|-----------|
| 7: RefApp UX | 3-4 | 4.25.5.1 patch |
| 8: UI Test Coverage | 3-4 | 4.25.5.1 patch |
| 9: Post-GA Polish | 4-6 | 4.25.6.0 |
| **Remaining** | **10-14 days** | |
