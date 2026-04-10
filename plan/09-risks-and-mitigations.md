# Risks and Mitigations

## High Severity Risks

### R1: iOS Swift-Only SDK — Cannot Use Traditional Binding Approach

**Problem**: The iOS HERE SDK's ObjC bridge header exposes only 4 types. Traditional .NET iOS binding via Objective-Sharpie would bind only `HereMapView`, `SDKInternalInitializer`, `SDKMapViewInitializer`, `SDKNativeEngineHolder` — effectively useless for the other 350+ types.

**Mitigation**: Native Library Interop pattern — create a Swift wrapper framework that re-exposes all APIs as `@objc`-annotated types. This is the MAUI Community Toolkit's recommended approach for Swift-only SDKs.

**Residual Risk**: The wrapper framework adds build complexity and must be rebuilt when the HERE SDK updates. Mitigated by automating xcframework build in CI.

**Verification**: `arm64-apple-ios.swiftinterface` has 9,060 lines with 128 sealed classes, ~160 structs, ~110 enums, 35 protocols. Each must be wrapped.

---

### R2: Swift Structs Cannot Cross @objc Boundary

**Problem**: The iOS SDK uses ~160 Swift structs (e.g., `GeoCoordinates`, `RouteHandle`, `Waypoint`). Swift structs **cannot** be annotated with `@objc`. They must be wrapped as `NSObject` subclasses, with manual conversion in/out.

**Mitigation**: For each struct, create an `@objc` wrapper class with:
- Properties matching the struct's fields (as ObjC-compatible types)
- `toSwift()` method returning the original struct
- `static fromSwift()` factory creating the wrapper from a struct

**Residual Risk**: Performance overhead from struct ↔ class conversion. Mitigated by keeping wrappers thin and using conversion only at boundary points.

**Verification**: Check every struct in `.swiftinterface` — all conform to `Swift.Hashable`, so they're simple value types with straightforward property lists.

---

### R3: Swift Enums Are UInt32-Backed, Not Int-Backed

**Problem**: All ~110 iOS SDK enums are `Swift.UInt32`-backed (e.g., `RoutingError : Swift.UInt32`). `@objc` enums must be `NSInteger` (Int) backed. Direct bridging is impossible.

**Mitigation**: Create wrapper enums backed by `NSInteger` with explicit raw value mapping:
```swift
@objc public enum HereRoutingError: NSInteger {
    case none = 0
    case networkError = 1
    // Map from UInt32 raw values
}
```

**Residual Risk**: Risk of raw value drift between SDK versions. Mitigated by code-generating wrapper enums from `.swiftinterface` and verifying during build.

**Verification**: All enums in the swiftinterface are `Swift.UInt32, Swift.CaseIterable, Swift.Codable` — consistent pattern.

---

### R4: Large API Surface — 350+ Types to Bind

**Problem**: Binding 350+ types is a multi-month effort if done manually. The actual scale is even larger than initially estimated: 189 structs (not ~160), 142 enums (103 top-level + 39 nested), 21 type aliases on iOS alone. Even with code generation, the Sharpie fixup and manual correction step is time-consuming.

**Mitigation**:
1. Phased delivery (Phase 1: ~50 types, Phase 2: ~150 types, Phase 3: ~150 types)
2. Code-generate Swift wrapper boilerplate from `.swiftinterface`
3. Code-generate `Metadata.xml` entries from Android `api.xml`
4. Automate Sharpie invocation and `[Verify]` detection

**Residual Risk**: Code generation may produce incorrect wrappers for complex types (generics, nested types). Mitigated by manual review of generated code and device test validation.

---

### R5: Objective-Sharpie Output Requires Heavy Manual Fixup

**Problem**: Sharpie annotates uncertain bindings with `[Verify]` attributes. These MUST be manually resolved. For 350+ types, this could be hundreds of `[Verify]` attributes.

**Mitigation**:
1. Run Sharpie per-phase (not all at once)
2. Automate removal of obvious `[Verify]` patterns
3. Budget manual fixup time per phase
4. Create validation scripts that check for unresolved `[Verify]` attributes

**Residual Risk**: Some `[Verify]` attributes require deep understanding of ObjC/Swift interop. May need iterative debugging on device.

---

## Medium Severity Risks

### R6: Generic Types Cannot Cross @objc Boundary

**Problem**: `CollectionOf<T>` and similar generic types cannot be `@objc`. The wrapper must use type-erased variants.

**Mitigation**: Create separate wrapper classes for each concrete generic instantiation used in the API. E.g., `HereStringCollection`, `HereMetadataCollection` instead of `HereCollectionOf<T>`.

**Verification**: Only 1 generic type found in the Swift interface: `CollectionOf<T>`. Impact is limited.

---

### R7: Memory Management — Swift ARC vs .NET GC

**Problem**: The wrapper framework introduces objects managed by both Swift's ARC and .NET's GC. Retain cycles or premature disposal could cause crashes.

**Mitigation**:
1. Implement `IDisposable` on all C# wrapper types
2. Use weak references for delegate protocols
3. Test with instruments (iOS) / profiler (Android) for retain cycles
4. Follow MAUI handler disposal patterns

---

### R8: Platform Behavior Differences

**Problem**: The same API may behave differently on Android vs iOS (e.g., error codes, default values, threading model).

**Mitigation**: Define unified error enums that are superset of both platforms. Document platform-specific behavior with `[Remarks]` in XML docs. Test on both platforms for every feature.

**Verification**: The Android and iOS SDKs have the same module structure (Core, Maps, Routing, Search, Traffic, Transport) and very similar type names — they are designed to be parallel APIs. Most differences are in threading and UI lifecycle.

---

### R9: Build Requires macOS for iOS

**Problem**: iOS binding requires Xcode, Objective-Sharpie, and macOS for building the NativeBridge xcframework.

**Mitigation**: CI uses `macos-latest` runner. Document local development requirements (macOS + Xcode). Provide pre-built xcframework in the repo for developers without Xcode (if licensing allows).

---

### R10: HERE SDK Updates May Break Bindings

**Problem**: When HERE releases a new SDK version, API changes may break existing bindings.

**Mitigation**:
1. Pin SDK version (4.25.5.0) in all references
2. Create binding regeneration workflow (`.github/workflows/ios-bindings.yml`)
3. Version NuGet packages to match SDK version
4. Code generation makes re-binding faster

---

## Low Severity Risks

### R11: Large Binary Sizes

**Problem**: The iOS xcframework is 831 MB (device 72 MB + simulator 114 MB). The Android AAR is 190 MB (64 MB classes + 130 MB native libs).

**Mitigation**: NuGet packages embed native dependencies. Build time may be slow but runtime is fine (only the target architecture's .so is loaded). Consider stripping debug symbols for release builds.

---

### R12: Android Binding Generator Warnings

**Problem**: The .NET Android binding generator will produce many warnings for ambiguous overloads, name collisions, etc.

**Mitigation**: Iterate on `Metadata.xml` transforms. Start with empty transforms, build, fix warnings. Document known warnings and their fixes.

---

### R13: CarPlay / Android Auto Are Platform-Specific

**Problem**: CarPlay (iOS) and Android Auto are platform-specific features that don't map to a unified API.

**Mitigation**: Expose these as platform-specific extensions, not in the unified API. Document in the API surface catalog as platform-only.

**Verification**: Examples include `HelloMapCarPlay` (iOS) and `HelloMapAndroidAuto` (Android). These use `MapSurface` (Android) and `MapView` on CarPlay — very different paradigms.

---

### R14: `@_hasMissingDesignatedInitializers` — No Subclassing

**Problem**: 128 iOS SDK classes are marked `@_hasMissingDesignatedInitializers`, meaning they cannot be subclassed from Swift (or from C#).

**Mitigation**: Use containment (has-a) pattern in wrappers. Create instances via factory methods. No subclassing is needed — we wrap, not extend.

---

## Risk Summary

| ID | Risk | Severity | Mitigation Confidence |
|---|---|---|---|
| R1 | Swift-only iOS SDK | HIGH | HIGH — Native Library Interop is proven pattern |
| R2 | Swift structs can't be @objc | HIGH | HIGH — wrapper classes are mechanical |
| R3 | UInt32 enums can't be @objc | MEDIUM | HIGH — NSInteger mapping is mechanical |
| R4 | 350+ types to bind | HIGH | MEDIUM — code-gen + phased delivery |
| R5 | Sharpie fixup | HIGH | MEDIUM — per-phase, automated detection |
| R6 | Generic types | MEDIUM | HIGH — limited to `CollectionOf<T>` |
| R7 | ARC vs GC | MEDIUM | MEDIUM — needs careful testing |
| R8 | Platform differences | MEDIUM | HIGH — parallel API design |
| R9 | macOS requirement | MEDIUM | HIGH — CI on macos-latest |
| R10 | SDK updates | MEDIUM | MEDIUM — code-gen + version pinning |
| R11 | Binary sizes | LOW | HIGH — acceptable for SDK wrapper |
| R12 | Binding warnings | LOW | HIGH — iterative Metadata.xml |
| R13 | CarPlay/AndroidAuto | LOW | HIGH — platform-specific extensions |
| R14 | Sealed classes | LOW | HIGH — containment pattern |