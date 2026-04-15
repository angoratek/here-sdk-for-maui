using Xunit;
using Here.Explore.Maui.Models;

namespace Here.Explore.Maui.Tests.Models;

public class GeoCircleTests
{
    [Fact]
    public void Constructor_SetsCenterAndRadius()
    {
        var center = new GeoCoordinates(52.5, 13.4);
        var circle = new GeoCircle(center, 1000);
        Assert.Equal(center, circle.Center);
        Assert.Equal(1000, circle.RadiusInMeters);
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        var a = new GeoCircle(new GeoCoordinates(52.5, 13.4), 1000);
        var b = new GeoCircle(new GeoCoordinates(52.5, 13.4), 1000);
        Assert.Equal(a, b);
    }
}