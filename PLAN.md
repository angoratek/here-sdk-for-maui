# HERE SDK for MAUI — Plan

> Single living plan document. Consolidates the former `plan/gap-analysis.md` and
> `plan/07-public-release-gaps.md` (removed 2026-09-01 — history preserved in git).
> Last refreshed 2026-09-01 against `main`.

## Current State

| Area | Status |
|------|--------|
| Android binding | Full AAR binding, all services functional |
| iOS NativeBridge | xcframework built; Map, Search, Routing, Traffic, **Isoline** engines exposed |
| MAUI library | 58 of 361 cross-platform API types (~16%), core services complete |
| Ref app | 5 pages, 5 VMs (+ViewModelBase), 7 controls, 7 converters; 2026-09-01 Airbnb-style polish pass applied |
| Unit tests | 253 passing |
| RefApp UI tests | 201 passing (ViewModel commands + state transitions + error/empty states) |
| Appium smoke | 15 NUnit tests on Android emulator + iOS XCUITest driver added |
| Device tests | ~180 tests across 10 files (Android builds clean, iOS blocked by AOT/env) |
| Version | 4.25.5.0 GA |
| Docs | README, docs/getting-started.md, CHANGELOG, XML docs, DocFX site |
| CI/CD | **No workflows in repo** (`.github/` absent) — see Backlog |

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

## Active Work: RefApp UX polish (continued)

- [x] Top safe-area margin — MAUI already applies the iOS safe-area inset, so the 60px top
      margin doubled it (~120px gap). `TopSafeMargin` iOS top reduced to 8. Verify on both
      platforms (screenshots) that the search bar sits tight under the status bar.
- [x] Loading overlays — full-screen dark scrim replaced by a centered rounded pill
      (spinner + label, themed, soft shadow, `InputTransparent` so the map stays
      interactive) on Explore, Directions, and Traffic pages.

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
| TrafficOnRoute | Not wrapped on iOS; Android needs native Route reference | Both |
| HERE native positioning | `ILocationService` uses MAUI Geolocation fallback | Both |
| Shared `Place` model | Missing `details`, `openingHours`, full `categories`, `chains` | Shared |

### Advanced / nice-to-have (P2)

3D markers, marker clustering, custom data sources (raster/point/tile),
camera animations (`FlyToAnimation`, `KeyframeTracks`), scene lights, mesh builders,
`MapAssets`, remaining ~303 catalog types. Coverage growth beyond the current 58/361.

### Community files / CI plumbing

- [ ] `.github/ISSUE_TEMPLATE/feature_request.md` (optional)
- [ ] Recreate CI workflows (build/publish/ui-tests) — currently absent from repo
- [ ] NuGet publish verification + signing in CI
- [ ] Coverage tooling (`coverlet.collector`), converter/error-mapping test suites

## Test coverage debt (Appium flows with no regression coverage)

| Priority | Flow |
|----------|------|
| 1 | Place card → "Get Directions" CTA (search → route journey) |
| 2 | Route sheet maneuvers / alternatives |
| 3 | Origin/destination suggestion selection |
| 4 | Transport mode selection (5 modes) |
| 5 | Traffic incidents toggle → list → selection |
| 6 | Traffic refresh button |
| 7 | Drawing tools: Polyline / Polygon / Circle |
| 8 | Demo Gallery preset activation |
| 9 | Map style scheme change (5 chips) |
| 10 | Map floating controls (zoom, center, style picker) |
| 11 | Tools MapView presence |
| 12 | Settings page navigation + back |

## Risk Register

| Risk | Impact | Mitigation |
|------|--------|------------|
| HERE SDK redistribution terms | High | EULA review pending for bundled AAR |
| iOS AOT removes unreachable error strings | Medium | Device/simulator tests, not stubs |
| HERE SDK 4.26+ breaking changes | Low | Pinned to 4.25.5.0; plan migration |
| `RefreshRouteOptions` removal in v4.28.0 | Low | Not bound — already excluded |