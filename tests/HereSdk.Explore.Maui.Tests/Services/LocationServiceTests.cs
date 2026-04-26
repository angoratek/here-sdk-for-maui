using Here.Explore.Maui.Models;
using Here.Explore.Maui.Services;
using NSubstitute;

namespace Here.Explore.Maui.Tests.Services;

public class LocationServiceTests
{
    [Fact]
    public void LocationService_Implements_ILocationService()
    {
        Assert.True(typeof(ILocationService).IsAssignableFrom(typeof(LocationService)));
    }

    [Fact]
    public void LocationService_Implements_IDisposable()
    {
        Assert.True(typeof(IDisposable).IsAssignableFrom(typeof(LocationService)));
    }

    [Fact]
    public async Task GetCurrentLocationAsync_Stub_ThrowsNotImplemented()
    {
        var service = new LocationService();
        await Assert.ThrowsAsync<NotImplementedException>(() => service.GetCurrentLocationAsync());
    }

    [Fact]
    public async Task StartListeningAsync_Stub_ThrowsNotImplemented()
    {
        var service = new LocationService();
        await Assert.ThrowsAsync<NotImplementedException>(() => service.StartListeningAsync());
    }

    [Fact]
    public async Task StopListeningAsync_Stub_ThrowsNotImplemented()
    {
        var service = new LocationService();
        await Assert.ThrowsAsync<NotImplementedException>(() => service.StopListeningAsync());
    }

    [Fact]
    public void IsListening_Default_IsFalse()
    {
        var service = new LocationService();
        Assert.False(service.IsListening);
    }

    [Fact]
    public void LocationChanged_CanSubscribe()
    {
        var service = new LocationService();
        var received = false;
        service.LocationChanged += (_, _) => received = true;

        var location = new Location(new GeoCoordinates(52.5, 13.4));
        service.GetType().GetMethod("RaiseLocationChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(service, new object?[] { location });

        Assert.True(received);
    }

    [Fact]
    public async Task ILocationService_GetCurrentLocationAsync_Mock()
    {
        var service = Substitute.For<ILocationService>();
        var expected = new Location(new GeoCoordinates(52.5, 13.4));
        service.GetCurrentLocationAsync(Arg.Any<CancellationToken>()).Returns(expected);

        var result = await service.GetCurrentLocationAsync();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ILocationService_StartListeningAsync_Mock()
    {
        var service = Substitute.For<ILocationService>();
        service.StartListeningAsync();
        service.Received(1).StartListeningAsync();
    }

    [Fact]
    public void ILocationService_StopListeningAsync_Mock()
    {
        var service = Substitute.For<ILocationService>();
        service.StopListeningAsync();
        service.Received(1).StopListeningAsync();
    }
}
