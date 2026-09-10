using Here.Explore.Maui.Helpers;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;

namespace Here.Explore.Maui.DeviceTests.Android;

public class MapServiceAndroidTests
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
    public void CircleGeometryHelper_LargeRadius_GeneratesMoreVertices()
    {
        var center = new GeoCoordinates(52.5, 13.4);
        var vertices = CircleGeometryHelper.GenerateCircleVertices(center, 100000);

        Assert.NotEmpty(vertices);
    }

    [Fact]
    public void CircleGeometryHelper_VerticesAreNearCenter()
    {
        var center = new GeoCoordinates(52.5, 13.4);
        var vertices = CircleGeometryHelper.GenerateCircleVertices(center, 1000);

        foreach (var v in vertices)
        {
            // Each vertex should be within reasonable distance from center
            var latDiff = Math.Abs(v.Latitude - center.Latitude);
            var lonDiff = Math.Abs(v.Longitude - center.Longitude);
            Assert.True(latDiff < 0.1);
            Assert.True(lonDiff < 0.1);
        }
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
    public void GeoCoordinates_Equality_DifferentValues_NotEqual()
    {
        var a = new GeoCoordinates(52.5, 13.4);
        var b = new GeoCoordinates(48.8, 2.3);
        Assert.NotEqual(a, b);
    }

    // ================================================================
    // MapMarker model tests
    // ================================================================

    [Fact]
    public void MapMarker_Model_CanBeCreated()
    {
        var marker = new MapMarker(new GeoCoordinates(52.5, 13.4));
        Assert.Equal(52.5, marker.Coordinates.Latitude);
        Assert.Equal(13.4, marker.Coordinates.Longitude);
    }

    [Fact]
    public void MapMarker_Model_WithCustomImage()
    {
        var marker = new MapMarker(
            new GeoCoordinates(52.5, 13.4),
            ImagePath: "custom-pin");
        Assert.Equal("custom-pin", marker.ImagePath);
    }

    [Fact]
    public void MapMarker_Model_WithAllProperties()
    {
        var marker = new MapMarker(
            new GeoCoordinates(52.5, 13.4),
            Text: "Test Marker",
            ImagePath: "pin",
            AnchorX: 0,
            AnchorY: 0);
        Assert.Equal("Test Marker", marker.Text);
        Assert.Equal(0, marker.AnchorX);
        Assert.Equal(0, marker.AnchorY);
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
    public void MapPolyline_Model_DefaultColor()
    {
        var vertices = new[] { new GeoCoordinates(0, 0), new GeoCoordinates(1, 1) };
        var polyline = new MapPolyline(vertices);
        Assert.Equal(0xFF0000FFu, polyline.Color);
    }

    [Fact]
    public void MapPolyline_Model_CustomWidth()
    {
        var vertices = new[] { new GeoCoordinates(0, 0), new GeoCoordinates(1, 1) };
        var polyline = new MapPolyline(vertices, 0xFFFF0000, 5);
        Assert.Equal(5, polyline.WidthInPixels);
        Assert.Equal(0xFFFF0000u, polyline.Color);
    }

    // ================================================================
    // MapPolygon model tests
    // ================================================================

    [Fact]
    public void MapPolygon_Model_CanBeCreated()
    {
        var vertices = new[] {
            new GeoCoordinates(0, 0),
            new GeoCoordinates(1, 0),
            new GeoCoordinates(0, 1) };
        var polygon = new MapPolygon(vertices);
        Assert.Equal(3, polygon.Vertices.Count);
    }

    [Fact]
    public void MapPolygon_Model_CustomFillColor()
    {
        var vertices = new[] { new GeoCoordinates(0, 0), new GeoCoordinates(1, 0), new GeoCoordinates(0, 1) };
        var polygon = new MapPolygon(vertices, FillColor: 0x3300FF00);
        Assert.Equal(0x3300FF00u, polygon.FillColor);
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
    public void MapCircle_Model_DefaultStrokeColor()
    {
        var circle = new MapCircle(new GeoCoordinates(0, 0), 100);
        Assert.Equal(0xFF0000FFu, circle.StrokeColor);
        Assert.Equal(2, circle.StrokeWidthInPixels);
    }

    [Fact]
    public void MapCircle_Model_CustomStroke()
    {
        var circle = new MapCircle(new GeoCoordinates(0, 0), 100,
            StrokeColor: 0xFFFF0000, StrokeWidthInPixels: 4);
        Assert.Equal(0xFFFF0000u, circle.StrokeColor);
        Assert.Equal(4, circle.StrokeWidthInPixels);
    }

    // ================================================================
    // MapScheme enum tests
    // ================================================================

    [Fact]
    public void MapScheme_AllValues_AreDefined()
    {
        var schemes = Enum.GetValues<MapScheme>();
        Assert.Contains(MapScheme.NormalDay, schemes);
        Assert.Contains(MapScheme.NormalNight, schemes);
        Assert.Contains(MapScheme.HybridDay, schemes);
        Assert.Contains(MapScheme.SatelliteDay, schemes);
        Assert.Contains(MapScheme.TerrainDay, schemes);
    }

    // ================================================================
    // MapMarker anchor tests
    // ================================================================

    [Fact]
    public void MapMarker_DefaultAnchor_IsNull()
    {
        var marker = new MapMarker(new GeoCoordinates(0, 0));
        Assert.Null(marker.AnchorX);
        Assert.Null(marker.AnchorY);
    }

    [Fact]
    public void MapMarker_CustomAnchor()
    {
        var marker = new MapMarker(new GeoCoordinates(0, 0), AnchorX: 0, AnchorY: 0);
        Assert.Equal(0, marker.AnchorX);
        Assert.Equal(0, marker.AnchorY);
    }

    // ================================================================
    // CameraAnimation model tests
    // ================================================================

    [Fact]
    public void CameraAnimation_CanBeCreated()
    {
        var animation = new CameraAnimation(
            new GeoCoordinates(52.5, 13.4),
            DurationInSeconds: 2.0,
            ZoomLevel: 15,
            Bearing: 45,
            Tilt: 30);
        Assert.Equal(52.5, animation.Target.Latitude);
        Assert.Equal(2.0, animation.DurationInSeconds);
        Assert.Equal(15, animation.ZoomLevel);
        Assert.Equal(45, animation.Bearing);
        Assert.Equal(30, animation.Tilt);
    }

    // ================================================================
    // LocationIndicator model tests
    // ================================================================

    [Fact]
    public void LocationIndicator_Model_CanBeCreated()
    {
        var indicator = new LocationIndicator(
            new GeoCoordinates(52.5, 13.4),
            Style: LocationIndicatorStyle.Navigation);
        Assert.Equal(LocationIndicatorStyle.Navigation, indicator.Style);
    }

    // ================================================================
    // MapMarkerCluster model tests
    // ================================================================

    [Fact]
    public void MapMarkerCluster_Model_CanBeCreated()
    {
        var cluster = new MapMarkerCluster();
        Assert.NotNull(cluster);
    }

    // ================================================================
    // MapMarker3D model tests
    // ================================================================

    [Fact]
    public void MapMarker3D_Model_CanBeCreated()
    {
        var marker = new MapMarker3D(new GeoCoordinates(52.5, 13.4));
        Assert.Equal(1.0, marker.Scale);
        Assert.Equal(0.0, marker.Bearing);
    }

    [Fact]
    public void MapMarker3D_Model_CustomScale()
    {
        var marker = new MapMarker3D(new GeoCoordinates(52.5, 13.4), Scale: 2.5);
        Assert.Equal(2.5, marker.Scale);
    }

    // ================================================================
    // MapArrow model tests
    // ================================================================

    [Fact]
    public void MapArrow_Model_CanBeCreated()
    {
        var vertices = new[] { new GeoCoordinates(52.5, 13.4), new GeoCoordinates(52.6, 13.5) };
        var arrow = new MapArrow(vertices, 3);
        Assert.Equal(2, arrow.Vertices.Count);
        Assert.Equal(3, arrow.WidthInPixels);
    }
}
