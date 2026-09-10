using Here.Explore.Maui.Services;

namespace Here.Explore.Maui.Tests.Services;

public class ServiceDisposeTests
{
    [Fact]
    public void MapService_Dispose_DoesNotThrow()
    {
        var service = new MapService();
        service.Dispose();
    }

    [Fact]
    public void LocationService_Dispose_DoesNotThrow()
    {
        var service = new LocationService();
        service.Dispose();
    }

    [Fact]
    public void RoutingService_Dispose_DoesNotThrow()
    {
        var service = new RoutingService();
        service.Dispose();
    }

    [Fact]
    public void SearchService_Dispose_DoesNotThrow()
    {
        var service = new SearchService();
        service.Dispose();
    }

    [Fact]
    public void TrafficService_Dispose_DoesNotThrow()
    {
        var service = new TrafficService();
        service.Dispose();
    }

    [Fact]
    public void Service_DoubleDispose_DoesNotThrow()
    {
        var service = new MapService();
        service.Dispose();
        service.Dispose();
    }
}