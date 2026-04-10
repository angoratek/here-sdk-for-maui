# iOS Binding Strategy — Native Library Interop

## The Problem

The HERE SDK for iOS is **Swift-first**. Traditional .NET iOS bindings use Objective-Sharpie against ObjC headers. But the HERE SDK's ObjC bridge header (`heresdk-Swift.h`) exposes only **4 types**:

```
@objc(HereMapView)           open class MapView : UIView        // the map view
@objc                          class SDKInternalInitializer     // internal init
@objc                          class SDKMapViewInitializer      // internal init
@objc                          class SDKNativeEngineHolder      // singleton holder
```

The **real API** (350+ types) lives in the Swift interface file. You cannot bind Swift interfaces directly with .NET — you need ObjC-visible types.

**Important corrections from verification:**
- iOS has **189 structs** (not ~160), **142 enums** (103 top-level + 39 nested), and **21 type aliases** (not 20)
- `RefreshRouteOptions` is sealed AND deprecated ("Will be removed in v4.28.0. Use `RoutingOptions` instead") — do NOT bind it
- `IconProvider` is the only genuinely non-sealed, non-@objc public class (besides `MapView`)
- 39 nested enums (e.g., `Easing.InstantiationErrorCode`, `MapMarker.TextStyle.Placement`, `MapMeasure.Kind`) need NSInteger wrappers too

## The Solution: Native Library Interop

Create a **Swift wrapper framework** (`HereSdk.Explore.iOS.NativeBridge`) that:
1. Imports the HERE SDK as a dependency
2. For each HERE SDK type, creates an `@objc`-annotated wrapper class
3. Exposes only the APIs we need through ObjC-compatible signatures
4. Builds as an xcframework (device + simulator)
5. We then run Objective-Sharpie on the **wrapper's** generated ObjC header

### Why This Works

- The wrapper framework's ObjC header will contain **all** our wrapper types
- Objective-Sharpie can process these headers normally
- The resulting `ApiDefinition.cs` maps our wrappers, not the raw HERE SDK
- Consumers of the MAUI library never see the wrapper — it's an implementation detail

## Architecture

```
┌─────────────────────────────────┐
│  HereSdk.Explore.Maui (C#)      │
│  Cross-platform unified API     │
└──────────┬──────────────────────┘
           │ references
┌──────────▼──────────────────────┐
│  HereSdk.Explore.iOS.Binding    │
│  ApiDefinition.cs (Sharpie-gen) │
│  StructsAndEnums.cs             │
└──────────┬──────────────────────┘
           │ links
┌──────────▼──────────────────────┐
│  HereSdk.Explore.iOS.NativeBridge│
│  (Swift wrapper xcframework)     │
│  @objc classes wrapping HERE SDK │
└──────────┬──────────────────────┘
           │ depends on
┌──────────▼──────────────────────┐
│  heresdk.xcframework (HERE SDK) │
│  Swift-only, 350+ types          │
└─────────────────────────────────┘
```

## Swift Wrapper Design Patterns

### Pattern 1: Class Wrapping

For each HERE SDK `public class`, create an `@objc` wrapper:

```swift
// NativeBridge/Routing/NativeRoutingEngine.swift
import heresdk

@objc(HereRoutingEngine)
public class HereRoutingEngine: NSObject {
    private let engine: RoutingEngine

    @objc public init() {
        self.engine = RoutingEngine()
        super.init()
    }

    @objc public func calculateRoute(
        waypoints: [HereWaypoint],
        options: HereRoutingOptions,
        completion: @escaping (HereRoutingError?, [HereRoute]?) -> Void
    ) {
        let swiftWaypoints = waypoints.map { $0.toSwift() }
        let swiftOptions = options.toSwift()
        engine.calculateRoute(waypoints: swiftWaypoints, options: swiftOptions) { error, routes in
            completion(error, routes?.map { HereRoute(from: $0) })
        }
    }
}
```

### Pattern 2: Struct → ObjC Class Conversion

Swift structs **cannot** be `@objc`. For each HERE SDK struct that crosses the boundary, create a wrapper class:

```swift
// NativeBridge/Core/NativeGeoCoordinates.swift
import heresdk

@objc(HereGeoCoordinates)
public class HereGeoCoordinates: NSObject {
    @objc public var latitude: Double
    @objc public var longitude: Double

    @objc public init(latitude: Double, longitude: Double) {
        self.latitude = latitude
        self.longitude = longitude
        super.init()
    }

    // Convert to Swift struct for internal use
    func toSwift() -> GeoCoordinates {
        GeoCoordinates(latitude: latitude, longitude: longitude)
    }

    // Convert from Swift struct for output
    static func from(_ swift: GeoCoordinates) -> HereGeoCoordinates {
        HereGeoCoordinates(latitude: swift.latitude, longitude: swift.longitude)
    }
}
```

### Pattern 3: Enum → NSInteger Conversion

Swift enums backed by `UInt32` can't be directly `@objc` unless they're `Int`-backed. Create NSInteger wrappers:

```swift
// NativeBridge/Core/NativeEnums.swift
import heresdk

@objc public enum HereRoutingError: NSInteger {
    case none = 0
    case networkError = 1
    case httpError = 2
    // ... map from RoutingError raw values

    func toSwift() -> RoutingError? {
        return RoutingError(rawValue: UInt32(self.rawValue)) ?? .none
    }
}
```

### Pattern 4: Protocol → @objc Protocol

Delegate protocols become `@objc` protocols:

```swift
// NativeBridge/Maps/NativeMapCameraDelegate.swift
import heresdk

@objc(HereMapCameraDelegate)
public protocol HereMapCameraDelegate: AnyObject {
    @objc optional func mapView(_ mapView: HereMapView, didChangeCameraState state: HereCameraState)
}
```

### Pattern 5: Closure → Completion Handler

Swift completion handlers map to `@objc` completion blocks:

```swift
// Already shown in Pattern 1 — completion: @escaping (Error?, [Result]?) -> Void
```

### Pattern 6: MapView — The Open Class

`MapView` is the ONLY open class and the ONLY type exposed in ObjC by the SDK itself. We get it "for free" but need a handler:

```swift
// NativeBridge/Maps/NativeMapView.swift
import heresdk

@objc(HereMapBridgeView)
public class HereMapBridgeView: NSObject {
    // MapView is already @objc(HereMapView) from the SDK
    // We don't need to wrap it — we can reference it directly from C#
    // But we provide factory and configuration methods

    @objc public static func createMapView() -> HereMapView {
        return HereMapView()
    }
}
```

## Build Process

### Step 1: Create Xcode Project

```bash
# Create framework project in src/HereSdk.Explore.iOS.NativeBridge/
# Add heresdk.xcframework as dependency
# Configure for minimum iOS 15.2
```

### Step 2: Build xcframework

```bash
#!/bin/bash
# build-xcframework.sh

SCHEME="HereSdkExploreNativeBridge"
FRAMEWORK_NAME="HereSdkExploreNativeBridge"
OUTPUT_DIR="./build"

# Build for device
xcodebuild archive \
    -scheme "$SCHEME" \
    -sdk iphoneos \
    -configuration Release \
    -archivePath "$OUTPUT_DIR/ios.xcarchive" \
    SKIP_INSTALL=NO \
    BUILD_LIBRARY_FOR_DISTRIBUTION=YES

# Build for simulator
xcodebuild archive \
    -scheme "$SCHEME" \
    -sdk iphonesimulator \
    -configuration Release \
    -archivePath "$OUTPUT_DIR/ios-simulator.xcarchive" \
    SKIP_INSTALL=NO \
    BUILD_LIBRARY_FOR_DISTRIBUTION=YES

# Create xcframework
xcodebuild -create-xcframework \
    -framework "$OUTPUT_DIR/ios.xcarchive/Products/Library/Frameworks/$FRAMEWORK_NAME.framework" \
    -framework "$OUTPUT_DIR/ios-simulator.xcarchive/Products/Library/Frameworks/$FRAMEWORK_NAME.framework" \
    -output "$OUTPUT_DIR/$FRAMEWORK_NAME.xcframework"

# Copy to iOS Binding project
cp -R "$OUTPUT_DIR/$FRAMEWORK_NAME.xcframework" \
    ../HereSdk.Explore.iOS.Binding/Libs/
```

### Step 3: Run Objective-Sharpie

```bash
#!/bin/bash
# bind-ios.sh

NATIVE_BRIDGE_DIR="src/HereSdk.Explore.iOS.NativeBridge"
BINDING_DIR="src/HereSdk.Explore.iOS.Binding"
XCFRAMEWORK="$BINDING_DIR/Libs/HereSdkExploreNativeBridge.xcframework"
HEADERS="$XCFRAMEWORK/ios-arm64/HereSdkExploreNativeBridge.framework/Headers"

# Get SDK version
IOS_SDK=$(xcrun --sdk iphoneos --show-sdk-path)
IOS_SDK_VERSION=$(xcrun --sdk iphoneos --show-sdk-version)

# Run Sharpie
sharpie bind \
    --sdk=iphoneos"$IOS_SDK_VERSION" \
    --output="$BINDING_DIR/SharpieOutput" \
    --namespace="Here.Explore.iOS" \
    --scope="$HEADERS" \
    "$HEADERS/HereSdkExploreNativeBridge-Swift.h" \
    -arch arm64
```

### Step 4: Manual Fixup

1. Copy relevant parts from `SharpieOutput/ApiDefinition.cs` to `ApiDefinition.cs`
2. Copy enums/structs to `StructsAndEnums.cs`
3. Remove all `[Verify]` attributes — resolve each one manually
4. Remove the `using` directive that references the framework at the top
5. Fix type mismatches (NSInteger → nint, CGFloat → nfloat, etc.)
6. Add `[Async]` attributes for completion-handler methods

### Step 5: Validate Binding

```bash
dotnet build src/HereSdk.Explore.iOS.Binding/HereSdk.Explore.iOS.Binding.csproj
```

## Type Inventory — What Must Be Wrapped

**Scale**: 128 sealed classes, 1 open class (MapView), 1 public non-sealed class (IconProvider), 2 @objc bridge classes, 189 structs, 142 enums (103 top-level + 39 nested), 35 protocols, 21 type aliases.

**Deprecation note**: `RefreshRouteOptions` is marked deprecated ("Will be removed in v4.28.0. Use `RoutingOptions` instead"). Do NOT bind it — use `RoutingOptions` only.

**Nested enums**: 39 nested enums inside parent classes (e.g., `Easing.InstantiationErrorCode`, `MapMarker.TextStyle.Placement`, `MapMeasure.Kind`, `MapContentSettings.TrafficRefreshPeriodErrorCode`, `SDKNativeEngine.PurgeMemoryStrategy`) each need NSInteger wrapper enums.

### Phase 1: Core + MapView (MUST wrap first)

| Swift Type | ObjC Wrapper Class | Priority |
|---|---|---|
| `SDKNativeEngine` | `HereSdkEngine` | P0 — SDK init |
| `SDKOptions` | `HereSdkOptions` | P0 — SDK init |
| `GeoCoordinates` | `HereGeoCoordinates` | P0 — used everywhere |
| `GeoBox` | `HereGeoBox` | P0 — map viewport |
| `GeoCircle` | `HereGeoCircle` | P1 |
| `GeoCorridor` | `HereGeoCorridor` | P1 |
| `GeoOrientation` | `HereGeoOrientation` | P1 |
| `GeoPolygon` | `HereGeoPolygon` | P1 |
| `GeoPolyline` | `HereGeoPolyline` | P1 |
| `Point2D` | `HerePoint2D` | P0 |
| `Size2D` | `HereSize2D` | P1 |
| `Location` | `HereLocation` | P0 |
| `MapView` | (already ObjC-visible) | P0 |
| `MapCamera` | `HereMapCamera` | P0 |
| `MapScene` | `HereMapScene` | P0 |
| `MapScheme` | `HereMapScheme` (NSInteger enum) | P0 |
| `Authentication` | `HereAuthentication` | P0 |
| `AuthenticationMode` | `HereAuthenticationMode` | P0 |

### Phase 2: Routing + Search

| Swift Type | ObjC Wrapper Class | Priority |
|---|---|---|
| `RoutingEngine` | `HereRoutingEngine` | P0 |
| `IsolineRoutingEngine` | `HereIsolineRoutingEngine` | P1 |
| `TransitRoutingEngine` | `HereTransitRoutingEngine` | P1 |
| `SearchEngine` | `HereSearchEngine` | P0 |
| `Route` | `HereRoute` | P0 |
| `Waypoint` | `HereWaypoint` | P0 |
| `RoutingOptions` | `HereRoutingOptions` | P0 |
| `CarOptions` | `HereCarOptions` | P0 |
| `TruckOptions` | `HereTruckOptions` | P1 |
| `PedestrianOptions` | `HerePedestrianOptions` | P1 |
| `TextQuery` | `HereTextQuery` | P0 |
| `CategoryQuery` | `HereCategoryQuery` | P1 |
| `Place` | `HerePlace` | P0 |
| `SearchOptions` | `HereSearchOptions` | P0 |
| `Suggestion` | `HereSuggestion` | P0 |
| ... | (see 05-api-surface-catalog.md for complete list) | |

### Phase 3: Traffic + Advanced

All remaining types from the `Traffic` module, advanced `Maps` features (markers, polylines, polygons, custom layers, gestures, animations), and `Transport` module.

## Risks & Mitigations

| Risk | Severity | Mitigation |
|---|---|---|
| **350+ types to wrap in Swift** | HIGH | Code-gen the boilerplate from `.swiftinterface`; phase delivery; start with most-used types |
| **Swift structs cannot be `@objc`** | HIGH | Create NSObject wrapper classes for each struct; add `toSwift()`/`fromSwift()` converters |
| **Swift enums are `UInt32`, not `Int`** | MEDIUM | Create `NSInteger`-backed wrapper enums; map raw values |
| **Generic types can't cross ObjC boundary** | MEDIUM | Type-erased wrappers: `CollectionOf<T>` → `HereCollectionOf` (untyped or multiple typed variants) |
| **Closures with complex types** | MEDIUM | Use delegate protocols instead of closures for complex callbacks |
| **128 sealed classes — no subclassing** | LOW | Wrap via containment (has-a), not inheritance |
| **Build requires macOS + Xcode** | LOW | CI uses `macos-latest`; document local setup |
| **Swift ABI changes between versions** | MEDIUM | Pin to minimum iOS 15.2; test on 15.x, 16.x, 17.x |
| **Objective-Sharpie output needs heavy manual fixup** | HIGH | Budget time for fixup; create validation scripts; check every `[Verify]` |
| **Memory management (Swift ARC vs .NET GC)** | HIGH | Careful ownership semantics in wrappers; test for retain cycles; use weak references where appropriate |
| **Large binary sizes** | LOW | xcframework is 831 MB; consider stripping debug symbols for release |

## Code Generation Strategy

Since we need ~128 class wrappers + ~160 struct wrappers + ~110 enum wrappers, we'll write a **code generator** that:

1. Parses `arm64-apple-ios.swiftinterface` (9,060 lines)
2. For each `public class` → generates `@objc` wrapper Swift class
3. For each `public struct` → generates `@objc` wrapper NSObject class with `toSwift()`/`fromSwift()`
4. For each `public enum : UInt32` → generates `@objc` NSInteger enum with raw value mapping
5. For each `public protocol : AnyObject` → generates `@objc` protocol
6. For each `typealias` closure → generates `@objc` delegate protocol or completion block

The generator will be a simple Python/Node script in `scripts/generate-ios-wrappers.py`.

## Validation Checklist (Per Type)

For each wrapped type:

- [ ] Swift type exists in `.swiftinterface`
- [ ] ObjC wrapper compiles in Xcode
- [ ] ObjC header is generated (check `build/.../Headers/*.h`)
- [ ] Sharpie generates `ApiDefinition.cs` entry
- [ ] `[Verify]` attributes are resolved
- [ ] C# binding compiles
- [ ] Type is accessible from `HereSdk.Explore.Maui` project
- [ ] Round-trip test: C# → ObjC → Swift → HERE SDK → Swift → ObjC → C#