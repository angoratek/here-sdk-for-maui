using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Routing;
using Here.Explore.Maui.Services;

namespace Here.Explore.Maui.DeviceTests.Android;

public class RoutingServiceAndroidTests
{
    // ================================================================
    // Service lifecycle
    // ================================================================

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
        var options = new RoutingOptions();
        Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CalculateRouteAsync(waypoints, options));
    }

    [Fact]
    public void RoutingService_CalculateIsoline_ThrowsWhenNotInitialized()
    {
        var service = new RoutingService();
        var options = new IsolineOptions(SectionTransportMode.Car, 5000);
        Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CalculateIsolineAsync(new GeoCoordinates(52.5, 13.4), options));
    }

    // ================================================================
    // Waypoint model
    // ================================================================

    [Fact]
    public void Waypoint_CanBeCreated()
    {
        var wp = new Waypoint(new GeoCoordinates(52.5, 13.4));
        Assert.Equal(52.5, wp.Coordinates.Latitude);
        Assert.Equal(13.4, wp.Coordinates.Longitude);
    }

    [Fact]
    public void Waypoint_WithType_CanBeCreated()
    {
        var wp = new Waypoint(new GeoCoordinates(52.5, 13.4), Type: WaypointType.Start);
        Assert.Equal(WaypointType.Start, wp.Type);
    }

    [Fact]
    public void Waypoint_Equality_Works()
    {
        var a = new Waypoint(new GeoCoordinates(52.5, 13.4));
        var b = new Waypoint(new GeoCoordinates(52.5, 13.4));
        Assert.Equal(a, b);
    }

    // ================================================================
    // RoutingOptions model
    // ================================================================

    [Fact]
    public void RoutingOptions_DefaultTransportIsCar()
    {
        var options = new RoutingOptions();
        Assert.Equal(SectionTransportMode.Car, options.TransportMode);
    }

    [Fact]
    public void RoutingOptions_CustomTransport()
    {
        var options = new RoutingOptions(TransportMode: SectionTransportMode.Pedestrian);
        Assert.Equal(SectionTransportMode.Pedestrian, options.TransportMode);
    }

    [Fact]
    public void RoutingOptions_DefaultOptimizationIsFastest()
    {
        var options = new RoutingOptions();
        Assert.Equal(OptimizationMode.Fastest, options.Optimization);
    }

    [Fact]
    public void RoutingOptions_CustomProperties()
    {
        var options = new RoutingOptions(
            TransportMode: SectionTransportMode.Truck,
            Optimization: OptimizationMode.Shortest,
            MaxAlternatives: 3);
        Assert.Equal(SectionTransportMode.Truck, options.TransportMode);
        Assert.Equal(OptimizationMode.Shortest, options.Optimization);
        Assert.Equal(3, options.MaxAlternatives);
    }

    // ================================================================
    // IsolineOptions model
    // ================================================================

    [Fact]
    public void IsolineOptions_CanBeCreated()
    {
        var options = new IsolineOptions(SectionTransportMode.Car, 5000);
        Assert.Equal(5000, options.RangeInMeters);
        Assert.Equal(SectionTransportMode.Car, options.TransportMode);
    }

    [Fact]
    public void IsolineOptions_WithMaxPoints()
    {
        var options = new IsolineOptions(SectionTransportMode.Pedestrian, 10000, MaxPoints: 500);
        Assert.Equal(500, options.MaxPoints);
    }

    // ================================================================
    // Route model
    // ================================================================

    [Fact]
    public void Route_CanBeCreated()
    {
        var route = new Route("route1", new List<Section>(), 15000, 900);
        Assert.Equal("route1", route.Handle);
        Assert.Equal(15000, route.LengthInMeters);
        Assert.Equal(900, route.DurationInSeconds);
    }

    [Fact]
    public void Route_EmptySections_IsValid()
    {
        var route = new Route("empty", new List<Section>(), 0, 0);
        Assert.Empty(route.Sections);
    }

    [Fact]
    public void Route_DurationText_FormatsCorrectly()
    {
        var route = new Route("r1", new List<Section>(), 5000, 3660);
        Assert.Contains("1h 1m", route.DurationText);
    }

    [Fact]
    public void Route_DurationText_SecondsOnly()
    {
        var route = new Route("r2", new List<Section>(), 1000, 45);
        Assert.Contains("45s", route.DurationText);
    }

    // ================================================================
    // Section model
    // ================================================================

    [Fact]
    public void Section_CanBeCreated()
    {
        var section = new Section(
            0,
            new GeoCoordinates(52.5, 13.4),
            new GeoCoordinates(52.6, 13.5),
            new List<Maneuver>(),
            SectionTransportMode.Car,
            10000,
            600,
            null);
        Assert.Equal(0, section.SectionIndex);
        Assert.Equal(10000, section.LengthInMeters);
        Assert.Equal(600, section.DurationInSeconds);
    }

    [Fact]
    public void Section_WithGeometry_CreatesPolyline()
    {
        var geometry = new[] { new GeoCoordinates(52.5, 13.4), new GeoCoordinates(52.6, 13.5) };
        var section = new Section(
            0,
            geometry[0],
            geometry[1],
            new List<Maneuver>(),
            SectionTransportMode.Car,
            5000,
            300,
            geometry);
        Assert.NotNull(section.Geometry);
        Assert.Equal(2, section.Geometry!.Count);
    }

    // ================================================================
    // Maneuver model
    // ================================================================

    [Fact]
    public void Maneuver_CanBeCreated()
    {
        var maneuver = new Maneuver(
            new GeoCoordinates(52.5, 13.4),
            ManeuverAction.Straight,
            "Continue on Main St",
            null, null, null, null, 500, 60);
        Assert.Equal(ManeuverAction.Straight, maneuver.Action);
        Assert.Equal("Continue on Main St", maneuver.Instruction);
        Assert.Equal(500, maneuver.LengthInMeters);
    }

    [Fact]
    public void Maneuver_DistanceText_FormatsCorrectly()
    {
        var maneuver = new Maneuver(
            new GeoCoordinates(52.5, 13.4),
            ManeuverAction.Left,
            "Turn left",
            null, null, null, null, 250, 30);
        Assert.Contains("250m", maneuver.DistanceText);
    }

    // ================================================================
    // RoutingResult model
    // ================================================================

    [Fact]
    public void RoutingResult_WithError()
    {
        var result = new RoutingResult(RoutingError.NoRouteFound, null);
        Assert.Equal(RoutingError.NoRouteFound, result.Error);
    }

    [Fact]
    public void RoutingResult_WithRoutes()
    {
        var routes = new[] { new Route("r1", new List<Section>(), 10000, 600) };
        var result = new RoutingResult(RoutingError.None, routes);
        Assert.Equal(RoutingError.None, result.Error);
        Assert.Single(result.Routes!);
    }

    // ================================================================
    // IsolineResult model
    // ================================================================

    [Fact]
    public void IsolineResult_CanBeCreated()
    {
        var isolines = new[] { new Isoline(new List<GeoCoordinates>(), 5000) };
        var result = new IsolineResult(RoutingError.None, isolines);
        Assert.Equal(RoutingError.None, result.Error);
        Assert.Single(result.Isolines!);
    }

    // ================================================================
    // SectionTransportMode enum
    // ================================================================

    [Fact]
    public void SectionTransportMode_AllModes_AreDefined()
    {
        var modes = Enum.GetValues<SectionTransportMode>();
        Assert.Contains(SectionTransportMode.Car, modes);
        Assert.Contains(SectionTransportMode.Truck, modes);
        Assert.Contains(SectionTransportMode.Pedestrian, modes);
        Assert.Contains(SectionTransportMode.Bicycle, modes);
        Assert.Contains(SectionTransportMode.Scooter, modes);
        Assert.Contains(SectionTransportMode.Bus, modes);
        Assert.Contains(SectionTransportMode.Taxi, modes);
        Assert.Contains(SectionTransportMode.Transit, modes);
    }

    // ================================================================
    // OptimizationMode enum
    // ================================================================

    [Fact]
    public void OptimizationMode_Values_AreDefined()
    {
        var modes = Enum.GetValues<OptimizationMode>();
        Assert.Contains(OptimizationMode.Fastest, modes);
        Assert.Contains(OptimizationMode.Shortest, modes);
    }

    // ================================================================
    // ManeuverAction enum
    // ================================================================

    [Fact]
    public void ManeuverAction_AllActions_AreDefined()
    {
        var actions = Enum.GetValues<ManeuverAction>();
        Assert.Contains(ManeuverAction.Straight, actions);
        Assert.Contains(ManeuverAction.Left, actions);
        Assert.Contains(ManeuverAction.Right, actions);
        Assert.Contains(ManeuverAction.Depart, actions);
        Assert.Contains(ManeuverAction.Arrive, actions);
        Assert.Contains(ManeuverAction.UTurnLeft, actions);
        Assert.Contains(ManeuverAction.Roundabout, actions);
    }

    // ================================================================
    // SDK-dependent tests (require credentials)
    // ================================================================

    [Fact(Skip = "Requires HERE SDK credentials and network access")]
    public void CalculateRoute_SanFranciscoToOakland_ReturnsRoute()
    {
        // End-to-end route calculation test
        // Requires HERE SDK initialized with valid credentials
    }

    [Fact(Skip = "Requires HERE SDK credentials and network access")]
    public void CalculateIsoline_ReturnsReachableArea()
    {
        // End-to-end isoline calculation test
    }
}
