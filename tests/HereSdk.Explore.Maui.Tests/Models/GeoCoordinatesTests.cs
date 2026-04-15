using Xunit;
using Here.Explore.Maui.Models;

namespace Here.Explore.Maui.Tests.Models;

public class GeoCoordinatesTests
{
    [Fact]
    public void Constructor_SetsCoordinates()
    {
        var coords = new GeoCoordinates(52.531268, 13.387659);
        Assert.Equal(52.531268, coords.Latitude);
        Assert.Equal(13.387659, coords.Longitude);
    }

    [Fact]
    public void IsValid_WithValidCoordinates_ReturnsTrue()
    {
        var coords = new GeoCoordinates(52.531268, 13.387659);
        Assert.True(coords.IsValid);
    }

    [Theory]
    [InlineData(-91, 0)]
    [InlineData(91, 0)]
    [InlineData(0, -181)]
    [InlineData(0, 181)]
    public void IsValid_WithInvalidCoordinates_ReturnsFalse(double lat, double lon)
    {
        var coords = new GeoCoordinates(lat, lon);
        Assert.False(coords.IsValid);
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        var a = new GeoCoordinates(52.5, 13.4);
        var b = new GeoCoordinates(52.5, 13.4);
        Assert.Equal(a, b);
        Assert.True(a == b);
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = new GeoCoordinates(52.5, 13.4);
        var b = new GeoCoordinates(48.8, 2.3);
        Assert.NotEqual(a, b);
        Assert.True(a != b);
    }
}