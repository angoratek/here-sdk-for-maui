using Xunit;
using Here.Explore.Maui.Models;

namespace Here.Explore.Maui.Tests.Models;

public class GeoBoxTests
{
    [Fact]
    public void Constructor_SetsCorners()
    {
        var sw = new GeoCoordinates(52.4, 13.3);
        var ne = new GeoCoordinates(52.6, 13.5);
        var box = new GeoBox(sw, ne);
        Assert.Equal(sw, box.SouthWest);
        Assert.Equal(ne, box.NorthEast);
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        var a = new GeoBox(new GeoCoordinates(52.4, 13.3), new GeoCoordinates(52.6, 13.5));
        var b = new GeoBox(new GeoCoordinates(52.4, 13.3), new GeoCoordinates(52.6, 13.5));
        Assert.Equal(a, b);
    }
}