# HERE SDK for MAUI — Plan

> Single living plan document. Consolidates the former `plan/gap-analysis.md` and
> `plan/07-public-release-gaps.md` (removed 2026-09-01 — history preserved in git).
> Last refreshed 2026-09-13 against `main`.

## Current State

| Area | Status |
|------|--------|
| Android binding | Full AAR binding; known unified-wrap gaps: `AddMapMarker3D` stub throws, `RemoveMapMarkerCluster` removes ALL markers (destructive), `MapDoubleTapped` listener never wired |
| iOS NativeBridge | xcframework built; Map, Search, Routing, Traffic, **Isoline** engines exposed |
| MAUI library | 58 of 361 cross-platform API types (~16%), core services complete |
| Ref app | One shared map (`MapHomePage`) with 4 overlay panels + custom tab bar; 5 VMs (+ViewModelBase), 7 controls, 7 converters; panel restructure 2026-09-13 |
| Unit tests | 261 passing (net10.0, no device) |
| RefApp UI tests | 212 passing (ViewModel commands + state transitions + error/empty states) |
| Appium smoke | 37 NUnit tests on Android emulator, green locally (iOS XCUITest driver supported) |
| Device tests | ~180 tests across 10 files (Android builds clean, iOS blocked by AOT/env) |
| Version | 4.25.5.0 GA |
| Docs | README, docs/getting-started.md, CHANGELOG, XML docs, DocFX site |
| CI/CD | 5 workflows: build, changelog, publish, release-changelog, ui-tests-android |

## Completed (pruned)

- **Phases 0–6**: scaffolding, Android binding, iOS NativeBridge, MapView + init, search,
  routing, traffic, packaging, CI scripts (`scripts/*.sh`), docs pipeline, ref app — shipped.
- **Public-release hygiene**: LICENSE, SECURITY.md, CONTRIBUTING.md, SUPPORT.md,
  CODEOWNERS, issue templates (bug_report), .editorconfig, .gitattributes, git remote,
  `appsettings.json` placeholder policy, README accuracy.
- **RefApp UX fixes (2026-07 audit)**: traffic flow polylines + incident locations on Android
  (`IncidentGeometryHelpers`), place-card category icons, `TopSafeMargin` shared resource,
  Tools page expanded by default, traffic bottom sheet 420px, floating button margins,
  AutomationId sweep, iOS XCUITest driver + cross-platform locators + readiness gate,
  iOS `opt_initialize` selector fix.
- **UI polish pass (2026-09-01)**: Shell nav bars hidden on all 4 map pages (full-bleed map),
  iOS 26 Liquid Glass opt-out (`UIDesignRequiresCompatibility`), opaque chip bars with soft
  shadows (`CategoryChipBar`, `TransportModePicker`), `MapStylePicker` aligned to 12px/44pt,
  suggestions show `Type` instead of raw place IDs.
- **Isoline routing on iOS (2026-09-01)**: `HereIsolineRoutingEngine` added to NativeBridge,
  bound in `ApiDefinition.cs`, `RoutingService.iOS.CalculateIsolineAsync` implemented.
- **Forward/reverse geocoding (2026-09-01)**: iOS NativeBridge wraps `searchByAddress` /
  `searchByCoordinates` and exposes `HereAddress` on `HerePlace`; shared `AddressQuery` model,
  two new `ISearchService.SearchAsync` overloads (forward + reverse), Android implementations;
  `Place` now carries an `Address`. RefApp: map tap reverse-geocodes and shows a place card.
- **TrafficOnRoute on both platforms (2026-09-09)**: `GetTrafficOnRouteAsync` now calls the SDK's
  `calculateTrafficOnRoute` (Android `RoutingEngine.calculateTrafficOnRoute`, iOS NativeBridge
  `HereRoutingEngine.calculateTrafficOnRoute`); native routes retained by handle for the query;
  shared model reshaped to the real API (`TrafficOnRoute`/`TrafficOnSection`/`TrafficOnSpan`,
  `TrafficIncidentOnRoute`, `TrafficOnRouteResult`), RefApp traffic toggle shows summed span delay.
- **iOS traffic enum mapping fixed (2026-09-09)**: `TrafficService.iOS` cast raw enum values
  directly into the shared enum (different member order → corrupted types/impacts); explicit
  raw-value mapping added, consistent with Android (Critical→Closed, Low→Minor), pinned by device tests.
- **LocationChanged fixed (2026-09-09)**: `StartListeningAsync` now starts the MAUI foreground
  listener (`StartListeningForegroundAsync`) and `StopListeningAsync` stops it — previously only the
  static event was subscribed and `LocationChanged` never fired on either platform.
- **Appium suite hardening (2026-09-12)**: fixed the 22/22 CI failure cascade — all BaseTest
  lookups poll (no implicit wait), TearDown restores a known app state (pop pushed pages /
  relaunch), stale place-card dismissal race fixed in `TapMapForPlaceCard`, bounded
  TerminateApp→ActivateApp restarts (2s sleep between, else "failed to complete startup" ANR),
  reverse-geocode failures now logged via REFAPP_DIAG, screenshot upload path corrected.
- **RefApp single shared map (2026-09-13)**: one `HereMapView` in `MapHomePage` behind 4 overlay
  panels + custom bottom tab bar (`PanelNavigationService`); per-tab private maps removed —
  drawn objects now persist across tab switches; map-tap gated by active panel; Reset Map in
  Tools; suggestion dropdowns dismiss on selection/map tap (debounce-cancel + suppress flags).
- **Appium place-card journey CI-green (2026-09-13/14)**: three CI-only failures fixed — stale
  `.Text` reads (`GetTextStaleSafe` retry), swallowed CTA tap during sheet collapse
  (`WaitForDirectionsPanelAfterCta` re-tap) + `adb emu geo fix` on the fresh AVD, and ungranted
  runtime permissions (`adb install -r -g`; uiautomator2 autoGrantPermissions only applies when
  Appium installs the app). Suite now 36/36 in CI.

## Active Work: RefApp UX polish round 2 (Airbnb-style)

Phases 0–5 implemented (2026-09-14): token foundation (`Themed` markup extension, implicit
styles, dark-mode-reactive code-built controls), BottomSheet chrome + scrim + body-drag,
drawn-object visuals (coral palette via `DrawingPalette`, polygon/circle outlines, polyline
cap, branded `marker_pin`), and the drawing-UX redesign (floating toolbar with Undo/Done/
Cancel, multi-drop markers, per-object delete list, sheet auto-collapse). Panel polish:
ErrorBanner/EmptyStateView/MapStylePicker tokens + Material glyphs, tab-bar hairline +
shadow, auto-height suggestion dropdowns, VM properties replacing text-converter hacks
(`HasSearchQuery`, `IsolineButtonGlyph`). Suites: 264 unit + 224 in-process + 37 Appium green
(2026-09-14). BottomSheet drag reworked onto PointerGestureRecognizer: Android's pan pipeline
delivers two Running events per move, stalls TotalY once the finger leaves the view bounds, and
suppresses pointer events when attached alongside them — the sheet now snaps from absolute
pressed/released positions (snap-at-release instead of live follow). Manual visual pass
light+dark done on Android emulator (all four tabs); iOS pass pending.

## Backlog

### NativeBridge / binding gaps (from gap-analysis, P1)

| Gap | Detail | Platforms |
|-----|--------|-----------|
| Picked place lookup | `searchByPickedPlace` not wrapped | iOS |
| Structured address search | `StructuredQuery` not wrapped | iOS |
| Extended callbacks | `SearchCallbackExtended` / `SuggestCallbackExtended` | iOS |
| EV/fuel models | `EVChargingStation`, `FuelStation`, `EVChargingPool` | iOS |
| WebDetails on Place | `WebImage`, `WebEditorial`, `WebRating` | iOS |
| Place model too thin | No categories, contact, details (iOS); Address now bound | iOS |
| Suggestion model too thin | No `SuggestionType` enum precision | iOS → Shared |
| `setCustomOption` / `sendRequest` | Not wrapped | iOS |
| Full `RoutingOptions` | Only `transportMode` — no avoidance, toll, EV, text options | iOS |
| Route serialization | `Route.serialize()`/`deserialize()` | iOS |
| HERE native positioning | Explore SDK has no positioning engine on either platform; `ILocationService` uses MAUI Geolocation (by design) | Both |
| Shared `Place` model | Missing `details`, `openingHours`, full `categories`, `chains` | Shared |

### Advanced / nice-to-have (P2)

3D markers, marker clustering, custom data sources (raster/point/tile),
camera animations (`FlyToAnimation`, `KeyframeTracks`), scene lights, mesh builders,
`MapAssets`, remaining ~303 catalog types. Coverage growth beyond the current 58/361.

### Community files / CI plumbing

- [ ] `.github/ISSUE_TEMPLATE/feature_request.md` (optional)
- [x] CI workflows recreated — build.yml, changelog.yml, publish.yml,
      release-changelog.yml, ui-tests-android.yml
- [ ] NuGet publish verification + signing in CI
- [ ] Coverage tooling (`coverlet.collector`), converter/error-mapping test suites

## Test coverage debt (Appium flows with no regression coverage)

Covered since 2026-09-12: place-card CTA journey (1), incidents/flow toggles (5),
traffic refresh (6), drawing tools (7), scheme chips (9), style picker (10),
Settings nav + back (12), map tap reverse geocode. (The per-tab "MapView is
present" checks (11) became panel-control assertions after the single shared
map restructure — one `HereMapView` now serves all panels.)

Still open:

| Priority | Flow |
|----------|------|
| 1 | Route sheet maneuvers / alternatives after route calculation |
| 2 | Origin/destination suggestion selection (autocomplete → route inputs) |
| 3 | Transport mode selection (5 modes) reflected in calculated route |
| 4 | Demo Gallery preset activation |
| 5 | Traffic incident list → selection → map highlight |

## Risk Register

| Risk | Impact | Mitigation |
|------|--------|------------|
| HERE SDK redistribution terms | High | EULA review pending for bundled AAR |
| iOS AOT removes unreachable error strings | Medium | Device/simulator tests, not stubs |
| HERE SDK 4.26+ breaking changes | Low | Pinned to 4.25.5.0; plan migration |
| `RefreshRouteOptions` removal in v4.28.0 | Low | Not bound — already excluded |