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
        │  Unit    │  ← xUnit (net9.0) — no platform needed (Phase 0+)
        │ Tests    │     - Model construction/validation
        │          │     - Service logic with mocks
        │          │     - Converter logic
        └──────────┘
```

## Test Projects

### 1. HereSdk.Explore.Maui.Tests (net9.0)

Pure unit tests — no device required. Tests shared logic only.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
    <PackageReference Include="xunit" Version="2.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
    <PackageReference Include="NSubstitute" Version="5.*" />
    <PackageReference Include="Mocks.Maui" Version="1.*" />
  </ItemGroup>
</Project>
```

**What to test:**

| Category | Examples | Count Est. |
|---|---|---|
| Model construction | `new GeoCoordinates(52.5, 13.4)` | ~50 |
| Model validation | `GeoCoordinates.IsValid`, `GeoBox.Contains()` | ~30 |
| Model equality | `GeoCoordinates` records are equal by value | ~20 |
| Service logic (mocked) | `SearchService.SearchAsync` calls engine with correct params | ~40 |
| Converter logic | Android/iOS type → shared type round-trip | ~30 |
| Error mapping | Native error codes → unified error enum | ~20 |
| Async pattern | `TaskCompletionSource` wrapping | ~10 |

**Testing pattern — Service with mocked platform:**

```csharp
public class SearchServiceTests
{
    [Fact]
    public async Task SearchAsync_WithTextQuery_CallsPlatformEngine()
    {
        // Arrange
        var mockEngine = Substitute.For<ISearchEnginePlatform>();
        var service = new SearchService(mockEngine);
        var query = new TextQuery("pizza");

        mockEngine.Search(default!, default!)
            .ReturnsForAnyArgs(call =>
            {
                var callback = call.Arg<SearchCallback>();
                callback.OnSuccess(new List<Place>()); // platform Place
                return true;
            });

        // Act
        var result = await service.SearchAsync(query, new SearchOptions());

        // Assert
        Assert.NotNull(result);
        await mockEngine.Received(1).Search(Arg.Any<TextQuery>(), Arg.Any<SearchOptions>());
    }
}
```

### 2. HereSdk.Explore.Maui.DeviceTests (net9.0-android;net9.0-ios)

Platform-specific tests that require the native SDK.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net9.0-android;net9.0-ios</TargetFrameworks>
  </PropertyGroup>
</Project>
```

**What to test:**

| Category | Examples |
|---|---|
| SDK initialization | `HereSdk.Initialize()` with real credentials |
| MapView creation | `HereMapView` handler creates platform view |
| Binding type access | Every bound type can be instantiated |
| Platform converter round-trip | `GeoCoordinates` → platform → back |
| Real API calls | Search, routing with real credentials (integration) |

**Running device tests:**

```bash
# Android emulator
dotnet test tests/HereSdk.Explore.Maui.DeviceTests -f net9.0-android

# iOS simulator
dotnet test tests/HereSdk.Explore.Maui.DeviceTests -f net9.0-ios
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

1. **Write model test first** (net9.0):
   ```csharp
   [Fact]
   public void RoutingOptions_DefaultValues_AreCorrect()
   {
       var options = new RoutingOptions();
       Assert.Equal(OptimizationMode.Fastest, options.OptimizationMode);
   }
   ```

2. **Write service test with mock** (net9.0):
   ```csharp
   [Fact]
   public async Task CalculateRouteAsync_ReturnsRoute()
   {
       var mockEngine = Substitute.For<IRoutingEnginePlatform>();
       var service = new RoutingService(mockEngine);
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

### Android Mock JAR

The SDK includes `heresdk-explore-mock-4.25.5.0.274356.jar` (765 KB) with mock implementations. Reference it in the Android device test project for offline testing.

### iOS Mocking

The iOS SDK doesn't provide a separate mock library. Use protocol-based mocking:
- All HERE SDK protocols (delegates) are `AnyObject` (class-only)
- Create mock implementations in Swift NativeBridge that return canned responses
- Or: mock at the C# level using interfaces (preferred for unit tests)

### C#-Level Mocking (Preferred)

All services are exposed as interfaces (`IMapService`, `IRoutingService`, etc.). Use NSubstitute or Moq to mock these interfaces in unit tests — no platform SDK needed.

```csharp
var mapService = Substitute.For<IMapService>();
mapService.GetCameraTargetAsync().Returns(new GeoCoordinates(52.5, 13.4));
```

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

- Run unit tests on every commit (CI)
- Run device tests on PR merge (CI, macOS runner)
- Run full integration tests before release
- Track code coverage with `coverlet.collector`
- Target: 80%+ coverage on shared logic (models, converters, services)