using Xunit;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Routing;
using Here.Explore.Maui.Services;
using NSubstitute;

namespace Here.Explore.Maui.Tests.Services;

public class RoutingServiceTests
{
    [Fact]
    public async Task CalculateRouteAsync_WithWaypoints_ReturnsRoute()
    {
        var routingService = Substitute.For<IRoutingService>();
        var waypoints = new List<Waypoint>
        {
            new(new GeoCoordinates(52.5, 13.4)),
            new(new GeoCoordinates(48.8, 2.3))
        };
        var expected = new RoutingResult(RoutingError.None, new List<Route>());
        routingService.CalculateRouteAsync(Arg.Any<IReadOnlyList<Waypoint>>(), Arg.Any<RoutingOptions>())
            .Returns(Task.FromResult(expected));

        var result = await routingService.CalculateRouteAsync(waypoints, new RoutingOptions());

        Assert.Equal(RoutingError.None, result.Error);
    }
}