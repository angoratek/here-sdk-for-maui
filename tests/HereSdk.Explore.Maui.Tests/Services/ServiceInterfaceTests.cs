using Xunit;
using Here.Explore.Maui.Services;

namespace Here.Explore.Maui.Tests.Services;

public class ServiceInterfaceTests
{
    [Fact]
    public void IMapService_Extends_IDisposable()
    {
        Assert.True(typeof(IDisposable).IsAssignableFrom(typeof(IMapService)));
    }

    [Fact]
    public void ISearchService_Extends_IDisposable()
    {
        Assert.True(typeof(IDisposable).IsAssignableFrom(typeof(ISearchService)));
    }

    [Fact]
    public void IRoutingService_Extends_IDisposable()
    {
        Assert.True(typeof(IDisposable).IsAssignableFrom(typeof(IRoutingService)));
    }

    [Fact]
    public void ITrafficService_Extends_IDisposable()
    {
        Assert.True(typeof(IDisposable).IsAssignableFrom(typeof(ITrafficService)));
    }

    [Fact]
    public void MapService_Implements_IMapService()
    {
        Assert.True(typeof(IMapService).IsAssignableFrom(typeof(MapService)));
    }

    [Fact]
    public void SearchService_Implements_ISearchService()
    {
        Assert.True(typeof(ISearchService).IsAssignableFrom(typeof(SearchService)));
    }

    [Fact]
    public void RoutingService_Implements_IRoutingService()
    {
        Assert.True(typeof(IRoutingService).IsAssignableFrom(typeof(RoutingService)));
    }

    [Fact]
    public void TrafficService_Implements_ITrafficService()
    {
        Assert.True(typeof(ITrafficService).IsAssignableFrom(typeof(TrafficService)));
    }

    [Fact]
    public void MapService_Implements_IDisposable()
    {
        Assert.True(typeof(IDisposable).IsAssignableFrom(typeof(MapService)));
    }

    [Fact]
    public void SearchService_Implements_IDisposable()
    {
        Assert.True(typeof(IDisposable).IsAssignableFrom(typeof(SearchService)));
    }

    [Fact]
    public void RoutingService_Implements_IDisposable()
    {
        Assert.True(typeof(IDisposable).IsAssignableFrom(typeof(RoutingService)));
    }

    [Fact]
    public void TrafficService_Implements_IDisposable()
    {
        Assert.True(typeof(IDisposable).IsAssignableFrom(typeof(TrafficService)));
    }
}