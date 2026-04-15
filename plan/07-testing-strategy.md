# Testing Strategy

## Testing Pyramid

```
        ┌──────────┐
        │  E2E /   │  ← Device/emulator tests (Phase 1+)
        │ Device   │     - MapView renders
        │ Tests    │     - Route calculates on real API
        ├──────────┤
        │ Integ.   │  ← Platform integration tests (Phase 1+)
        │ Tests    │     - Binding types accessible from MAUI
        │          │     - Platform converters round-trip
        ├──────────┤
        │  Unit    │  ← xUnit (net10.0) — no platform needed (Phase 0+)
        │ Tests    │     - Model construction/validation
        │          │     - Service logic with mocks
        │          │     - Converter logic
        └──────────┘
```

## Test Projects

### 1. HereSdk.Explore.Maui.Tests (net10.0)

Pure unit tests — no device required. Tests shared logic only.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
    <PackageReference Include="xunit" Version="2.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
    <PackageReference Include="NSubstitute" Version="5.*" />
  </ItemGroup>
  <!-- Shared source from the MAUI library via Compile Include -->
</Project>
```

**Current test files (20 files, 171 tests):**

| Category | Files | Count |
|---|---|---|
| Model construction/validation | `Models/GeoCoordinatesTests.cs`, `GeoBoxTests.cs`, `GeoCircleTests.cs`, `MapModelTests.cs`, `MapModelExtendedTests.cs`, `SearchModelTests.cs`, `TrafficModelTests.cs`, `RouteModelTests.cs`, `CoreModelTests.cs`, `HereSdkOptionsTests.cs` | ~80 |
| Service logic (mocked) | `Services/MapServiceTests.cs`, `SearchServiceTests.cs`, `SearchServiceExtendedTests.cs`, `RoutingServiceTests.cs`, `RoutingServiceExtendedTests.cs`, `TrafficServiceTests.cs`, `TrafficServiceExtendedTests.cs`, `ServiceInterfaceTests.cs`, `ServiceDisposalTests.cs` | ~80 |
| SDK initialization | `HereSdkTests.cs` | ~11 |

**Source linking pattern**: The unit test project uses `Compile Include` with `Link` to include shared source files from the MAUI library directly, rather than a `ProjectReference`. This avoids pulling in platform-specific dependencies (Android/iOS) that can't compile in a net10.0 (no-platform) project. Platform-specific files (`*.Android.cs`, `*.iOS.cs`) are excluded.

**What's still needed:**
- Converter tests (~30): Platform converter logic tested indirectly via service tests; dedicated converter test files would improve coverage
- Error mapping tests (~20): Error enum mapping tested in model tests; dedicated tests for cross-platform error mapping
- Async pattern tests (~10): TaskCompletionSource wrapping tested in service tests; dedicated tests for edge cases (cancellation, timeout)
- Code coverage collection: Add `coverlet.collector` package and `--collect:"XPlat Code Coverage"` to test runs

### 2. HereSdk.Explore.Maui.DeviceTests (net10.0-android;net10.0-ios)

Platform-specific tests that require the native SDK.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net10.0-android;net10.0-ios</TargetFrameworks>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
    <PackageReference Include="xunit" Version="2.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\HereSdk.Explore.Maui\HereSdk.Explore.Maui.csproj" />
  </ItemGroup>
</Project>
```

**Current device test files:**

| File | Platform | Description |
|---|---|---|
| `Android/MapViewAndroidTests.cs` | Android | MapView handler creation |
| `iOS/MapViewiOSTests.cs` | iOS | MapView handler creation |

**What's still needed:**
- SDK initialization with real credentials
- Binding type access (instantiation of key types)
- Platform converter round-trip (GeoCoordinates → platform → back)
- Real API calls (search, routing with credentials)
- Android mock JAR reference (already in .csproj but needs actual JAR file)

**Running device tests:**

```bash
# Android emulator
dotnet test tests/HereSdk.Explore.Maui.DeviceTests -f net10.0-android

# iOS simulator
dotnet test tests/HereSdk.Explore.Maui.DeviceTests -f net10.0-ios
```

## TDD Workflow (Per Feature)

### Red → Green → Refactor

1. **RED**: Write a failing test for the new feature
   - Unit test for shared models/logic
   - Device test for platform behavior

2. **GREEN**: Write minimum code to pass
   - Add model type in `HereSdk.Explore.Maui`
   - Add platform converter
   - Add binding type if missing

3. **REFACTOR**: Clean up
   - Remove duplication
   - Improve naming
   - Add edge cases

### Per-Type TDD Checklist

For each type being bound (e.g., `RoutingEngine`):

1. **Write model test first** (net10.0):
   ```csharp
   [Fact]
   public void RoutingOptions_DefaultValues_AreCorrect()
   {
       var options = new RoutingOptions();
       Assert.Equal(OptimizationMode.Fastest, options.OptimizationMode);
   }
   ```

2. **Write service test with mock** (net10.0):
   ```csharp
   [Fact]
   public async Task CalculateRouteAsync_ReturnsRoute()
   {
       var mockEngine = Substitute.For<Com.Here.Sdk.Routing.RoutingEngine>();
       // ... setup mock behavior
       var service = new RoutingService();
       var result = await service.CalculateRouteAsync(waypoints, options);
       Assert.NotNull(result);
   }
   ```

3. **Write device test** (platform TFM):
   ```csharp
   [Fact]
   public void AndroidBinding_RoutingEngine_CanBeCreated()
   {
       var engine = new Com.Here.Sdk.Routing.RoutingEngine();
       Assert.NotNull(engine);
   }
   ```

4. **Implement** the model, converter, and service to make tests pass.

5. **Validate** against API reference.

## Mock Strategy

### C#-Level Mocking (Primary — Used for Unit Tests)

All services are exposed as interfaces (`IMapService`, `IRoutingService`, etc.). Use NSubstitute to mock these interfaces in unit tests — no platform SDK needed.

```csharp
var mapService = Substitute.For<IMapService>();
mapService.GetCameraTargetAsync().Returns(new GeoCoordinates(52.5, 13.4));
```

### Android Mock JAR

The SDK includes `heresdk-explore-mock-4.25.5.0.274356.jar` with mock implementations. Referenced in the device test .csproj for Android. Used for offline testing of binding type access.

### iOS Mocking

The iOS SDK doesn't provide a separate mock library. Use:
- Protocol-based mocking at the C# level (preferred for unit tests)
- Mock implementations in the Swift NativeBridge for device tests (if needed)

## Test Naming Convention

```
{MethodName}_{Scenario}_{Expected}

Examples:
- CalculateRouteAsync_WithValidWaypoints_ReturnsRoute
- SearchAsync_WithEmptyQuery_ThrowsArgumentException
- ToShared_WithAndroidGeoCoordinates_ReturnsEquivalent
- GeoCoordinates_WithInvalidLatitude_IsNotValid
```

## Continuous Testing

- Run unit tests on every commit (CI via build.yml)
- Run device tests on PR merge (CI, macOS runner for iOS, Windows for Android)
- Run full integration tests before release
- Track code coverage with `coverlet.collector` (to be added)
- Target: 80%+ coverage on shared logic (models, converters, services)

## Key Testing Notes

1. **Android Java.Lang.Enum types** cannot be used in C# switch — service tests must verify if/else if comparison patterns
2. **TransportSpecification** uses builder pattern (CarBuilder, TruckBuilder) — mock the builder chain
3. **SearchEngine has two API styles**: `Search()` (ISearchCallbackExtended) and `SearchByText()` (SearchCompletedHandler) — test both paths
4. **Duration type** is `Com.Here.Time.HereDuration` — partial class extended for convenience
5. **InternalsVisibleTo**: The MAUI library exposes internals to the test project for `MapService.Raise*` methods