using Here.Explore.Maui.Models;
using Here.Explore.Maui.Services;
using Location = Here.Explore.Maui.Models.Location;

namespace Here.Explore.Maui.DeviceTests.iOS;

public class LocationServiceiOSTests
{
    [Fact]
    public void LocationService_CanBeInstantiated()
    {
        var service = new LocationService();
        Assert.NotNull(service);
        Assert.False(service.IsListening);
    }

    [Fact]
    public void LocationService_Dispose_DoesNotThrow()
    {
        var service = new LocationService();
        service.Dispose();
        Assert.True(true);
    }

    [Fact]
    public void LocationService_LocationChanged_CanRaiseEvent()
    {
        var service = new LocationService();
        var received = false;
        service.LocationChanged += (_, _) => received = true;

        var location = new Location(new GeoCoordinates(52.5, 13.4));
        service.GetType().GetMethod("RaiseLocationChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(service, new object?[] { location });

        Assert.True(received);
    }
}
