using Here.Explore.Maui.Helpers;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;

namespace Here.Explore.Maui.DeviceTests.iOS;

public class MapServiceiOSTests
{
    // ================================================================
    // CircleGeometryHelper tests
    // ================================================================

    [Fact]
    public void CircleGeometryHelper_GeneratesVertices()
    {
        var center = new GeoCoordinates(52.5, 13.4);
        var vertices = CircleGeometryHelper.GenerateCircleVertices(center, 1000);

        Assert.NotEmpty(vertices);
        Assert.True(vertices.Count >= 3);
    }

    [Fact]
    public void CircleGeometryHelper_SmallRadius_StillGeneratesPolygon()
    {
        var center = new GeoCoordinates(0, 0);
        var vertices = CircleGeometryHelper.GenerateCircleVertices(center, 10);

        Assert.NotEmpty(vertices);
        Assert.True(vertices.Count >= 3);
    }

    [Fact]
    public void CircleGeometryHelper_LargeRadius_GeneratesVertices()
    {
        var center = new GeoCoordinates(52.5, 13.4);
        var vertices = CircleGeometryHelper.GenerateCircleVertices(center, 50000);

        Assert.NotEmpty(vertices);
    }

    // ================================================================
    // GeoCoordinates model tests
    // ================================================================

    [Fact]
    public void GeoCoordinates_CanBeCreated()
    {
        var coords = new GeoCoordinates(52.5, 13.4);
        Assert.Equal(52.5, coords.Latitude);
        Assert.Equal(13.4, coords.Longitude);
    }

    [Fact]
    public void GeoCoordinates_Equality_Works()
    {
        var a = new GeoCoordinates(52.5, 13.4);
        var b = new GeoCoordinates(52.5, 13.4);
        Assert.Equal(a, b);
    }

    [Fact]
    public void GeoCoordinates_NegativeValues()
    {
        var coords = new GeoCoordinates(-33.8, 151.2);
        Assert.Equal(-33.8, coords.Latitude);
        Assert.Equal(151.2, coords.Longitude);
    }

    // ================================================================
    // MapMarker model tests
    // ================================================================

    [Fact]
    public void MapMarker_Model_CanBeCreated()
    {
        var marker = new MapMarker(new GeoCoordinates(52.5, 13.4));
        Assert.Equal(52.5, marker.Coordinates.Latitude);
    }

    [Fact]
    public void MapMarker_Model_WithImageId()
    {
        var marker = new MapMarker(new GeoCoordinates(0, 0), ImagePath: "custom-pin");
        Assert.Equal("custom-pin", marker.ImagePath);
    }

    // ================================================================
    // MapCircle model tests
    // ================================================================

    [Fact]
    public void MapCircle_Model_CanBeCreated()
    {
        var circle = new MapCircle(new GeoCoordinates(52.5, 13.4), 500, FillColor: 0x330000FF);
        Assert.Equal(500, circle.RadiusInMeters);
        Assert.Equal(0x330000FFu, circle.FillColor);
    }

    [Fact]
    public void MapCircle_Model_DefaultStroke()
    {
        var circle = new MapCircle(new GeoCoordinates(0, 0), 100);
        Assert.Equal(0xFF0000FFu, circle.StrokeColor);
        Assert.Equal(2, circle.StrokeWidthInPixels);
    }

    // ================================================================
    // MapPolyline model tests
    // ================================================================

    [Fact]
    public void MapPolyline_Model_CanBeCreated()
    {
        var vertices = new[] { new GeoCoordinates(52.5, 13.4), new GeoCoordinates(52.6, 13.5) };
        var polyline = new MapPolyline(vertices);
        Assert.Equal(2, polyline.Vertices.Count);
    }

    [Fact]
    public void MapPolyline_Model_CustomColor()
    {
        var vertices = new[] { new GeoCoordinates(0, 0), new GeoCoordinates(1, 1) };
        var polyline = new MapPolyline(vertices, 0xFFFF5500, 3);
        Assert.Equal(0xFFFF5500u, polyline.Color);
    }

    // ================================================================
    // MapPolygon model tests
    // ================================================================

    [Fact]
    public void MapPolygon_Model_CanBeCreated()
    {
        var vertices = new[] {
            new GeoCoordinates(0, 0), new GeoCoordinates(1, 0), new GeoCoordinates(0, 1) };
        var polygon = new MapPolygon(vertices);
        Assert.Equal(3, polygon.Vertices.Count);
    }

    // ================================================================
    // MapScheme enum tests
    // ================================================================

    [Fact]
    public void MapScheme_HasExpectedValues()
    {
        var schemes = Enum.GetValues<MapScheme>();
        Assert.Contains(MapScheme.NormalDay, schemes);
        Assert.Contains(MapScheme.NormalNight, schemes);
    }

    // ================================================================
    // CameraAnimation model tests
    // ================================================================

    [Fact]
    public void CameraAnimation_CanBeCreated()
    {
        var animation = new CameraAnimation(
            new GeoCoordinates(52.5, 13.4),
            DurationInSeconds: 1.5,
            ZoomLevel: 14);
        Assert.Equal(1.5, animation.DurationInSeconds);
        Assert.Equal(14, animation.ZoomLevel);
    }
}
