# HERE SDK Explore for .NET MAUI — Master Plan

> This plan has been verified against both the Android Javadoc and iOS Swift interface.
> See [Corrections Log](plan/05-api-surface-catalog.md) for discrepancies found and fixed.

## Quick Reference

| What | Value |
|---|---|
| SDK | HERE Explore SDK v4.25.5.0 |
| .NET | .NET 9 |
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

## Phase Summary

| Phase | Scope | Status |
|---|---|---|
| 0 | Foundation: scaffolding, Android binding, iOS NativeBridge skeleton | ✅ Complete |
| 1 | MapView + SDK Init: map display, camera, gestures, markers | ✅ Scaffolded |
| 2 | Search + Routing: full search & routing across both platforms | ✅ Scaffolded |
| 3 | Traffic + Advanced: traffic, map items, advanced features | ✅ Scaffolded |
| 4 | Polish + NuGet: coverage audit, packaging, CI/CD, docs | Scaffolded |

> **Note**: All phases are scaffolded with code structure, models, services, NativeBridge wrappers, and tests.
> Runtime build verification requires: `dotnet` SDK (9.0+) and Xcode (15+) with developer tools configured.
> Run `sudo xcode-select -s /Applications/Xcode.app/Contents/Developer` and install .NET 9 SDK to verify builds.

## Validation Rule

**Every feature MUST be validated against both API references before marking complete:**
- Android: `tmp/android-inspect/api-reference/`
- iOS: `tmp/ios-inspect/heresdk-explore-ios-4.25.5.0.274356/heresdk/frameworks/heresdk.xcframework/ios-arm64/heresdk.framework/Modules/heresdk.swiftmodule/arm64-apple-ios.swiftinterface`