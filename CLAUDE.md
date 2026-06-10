# CLAUDE.md — Project Instructions

## Git Rules

- **Never commit without explicit user approval.** Always ask before committing.
- Do not push to remote unless explicitly asked.
- When allowed, create 1-liner commit messages, precise yet concise, e.g. "feat: move validate-cli.sh to scripts/ (374 assertions, timing, CI job), fix clippy redundant_field_names, optimize api_key clone."
- Never use the Co-author tag in commit messages.

### Commit Style

- **1-liner only** — precise, concise, no body paragraphs
- **Never** add `Co-Authored-By`, `Signed-off-by`, or any trailer/footer
- Format: `<type>: <imperative description>`
- Types: `feat:`, `fix:`, `refactor:`, `test:`, `docs:`, `ci:`, `chore:`
- Examples:
  - `feat: add token-bucket rate limiter to ig-ratelimit`
  - `fix: map Graph API error code 2534022 to WindowExpired`
  - `test: add wiremock tests for all API error code mappings`

## Behavioral Guidelines

Behavioral guidelines to reduce common LLM coding mistakes. Merge with project-specific instructions as needed.

Tradeoff: These guidelines bias toward caution over speed. For trivial tasks, use judgment.
1. Think Before Coding

Don't assume. Don't hide confusion. Surface tradeoffs.

Before implementing:

    State your assumptions explicitly. If uncertain, ask.
    If multiple interpretations exist, present them - don't pick silently.
    If a simpler approach exists, say so. Push back when warranted.
    If something is unclear, stop. Name what's confusing. Ask.

2. Simplicity First

Minimum code that solves the problem. Nothing speculative.

    No features beyond what was asked.
    No abstractions for single-use code.
    No "flexibility" or "configurability" that wasn't requested.
    No error handling for impossible scenarios.
    If you write 200 lines and it could be 50, rewrite it.

Ask yourself: "Would a senior engineer say this is overcomplicated?" If yes, simplify.
3. Surgical Changes

Touch only what you must. Clean up only your own mess.

When editing existing code:

    Don't "improve" adjacent code, comments, or formatting.
    Don't refactor things that aren't broken.
    Match existing style, even if you'd do it differently.
    If you notice unrelated dead code, mention it - don't delete it.

When your changes create orphans:

    Remove imports/variables/functions that YOUR changes made unused.
    Don't remove pre-existing dead code unless asked.

The test: Every changed line should trace directly to the user's request.
4. Goal-Driven Execution

Define success criteria. Loop until verified.

Transform tasks into verifiable goals:

    "Add validation" → "Write tests for invalid inputs, then make them pass"
    "Fix the bug" → "Write a test that reproduces it, then make it pass"
    "Refactor X" → "Ensure tests pass before and after"

For multi-step tasks, state a brief plan:

1. [Step] → verify: [check]
2. [Step] → verify: [check]
3. [Step] → verify: [check]

Strong success criteria let you loop independently. Weak criteria ("make it work") require constant clarification.

These guidelines are working if: fewer unnecessary changes in diffs, fewer rewrites due to overcomplication, and clarifying questions come before implementation rather than after mistakes.

## Project Overview

HERE SDK Explore Edition bindings for .NET MAUI. Wraps the HERE Explore SDK v4.25.5.0 for Android and iOS into a unified, idiomatic C# API.

**Key constraint**: The iOS SDK is Swift-only — its ObjC bridge header exposes only 4 types. We use a Swift wrapper framework (`NativeBridge`) to re-expose all APIs via `@objc` annotations, then bind that with Objective-Sharpie.

## Repository Layout

```
src/
  HereSdk.Explore.Android.Binding/    # Android AAR binding (Metadata.xml transforms)
  HereSdk.Explore.iOS.NativeBridge/   # Swift wrapper framework (Xcode project)
  HereSdk.Explore.iOS.Binding/        # iOS binding (ApiDefinition.cs, StructsAndEnums.cs)
  HereSdk.Explore.Maui/               # Cross-platform MAUI class library
  HereSdk.Explore.Maui.RefApp/        # Demo/reference app
tests/
  HereSdk.Explore.Maui.Tests/         # xUnit unit tests (net10.0, no device needed)
  HereSdk.Explore.Maui.DeviceTests/   # Platform device tests (net10.0-android;net10.0-ios)
scripts/
  build.sh, build-android.sh, build-ios-native.sh, bind-ios.sh, test.sh, pack.sh, clean.sh
plan/                                  # Design documents (this plan)
tmp/                                   # SDK archives (gitignored)
Version.props                          # Centralized version numbers (HereSdkVersion, PackageVersion)
```

## Build Commands

```bash
# Full build (from repo root)
./scripts/build.sh

# Android binding only
dotnet build src/HereSdk.Explore.Android.Binding -c Release

# iOS: build NativeBridge xcframework first
cd src/HereSdk.Explore.iOS.NativeBridge
xcodegen generate  # regenerate Xcode project from project.yml
cd ../..
./scripts/build-ios-native.sh   # builds xcframework for device + simulator, copies to Binding/Libs/

# Run Objective-Sharpie against xcframework headers (optional, manual fixup required)
./scripts/bind-ios.sh

# Build iOS binding
dotnet build src/HereSdk.Explore.iOS.Binding -c Release

# MAUI library (both platforms)
dotnet build src/HereSdk.Explore.Maui -f net10.0-android -c Release
dotnet build src/HereSdk.Explore.Maui -f net10.0-ios -c Release

# Unit tests (no device needed)
dotnet test tests/HereSdk.Explore.Maui.Tests -c Release

# Device tests
dotnet test tests/HereSdk.Explore.Maui.DeviceTests -f net10.0-android -c Release
dotnet test tests/HereSdk.Explore.Maui.DeviceTests -f net10.0-ios -c Release
```

## Conventions

### Naming

- **Unified C# API**: `Here.Explore.Maui` namespace — PascalCase, idiomatic C#
- **Android binding namespace**: `Com.Here.Sdk.*` (auto-generated from AAR, remapped via `AndroidNamespaceReplacement`)
- **iOS binding namespace**: `Here.Explore.iOS` (from Sharpie output)
- **Models**: Immutable `record` types (e.g., `GeoCoordinates`, `Route`, `Waypoint`)
- **Services**: Interfaces (`IMapService`, `IRoutingService`) with platform-specific partial classes
- **Platform converters**: Extension methods in `PlatformConverters/` with `ToShared()` / `ToAndroid()` / `ToiOS()`
- **iOS NativeBridge**: Prefix `Here` on all ObjC wrapper types (e.g., `HereGeoCoordinates`, `HereRoutingEngine`)

### Architecture Patterns

- **Handler pattern** for `HereMapView` (MAUI handler, not custom renderer)
- **Service pattern**: Interface in shared code, platform implementation via `#if ANDROID` / `#if IOS` partial classes
- **Async**: Java callbacks and Swift closures → `TaskCompletionSource<T>` → `async/await`
- **Events**: Java listener interfaces → C# events; Swift delegate protocols → C# events
- **Errors**: Platform error codes → unified C# enums (e.g., `RoutingError`, `SearchError`)
- **Disposal**: All platform wrapper types implement `IDisposable` to manage native resources

### File Organization

- Platform-specific code uses `#if ANDROID` / `#if IOS` preprocessor directives in partial classes
- Each service has: `IService.cs` (interface), `Service.cs` (shared partial), `Service.Android.cs`, `Service.iOS.cs`
- Converters: `GeoCoordinatesConverter.Android.cs`, `GeoCoordinatesConverter.iOS.cs`

## iOS Binding — Critical Details

1. **Swift structs cannot be `@objc`** — each struct needs an `NSObject` wrapper class with `toSwift()` / `fromSwift()` converters
2. **Swift enums are `UInt32`-backed** — need `NSInteger` wrapper enums with explicit raw value mapping. **Never use `.localizedDescription`** on these enums — use `String(describing:)` instead
3. **128 sealed classes** — use containment (has-a), never inheritance
4. **`RefreshRouteOptions`** is deprecated (removal in v4.28.0) — do NOT bind it
5. **39 nested enums** (e.g., `Easing.InstantiationErrorCode`) need separate NSInteger wrappers
6. **`CollectionOf<T>`** exists only on iOS — Android erases the type parameter
7. **Objective-Sharpie output** always needs manual fixup — resolve every `[Verify]` attribute
8. **Memory management**: Use weak references for delegate protocols; implement `IDisposable` on all C# wrappers; test for retain cycles
9. **TransportSpecification uses builder pattern**: `TransportSpecification.CarBuilder().build()`, NOT `CarSpecifications().transportSpecification`
10. **MapView is ObjC-visible** as `HereMapView` (UIView subclass) — wrapped via `HereMapBridgeView.Create()` + `.PlatformView`
11. **MapScene manages markers** via `addMapMarker()`/`removeMapMarker()`, not MapBridgeView
12. **MapService is NOT in DI** — it's created by the handler and accessed via `HereMapView.Map`. Other services (IRoutingService, ISearchService, ITrafficService, ILocationService) are DI singletons.
13. **Map events are on IMapService only** — `HereMapView` does NOT have its own events; subscribe to `mapView.Map.CameraStateChanged`, `mapView.Map.MapTapped`, etc.
14. **iOS gesture delegates** are bound as concrete classes (`HereTapDelegate`, `HereLongPressDelegate`, `HereDoubleTapDelegate`) — subclass them, don't implement the `IHereTapDelegate` interface
15. **iOS xcframework filename** is `HereSdkExploreNativeBridge.xcframework` (no dots), not `HereSdk.Explore.iOS.NativeBridge.xcframework`
16. **Int32 properties in ApiDefinition.cs** use `int`, not `nint` (which maps to NSInteger, 64-bit on arm64)
17. **HERE native positioning is NOT exposed in iOS NativeBridge yet** — `ILocationService` uses `Microsoft.Maui.Devices.Sensors.Geolocation` as the primary source on both platforms

## Android Binding — Critical Details

1. **`Metadata.xml`** is in `Transforms/` — use it to fix namespace collisions, rename types, remove internal classes
2. **`com.here.sdk.core.utilities`** is empty — omit from namespace mapping
3. **`FuelType`** belongs to `com.here.sdk.transport`, NOT search
4. **`AuthenticationMode`, `LogControl`, `SDKBuildInformation`, `SDKLogger`** belong to `com.here.sdk.core.engine`, not `core`
5. **Listener interfaces** should become C# events (use `eventName` attribute in Metadata.xml)
6. **Callback interfaces** (single method) should become `EventHandler<TEventArgs>`
7. **Remove** `*Impl`, `*Internal`, `Android*` platform classes from binding
8. **The mock JAR** (`heresdk-explore-mock-*.jar`) is used for testing — reference it in the test project

## Reference App Debugging & Testing

1. **Debug logging is mandatory** for every map service operation (entry, exit, coordinates, errors):
   - Shared code: `System.Diagnostics.Debug.WriteLine("[REFAPP_DIAG] message")`
   - Android handler: `Android.Util.Log.Debug("REFAPP_DIAG", "message")`
   - iOS handler: `System.Diagnostics.Debug.WriteLine("[REFAPP_DIAG] message")`
   - iOS NativeBridge: `NSLog("REFAPP_DIAG: %@", message)`
2. **Test cadence**: Every feature change must include:
   - Unit test in `tests/HereSdk.Explore.Maui.Tests` for models, converters, and service logic.
   - UI test in `tests/HereSdk.Explore.Maui.RefApp.UITests` for ViewModel commands and state changes.
   - Device test in `tests/HereSdk.Explore.Maui.DeviceTests` when native bindings or platform handlers are touched.
3. **CI gate**: All three test projects must pass before a PR is considered merge-ready:
   ```bash
   dotnet test tests/HereSdk.Explore.Maui.Tests -c Release
   dotnet test tests/HereSdk.Explore.Maui.RefApp.UITests -c Release
   dotnet test tests/HereSdk.Explore.Maui.DeviceTests -f net10.0-android -c Release
   dotnet test tests/HereSdk.Explore.Maui.DeviceTests -f net10.0-ios -c Release
   ```
4. **Common emulator gotchas**:
   - Android: `MapView` requires hardware acceleration; enable it in `AndroidManifest.xml` (`android:hardwareAccelerated="true"`).
   - iOS: The xcframework must be copied to `src/HereSdk.Explore.iOS.Binding/Libs/` after every NativeBridge rebuild.
   - Android context null during startup: use embedded resources for config, not `Application.Context` (see memory `android_context_null_during_startup.md`).
5. **Build often, test early**: Run `dotnet build` and the relevant test subset after every file change. Do not batch days of work without building.

## API Validation

Every type MUST be validated against BOTH API references before marking complete:

- **Android**: `tmp/android-inspect/api-reference/` (Javadoc HTML)
- **iOS Swift interface**: `tmp/ios-inspect/heresdk-explore-ios-4.25.5.0.274356/heresdk/frameworks/heresdk.xcframework/ios-arm64/heresdk.framework/Modules/heresdk.swiftmodule/arm64-apple-ios.swiftinterface`

Validation checklist per type:
1. Type exists in BOTH Android and iOS API references
2. Method signatures are equivalent
3. Return types map to the same cross-platform type
4. Error/exception types are handled consistently
5. Async patterns map correctly (Java callbacks → C# Tasks, Swift closures → C# Tasks)

If a type exists on only one platform, document it as platform-specific in `plan/05-api-surface-catalog.md`.

## Testing

- **Unit tests** (`tests/HereSdk.Explore.Maui.Tests`): net10.0, no device needed. Test models, converters, service logic with mocks.
- **Device tests** (`tests/HereSdk.Explore.Maui.DeviceTests`): net10.0-android;net10.0-ios. Require emulator/simulator. Test actual SDK initialization, binding type access, platform converters.
- **Mock framework**: NSubstitute for C# interfaces; Android mock JAR for Android device tests.
- **Naming**: `{MethodName}_{Scenario}_{Expected}`
- **Target**: 80%+ coverage on shared logic (models, converters, services)

## Phased Delivery

| Phase | Scope | Status |
|-------|-------|--------|
| 0 | Foundation: scaffolding, Android binding, iOS NativeBridge skeleton | Complete |
| 1 | MapView + SDK Init: map display, camera, gestures, markers | Complete (Android + iOS xcframework built) |
| 2 | Search + Routing: full search & routing across both platforms | Complete (Android + iOS NativeBridge functional) |
| 3 | Traffic + Advanced: traffic, map items, advanced features | Complete (Android + iOS NativeBridge functional) |
| 4 | Polish + NuGet: coverage audit, packaging, CI/CD, docs, ref app UX | In Progress (4.1-4.8 done, 4.9 remaining) |

## Key Discrepancies Found (from verification)

1. `CollectionOf<T>` exists ONLY in iOS, NOT in Android
2. `RefreshRouteOptions` is sealed AND deprecated (v4.28.0) — do NOT bind
3. iOS has 189 structs (not ~160), 142 enums (103 top-level + 39 nested), 21 type aliases
4. `com.here.sdk.core.utilities` is empty — removed from namespace mapping
5. `FuelType` belongs to Transport module, NOT Search
6. `AuthenticationMode`, `LogControl`, `SDKBuildInformation`, `SDKLogger` belong to `core.engine`, not `core`
7. **Map circles** — HERE SDK has no native circle primitive on either platform; implemented as polygon approximation via `CircleGeometryHelper`
8. **iOS NativeBridge lacks positioning types** — `ILocationService` uses MAUI Geolocation as cross-platform fallback