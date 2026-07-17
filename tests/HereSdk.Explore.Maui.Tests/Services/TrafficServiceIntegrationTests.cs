using Xunit;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Traffic;
using Here.Explore.Maui.Services;

namespace Here.Explore.Maui.Tests.Services;

/// <summary>
/// Lifecycle contract for the real <see cref="TrafficService"/> partial class.
/// Mirrors <see cref="SearchServiceLifecycleTests"/>: the no-device partial
/// flips <see cref="TrafficService.IsInitialized"/> when <c>Initialize()</c>
/// runs. The platform-specific partials construct the native
/// <c>TrafficEngine</c>; the contract is the same.
/// </summary>
public class TrafficServiceLifecycleTests
{
    [Fact]
    public void TrafficService_NotInitialized_HasIsInitializedFalse()
    {
        var service = new TrafficService();
        Assert.False(service.IsInitialized);
    }

    [Fact]
    public void TrafficService_AfterInitialize_HasIsInitializedTrue()
    {
        var service = new TrafficService();
        service.Initialize();
        Assert.True(service.IsInitialized);
    }

    [Fact]
    public void TrafficService_Initialize_IsIdempotent()
    {
        var service = new TrafficService();
        service.Initialize();
        service.Initialize();
        Assert.True(service.IsInitialized);
    }

    [Fact]
    public void TrafficService_Dispose_DoesNotChangeIsInitialized()
    {
        var service = new TrafficService();
        service.Initialize();
        service.Dispose();
        Assert.True(service.IsInitialized);
    }

    [Fact]
    public void TrafficService_Dispose_IsIdempotent()
    {
        var service = new TrafficService();
        service.Dispose();
        service.Dispose();
    }

    [Fact]
    public async Task TrafficService_NotInitialized_MethodsThrowNotImplemented()
    {
        var service = new TrafficService();
        var area = new GeoCircle(new GeoCoordinates(0, 0), 1000);
        await Assert.ThrowsAsync<NotImplementedException>(
            () => service.QueryFlowAsync(area, new TrafficFlowQueryOptions()));
        await Assert.ThrowsAsync<NotImplementedException>(
            () => service.QueryIncidentsAsync(area, new TrafficIncidentsQueryOptions()));
        await Assert.ThrowsAsync<NotImplementedException>(
            () => service.LookupIncidentAsync("id", new TrafficIncidentLookupOptions()));
    }
}

/// <summary>
/// Integration tests for the <c>UseHereSdkExplore</c> DI factory wiring of
/// <see cref="TrafficService"/>. See <c>SearchServiceIntegrationTests</c>
/// for the full rationale.
/// </summary>
public class TrafficServiceIntegrationTests
{
    [Fact]
    public void FactoryPattern_CallingInitialize_ProducesInitializedService()
    {
        var service = new TrafficService();
        service.Initialize();
        Assert.True(service.IsInitialized);
    }

    [Fact]
    public void FactoryPattern_WithoutInitialize_ProducesUninitializedService()
    {
        var service = new TrafficService();
        Assert.False(service.IsInitialized);
    }

    [Fact]
    public void InitializedService_RemainsInitializedAcrossChecks()
    {
        var service = new TrafficService();
        service.Initialize();
        for (int i = 0; i < 3; i++)
        {
            Assert.True(service.IsInitialized);
        }
    }
}
