using Xunit;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;

namespace Here.Explore.Maui.Tests.Models;

public class MapModelExtendedTests
{
    [Fact]
    public void CameraAnimation_AllParameters()
    {
        var target = new GeoCoordinates(52.5, 13.4);
        var animation = new CameraAnimation(target, ZoomLevel: 10, Bearing: 45, Tilt: 30, DurationInSeconds: 2.5);

        Assert.Equal(target, animation.Target);
        Assert.Equal(10, animation.ZoomLevel);
        Assert.Equal(45, animation.Bearing);
        Assert.Equal(30, animation.Tilt);
        Assert.Equal(2.5, animation.DurationInSeconds);
    }

    [Fact]
    public void MapMarker_WithText()
    {
        var marker = new MapMarker(new GeoCoordinates(52.5, 13.4), Text: "Berlin", AnchorX: 0, AnchorY: 0);

        Assert.Equal("Berlin", marker.Text);
        Assert.Equal(0, marker.AnchorX);
        Assert.Equal(0, marker.AnchorY);
    }

    [Fact]
    public void MapMarker3D_AllParameters()
    {
        var marker = new MapMarker3D(new GeoCoordinates(52.5, 13.4), ImagePath: "model.glb", Scale: 3.0, Bearing: 90, Tilt: 45);

        Assert.Equal("model.glb", marker.ImagePath);
        Assert.Equal(3.0, marker.Scale);
        Assert.Equal(90.0, marker.Bearing);
        Assert.Equal(45.0, marker.Tilt);
    }

    [Fact]
    public void MapPolyline_CustomColorAndWidth()
    {
        var vertices = new List<GeoCoordinates> { new(52.5, 13.4), new(52.6, 13.5) };
        var polyline = new MapPolyline(vertices, Color: 0x00FF00FF, WidthInPixels: 10);

        Assert.Equal((uint)0x00FF00FF, polyline.Color);
        Assert.Equal(10, polyline.WidthInPixels);
    }

    [Fact]
    public void MapPolygon_CustomFillColor()
    {
        var vertices = new List<GeoCoordinates> { new(52.5, 13.4), new(52.6, 13.5), new(52.55, 13.3) };
        var polygon = new MapPolygon(vertices, FillColor: 0x80FF0000);

        Assert.Equal((uint)0x80FF0000, polygon.FillColor);
    }

    [Fact]
    public void MapPolygon_StrokeDefaults_AreDisabled()
    {
        var vertices = new List<GeoCoordinates> { new(52.5, 13.4), new(52.6, 13.5), new(52.55, 13.3) };
        var polygon = new MapPolygon(vertices);

        Assert.Equal(0u, polygon.StrokeColor);
        Assert.Equal(0, polygon.StrokeWidthInPixels);
    }

    [Fact]
    public void MapPolygon_WithStroke_ParamsAppendPositionally()
    {
        var vertices = new List<GeoCoordinates> { new(52.5, 13.4), new(52.6, 13.5), new(52.55, 13.3) };
        var polygon = new MapPolygon(vertices, FillColor: 0x33000000, StrokeColor: 0xFFFF385C, StrokeWidthInPixels: 3);

        Assert.Equal((uint)0xFFFF385C, polygon.StrokeColor);
        Assert.Equal(3, polygon.StrokeWidthInPixels);
    }

    [Fact]
    public void MapPolyline_Cap_DefaultsToRound()
    {
        var vertices = new List<GeoCoordinates> { new(52.5, 13.4), new(52.6, 13.5) };

        Assert.Equal(LineCap.Round, new MapPolyline(vertices).Cap);
        Assert.Equal(LineCap.Butt, new MapPolyline(vertices, Cap: LineCap.Butt).Cap);
    }

    [Fact]
    public void MapArrow_CustomColorAndWidth()
    {
        var vertices = new List<GeoCoordinates> { new(52.5, 13.4), new(52.6, 13.5) };
        var arrow = new MapArrow(vertices, Color: 0xFFFF0000, WidthInPixels: 8);

        Assert.Equal((uint)0xFFFF0000, arrow.Color);
        Assert.Equal(8, arrow.WidthInPixels);
    }

    [Fact]
    public void MapPickResult_Created()
    {
        var coords = new GeoCoordinates(52.5, 13.4);
        var result = new MapPickResult(coords);

        Assert.Equal(coords, result.Coordinates);
        Assert.Null(result.PlatformResult);
    }

    [Fact]
    public void MapPickResult_WithPlatformResult()
    {
        var coords = new GeoCoordinates(52.5, 13.4);
        var result = new MapPickResult(coords, PlatformResult: "native-object");

        Assert.Equal("native-object", result.PlatformResult);
    }

    [Fact]
    public void CameraStateChangedEventArgs_Created()
    {
        var args = new CameraStateChangedEventArgs(new GeoCoordinates(52.5, 13.4), 10, 45, 30);

        Assert.Equal(52.5, args.Target.Latitude);
        Assert.Equal(10, args.ZoomLevel);
        Assert.Equal(45, args.Bearing);
        Assert.Equal(30, args.Tilt);
    }

    [Fact]
    public void MapTappedEventArgs_Created()
    {
        var args = new MapTappedEventArgs(new GeoCoordinates(52.5, 13.4), new Point2D(100, 200));

        Assert.Equal(52.5, args.Coordinates.Latitude);
        Assert.Equal(100, args.ScreenPoint.X);
        Assert.Equal(200, args.ScreenPoint.Y);
    }

    [Fact]
    public void DrawOrderType_Values()
    {
        Assert.Equal(0, (int)DrawOrderType.AbovePolygons);
        Assert.Equal(1, (int)DrawOrderType.AbovePolygonsAndAdas);
        Assert.Equal(2, (int)DrawOrderType.BelowPolygons);
    }

    [Fact]
    public void LineCap_Values()
    {
        Assert.Equal(0, (int)LineCap.Round);
        Assert.Equal(1, (int)LineCap.Square);
        Assert.Equal(2, (int)LineCap.Butt);
    }

    [Fact]
    public void MapContentCategory_Values()
    {
        Assert.Equal(0, (int)MapContentCategory.NoCategory);
        Assert.Equal(1, (int)MapContentCategory.PoiCategory);
        Assert.Equal(2, (int)MapContentCategory.TrafficIncidentCategory);
        Assert.Equal(3, (int)MapContentCategory.CarCategory);
        Assert.Equal(4, (int)MapContentCategory.TruckCategory);
        Assert.Equal(5, (int)MapContentCategory.PedestrianCategory);
    }

    [Fact]
    public void GeoPolygon_Created()
    {
        var vertices = new List<GeoCoordinates> { new(52.5, 13.4), new(52.6, 13.5), new(52.55, 13.3) };
        var polygon = new GeoPolygon(vertices);

        Assert.Equal(3, polygon.Vertices.Count);
    }

    [Fact]
    public void GeoPolyline_Created()
    {
        var vertices = new List<GeoCoordinates> { new(52.5, 13.4), new(52.6, 13.5) };
        var polyline = new GeoPolyline(vertices);

        Assert.Equal(2, polyline.Vertices.Count);
    }

    [Fact]
    public void GeoCorridor_Created()
    {
        var polyline = new GeoPolyline(new List<GeoCoordinates> { new(52.5, 13.4), new(52.6, 13.5) });
        var corridor = new GeoCorridor(polyline, 500);

        Assert.Equal(500, corridor.RadiusInMeters);
        Assert.Equal(2, corridor.Polyline.Vertices.Count);
    }

    [Fact]
    public void Point2D_Created()
    {
        var point = new Point2D(100, 200);
        Assert.Equal(100, point.X);
        Assert.Equal(200, point.Y);
    }

    [Fact]
    public void Size2D_Created()
    {
        var size = new Size2D(800, 600);
        Assert.Equal(800, size.Width);
        Assert.Equal(600, size.Height);
    }

    [Fact]
    public void Rectangle2D_Created()
    {
        var rect = new Rectangle2D(new Point2D(10, 20), new Size2D(100, 50));
        Assert.Equal(10, rect.Origin.X);
        Assert.Equal(20, rect.Origin.Y);
        Assert.Equal(100, rect.Size.Width);
        Assert.Equal(50, rect.Size.Height);
    }

    [Fact]
    public void GeoCoordinatesUpdate_ApplyTo_OverridesLatitude()
    {
        var original = new GeoCoordinates(52.5, 13.4);
        var update = new GeoCoordinatesUpdate(Latitude: 48.8);
        var result = update.ApplyTo(original);

        Assert.Equal(48.8, result.Latitude);
        Assert.Equal(13.4, result.Longitude);
    }

    [Fact]
    public void GeoCoordinatesUpdate_ApplyTo_OverridesLongitude()
    {
        var original = new GeoCoordinates(52.5, 13.4);
        var update = new GeoCoordinatesUpdate(Longitude: 2.35);
        var result = update.ApplyTo(original);

        Assert.Equal(52.5, result.Latitude);
        Assert.Equal(2.35, result.Longitude);
    }

    [Fact]
    public void GeoCoordinatesUpdate_ApplyTo_AllNull_ReturnsOriginal()
    {
        var original = new GeoCoordinates(52.5, 13.4);
        var update = new GeoCoordinatesUpdate();
        var result = update.ApplyTo(original);

        Assert.Equal(52.5, result.Latitude);
        Assert.Equal(13.4, result.Longitude);
    }

    [Fact]
    public void GeoOrientationUpdate_ApplyTo_OverridesTilt()
    {
        var original = new GeoOrientation(90.0, 30.0);
        var update = new GeoOrientationUpdate(Tilt: 60.0);
        var result = update.ApplyTo(original);

        Assert.Equal(90.0, result.Bearing);
        Assert.Equal(60.0, result.Tilt);
    }

    [Fact]
    public void HereSdkCachePolicy_Values()
    {
        Assert.Equal(0, (int)HereSdkCachePolicy.Default);
        Assert.Equal(1, (int)HereSdkCachePolicy.NoCache);
        Assert.Equal(2, (int)HereSdkCachePolicy.OfflineOnly);
    }
}