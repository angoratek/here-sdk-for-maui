using Xunit;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;

namespace Here.Explore.Maui.Tests.Controls;

public class GestureEventTests
{
    // --- MapTappedEventArgs (existing) ---

    [Fact]
    public void MapTappedEventArgs_Created()
    {
        var e = new MapTappedEventArgs(new GeoCoordinates(52.5, 13.4), new Point2D(100, 200));

        Assert.Equal(52.5, e.Coordinates.Latitude);
        Assert.Equal(13.4, e.Coordinates.Longitude);
        Assert.Equal(100, e.ScreenPoint.X);
        Assert.Equal(200, e.ScreenPoint.Y);
    }

    // --- MapDoubleTappedEventArgs ---

    [Fact]
    public void MapDoubleTappedEventArgs_Created()
    {
        var e = new MapDoubleTappedEventArgs(new GeoCoordinates(52.5, 13.4), new Point2D(100, 200));

        Assert.Equal(52.5, e.Coordinates.Latitude);
        Assert.Equal(100, e.ScreenPoint.X);
    }

    // --- MapLongPressedEventArgs ---

    [Fact]
    public void MapLongPressedEventArgs_Created()
    {
        var e = new MapLongPressedEventArgs(new GeoCoordinates(52.5, 13.4), new Point2D(100, 200));

        Assert.Equal(52.5, e.Coordinates.Latitude);
        Assert.Equal(100, e.ScreenPoint.X);
    }

    // --- MapPannedEventArgs ---

    [Fact]
    public void MapPannedEventArgs_Created()
    {
        var e = new MapPannedEventArgs(new GeoCoordinates(52.5, 13.4), new Point2D(50, 75));

        Assert.Equal(52.5, e.Coordinates.Latitude);
        Assert.Equal(50, e.Delta.X);
        Assert.Equal(75, e.Delta.Y);
    }

    // --- MapPinchRotatedEventArgs ---

    [Fact]
    public void MapPinchRotatedEventArgs_Created()
    {
        var e = new MapPinchRotatedEventArgs(1.5, 45.0);

        Assert.Equal(1.5, e.Scale);
        Assert.Equal(45.0, e.RotationInDegrees);
    }
}