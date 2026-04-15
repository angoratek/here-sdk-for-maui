using Xunit;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;
using Here.Explore.Maui.Models.Routing;
using Here.Explore.Maui.Models.Search;
using Here.Explore.Maui.Models.Traffic;
using Here.Explore.Maui.Services;
using NSubstitute;

namespace Here.Explore.Maui.Tests.Services;

public class ServiceDisposalTests
{
    [Fact]
    public void MapService_Disposes()
    {
        var service = new MapService();
        service.Dispose();
        // Double-dispose should not throw
        service.Dispose();
    }

    [Fact]
    public void SearchService_Disposes()
    {
        var service = new SearchService();
        service.Dispose();
        service.Dispose();
    }

    [Fact]
    public void RoutingService_Disposes()
    {
        var service = new RoutingService();
        service.Dispose();
        service.Dispose();
    }

    [Fact]
    public void TrafficService_Disposes()
    {
        var service = new TrafficService();
        service.Dispose();
        service.Dispose();
    }

    [Fact]
    public async Task MapService_Stubs_ThrowNotImplemented()
    {
        var service = new MapService();

        Assert.Throws<NotImplementedException>(() => service.ZoomLevel);
        Assert.Throws<NotImplementedException>(() => service.Bearing);
        Assert.Throws<NotImplementedException>(() => service.Tilt);
        await Assert.ThrowsAsync<NotImplementedException>(() => service.GetCameraTargetAsync());
        await Assert.ThrowsAsync<NotImplementedException>(() => service.SetCameraTargetAsync(new GeoCoordinates(0, 0)));
        await Assert.ThrowsAsync<NotImplementedException>(() => service.LoadSceneAsync(MapScheme.NormalDay));
        Assert.Throws<NotImplementedException>(() => service.AddMapMarker(new MapMarker(new GeoCoordinates(0, 0))));
    }

    [Fact]
    public async Task SearchService_Stubs_ThrowNotImplemented()
    {
        var service = new SearchService();

        await Assert.ThrowsAsync<NotImplementedException>(
            () => service.SearchAsync(new TextQuery("test"), new SearchOptions()));
        await Assert.ThrowsAsync<NotImplementedException>(
            () => service.SuggestAsync(new TextQuery("test"), new SearchOptions()));
        await Assert.ThrowsAsync<NotImplementedException>(
            () => service.GetPlaceByIdAsync("id"));
    }

    [Fact]
    public async Task RoutingService_Stubs_ThrowNotImplemented()
    {
        var service = new RoutingService();

        await Assert.ThrowsAsync<NotImplementedException>(
            () => service.CalculateRouteAsync(new List<Waypoint>(), new RoutingOptions()));
        await Assert.ThrowsAsync<NotImplementedException>(
            () => service.CalculateIsolineAsync(new GeoCoordinates(0, 0), new IsolineOptions()));
        await Assert.ThrowsAsync<NotImplementedException>(
            () => service.GetTrafficOnRouteAsync(new Route("h", new List<Section>(), 0, 0)));
    }

    [Fact]
    public async Task TrafficService_Stubs_ThrowNotImplemented()
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

    [Fact]
    public void MapService_RaisesCameraStateChanged()
    {
        var service = new TestableMapService();
        CameraStateChangedEventArgs? received = null;
        service.CameraStateChanged += (_, e) => received = e;

        var args = new CameraStateChangedEventArgs(new GeoCoordinates(52.5, 13.4), 10, 45, 30);
        service.TestRaiseCameraStateChanged(args);

        Assert.NotNull(received);
        Assert.Equal(52.5, received!.Target.Latitude);
        Assert.Equal(10, received.ZoomLevel);
    }

    [Fact]
    public void MapService_RaisesMapIdle()
    {
        var service = new TestableMapService();
        var raised = false;
        service.MapIdle += (_, _) => raised = true;

        service.TestRaiseMapIdle();
        Assert.True(raised);
    }

    [Fact]
    public void MapService_RaisesMapTapped()
    {
        var service = new TestableMapService();
        MapTappedEventArgs? received = null;
        service.MapTapped += (_, e) => received = e;

        var args = new MapTappedEventArgs(new GeoCoordinates(52.5, 13.4), new Point2D(100, 200));
        service.TestRaiseMapTapped(args);

        Assert.NotNull(received);
        Assert.Equal(52.5, received!.Coordinates.Latitude);
    }

    [Fact]
    public void TrafficOnRoute_Created()
    {
        var traffic = new TrafficOnRoute("handle-123", DelayInSeconds: 120);
        Assert.Equal("handle-123", traffic.RouteHandle);
        Assert.Equal(120, traffic.DelayInSeconds);
        Assert.Null(traffic.Incidents);
    }

    [Fact]
    public void TrafficIncidentOnRoute_Created()
    {
        var incident = new TrafficIncidentOnRoute("inc-1", "Accident", TrafficIncidentType.Accident,
            TrafficIncidentImpact.Major, AffectedSectionIndex: 0);
        Assert.Equal("inc-1", incident.Id);
        Assert.Equal(0, incident.AffectedSectionIndex);
    }

    [Fact]
    public void RoutingResult_WithNoRoutes()
    {
        var result = new RoutingResult(RoutingError.NoRouteFound, null);
        Assert.Equal(RoutingError.NoRouteFound, result.Error);
        Assert.Null(result.Routes);
    }

    [Fact]
    public void SearchResult_WithNoPlaces()
    {
        var result = new SearchResult(SearchError.NoResults, null);
        Assert.Equal(SearchError.NoResults, result.Error);
        Assert.Null(result.Places);
    }

    /// <summary>
    /// Testable subclass that exposes Raise* methods.
    /// </summary>
    private class TestableMapService : MapService
    {
        public void TestRaiseCameraStateChanged(CameraStateChangedEventArgs e) => RaiseCameraStateChanged(e);
        public void TestRaiseMapIdle() => RaiseMapIdle();
        public void TestRaiseMapTapped(MapTappedEventArgs e) => RaiseMapTapped(e);
    }
}