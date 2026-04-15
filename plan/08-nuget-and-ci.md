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

Defined in `Directory.Build.props`:
```xml
<HereSdkVersion>4.25.5.0</HereSdkVersion>
<PackageVersion>4.25.5.0-alpha1</PackageVersion>
```

### Package Metadata

Shared metadata (applied to all packages with `PackageId`) is defined in `Directory.Build.props`:

```xml
<PropertyGroup Condition="'$(PackageId)' != ''">
    <Authors>angoratek</Authors>
    <RepositoryUrl>https://github.com/angoratek/here-sdk-for-maui</RepositoryUrl>
    <RepositoryType>git</RepositoryType>
    <PackageProjectUrl>https://github.com/angoratek/here-sdk-for-maui</PackageProjectUrl>
    <Copyright>Copyright © 2026 angoratek</Copyright>
</PropertyGroup>
```

Per-package metadata in `.csproj` files:

| Property | Android Binding | iOS Binding | MAUI Library |
|---|---|---|---|
| `PackageId` | `HereSdk.Explore.Android.Binding` | `HereSdk.Explore.iOS.Binding` | `HereSdk.Explore.Maui` |
| `Description` | Android binding for .NET MAUI | iOS binding (Native Library Interop) | Cross-platform maps, routing, search, traffic |
| `PackageTags` | here;sdk;android;binding;maui;maps | here;sdk;ios;binding;maui;maps;native-interop | here;sdk;maui;maps;routing;search;traffic;navigation |
| `PackageLicenseExpression` | MIT | MIT | MIT |
| `GenerateDocumentationFile` | true | true | true |
| `PackageReadmeFile` | — | — | README.md |

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
# List package contents
unzip -l artifacts/HereSdk.Explore.Maui.*.nupkg

# Check that platform-specific libs are present
# lib/net10.0-android/HereSdk.Explore.Maui.dll
# lib/net10.0-android/HereSdk.Explore.Android.Binding.dll
# lib/net10.0-ios/HereSdk.Explore.Maui.dll
# lib/net10.0-ios/HereSdk.Explore.iOS.Binding.dll
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
  unit-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - name: Run unit tests
        run: dotnet test tests/HereSdk.Explore.Maui.Tests -c Release

  build-android:
    runs-on: windows-latest
    needs: unit-tests
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - run: dotnet workload install maui-android
      - name: Build Android binding
        run: dotnet build src/HereSdk.Explore.Android.Binding -c Release
      - name: Build MAUI library (Android)
        run: dotnet build src/HereSdk.Explore.Maui -f net10.0-android -c Release

  build-ios:
    runs-on: macos-latest
    needs: unit-tests
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - run: dotnet workload install maui-ios
      - name: Build iOS NativeBridge
        run: |
          cd src/HereSdk.Explore.iOS.NativeBridge
          ./build-xcframework.sh
      - name: Run Sharpie + fix bindings
        run: ./scripts/bind-ios.sh
      - name: Build iOS binding
        run: dotnet build src/HereSdk.Explore.iOS.Binding -c Release
      - name: Build MAUI library (iOS)
        run: dotnet build src/HereSdk.Explore.Maui -f net10.0-ios -c Release
```

**Notes:**
- .NET 10.0.x is required (project targets net10.0-android and net10.0-ios)
- Android build runs on Windows (most reliable for MAUI Android workloads)
- iOS build runs on macOS (required for Xcode and Objective-Sharpie)
- iOS NativeBridge xcframework must be built before the binding project
- AAR is gitignored (too large for git); CI needs to download or restore from cache

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
          dotnet-version: '10.0.x'
      - run: dotnet workload install maui
      - name: Build iOS NativeBridge
        run: |
          cd src/HereSdk.Explore.iOS.NativeBridge
          ./build-xcframework.sh
      - name: Run Sharpie + fix bindings
        run: ./scripts/bind-ios.sh
      - name: Pack NuGet packages
        run: |
          dotnet pack src/HereSdk.Explore.Android.Binding -c Release -p:PackageVersion=${{ github.ref_name }}
          dotnet pack src/HereSdk.Explore.iOS.Binding -c Release -p:PackageVersion=${{ github.ref_name }}
          dotnet pack src/HereSdk.Explore.Maui -c Release -p:PackageVersion=${{ github.ref_name }}
      - name: Publish to NuGet.org
        run: dotnet nuget push **/*.nupkg --source https://api.nuget.org/v3/index.json --skip-duplicate --api-key ${{ secrets.NUGET_API_KEY }}
```

**Required secrets:**
- `NUGET_API_KEY` — NuGet.org API key for pushing packages

### iOS Binding Regeneration (`.github/workflows/ios-bindings.yml`)

Not yet created. Planned workflow for regenerating iOS bindings when the SDK updates:

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

### AAR Restoration for CI

The AAR (~61 MB) is gitignored. CI needs a way to restore it:

**Option A** — Download from HERE developer portal (requires credentials)
**Option B** — Store as GitHub Actions cache or artifact
**Option C** — Use a private NuGet feed for the AAR

Currently, the AAR must be manually placed in `src/HereSdk.Explore.Android.Binding/Jars/` before CI runs. This needs to be automated before CI can run unattended.

## Build Scripts

### scripts/build.sh

Full build: Android binding + iOS NativeBridge + iOS binding + MAUI library + tests.

### scripts/build-android.sh

Android-only build.

### scripts/build-ios-native.sh

Builds the iOS NativeBridge xcframework (requires Xcode).

### scripts/bind-ios.sh

Runs Objective-Sharpie and applies fixups to generate `ApiDefinition.cs` and `StructsAndEnums.cs`.

### scripts/test.sh

Runs unit tests + device tests.

### scripts/pack.sh

Packs all three NuGet packages and moves them to `artifacts/`.

### scripts/clean.sh

Removes `bin/`, `obj/`, and `artifacts/` directories.

## Known CI Gaps

| Gap | Status | Resolution |
|---|---|---|
| AAR not in git | Open | Need download script or private feed |
| iOS xcframework build requires Xcode | Open | CI uses macos-latest (has Xcode) |
| No code coverage collection | Open | Add `coverlet.collector` package |
| No Android device tests in CI | Open | Need emulator setup or Firebase Test Lab |
| No iOS device tests in CI | Open | Need simulator setup in macOS runner |
| No iOS binding regeneration workflow | Open | Create `ios-bindings.yml` |
| No Android binding regeneration workflow | Open | Create script for AAR update automation |