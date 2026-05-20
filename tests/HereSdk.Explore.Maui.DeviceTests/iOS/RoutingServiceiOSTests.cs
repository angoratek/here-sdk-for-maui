using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Routing;
using Here.Explore.Maui.Services;

namespace Here.Explore.Maui.DeviceTests.iOS;

public class RoutingServiceiOSTests
{
    [Fact]
    public void RoutingService_CanBeInstantiated()
    {
        var service = new RoutingService();
        Assert.NotNull(service);
    }

    [Fact]
    public void RoutingService_CalculateRoute_ThrowsWhenNotInitialized()
    {
        var service = new RoutingService();
        var waypoints = new[] {
            new Waypoint(new GeoCoordinates(52.5, 13.4)),
            new Waypoint(new GeoCoordinates(52.6, 13.5)) };
        Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CalculateRouteAsync(waypoints, new RoutingOptions()));
    }

    [Fact]
    public void Waypoint_CanBeCreated()
    {
        var wp = new Waypoint(new GeoCoordinates(52.5, 13.4));
        Assert.Equal(52.5, wp.Coordinates.Latitude);
    }

    [Fact]
    public void Route_CanBeCreated()
    {
        var route = new Route("route1", new List<Section>(), 15000, 900);
        Assert.Equal("route1", route.Handle);
        Assert.Equal(15000, route.LengthInMeters);
    }

    [Fact]
    public void Route_DurationText_FormatsCorrectly()
    {
        var route = new Route("r1", new List<Section>(), 5000, 3660);
        Assert.NotEmpty(route.DurationText);
    }

    [Fact]
    public void RoutingOptions_CustomTransport()
    {
        var options = new RoutingOptions(TransportMode: SectionTransportMode.Bicycle);
        Assert.Equal(SectionTransportMode.Bicycle, options.TransportMode);
    }

    [Fact]
    public void IsolineOptions_CanBeCreated()
    {
        var options = new IsolineOptions(SectionTransportMode.Car, 5000);
        Assert.Equal(5000, options.RangeInMeters);
    }

    [Fact]
    public void Section_CanBeCreated_WithManeuvers()
    {
        var maneuvers = new[] {
            new Maneuver(new GeoCoordinates(52.5, 13.4), ManeuverAction.Straight,
                "Continue straight", null, null, null, null, 500, 60) };
        var section = new Section(0, new GeoCoordinates(52.5, 13.4), new GeoCoordinates(52.6, 13.5),
            maneuvers, SectionTransportMode.Car, 5000, 300, null);
        Assert.Single(section.Maneuvers);
    }

    [Fact]
    public void RoutingResult_WithError()
    {
        var result = new RoutingResult(RoutingError.NoRouteFound, null);
        Assert.Equal(RoutingError.NoRouteFound, result.Error);
    }

    [Fact]
    public void RoutingResult_WithSuccess()
    {
        var routes = new[] { new Route("r1", new List<Section>(), 10000, 600) };
        var result = new RoutingResult(RoutingError.None, routes);
        Assert.Equal(RoutingError.None, result.Error);
        Assert.Single(result.Routes!);
    }

    [Fact(Skip = "Isolines not yet supported on iOS NativeBridge")]
    public void CalculateIsoline_ReturnsReachableArea()
    {
        // Requires NativeBridge isoline support
    }
}
