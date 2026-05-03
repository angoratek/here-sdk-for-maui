using Xunit;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Routing;
using Here.Explore.Maui.Helpers;

namespace Here.Explore.Maui.Tests.Helpers;

public class RouteGeometryHelperTests
{
    [Fact]
    public void ExtractGeometry_UsesSectionGeometry_WhenAvailable()
    {
        var geometry = new List<GeoCoordinates>
        {
            new(52.5, 13.4),
            new(52.51, 13.41),
            new(52.52, 13.42)
        };
        var section = new Section(
            0,
            new GeoCoordinates(52.5, 13.4),
            new GeoCoordinates(52.52, 13.42),
            new List<Maneuver>
            {
                new(new GeoCoordinates(52.5, 13.4), ManeuverAction.Depart),
                new(new GeoCoordinates(52.52, 13.42), ManeuverAction.Arrive)
            },
            SectionTransportMode.Car,
            2000,
            120,
            geometry);
        var route = new Route("r1", new List<Section> { section }, 2000, 120);

        var result = RouteGeometryHelper.ExtractGeometry(route);

        Assert.Equal(3, result.Count);
        Assert.Equal(52.5, result[0].Latitude);
        Assert.Equal(52.51, result[1].Latitude);
        Assert.Equal(52.52, result[2].Latitude);
    }

    [Fact]
    public void ExtractGeometry_FallsBackToManeuvers_WhenGeometryIsNull()
    {
        var section = new Section(
            0,
            new GeoCoordinates(52.5, 13.4),
            new GeoCoordinates(52.52, 13.42),
            new List<Maneuver>
            {
                new(new GeoCoordinates(52.5, 13.4), ManeuverAction.Depart),
                new(new GeoCoordinates(52.51, 13.41), ManeuverAction.Straight),
                new(new GeoCoordinates(52.52, 13.42), ManeuverAction.Arrive)
            },
            SectionTransportMode.Car,
            2000,
            120);
        var route = new Route("r1", new List<Section> { section }, 2000, 120);

        var result = RouteGeometryHelper.ExtractGeometry(route);

        Assert.Equal(3, result.Count);
        Assert.Equal(52.5, result[0].Latitude);
        Assert.Equal(52.51, result[1].Latitude);
        Assert.Equal(52.52, result[2].Latitude);
    }

    [Fact]
    public void ExtractGeometry_CombinesMultipleSections()
    {
        var s1 = new Section(
            0,
            new GeoCoordinates(0, 0),
            new GeoCoordinates(1, 1),
            new List<Maneuver> { new(new GeoCoordinates(0, 0), ManeuverAction.Depart) },
            SectionTransportMode.Car,
            1000,
            60,
            new List<GeoCoordinates> { new(0, 0), new(0.5, 0.5), new(1, 1) });
        var s2 = new Section(
            1,
            new GeoCoordinates(1, 1),
            new GeoCoordinates(2, 2),
            new List<Maneuver> { new(new GeoCoordinates(1, 1), ManeuverAction.Arrive) },
            SectionTransportMode.Car,
            1000,
            60,
            new List<GeoCoordinates> { new(1, 1), new(1.5, 1.5), new(2, 2) });
        var route = new Route("r1", new List<Section> { s1, s2 }, 2000, 120);

        var result = RouteGeometryHelper.ExtractGeometry(route);

        Assert.Equal(6, result.Count);
    }

    [Fact]
    public void CreatePolyline_ReturnsValidPolyline()
    {
        var geometry = new List<GeoCoordinates>
        {
            new(52.5, 13.4),
            new(52.52, 13.42)
        };

        var polyline = RouteGeometryHelper.CreatePolyline(geometry);

        Assert.NotNull(polyline);
        Assert.Equal(2, polyline.Vertices.Count);
        Assert.Equal(0xFF0000FFu, polyline.Color);
        Assert.Equal(6, polyline.WidthInPixels);
    }

    [Fact]
    public void CreatePolyline_ThrowsWhenGeometryEmpty()
    {
        var geometry = new List<GeoCoordinates>();

        Assert.Throws<ArgumentException>(() => RouteGeometryHelper.CreatePolyline(geometry));
    }
}
