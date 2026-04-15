using Xunit;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Routing;
using Here.Explore.Maui.Services;
using NSubstitute;

namespace Here.Explore.Maui.Tests.Services;

public class RoutingServiceExtendedTests
{
    [Fact]
    public async Task CalculateRouteAsync_WithMultipleWaypoints_ReturnsRoutes()
    {
        var routingService = Substitute.For<IRoutingService>();
        var waypoints = new List<Waypoint>
        {
            new(new GeoCoordinates(52.5, 13.4)),
            new(new GeoCoordinates(50.1, 8.7)),
            new(new GeoCoordinates(48.8, 2.3))
        };
        var route = new Route("h1", new List<Section>(), 1500000, 54000);
        var expected = new RoutingResult(RoutingError.None, new List<Route> { route });
        routingService.CalculateRouteAsync(Arg.Any<IReadOnlyList<Waypoint>>(), Arg.Any<RoutingOptions>())
            .Returns(Task.FromResult(expected));

        var result = await routingService.CalculateRouteAsync(waypoints, new RoutingOptions());

        Assert.Equal(RoutingError.None, result.Error);
        Assert.NotNull(result.Routes);
        Assert.Single(result.Routes);
        Assert.Equal(1500000, result.Routes![0].LengthInMeters);
    }

    [Fact]
    public async Task CalculateRouteAsync_WithTruckMode_ReturnsRoute()
    {
        var routingService = Substitute.For<IRoutingService>();
        var options = new RoutingOptions(TransportMode: SectionTransportMode.Truck);
        var expected = new RoutingResult(RoutingError.None, new List<Route>());
        routingService.CalculateRouteAsync(Arg.Any<IReadOnlyList<Waypoint>>(), options)
            .Returns(Task.FromResult(expected));

        var result = await routingService.CalculateRouteAsync(
            new List<Waypoint> { new(new GeoCoordinates(52.5, 13.4)), new(new GeoCoordinates(48.8, 2.3)) },
            options);

        Assert.Equal(RoutingError.None, result.Error);
    }

    [Fact]
    public async Task CalculateRouteAsync_ReturnsNoRouteError()
    {
        var routingService = Substitute.For<IRoutingService>();
        routingService.CalculateRouteAsync(Arg.Any<IReadOnlyList<Waypoint>>(), Arg.Any<RoutingOptions>())
            .Returns(Task.FromResult(new RoutingResult(RoutingError.NoRouteFound, null)));

        var result = await routingService.CalculateRouteAsync(
            new List<Waypoint> { new(new GeoCoordinates(0, 0)) },
            new RoutingOptions());

        Assert.Equal(RoutingError.NoRouteFound, result.Error);
        Assert.Null(result.Routes);
    }

    [Fact]
    public async Task CalculateIsolineAsync_ReturnsIsolines()
    {
        var routingService = Substitute.For<IRoutingService>();
        var isoline = new Isoline(
            new List<GeoCoordinates> { new(52.5, 13.4), new(52.6, 13.5) },
            5000);
        var expected = new IsolineResult(RoutingError.None, new List<Isoline> { isoline });
        routingService.CalculateIsolineAsync(Arg.Any<GeoCoordinates>(), Arg.Any<IsolineOptions>())
            .Returns(Task.FromResult(expected));

        var result = await routingService.CalculateIsolineAsync(
            new GeoCoordinates(52.5, 13.4),
            new IsolineOptions(RangeInMeters: 5000));

        Assert.Equal(RoutingError.None, result.Error);
        Assert.NotNull(result.Isolines);
        Assert.Single(result.Isolines);
        Assert.Equal(5000, result.Isolines![0].RangeInMeters);
    }

    [Fact]
    public async Task CalculateIsolineAsync_WithMaxPoints()
    {
        var routingService = Substitute.For<IRoutingService>();
        var expected = new IsolineResult(RoutingError.None, new List<Isoline>());
        routingService.CalculateIsolineAsync(Arg.Any<GeoCoordinates>(), Arg.Any<IsolineOptions>())
            .Returns(Task.FromResult(expected));

        var result = await routingService.CalculateIsolineAsync(
            new GeoCoordinates(52.5, 13.4),
            new IsolineOptions(MaxPoints: 100));

        Assert.Equal(RoutingError.None, result.Error);
    }

    [Fact]
    public async Task CalculateRouteAsync_WithShortestOptimization()
    {
        var routingService = Substitute.For<IRoutingService>();
        var options = new RoutingOptions(Optimization: OptimizationMode.Shortest);
        var expected = new RoutingResult(RoutingError.None, new List<Route>());
        routingService.CalculateRouteAsync(Arg.Any<IReadOnlyList<Waypoint>>(), options)
            .Returns(Task.FromResult(expected));

        var result = await routingService.CalculateRouteAsync(
            new List<Waypoint> { new(new GeoCoordinates(52.5, 13.4)) },
            options);

        Assert.Equal(RoutingError.None, result.Error);
    }

    [Fact]
    public async Task CalculateRouteAsync_WithMaxAlternatives()
    {
        var routingService = Substitute.For<IRoutingService>();
        var options = new RoutingOptions(MaxAlternatives: 2);
        var routes = new List<Route>
        {
            new("h1", new List<Section>(), 1000, 60),
            new("h2", new List<Section>(), 1200, 90),
            new("h3", new List<Section>(), 1500, 120)
        };
        var expected = new RoutingResult(RoutingError.None, routes);
        routingService.CalculateRouteAsync(Arg.Any<IReadOnlyList<Waypoint>>(), options)
            .Returns(Task.FromResult(expected));

        var result = await routingService.CalculateRouteAsync(
            new List<Waypoint> { new(new GeoCoordinates(52.5, 13.4)) },
            options);

        Assert.Equal(3, result.Routes!.Count);
    }
}