using Here.Explore.Maui.Helpers;
using Here.Explore.Maui.Models;

namespace Here.Explore.Maui.Tests.Helpers;

public class CircleGeometryHelperTests
{
    [Fact]
    public void GenerateCircleVertices_ZeroRadius_ReturnsEmpty()
    {
        var center = new GeoCoordinates(0, 0);
        var vertices = CircleGeometryHelper.GenerateCircleVertices(center, 0);
        Assert.Empty(vertices);
    }

    [Fact]
    public void GenerateCircleVertices_NegativeRadius_ReturnsEmpty()
    {
        var center = new GeoCoordinates(0, 0);
        var vertices = CircleGeometryHelper.GenerateCircleVertices(center, -100);
        Assert.Empty(vertices);
    }

    [Fact]
    public void GenerateCircleVertices_PositiveRadius_ReturnsClosedLoop()
    {
        var center = new GeoCoordinates(52.5, 13.4);
        var vertices = CircleGeometryHelper.GenerateCircleVertices(center, 1000);

        Assert.True(vertices.Count > 0);
        // First and last should be the same (closed loop)
        Assert.Equal(vertices[0].Latitude, vertices[^1].Latitude, precision: 10);
        Assert.Equal(vertices[0].Longitude, vertices[^1].Longitude, precision: 10);
    }

    [Fact]
    public void GenerateCircleVertices_DefaultSegments_Returns65Points()
    {
        var center = new GeoCoordinates(0, 0);
        var vertices = CircleGeometryHelper.GenerateCircleVertices(center, 1000);
        // 64 segments + 1 closing point = 65
        Assert.Equal(65, vertices.Count);
    }

    [Fact]
    public void GenerateCircleVertices_CenterIsPreserved_Approximately()
    {
        var center = new GeoCoordinates(52.5, 13.4);
        var vertices = CircleGeometryHelper.GenerateCircleVertices(center, 1000);

        var avgLat = vertices.Average(v => v.Latitude);
        var avgLon = vertices.Average(v => v.Longitude);

        Assert.Equal(center.Latitude, avgLat, precision: 3);
        Assert.Equal(center.Longitude, avgLon, precision: 3);
    }

    [Fact]
    public void GenerateCircleVertices_RadiusIsApproximate()
    {
        var center = new GeoCoordinates(0, 0);
        var radius = 1000.0;
        var vertices = CircleGeometryHelper.GenerateCircleVertices(center, radius);

        // At equator, 1 degree ~ 111320m
        // So radius 1000m ~ 0.009 degrees
        var maxLatDelta = vertices.Max(v => Math.Abs(v.Latitude - center.Latitude));
        var expectedDelta = radius / 111320.0;

        Assert.True(maxLatDelta > expectedDelta * 0.8);
        Assert.True(maxLatDelta < expectedDelta * 1.2);
    }
}
