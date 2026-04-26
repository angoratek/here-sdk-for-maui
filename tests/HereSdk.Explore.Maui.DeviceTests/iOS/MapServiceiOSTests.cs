using Here.Explore.Maui.Helpers;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;

namespace Here.Explore.Maui.DeviceTests.iOS;

public class MapServiceiOSTests
{
    [Fact]
    public void CircleGeometryHelper_GeneratesVertices()
    {
        var center = new GeoCoordinates(52.5, 13.4);
        var vertices = CircleGeometryHelper.GenerateCircleVertices(center, 1000);

        Assert.NotEmpty(vertices);
        Assert.True(vertices.Count >= 3);
    }

    [Fact]
    public void MapCircle_Model_CanBeCreated()
    {
        var circle = new MapCircle(new GeoCoordinates(52.5, 13.4), 500, FillColor: 0x330000FF);
        Assert.Equal(500, circle.RadiusInMeters);
        Assert.Equal(0x330000FF, circle.FillColor);
    }
}
