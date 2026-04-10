# Android Binding Strategy

## Overview

The Android binding is **straightforward**. The HERE SDK provides a standard AAR containing:
- `classes.jar` (950 KB) — all Java/Kotlin bytecode
- `jni/{arm64-v8a,armeabi-v7a,x86,x86_64}/libheresdk.so` — native libraries (~130 MB total)
- `AndroidManifest.xml` — declares `minSdkVersion="24"`, `ACCESS_NETWORK_STATE`, `INTERNET`
- `assets/` — map rendering resources (geoviz configs, shaders, 3D models, logos)

The .NET Android binding generator will consume the AAR directly via the `AndroidLibrary` build action and produce C# wrappers automatically.

## AAR Structure (Validated)

```
heresdk-explore-android-4.25.5.0.274356.aar
├── AndroidManifest.xml          (minSdkVersion=24, INTERNET, ACCESS_NETWORK_STATE)
├── classes.jar                  (Java bytecode — 950 KB)
├── jni/
│   ├── arm64-v8a/libheresdk.so  (34 MB)
│   ├── armeabi-v7a/libheresdk.so (24 MB)
│   ├── x86/libheresdk.so       (36 MB)
│   └── x86_64/libheresdk.so    (36 MB)
├── assets/
│   ├── HERE_logo_full.svg
│   ├── arrow_cap_medium.obj
│   ├── location_indicator_*.obj/png
│   ├── geoviz/assets/oslo/     (map style configs)
│   └── magma/                  (shader configs)
└── R.txt
```

## Java Package → C# Namespace Mapping

| Java Package | C# Namespace | API Types (verified from Javadoc) |
|---|---|---|
| `com.here.sdk.core` | `Here.Explore.Core` | 36 classes, 12 enums, 4 interfaces — GeoCoordinates, GeoBox, Angle, Location, etc. |
| `com.here.sdk.core.engine` | `Here.Explore.Engine` | 21 classes, 9 enums, 3 interfaces — SDKNativeEngine, SDKOptions, Authentication, AuthenticationMode, LogControl, SDKBuildInformation, SDKLogger |
| `com.here.sdk.core.errors` | `Here.Explore.Core.Errors` | 1 exception — InstantiationErrorException |
| `com.here.sdk.core.threading` | `Here.Explore.Threading` | 1 class, 1 enum, 4 interfaces — TaskHandle, Runnable |
| `com.here.sdk.animation` | `Here.Explore.Animation` | 9 classes, 7 enums, 4 exceptions, 1 interface — Easing, MapCameraAnimation* |
| `com.here.sdk.mapview` | `Here.Explore.Maps` | 76 classes, 38 enums, 12 exceptions, 14 interfaces — MapView, MapCamera, MapScene, MapMarker*, etc. |
| `com.here.sdk.mapview.datasource` | `Here.Explore.Maps.DataSource` | 32 classes, 3 enums, 17 interfaces — custom tile sources, data sources |
| `com.here.sdk.routing` | `Here.Explore.Routing` | 85 classes, 41 enums, 4 interfaces — RoutingEngine, Route, Maneuver, etc. |
| `com.here.sdk.search` | `Here.Explore.Search` | 50 classes, 11 enums, 1 exception, 7 interfaces — SearchEngine, Place, Suggestion, etc. |
| `com.here.sdk.traffic` | `Here.Explore.Traffic` | 9 classes, 6 enums, 5 interfaces — TrafficEngine, TrafficFlow, TrafficIncident |
| `com.here.sdk.transport` | `Here.Explore.Transport` | 25 classes, 11 enums — VehicleSpecification builders, transport profiles |
| `com.here.sdk.gestures` | `Here.Explore.Gestures` | 4 classes, 2 enums, 7 interfaces — Gestures, gesture listeners |
| `com.here.sdk.engine` | `Here.Explore.Engine` | 1 class — InitProvider |
| `com.here.time` | `Here.Explore.Time` | 1 class — Duration |

> **Note**: `com.here.sdk.core.utilities` is empty (no types) — omit from namespace remapping.

## Metadata.xml Transforms

The binding generator produces `api.xml` from the Java bytecode. We customize it via `Transforms/Metadata.xml`.

### Key Transform Categories

#### 1. Namespace Remapping

Use `AndroidNamespaceReplacement` items in .csproj (already configured) rather than individual `managedName` attrs.

#### 2. Listener → Event Pattern

Java listener interfaces should become C# events. For each listener:

```xml
<!-- Example: MapCameraListener → MapCamera events -->
<attr path="/api/package[@name='com.here.sdk.mapview']/interface[@name='MapCameraListener']"
      name="managedName">MapCameraDelegate</attr>
<attr path="/api/package[@name='com.here.sdk.mapview']/interface[@name='MapCameraListener']/method[@name='onCameraStateChanged']"
      name="eventName">CameraStateChanged</attr>
```

#### 3. Callback Interfaces → EventHandler

Java callback interfaces with a single method should become `EventHandler<TEventArgs>`:

```xml
<!-- RoutingEngine.CalculateRouteCallback -->
<attr path="/api/package[@name='com.here.sdk.routing']/interface[@name='CalculateRouteCallback']"
      name="managedName">RouteCalculatedHandler</attr>
```

#### 4. Remove Internal/Implementation Classes

```xml
<!-- Remove internal Android platform classes -->
<remove-node path="/api/package[@name='com.here.sdk']/class[@name='AndroidAssetsLoader']" />
<remove-node path="/api/package[@name='com.here.sdk']/class[@name='AndroidContextConverter']" />
<remove-node path="/api/package[@name='com.here.sdk']/class[starts-with(@name,'Android')]" />
<!-- Remove all *Impl classes -->
<remove-node path="/api/package/class[substring(@name, string-length(@name)-3)='Impl']" />
<remove-node path="/api/package/class[substring(@name, string-length(@name)-8)='Internal']" />
```

#### 5. Fix Method Names with Java Keywords

```xml
<attr path="/api/package[@name='com.here.sdk.core']/class[@name='GeoCoordinates']/method[@name='toString']"
      name="managedName">ToString</attr>
```

## Build Process

```bash
# 1. Place AAR in Jars/
cp tmp/heresdk-explore-android-*/heresdk-explore-android-*.aar src/HereSdk.Explore.Android.Binding/Jars/

# 2. Build the binding
dotnet build src/HereSdk.Explore.Android.Binding/HereSdk.Explore.Android.Binding.csproj

# 3. The binding generator creates api.xml, then applies Metadata.xml transforms
#    Output: bin/Debug/net9.0-android/HereSdk.Explore.Android.Binding.dll

# 4. Verify by checking generated C# types match expected count
dotnet build --verbosity detailed 2>&1 | grep "warning"  # Check for binding warnings
```

## Mock JAR for Testing

The SDK includes `heresdk-explore-mock-4.25.5.0.274356.jar` (765 KB) which provides mock implementations for testing. This will be used in the test project:

```xml
<ItemGroup>
  <AndroidLibrary Include="Jars\heresdk-explore-mock-4.25.5.0.274356.jar" />
</ItemGroup>
```

## Validation Checklist (Per Phase)

For each type being bound:

- [ ] Type exists in `tmp/android-inspect/api-reference/` Javadoc
- [ ] Constructor/method signatures match expected pattern
- [ ] Return types are correctly mapped (Java → C#)
- [ ] Listener interfaces produce C# events
- [ ] Async callbacks can be wrapped as `Task<T>`
- [ ] No binding warnings for the type
- [ ] Type is accessible from `HereSdk.Explore.Maui` project reference

## Binding Warnings Strategy

The .NET Android binding generator WILL produce warnings (ambiguous overloads, name collisions, etc.). Handle them:

1. **Ambiguous method overloads**: Use `managedName` to disambiguate
2. **Name collisions**: Rename one variant with `managedName`
3. **Invalid C# identifiers**: Rename with `managedName`
4. **Deprecated Java members**: Remove with `<remove-node>` or suppress
5. **Generic type erasure**: May need `[JavaTypeParameters]` attribute

The first build will produce a `Transforms/Metadata.xml` seed from the generator warnings. We'll iterate on this.