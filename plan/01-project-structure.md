# Project Structure

## Solution Layout

```
here-sdk-for-maui/
├── .github/
│   └── workflows/
│       ├── build.yml                    # Build + test on PR
│       ├── publish.yml                  # Pack + push NuGet on tag
│       └── ios-bindings.yml            # iOS binding regeneration
│
├── plan/                                # This plan
│
├── src/
│   ├── HereSdk.Explore.Android.Binding/       # Android binding library
│   │   ├── HereSdk.Explore.Android.Binding.csproj
│   │   ├── Transforms/
│   │   │   └── Metadata.xml
│   │   └── Jars/
│   │       └── heresdk-explore-android-4.25.5.0.274356.aar
│   │
│   ├── HereSdk.Explore.iOS.NativeBridge/      # Swift wrapper framework (Xcode)
│   │   ├── HereSdk.Explore.iOS.NativeBridge.xcodeproj
│   │   ├── HereSdk.Explore.iOS.NativeBridge/
│   │   │   ├── NativeBridge.h
│   │   │   ├── Core/
│   │   │   │   ├── NativeSDKOptions.swift
│   │   │   │   ├── NativeSDKNativeEngine.swift
│   │   │   │   ├── NativeGeoCoordinates.swift
│   │   │   │   └── ...
│   │   │   ├── Maps/
│   │   │   │   ├── NativeMapView.swift
│   │   │   │   ├── NativeMapCamera.swift
│   │   │   │   └── ...
│   │   │   ├── Routing/
│   │   │   ├── Search/
│   │   │   ├── Traffic/
│   │   │   └── Transport/
│   │   ├── build-xcframework.sh          # Build FAT xcframework
│   │   └── Makefile
│   │
│   ├── HereSdk.Explore.iOS.Binding/     # iOS binding library (C#)
│   │   ├── HereSdk.Explore.iOS.Binding.csproj
│   │   ├── ApiDefinition.cs
│   │   ├── StructsAndEnums.cs
│   │   └── Libs/
│   │       └── HereSdk.Explore.iOS.NativeBridge.xcframework
│   │
│   ├── HereSdk.Explore.Maui/            # Cross-platform MAUI class library
│   │   ├── HereSdk.Explore.Maui.csproj
│   │   ├── HereSdk.cs                   # SDK initialization entry point
│   │   ├── Models/
│   │   │   ├── Core/
│   │   │   │   ├── GeoCoordinates.cs
│   │   │   │   ├── GeoBox.cs
│   │   │   │   ├── GeoCircle.cs
│   │   │   │   ├── GeoCorridor.cs
│   │   │   │   ├── GeoOrientation.cs
│   │   │   │   ├── GeoPolygon.cs
│   │   │   │   ├── GeoPolyline.cs
│   │   │   │   ├── Point2D.cs
│   │   │   │   ├── Size2D.cs
│   │   │   │   └── ...
│   │   │   ├── Routing/
│   │   │   │   ├── Route.cs
│   │   │   │   ├── RoutingOptions.cs
│   │   │   │   ├── Waypoint.cs
│   │   │   │   └── ...
│   │   │   ├── Search/
│   │   │   ├── Traffic/
│   │   │   └── Transport/
│   │   ├── Services/
│   │   │   ├── IHereSdkService.cs         # SDK init interface
│   │   │   ├── IMapService.cs
│   │   │   ├── IRoutingService.cs
│   │   │   ├── ISearchService.cs
│   │   │   ├── ITrafficService.cs
│   │   │   ├── HereSdkService.cs          # partial — shared
│   │   │   ├── MapService.cs              # partial — shared
│   │   │   ├── MapService.Android.cs
│   │   │   ├── MapService.iOS.cs
│   │   │   ├── RoutingService.cs
│   │   │   ├── RoutingService.Android.cs
│   │   │   ├── RoutingService.iOS.cs
│   │   │   ├── SearchService.cs
│   │   │   ├── SearchService.Android.cs
│   │   │   ├── SearchService.iOS.cs
│   │   │   ├── TrafficService.cs
│   │   │   ├── TrafficService.Android.cs
│   │   │   └── TrafficService.iOS.cs
│   │   ├── Controls/
│   │   │   ├── HereMapView.cs             # Cross-platform virtual view
│   │   │   └── IHereMapView.cs
│   │   ├── Handlers/
│   │   │   ├── HereMapViewHandler.cs      # partial — shared
│   │   │   ├── HereMapViewHandler.Android.cs
│   │   │   └── HereMapViewHandler.iOS.cs
│   │   └── PlatformConverters/
│   │       ├── GeoCoordinatesConverter.Android.cs
│   │       └── GeoCoordinatesConverter.iOS.cs
│   │
│   └── HereSdk.Explore.Maui.RefApp/     # Demo MAUI app
│       ├── HereSdk.Explore.Maui.RefApp.csproj
│       ├── App.xaml / App.xaml.cs
│       ├── MainPage.xaml / MainPage.xaml.cs
│       ├── ViewModels/
│       │   ├── MapViewModel.cs
│       │   ├── SearchViewModel.cs
│       │   ├── RoutingViewModel.cs
│       │   └── TrafficViewModel.cs
│       ├── Pages/
│       │   ├── MapPage.xaml
│       │   ├── SearchPage.xaml
│       │   ├── RoutingPage.xaml
│       │   └── TrafficPage.xaml
│       ├── Platforms/
│       │   ├── Android/
│       │   │   ├── AndroidManifest.xml
│       │   │   └── MainActivity.cs
│       │   └── iOS/
│       │       ├── Info.plist
│       │       └── AppDelegate.cs
│       └── Resources/
│
├── tests/
│   ├── HereSdk.Explore.Maui.Tests/            # xUnit (net9.0)
│   │   ├── HereSdk.Explore.Maui.Tests.csproj
│   │   ├── Models/
│   │   │   ├── GeoCoordinatesTests.cs
│   │   │   └── ...
│   │   └── Services/
│   │       ├── MapServiceTests.cs
│   │       ├── RoutingServiceTests.cs
│   │       ├── SearchServiceTests.cs
│   │       └── TrafficServiceTests.cs
│   │
│   └── HereSdk.Explore.Maui.DeviceTests/      # Platform device runner tests
│       ├── HereSdk.Explore.Maui.DeviceTests.csproj
│       ├── Android/
│       │   └── MapViewAndroidTests.cs
│       └── iOS/
│           └── MapViewiOSTests.cs
│
├── scripts/
│   ├── build.sh                         # Build everything
│   ├── build-android.sh                 # Build Android binding only
│   ├── build-ios-native.sh              # Build iOS Swift wrapper
│   ├── bind-ios.sh                      # Run Sharpie + fix bindings
│   ├── test.sh                          # Run all tests
│   ├── pack.sh                          # Create NuGet packages
│   └── clean.sh                         # Clean all artifacts
│
├── Directory.Build.props                # Shared build settings
├── Directory.Build.targets              # Shared build targets
├── global.json                          # Pin .NET SDK version
├── .gitignore
├── CLAUDE.md                            # AI assistant instructions
├── README.md                            # Project documentation
├── LICENSE
└── tmp/                                 # SDK archives (gitignored)
    ├── heresdk-explore-android-4.25.5.0.274356.zip
    └── heresdk-explore-ios-4.25.5.0.274356.zip
```

## Directory.Build.props

```xml
<Project>
  <PropertyGroup>
    <NetVersion>net9.0</NetVersion>
    <MauiPlatforms>$(NetVersion)-android;$(NetVersion)-ios</MauiPlatforms>
    <Nullable>enable</Nullable>
    <LangVersion>latest</LangVersion>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <HereSdkVersion>4.25.5.0</HereSdkVersion>
    <PackageVersion>4.25.5.0-alpha1</PackageVersion>
  </PropertyGroup>
</Project>
```

## Project File Details

### HereSdk.Explore.Android.Binding.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0-android</TargetFramework>
    <!-- AAR binding project -->
  </PropertyGroup>
  <ItemGroup>
    <AndroidLibrary Include="Jars\heresdk-explore-android-4.25.5.0.274356.aar" />
  </ItemGroup>
  <!-- Namespace remapping -->
  <ItemGroup>
    <AndroidNamespaceReplacement Include="com.here.sdk.core" Replacement="Here.Explore.Core" />
    <AndroidNamespaceReplacement Include="com.here.sdk.mapview" Replacement="Here.Explore.Maps" />
    <AndroidNamespaceReplacement Include="com.here.sdk.routing" Replacement="Here.Explore.Routing" />
    <AndroidNamespaceReplacement Include="com.here.sdk.search" Replacement="Here.Explore.Search" />
    <AndroidNamespaceReplacement Include="com.here.sdk.traffic" Replacement="Here.Explore.Traffic" />
    <AndroidNamespaceReplacement Include="com.here.sdk.transport" Replacement="Here.Explore.Transport" />
    <AndroidNamespaceReplacement Include="com.here.sdk.animation" Replacement="Here.Explore.Animation" />
    <AndroidNamespaceReplacement Include="com.here.sdk.gestures" Replacement="Here.Explore.Gestures" />
    <AndroidNamespaceReplacement Include="com.here.sdk.core.engine" Replacement="Here.Explore.Engine" />
    <AndroidNamespaceReplacement Include="com.here.sdk.core.threading" Replacement="Here.Explore.Threading" />
    <AndroidNamespaceReplacement Include="com.here.sdk.gestures" Replacement="Here.Explore.Gestures" />
    <AndroidNamespaceReplacement Include="com.here.sdk.engine" Replacement="Here.Explore.Engine" />
    <!-- Note: com.here.sdk.core.utilities is empty, no mapping needed -->
  </ItemGroup>
</Project>
```

### HereSdk.Explore.iOS.Binding.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0-ios</TargetFramework>
    <IsBindingProject>true</IsBindingProject>
  </PropertyGroup>
  <ItemGroup>
    <ObjcBindingApiDefinition Include="ApiDefinition.cs" />
    <ObjcBindingCoreSource Include="StructsAndEnums.cs" />
  </ItemGroup>
  <ItemGroup>
    <NativeReference Include="Libs\HereSdk.Explore.iOS.NativeBridge.xcframework">
      <Kind>Framework</Kind>
      <Frameworks>Foundation UIKit CoreLocation</Frameworks>
      <LinkerFlags>-L "$(XcodeDeveloperDirectory)/Toolchains/XcodeDefault.xctoolchain/usr/lib/swift/iphonesimulator" -L "$(XcodeDeveloperDirectory)/Toolchains/XcodeDefault.xctoolchain/usr/lib/swift/iphoneos" -Wl,-rpath -Wl,@executable_path/Frameworks</LinkerFlags>
    </NativeReference>
  </ItemGroup>
</Project>
```

### HereSdk.Explore.Maui.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net9.0-android;net9.0-ios</TargetFrameworks>
    <UseMaui>true</UseMaui>
    <SingleProject>true</SingleProject>
    <PackageId>HereSdk.Explore.Maui</PackageId>
  </PropertyGroup>
  <ItemGroup Condition="$(TargetFramework.Contains('android'))">
    <ProjectReference Include="..\HereSdk.Explore.Android.Binding\HereSdk.Explore.Android.Binding.csproj" />
  </ItemGroup>
  <ItemGroup Condition="$(TargetFramework.Contains('ios'))">
    <ProjectReference Include="..\HereSdk.Explore.iOS.Binding\HereSdk.Explore.iOS.Binding.csproj" />
  </ItemGroup>
</Project>
```

### HereSdk.Explore.Maui.RefApp.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">
  <PropertyGroup>
    <TargetFrameworks>net9.0-android;net9.0-ios</TargetFrameworks>
    <UseMaui>true</UseMaui>
    <OutputType>Exe</OutputType>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\HereSdk.Explore.Maui\HereSdk.Explore.Maui.csproj" />
  </ItemGroup>
</Project>
```

## Native SDK Artifacts (gitignored)

The `tmp/` directory holds the original SDK archives and is **not committed**. The extracted binaries needed for binding are:

| Artifact | Destination | Notes |
|---|---|---|
| `heresdk-explore-android-*.aar` | `src/HereSdk.Explore.Android.Binding/Jars/` | ~64 MB |
| `heresdk-explore-ios-*.xcframework` | Built by `HereSdk.Explore.iOS.NativeBridge` | ~831 MB raw |
| `heresdk-explore-mock-*.jar` | `tests/` | Testing mock |
| iOS xcframework | `src/HereSdk.Explore.iOS.Binding/Libs/` | Built output |

Scripts will handle extraction and placement.
