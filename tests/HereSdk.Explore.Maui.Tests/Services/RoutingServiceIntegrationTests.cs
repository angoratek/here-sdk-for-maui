using Xunit;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Routing;
using Here.Explore.Maui.Models.Traffic;
using Here.Explore.Maui.Services;

namespace Here.Explore.Maui.Tests.Services;

/// <summary>
/// Lifecycle contract for the real <see cref="RoutingService"/> partial class.
/// Mirrors <see cref="SearchServiceLifecycleTests"/>: the no-device partial
/// flips <see cref="RoutingService.IsInitialized"/> when
/// <c>Initialize()</c> runs, and the production <c>UseHereSdkExplore</c>
/// factory depends on that contract. The platform-specific partials
/// construct the native <c>RoutingEngine</c>; the contract is the same.
/// </summary>
public class RoutingServiceLifecycleTests
{
    [Fact]
    public void RoutingService_NotInitialized_HasIsInitializedFalse()
    {
        var service = new RoutingService();
        Assert.False(service.IsInitialized);
    }

    [Fact]
    public void RoutingService_AfterInitialize_HasIsInitializedTrue()
    {
        var service = new RoutingService();
        service.Initialize();
        Assert.True(service.IsInitialized);
    }

    [Fact]
    public void RoutingService_Initialize_IsIdempotent()
    {
        var service = new RoutingService();
        service.Initialize();
        service.Initialize();
        Assert.True(service.IsInitialized);
    }

    [Fact]
    public void RoutingService_Dispose_DoesNotChangeIsInitialized()
    {
        var service = new RoutingService();
        service.Initialize();
        service.Dispose();
        Assert.True(service.IsInitialized);
    }

    [Fact]
    public void RoutingService_Dispose_IsIdempotent()
    {
        var service = new RoutingService();
        service.Dispose();
        service.Dispose();
    }

    [Fact]
    public async Task RoutingService_NotInitialized_MethodsThrowNotImplemented()
    {
        // Same rationale as SearchServiceLifecycleTests: the no-device
        // stub throws NotImplementedException; the platform partials throw
        // InvalidOperationException ("RoutingService not initialized.").
        // Both forms are covered in their respective test projects.
        var service = new RoutingService();
        var waypoints = new List<Waypoint>
        {
            new(new GeoCoordinates(52.5, 13.4)),
            new(new GeoCoordinates(52.6, 13.5))
        };
        await Assert.ThrowsAsync<NotImplementedException>(
            () => service.CalculateRouteAsync(waypoints, new RoutingOptions()));
        await Assert.ThrowsAsync<NotImplementedException>(
            () => service.CalculateIsolineAsync(new GeoCoordinates(0, 0), new IsolineOptions()));
        await Assert.ThrowsAsync<NotImplementedException>(
            () => service.GetTrafficOnRouteAsync(new Route("h", new List<Section>(), 0, 0)));
    }
}

/// <summary>
/// Integration tests for the <c>UseHereSdkExplore</c> DI factory wiring of
/// <see cref="RoutingService"/>. See <c>SearchServiceIntegrationTests</c>
/// for the full rationale. Tests run in the no-device test project
/// against the stub partial — they verify the contract the production
/// factory relies on, in milliseconds, before any UI test runs.
/// </summary>
public class RoutingServiceIntegrationTests
{
    [Fact]
    public void FactoryPattern_CallingInitialize_ProducesInitializedService()
    {
        var service = new RoutingService();
        service.Initialize();
        Assert.True(service.IsInitialized);
    }

    [Fact]
    public void FactoryPattern_WithoutInitialize_ProducesUninitializedService()
    {
        var service = new RoutingService();
        Assert.False(service.IsInitialized);
    }

    [Fact]
    public void InitializedService_RemainsInitializedAcrossChecks()
    {
        var service = new RoutingService();
        service.Initialize();
        for (int i = 0; i < 3; i++)
        {
            Assert.True(service.IsInitialized);
        }
    }
}
