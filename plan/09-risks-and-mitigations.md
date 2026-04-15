# Risks and Mitigations

## High Severity Risks

### R1: iOS Swift-Only SDK — Cannot Use Traditional Binding Approach

**Problem**: The iOS HERE SDK's ObjC bridge header exposes only 4 types. Traditional .NET iOS binding via Objective-Sharpie would bind only `HereMapView`, `SDKInternalInitializer`, `SDKMapViewInitializer`, `SDKNativeEngineHolder` — effectively useless for the other 350+ types.

**Mitigation**: Native Library Interop pattern — create a Swift wrapper framework that re-exposes all APIs as `@objc`-annotated types. This is the MAUI Community Toolkit's recommended approach for Swift-only SDKs.

**Status**: PARTIALLY MITIGATED — Swift wrapper stubs created for all Phase 1-3 types. xcframework build blocked on Xcode project creation (needs GUI or XcodeGen).

**Residual Risk**: The wrapper framework adds build complexity and must be rebuilt when the HERE SDK updates. Mitigated by automating xcframework build in CI.

**Verification**: `arm64-apple-ios.swiftinterface` has 9,060 lines with 128 sealed classes, 189 structs, 142 enums (103 top-level + 39 nested), 21 type aliases, 35 protocols. Each must be wrapped.

---

### R2: Swift Structs Cannot Cross @objc Boundary

**Problem**: The iOS SDK uses ~189 Swift structs (e.g., `GeoCoordinates`, `RouteHandle`, `Waypoint`). Swift structs **cannot** be annotated with `@objc`. They must be wrapped as `NSObject` subclasses, with manual conversion in/out.

**Mitigation**: For each struct, create an `@objc` wrapper class with:
- Properties matching the struct's fields (as ObjC-compatible types)
- `toSwift()` method returning the original struct
- `static fromSwift()` factory creating the wrapper from a struct

**Status**: MITIGATED — Pattern established for all Phase 1-3 struct types. 14 additional nested structs identified needing NSObject wrappers (see 05-api-surface-catalog.md "Phase 3 Validation — iOS Nested Types Requiring Wrappers").

**Residual Risk**: Performance overhead from struct ↔ class conversion. Mitigated by keeping wrappers thin and using conversion only at boundary points.

---

### R3: Swift Enums Are UInt32-Backed, Not Int-Backed

**Problem**: All ~142 iOS SDK enums are `Swift.UInt32`-backed (e.g., `RoutingError : Swift.UInt32`). `@objc` enums must be `NSInteger` (Int) backed. Direct bridging is impossible.

**Mitigation**: Create wrapper enums backed by `NSInteger` with explicit raw value mapping:
```swift
@objc public enum HereRoutingError: NSInteger {
    case none = 0
    case networkError = 1
    // Map from UInt32 raw values
}
```

**Status**: MITIGATED — Pattern established for all top-level enums. 24 nested enums identified needing NSInteger wrappers (see 05-api-surface-catalog.md "Phase 3 Validation — iOS Nested Types Requiring Wrappers").

**Residual Risk**: Risk of raw value drift between SDK versions. Mitigated by code-generating wrapper enums from `.swiftinterface` and verifying during build.

---

### R4: Large API Surface — 350+ Types to Bind

**Problem**: Binding 350+ types is a multi-month effort if done manually. The actual scale is even larger than initially estimated: 189 structs (not ~160), 142 enums (103 top-level + 39 nested), 21 type aliases on iOS alone. Even with code generation, the Sharpie fixup and manual correction step is time-consuming.

**Mitigation**:
1. Phased delivery (Phase 0–3 complete, Phase 4 in progress)
2. Code-generate Swift wrapper boilerplate from `.swiftinterface`
3. Code-generate `Metadata.xml` entries from Android `api.xml`
4. Automate Sharpie invocation and `[Verify]` detection

**Status**: MITIGATED — Phase 0-3 complete (Android). 171 unit tests pass. Android build successful. Phase 4 (polish) in progress.

**Residual Risk**: Code generation may produce incorrect wrappers for complex types (generics, nested types). Mitigated by manual review of generated code and device test validation.

---

### R5: Objective-Sharpie Output Requires Heavy Manual Fixup

**Problem**: Sharpie annotates uncertain bindings with `[Verify]` attributes. These MUST be manually resolved. For 350+ types, this could be hundreds of `[Verify]` attributes.

**Mitigation**:
1. Run Sharpie per-phase (not all at once)
2. Automate removal of obvious `[Verify]` patterns
3. Budget manual fixup time per phase
4. Create validation scripts that check for unresolved `[Verify]` attributes

**Status**: DEFERRED — iOS binding currently uses stub `ApiDefinition.cs` and `StructsAndEnums.cs`. Full Sharpie run deferred until xcframework is built.

**Residual Risk**: Some `[Verify]` attributes require deep understanding of ObjC/Swift interop. May need iterative debugging on device.

---

## Medium Severity Risks

### R6: Generic Types Cannot Cross @objc Boundary

**Problem**: `CollectionOf<T>` and similar generic types cannot be `@objc`. The wrapper must use type-erased variants.

**Mitigation**: Create separate wrapper classes for each concrete generic instantiation used in the API. E.g., `HereStringCollection`, `HereMetadataCollection` instead of `HereCollectionOf<T>`.

**Status**: LOW IMPACT — Only 1 generic type found in the Swift interface: `CollectionOf<T>`. It exists ONLY on iOS, NOT in Android (discrepancy #1 in PLAN.md). Impact is limited.

---

### R7: Memory Management — Swift ARC vs .NET GC

**Problem**: The wrapper framework introduces objects managed by both Swift's ARC and .NET's GC. Retain cycles or premature disposal could cause crashes.

**Mitigation**:
1. Implement `IDisposable` on all C# wrapper types ✅
2. Use weak references for delegate protocols ✅
3. Test with instruments (iOS) / profiler (Android) for retain cycles ⏳
4. Follow MAUI handler disposal patterns ✅

**Status**: IMPLEMENTED — All service interfaces extend `IDisposable` via `IHereSdkService`. All service implementations follow the `Dispose(bool)` pattern. Runtime testing still needed.

---

### R8: Platform Behavior Differences

**Problem**: The same API may behave differently on Android vs iOS (e.g., error codes, default values, threading model).

**Mitigation**: Define unified error enums that are superset of both platforms. Document platform-specific behavior with `<remarks>` in XML docs. Test on both platforms for every feature.

**Status**: PARTIALLY MITIGATED — Unified error enums defined. XML docs with `<summary>` added to all public members. Platform-specific `<remarks>` not yet added.

**Verification**: The Android and iOS SDKs have the same module structure (Core, Maps, Routing, Search, Traffic, Transport) and very similar type names — they are designed to be parallel APIs. Most differences are in threading and UI lifecycle.

---

### R9: Build Requires macOS for iOS

**Problem**: iOS binding requires Xcode, Objective-Sharpie, and macOS for building the NativeBridge xcframework.

**Mitigation**: CI uses `macos-latest` runner. Document local development requirements (macOS + Xcode). Provide pre-built xcframework in the repo for developers without Xcode (if licensing allows).

**Status**: MITIGATED — CI workflows use macos-latest. Build scripts handle both platforms.

---

### R10: HERE SDK Updates May Break Bindings

**Problem**: When HERE releases a new SDK version, API changes may break existing bindings.

**Mitigation**:
1. Pin SDK version (4.25.5.0) in all references ✅
2. Create binding regeneration workflow ⏳ (planned but not created)
3. Version NuGet packages to match SDK version ✅
4. Code generation makes re-binding faster ✅

**Status**: PARTIALLY MITIGATED — SDK version pinned. NuGet versioning follows SDK version. Binding regeneration workflows not yet created.

---

## Low Severity Risks

### R11: Large Binary Sizes

**Problem**: The iOS xcframework is 831 MB (device 72 MB + simulator 114 MB). The Android AAR is 190 MB (64 MB classes + 130 MB native libs).

**Status**: ACCEPTED — NuGet packages embed native dependencies. Build time may be slow but runtime is fine (only the target architecture's .so is loaded). Consider stripping debug symbols for release builds.

---

### R12: Android Binding Generator Warnings

**Problem**: The .NET Android binding generator will produce many warnings for ambiguous overloads, name collisions, etc.

**Status**: MITIGATED — `Metadata.xml` transforms applied. `NoWarn` suppresses known binding warnings (CS0108, CS0618, CS8766, CS0535, BG8A00). `TreatWarningsAsErrors` set to `false` for binding project.

---

### R13: CarPlay / Android Auto Are Platform-Specific

**Problem**: CarPlay (iOS) and Android Auto are platform-specific features that don't map to a unified API.

**Status**: OUT OF SCOPE — Not included in the unified API. Documented as platform-specific extensions in the API surface catalog. Not planned for Phase 4.

---

### R14: `@_hasMissingDesignatedInitializers` — No Subclassing

**Problem**: 128 iOS SDK classes are marked `@_hasMissingDesignatedInitializers`, meaning they cannot be subclassed from Swift (or from C#).

**Status**: MITIGATED — Use containment (has-a) pattern in wrappers. Create instances via factory methods. No subclassing is needed — we wrap, not extend.

---

## Risk Summary

| ID | Risk | Severity | Status | Mitigation Confidence |
|---|---|---|---|---|
| R1 | Swift-only iOS SDK | HIGH | Partially mitigated (xcframework build pending) | HIGH |
| R2 | Swift structs can't be @objc | HIGH | Mitigated (pattern established) | HIGH |
| R3 | UInt32 enums can't be @objc | HIGH | Mitigated (pattern established) | HIGH |
| R4 | 350+ types to bind | HIGH | Mitigated (Phases 0-3 complete) | HIGH |
| R5 | Sharpie fixup | HIGH | Deferred (pending xcframework) | MEDIUM |
| R6 | Generic types | MEDIUM | Low impact (1 generic type) | HIGH |
| R7 | ARC vs GC | MEDIUM | Implemented (IDisposable), needs runtime testing | MEDIUM |
| R8 | Platform differences | MEDIUM | Partially mitigated (unified enums, XML docs pending remarks) | HIGH |
| R9 | macOS requirement | MEDIUM | Mitigated (CI on macos-latest) | HIGH |
| R10 | SDK updates | MEDIUM | Partially mitigated (version pinning) | MEDIUM |
| R11 | Binary sizes | LOW | Accepted | HIGH |
| R12 | Binding warnings | LOW | Mitigated (Metadata.xml + NoWarn) | HIGH |
| R13 | CarPlay/AndroidAuto | LOW | Out of scope | HIGH |
| R14 | Sealed classes | LOW | Mitigated (containment pattern) | HIGH |

## New Risks Identified During Implementation

### R15: .NET Version Migration (9 → 10)

**Problem**: The project migrated from .NET 9 to .NET 10 (preview) mid-development. CI workflows, plan docs, and some build scripts initially referenced net9.0 TFMs. The .NET 10 SDK (10.0.201) is a preview release and may have breaking changes or limited CI support.

**Mitigation**: All CI workflows and plan docs updated to reference .NET 10.0. `global.json` pins SDK version with `latestFeature` roll-forward. Monitor .NET 10 release schedule for GA stability.

**Status**: MITIGATED — All references updated. Builds succeed with .NET 10.

### R16: AAR Not in Git Repository

**Problem**: The HERE SDK AAR (~61 MB) is gitignored and must be manually placed before builds can succeed. CI will fail without it.

**Mitigation**: Document manual AAR placement. Create download script or use private package feed for CI.

**Status**: OPEN — CI gap acknowledged in 08-nuget-and-ci.md.

### R17: Test Project Source Linking

**Problem**: The unit test project uses `Compile Include` to link source files from the MAUI library rather than a `ProjectReference`. This is because a ProjectReference would pull in platform-specific TFMs (net10.0-android, net10.0-ios) that can't compile in a net10.0-only project. This means test coverage metrics may not reflect the actual compiled code paths.

**Mitigation**: This is the recommended pattern for testing cross-platform MAUI libraries without a device. Platform-specific code (Android/iOS implementations) is tested via device tests. Shared logic coverage is accurate.

**Status**: ACCEPTED — Pattern works correctly for unit testing shared logic.