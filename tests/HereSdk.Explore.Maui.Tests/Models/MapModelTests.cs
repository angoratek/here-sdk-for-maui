using Xunit;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;

namespace Here.Explore.Maui.Tests.Models;

public class MapModelTests
{
    [Fact]
    public void MapMarker_CreatedWithCoordinates()
    {
        var marker = new MapMarker(new GeoCoordinates(52.5, 13.4));

        Assert.Equal(52.5, marker.Coordinates.Latitude);
        Assert.Equal(13.4, marker.Coordinates.Longitude);
        Assert.Null(marker.ImagePath);
    }

    [Fact]
    public void MapMarker_CreatedWithImage()
    {
        var marker = new MapMarker(new GeoCoordinates(52.5, 13.4), ImagePath: "pin.png");

        Assert.Equal("pin.png", marker.ImagePath);
    }

    [Fact]
    public void MapMarker3D_CreatedWithScale()
    {
        var marker = new MapMarker3D(new GeoCoordinates(52.5, 13.4), Scale: 2.0);

        Assert.Equal(2.0, marker.Scale);
        Assert.Equal(0.0, marker.Bearing);
    }

    [Fact]
    public void MapPolyline_CreatedWithDefaults()
    {
        var vertices = new List<GeoCoordinates> { new(52.5, 13.4), new(52.6, 13.5) };
        var polyline = new MapPolyline(vertices);

        Assert.Equal(2, polyline.Vertices.Count);
        Assert.Equal((uint)0xFF0000FF, polyline.Color);
        Assert.Equal(5, polyline.WidthInPixels);
    }

    [Fact]
    public void MapPolygon_CreatedWithDefaults()
    {
        var vertices = new List<GeoCoordinates> { new(52.5, 13.4), new(52.6, 13.5), new(52.55, 13.3) };
        var polygon = new MapPolygon(vertices);

        Assert.Equal(3, polygon.Vertices.Count);
        Assert.Equal((uint)0x330000FF, polygon.FillColor);
    }

    [Fact]
    public void MapArrow_CreatedWithDefaults()
    {
        var vertices = new List<GeoCoordinates> { new(52.5, 13.4), new(52.6, 13.5) };
        var arrow = new MapArrow(vertices);

        Assert.Equal((uint)0xFF0000FF, arrow.Color);
        Assert.Equal(5, arrow.WidthInPixels);
    }

    [Fact]
    public void CameraAnimation_CreatedWithDefaults()
    {
        var target = new GeoCoordinates(52.5, 13.4);
        var animation = new CameraAnimation(target);

        Assert.Null(animation.ZoomLevel);
        Assert.Null(animation.Bearing);
        Assert.Null(animation.Tilt);
        Assert.Equal(1.0, animation.DurationInSeconds);
    }

    [Fact]
    public void MapScheme_ValuesMatch()
    {
        Assert.Equal(0, (int)MapScheme.NormalDay);
        Assert.Equal(1, (int)MapScheme.NormalNight);
        Assert.Equal(2, (int)MapScheme.HybridDay);
        Assert.Equal(3, (int)MapScheme.SatelliteDay);
        Assert.Equal(4, (int)MapScheme.TerrainDay);
    }
}