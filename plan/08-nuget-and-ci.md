# NuGet Packaging and CI/CD

## NuGet Packages

### Package Structure

Three packages are published to NuGet:

| Package ID | Project | Contents |
|---|---|---|
| `HereSdk.Explore.Android.Binding` | `HereSdk.Explore.Android.Binding` | Android AAR binding DLL |
| `HereSdk.Explore.iOS.Binding` | `HereSdk.Explore.iOS.Binding` | iOS xcframework binding DLL |
| `HereSdk.Explore.Maui` | `HereSdk.Explore.Maui` | Cross-platform MAUI lib (depends on above) |

Most consumers will only reference `HereSdk.Explore.Maui`, which pulls in the platform bindings transitively.

### Versioning

- Follow the HERE SDK version: `4.25.5.0`
- Pre-release suffix: `-alpha1`, `-beta1`, `-rc1`
- Stable: `4.25.5.0`
- CI build: `4.25.5.0-ci.{build_number}`

```xml
<!-- In each .csproj -->
<PropertyGroup>
  <PackageId>HereSdk.Explore.Maui</PackageId>
  <Version>$(PackageVersion)</Version>
  <Description>HERE SDK Explore Edition for .NET MAUI — cross-platform maps, routing, search, and traffic</Description>
  <PackageTags>here;sdk;maui;maps;routing;search;traffic;navigation</PackageTags>
  <PackageProjectUrl>https://github.com/{org}/here-sdk-for-maui</PackageProjectUrl>
  <PackageLicenseExpression>MIT</PackageLicenseExpression>
  <PackageReadmeFile>README.md</PackageReadmeFile>
  <PackageIcon>here-logo.png</PackageIcon>
</PropertyGroup>
```

### Packing Commands

```bash
# Pack all libraries
dotnet pack src/HereSdk.Explore.Android.Binding -c Release -p:PackageVersion=4.25.5.0
dotnet pack src/HereSdk.Explore.iOS.Binding -c Release -p:PackageVersion=4.25.5.0
dotnet pack src/HereSdk.Explore.Maui -c Release -p:PackageVersion=4.25.5.0
```

### Package Verification

After packing, verify:

```bash
# List contents
dotnet nuget locals all --list
nuget explore HereSdk.Explore.Maui.4.25.5.0.nupkg

# Check that platform-specific libs are present
# lib/net9.0-android/HereSdk.Explore.Maui.dll
# lib/net9.0-android/HereSdk.Explore.Android.Binding.dll
# lib/net9.0-ios/HereSdk.Explore.Maui.dll
# lib/net9.0-ios/HereSdk.Explore.iOS.Binding.dll
```

### Consuming the Package

```xml
<!-- In a MAUI app's .csproj -->
<ItemGroup>
  <PackageReference Include="HereSdk.Explore.Maui" Version="4.25.5.0" />
</ItemGroup>
```

```csharp
// In MauiProgram.cs
using Here.Explore.Maui;

builder.UseHereSdkExplore(new HereSdkOptions
{
    AccessKeyId = "YOUR_KEY",
    AccessKeySecret = "YOUR_SECRET"
});
```

## CI/CD — GitHub Actions

### Build Pipeline (`.github/workflows/build.yml`)

```yaml
name: Build & Test

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  build-android:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'
      - run: dotnet workload install maui-android
      - run: dotnet build src/HereSdk.Explore.Android.Binding -c Release
      - run: dotnet build src/HereSdk.Explore.Maui -f net9.0-android -c Release
      - run: dotnet test tests/HereSdk.Explore.Maui.Tests -c Release

  build-ios:
    runs-on: macos-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'
      - run: dotnet workload install maui-ios
      - name: Build iOS NativeBridge
        run: |
          cd src/HereSdk.Explore.iOS.NativeBridge
          ./build-xcframework.sh
      - name: Run Sharpie + fix bindings
        run: ./scripts/bind-ios.sh
      - run: dotnet build src/HereSdk.Explore.iOS.Binding -c Release
      - run: dotnet build src/HereSdk.Explore.Maui -f net9.0-ios -c Release
      - run: dotnet test tests/HereSdk.Explore.Maui.Tests -c Release

  device-tests-android:
    runs-on: macos-latest
    needs: build-android
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'
      - run: dotnet workload install maui-android
      - name: Create Android emulator
        uses: reactivecircus/android-emulator-runner@v2
        with:
          api-level: 34
          target: default
          arch: x86_64
          script: dotnet test tests/HereSdk.Explore.Maui.DeviceTests -f net9.0-android -c Release

  device-tests-ios:
    runs-on: macos-latest
    needs: build-ios
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'
      - run: dotnet workload install maui-ios
      - run: dotnet test tests/HereSdk.Explore.Maui.DeviceTests -f net9.0-ios -c Release
```

### Publish Pipeline (`.github/workflows/publish.yml`)

```yaml
name: Publish NuGet

on:
  push:
    tags:
      - 'v*'

jobs:
  publish:
    runs-on: macos-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'
      - run: dotnet workload install maui
      - name: Build iOS NativeBridge
        run: |
          cd src/HereSdk.Explore.iOS.NativeBridge
          ./build-xcframework.sh
      - name: Pack
        run: |
          dotnet pack src/HereSdk.Explore.Android.Binding -c Release -p:PackageVersion=${{ github.ref_name }}
          dotnet pack src/HereSdk.Explore.iOS.Binding -c Release -p:PackageVersion=${{ github.ref_name }}
          dotnet pack src/HereSdk.Explore.Maui -c Release -p:PackageVersion=${{ github.ref_name }}
      - name: Publish to NuGet
        run: dotnet nuget push **/*.nupkg --source https://api.nuget.org/v3/index.json --skip-duplicate --api-key ${{ secrets.NUGET_API_KEY }}
```

### iOS Binding Regeneration (`.github/workflows/ios-bindings.yml`)

```yaml
name: Regenerate iOS Bindings

on:
  workflow_dispatch:
    inputs:
      sharpie_args:
        description: 'Additional Sharpie arguments'
        required: false
        default: ''

jobs:
  regenerate:
    runs-on: macos-latest
    steps:
      - uses: actions/checkout@v4
      - name: Build NativeBridge xcframework
        run: |
          cd src/HereSdk.Explore.iOS.NativeBridge
          ./build-xcframework.sh
      - name: Run Objective-Sharpie
        run: ./scripts/bind-ios.sh ${{ github.event.inputs.sharpie_args }}
      - name: Create PR with updated bindings
        uses: peter-evans/create-pull-request@v6
        with:
          title: 'Update iOS bindings from Objective-Sharpie'
          commit-message: 'Regenerate iOS bindings'
          branch: update-ios-bindings
```

## Build Scripts

### scripts/build.sh

```bash
#!/bin/bash
set -euo pipefail

echo "=== Building Android Binding ==="
dotnet build src/HereSdk.Explore.Android.Binding -c Release

echo "=== Building iOS NativeBridge ==="
cd src/HereSdk.Explore.iOS.NativeBridge
./build-xcframework.sh
cd ../..

echo "=== Building iOS Binding ==="
dotnet build src/HereSdk.Explore.iOS.Binding -c Release

echo "=== Building MAUI Library (Android) ==="
dotnet build src/HereSdk.Explore.Maui -f net9.0-android -c Release

echo "=== Building MAUI Library (iOS) ==="
dotnet build src/HereSdk.Explore.Maui -f net9.0-ios -c Release

echo "=== Running Unit Tests ==="
dotnet test tests/HereSdk.Explore.Maui.Tests -c Release

echo "=== Build Complete ==="
```

### scripts/test.sh

```bash
#!/bin/bash
set -euo pipefail

echo "=== Running Unit Tests ==="
dotnet test tests/HereSdk.Explore.Maui.Tests -c Release --collect:"XPlat Code Coverage"

echo "=== Running Android Device Tests ==="
dotnet test tests/HereSdk.Explore.Maui.DeviceTests -f net9.0-android -c Release

echo "=== Running iOS Device Tests ==="
dotnet test tests/HereSdk.Explore.Maui.DeviceTests -f net9.0-ios -c Release
```

### scripts/pack.sh

```bash
#!/bin/bash
set -euo pipefail

VERSION="${1:-4.25.5.0}"

echo "=== Packing Android Binding ==="
dotnet pack src/HereSdk.Explore.Android.Binding -c Release -p:PackageVersion=$VERSION

echo "=== Building iOS NativeBridge ==="
cd src/HereSdk.Explore.iOS.NativeBridge
./build-xcframework.sh
cd ../..

echo "=== Packing iOS Binding ==="
dotnet pack src/HereSdk.Explore.iOS.Binding -c Release -p:PackageVersion=$VERSION

echo "=== Packing MAUI Library ==="
dotnet pack src/HereSdk.Explore.Maui -c Release -p:PackageVersion=$VERSION

echo "=== Packages created in artifacts/ ==="
mkdir -p artifacts
find . -name "*.nupkg" -exec mv {} artifacts/ \;
ls -la artifacts/
```