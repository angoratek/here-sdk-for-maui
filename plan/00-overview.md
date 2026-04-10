# HERE SDK Explore for .NET MAUI — Master Plan

> SDK Version: 4.25.5.0 (Explore Edition)
> .NET Target: .NET 9
> Platforms: Android (API 24+), iOS (15.2+)
> No Windows, no Mac Catalyst

## Decision Record

| Decision | Choice | Rationale |
|---|---|---|
| iOS Binding | Native Library Interop (Swift wrapper → ObjC → Sharpie) | iOS SDK is Swift-only; ObjC bridge header exposes only 4/350+ types |
| Cross-platform API | Unified idiomatic C# API | Hides platform quirks, testable, consumer-friendly |
| .NET Version | .NET 9 | Latest stable, best MAUI tooling |
| Scope | Phased delivery | ~350 types across 6 modules; ship incrementally |
| Package naming | `HereSdk.Explore.*` | Distinguishes from Navigate edition |
| Test strategy | TDD with mock JAR (Android) + protocol mocks (iOS) | Catch regressions early; HERE provides mock JAR |

## Critical Finding: iOS Binding Challenge

The iOS HERE SDK is **Swift-first**. The ObjC bridge header (`heresdk-Swift.h`) exposes only 4 types:
- `HereMapView` (UIView subclass)
- `SDKInternalInitializer`
- `SDKMapViewInitializer`
- `SDKNativeEngineHolder`

The real API is in `arm64-apple-ios.swiftinterface` (9,060 lines) with 128 sealed classes (`@_hasMissingDesignatedInitializers`), 1 open class (`MapView`), ~160 structs, ~110 enums, 35 protocols, 20 type aliases.

**Objective-Sharpie alone would bind only 4 types — useless.** We must write a Swift wrapper framework that re-exposes the API through `@objc` annotations, then run Sharpie on the generated ObjC header from that wrapper.

## Sub-Documents

| File | Content |
|---|---|
| [01-project-structure.md](01-project-structure.md) | Solution layout, csproj details, build props |
| [02-android-binding.md](02-android-binding.md) | Android AAR binding strategy, Metadata.xml, naming |
| [03-ios-binding.md](03-ios-binding.md) | iOS Native Library Interop deep-dive, Swift wrapper, Sharpie |
| [04-cross-platform-api.md](04-cross-platform-api.md) | Unified MAUI API design, handlers, services |
| [05-api-surface-catalog.md](05-api-surface-catalog.md) | Complete type catalog from both SDKs — THE validation reference |
| [06-phased-delivery.md](06-phased-delivery.md) | Phase-by-phase implementation plan with acceptance criteria |
| [07-testing-strategy.md](07-testing-strategy.md) | TDD approach, mock patterns, device tests |
| [08-nuget-and-ci.md](08-nuget-and-ci.md) | Packaging, GitHub Actions, versioning |
| [09-risks-and-mitigations.md](09-risks-and-mitigations.md) | Risk register with mitigations |

## API Reference Validation Rule

**Every feature implemented MUST be validated against both API references:**
- Android: `tmp/android-inspect/api-reference/` (Javadoc)
- iOS: `tmp/ios-inspect/api-reference/` (jazzy-generated)
- iOS Swift interface: `tmp/ios-inspect/heresdk-explore-ios-4.25.5.0.274356/heresdk/frameworks/heresdk.xcframework/ios-arm64/heresdk.framework/Modules/heresdk.swiftmodule/arm64-apple-ios.swiftinterface`

Before marking any type/method as "bound", verify:
1. The type exists in BOTH Android and iOS API references
2. Method signatures are equivalent (parameter names may differ)
3. Return types map to the same cross-platform type
4. Error/exception types are handled consistently
5. Async patterns map correctly (Java callbacks → C# Tasks, Swift closures → C# Tasks)

If a type exists on only one platform, document it as platform-specific in the API surface catalog.
