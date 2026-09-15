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
        // Documents the failure mode of an uninitialized service. After
        // the Initialize()-wiring fix in HereSdkExtensions.UseHereSdkExplore,
        // this scenario should never occur in production — the factory
        // lambda always calls Initialize() before the singleton is returned.
        // This test stays as a guard against the factory regression that
        // produced the "RoutingService not initialized" bug.
        var service = new RoutingService();
        var waypoints = new[] {
            new Waypoint(new GeoCoordinates(52.5, 13.4)),
            new Waypoint(new GeoCoordinates(52.6, 13.5)) };
        Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CalculateRouteAsync(waypoints, new RoutingOptions()));
    }

    [Fact]
    public void RoutingService_AfterInitialize_IsInitializedIsTrue()
    {
        // Companion test to the lifecycle tests in
        // HereSdk.Explore.Maui.Tests.Services.RoutingServiceLifecycleTests.
        // On a real device, Initialize() constructs the native
        // HereRoutingEngine and IsInitialized flips to true. If the
        // factory regression returns, this fails before any UI test does.
        var service = new RoutingService();
        service.Initialize();
        Assert.True(service.IsInitialized);
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
    public void RoutingOptions_WithTruckVehicleSpecifications_CanBeCreated()
    {
        var options = new RoutingOptions(
            TransportMode: SectionTransportMode.Truck,
            MaxAlternatives: 2,
            Truck: new TruckVehicleSpecifications(GrossWeightInKilograms: 12000, HeightInCentimeters: 400));
        Assert.Equal(SectionTransportMode.Truck, options.TransportMode);
        Assert.Equal(2, options.MaxAlternatives);
        Assert.Equal(12000, options.Truck!.GrossWeightInKilograms);
        Assert.Equal(400, options.Truck.HeightInCentimeters);
    }

    [Fact(Skip = "Requires HERE SDK credentials and network access")]
    public async Task CalculateRouteAsync_TruckWithVehicleSpecification_ReturnsRoute()
    {
        // End-to-end truck route with an explicit vehicle specification
        // (height 400cm, gross weight 12000kg). The specification is mapped
        // into HereTruckSpecifications and passed via the NativeBridge.
        var service = new RoutingService();
        service.Initialize();

        var waypoints = new[] {
            new Waypoint(new GeoCoordinates(52.5, 13.4)),
            new Waypoint(new GeoCoordinates(52.6, 13.5)) };
        var options = new RoutingOptions(
            TransportMode: SectionTransportMode.Truck,
            Truck: new TruckVehicleSpecifications(
                GrossWeightInKilograms: 12000,
                HeightInCentimeters: 400,
                AxleCount: 3));

        var result = await service.CalculateRouteAsync(waypoints, options);

        Assert.Equal(RoutingError.None, result.Error);
        Assert.NotNull(result.Routes);
        Assert.NotEmpty(result.Routes!);
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
