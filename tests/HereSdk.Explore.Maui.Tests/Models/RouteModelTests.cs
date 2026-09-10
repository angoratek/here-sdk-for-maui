using Xunit;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Routing;
using Here.Explore.Maui.Models.Traffic;

namespace Here.Explore.Maui.Tests.Models;

public class RouteModelTests
{
    [Fact]
    public void Waypoint_CreatedWithCoordinates()
    {
        var wp = new Waypoint(new GeoCoordinates(52.5, 13.4));

        Assert.Equal(52.5, wp.Coordinates.Latitude);
        Assert.Equal(13.4, wp.Coordinates.Longitude);
        Assert.Equal(WaypointType.Stop, wp.Type);
        Assert.Null(wp.Name);
        Assert.Null(wp.Hint);
    }

    [Fact]
    public void Waypoint_CreatedWithAllFields()
    {
        var wp = new Waypoint(new GeoCoordinates(52.5, 13.4), WaypointType.Start, "Start Point", "hint123");

        Assert.Equal(WaypointType.Start, wp.Type);
        Assert.Equal("Start Point", wp.Name);
        Assert.Equal("hint123", wp.Hint);
    }

    [Fact]
    public void WaypointType_Values()
    {
        Assert.Equal(0, (int)WaypointType.Stop);
        Assert.Equal(1, (int)WaypointType.Start);
        Assert.Equal(2, (int)WaypointType.Through);
    }

    [Fact]
    public void RoutingOptions_Defaults()
    {
        var opts = new RoutingOptions();

        Assert.Equal(OptimizationMode.Fastest, opts.Optimization);
        Assert.Equal(SectionTransportMode.Car, opts.TransportMode);
        Assert.Null(opts.MaxAlternatives);
        Assert.Null(opts.DepartureTime);
    }

    [Fact]
    public void RoutingOptions_WithOverrides()
    {
        var opts = new RoutingOptions(OptimizationMode.Shortest, SectionTransportMode.Truck, 3, 1700000000.0);

        Assert.Equal(OptimizationMode.Shortest, opts.Optimization);
        Assert.Equal(SectionTransportMode.Truck, opts.TransportMode);
        Assert.Equal(3, opts.MaxAlternatives);
        Assert.Equal(1700000000.0, opts.DepartureTime);
    }

    [Fact]
    public void IsolineOptions_Defaults()
    {
        var opts = new IsolineOptions();

        Assert.Equal(SectionTransportMode.Car, opts.TransportMode);
        Assert.Equal(10000, opts.RangeInMeters);
        Assert.Null(opts.MaxPoints);
    }

    [Fact]
    public void SectionTransportMode_AllValues()
    {
        Assert.Equal(0, (int)SectionTransportMode.Car);
        Assert.Equal(1, (int)SectionTransportMode.Truck);
        Assert.Equal(2, (int)SectionTransportMode.Pedestrian);
        Assert.Equal(3, (int)SectionTransportMode.Bicycle);
        Assert.Equal(4, (int)SectionTransportMode.Scooter);
        Assert.Equal(5, (int)SectionTransportMode.Bus);
        Assert.Equal(6, (int)SectionTransportMode.Taxi);
        Assert.Equal(7, (int)SectionTransportMode.Transit);
    }

    [Fact]
    public void Route_CreatedWithSections()
    {
        var sections = new List<Section>
        {
            new(0, new GeoCoordinates(52.5, 13.4), new GeoCoordinates(48.8, 2.3),
                new List<Maneuver>(), SectionTransportMode.Car, 1050000, 36000)
        };
        var route = new Route("handle-123", sections, 1050000, 36000);

        Assert.Equal("handle-123", route.Handle);
        Assert.Single(route.Sections);
        Assert.Equal(1050000, route.LengthInMeters);
        Assert.Equal(36000, route.DurationInSeconds);
    }

    [Fact]
    public void Section_CreatedWithManeuvers()
    {
        var maneuvers = new List<Maneuver>
        {
            new(new GeoCoordinates(52.5, 13.4), ManeuverAction.Depart, "Head north"),
            new(new GeoCoordinates(52.6, 13.5), ManeuverAction.Left, "Turn left"),
            new(new GeoCoordinates(48.8, 2.3), ManeuverAction.Arrive, "Arrive")
        };
        var section = new Section(0, new GeoCoordinates(52.5, 13.4), new GeoCoordinates(48.8, 2.3),
            maneuvers, SectionTransportMode.Car, 1050000, 36000);

        Assert.Equal(3, section.Maneuvers.Count);
        Assert.Equal(ManeuverAction.Depart, section.Maneuvers[0].Action);
        Assert.Equal(ManeuverAction.Left, section.Maneuvers[1].Action);
        Assert.Equal(ManeuverAction.Arrive, section.Maneuvers[2].Action);
    }

    [Fact]
    public void ManeuverAction_AllValues()
    {
        Assert.Equal(0, (int)ManeuverAction.Depart);
        Assert.Equal(1, (int)ManeuverAction.Arrive);
        Assert.Equal(2, (int)ManeuverAction.Left);
        Assert.Equal(3, (int)ManeuverAction.Right);
        Assert.Equal(4, (int)ManeuverAction.SharpLeft);
        Assert.Equal(5, (int)ManeuverAction.SharpRight);
        Assert.Equal(6, (int)ManeuverAction.SlightLeft);
        Assert.Equal(7, (int)ManeuverAction.SlightRight);
        Assert.Equal(8, (int)ManeuverAction.Straight);
        Assert.Equal(9, (int)ManeuverAction.UTurnLeft);
        Assert.Equal(10, (int)ManeuverAction.UTurnRight);
    }

    [Fact]
    public void Maneuver_CreatedWithOptionalFields()
    {
        var m = new Maneuver(
            new GeoCoordinates(52.5, 13.4),
            ManeuverAction.Left,
            "Turn left onto Main St",
            BearingBefore: 180.0,
            BearingAfter: 90.0,
            NextRoadName: "Main St",
            NextRoadNumber: "B1",
            LengthInMeters: 500.0,
            DurationInSeconds: 30
        );

        Assert.Equal("Turn left onto Main St", m.Instruction);
        Assert.Equal(180.0, m.BearingBefore);
        Assert.Equal(90.0, m.BearingAfter);
        Assert.Equal("Main St", m.NextRoadName);
        Assert.Equal("B1", m.NextRoadNumber);
        Assert.Equal(500.0, m.LengthInMeters);
        Assert.Equal(30, m.DurationInSeconds);
    }

    [Fact]
    public void RoutingResult_WithRoutes()
    {
        var routes = new List<Route>
        {
            new("h1", new List<Section>(), 1000, 60)
        };
        var result = new RoutingResult(RoutingError.None, routes);

        Assert.Equal(RoutingError.None, result.Error);
        Assert.Single(result.Routes!);
    }

    [Fact]
    public void RoutingResult_WithError()
    {
        var result = new RoutingResult(RoutingError.NoRouteFound, null);

        Assert.Equal(RoutingError.NoRouteFound, result.Error);
        Assert.Null(result.Routes);
    }

    [Fact]
    public void Isoline_CreatedWithPolygon()
    {
        var polygon = new List<GeoCoordinates>
        {
            new(52.5, 13.4),
            new(52.6, 13.5),
            new(52.55, 13.3)
        };
        var isoline = new Isoline(polygon, 10000);

        Assert.Equal(3, isoline.Polygon.Count);
        Assert.Equal(10000, isoline.RangeInMeters);
    }

    [Fact]
    public void OptimizationMode_Values()
    {
        Assert.Equal(0, (int)OptimizationMode.Fastest);
        Assert.Equal(1, (int)OptimizationMode.Shortest);
    }

    [Fact]
    public void RoutingError_Values()
    {
        Assert.Equal(0, (int)RoutingError.None);
        Assert.Equal(1, (int)RoutingError.NetworkError);
        Assert.Equal(2, (int)RoutingError.HttpError);
        Assert.Equal(3, (int)RoutingError.NoRouteFound);
    }

    [Fact]
    public void TrafficOnRoute_CreatedWithSections()
    {
        var geometry = new List<GeoCoordinates> { new(52.5, 13.4), new(52.6, 13.5) };
        var span = new TrafficOnSpan(
            JamFactor: 7.5, LengthInMeters: 1200, BaseSpeedInMetersPerSecond: 25,
            TrafficSpeedInMetersPerSecond: 8, TrafficDelayInSeconds: 90, DurationInSeconds: 150,
            GeometryOffset: 1, IncidentIndices: new List<int> { 0 });
        var incident = new TrafficIncidentOnRoute("inc-1", TrafficIncidentType.Construction,
            TrafficIncidentImpact.Major, "Construction");
        var section = new TrafficOnSection(geometry, new[] { span }, new[] { incident });
        var tor = new TrafficOnRoute(0, 250, new[] { section });

        Assert.Equal(0, tor.LastTraveledSectionIndex);
        Assert.Equal(250, tor.TraveledDistanceOnLastSectionInMeters);
        Assert.Single(tor.TrafficSections);
        Assert.Equal(2, tor.TrafficSections[0].Geometry.Count);
        Assert.Single(tor.TrafficSections[0].TrafficSpans);
        Assert.Equal(7.5, tor.TrafficSections[0].TrafficSpans[0].JamFactor);
        Assert.Equal(1, tor.TrafficSections[0].TrafficSpans[0].GeometryOffset);
        Assert.Equal(90, tor.TrafficSections[0].TrafficSpans[0].TrafficDelayInSeconds);
        Assert.Single(tor.TrafficSections[0].TrafficIncidents);
        Assert.Equal("inc-1", tor.TrafficSections[0].TrafficIncidents[0].Id);
    }

    [Fact]
    public void TrafficOnRouteResult_CreatedWithError()
    {
        var result = new TrafficOnRouteResult(RoutingError.None, null);
        Assert.Equal(RoutingError.None, result.Error);
        Assert.Null(result.TrafficOnRoute);

        var tor = new TrafficOnRoute(1, 0, new List<TrafficOnSection>());
        var okResult = new TrafficOnRouteResult(RoutingError.None, tor);
        Assert.Equal(tor, okResult.TrafficOnRoute);
    }

    [Fact]
    public void TrafficOnSpan_DefaultsAndFields()
    {
        var span = new TrafficOnSpan(2.0, 500, 13.9, 5.6, 30, 40, 0, Array.Empty<int>());
        Assert.Equal(2.0, span.JamFactor);
        Assert.Equal(500, span.LengthInMeters);
        Assert.Equal(13.9, span.BaseSpeedInMetersPerSecond);
        Assert.Equal(5.6, span.TrafficSpeedInMetersPerSecond);
        Assert.Equal(30, span.TrafficDelayInSeconds);
        Assert.Equal(40, span.DurationInSeconds);
        Assert.Equal(0, span.GeometryOffset);
        Assert.Empty(span.IncidentIndices);
    }
}