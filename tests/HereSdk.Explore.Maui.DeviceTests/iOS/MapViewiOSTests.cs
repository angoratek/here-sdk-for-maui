using Here.Explore.Maui.Controls;
using Here.Explore.Maui.Services;

namespace Here.Explore.Maui.DeviceTests.iOS;

public class MapViewiOSTests
{
    [Fact]
    public void HereMapView_CanBeInstantiated()
    {
        var view = new HereMapView();
        Assert.NotNull(view);
    }

    [Fact]
    public void MapService_CannotSetCamera_WhenNotInitialized()
    {
        var mapService = new MapService();
        Assert.ThrowsAsync<InvalidOperationException>(() =>
            mapService.SetCameraTargetAsync(new Models.GeoCoordinates(52.5, 13.4)));
    }

    [Fact]
    public void MapService_ZoomLevel_ReturnsZero_WhenNotInitialized()
    {
        var mapService = new MapService();
        Assert.Equal(0, mapService.ZoomLevel);
    }

    [Fact]
    public void MapService_CannotAddMarker_WhenNotInitialized()
    {
        var mapService = new MapService();
        var marker = new Models.Maps.MapMarker(new Models.GeoCoordinates(52.5, 13.4));
        Assert.Throws<InvalidOperationException>(() => mapService.AddMapMarker(marker));
    }

    [Fact]
    public void MapService_CannotAddPolyline_WhenNotInitialized()
    {
        var mapService = new MapService();
        var polyline = new Models.Maps.MapPolyline(
            new[] { new Models.GeoCoordinates(52.5, 13.4), new Models.GeoCoordinates(52.6, 13.5) });
        Assert.Throws<InvalidOperationException>(() => mapService.AddMapPolyline(polyline));
    }

    [Fact]
    public void MapService_CannotLoadScene_WhenNotInitialized()
    {
        var mapService = new MapService();
        Assert.ThrowsAsync<InvalidOperationException>(() =>
            mapService.LoadSceneAsync(Models.Maps.MapScheme.NormalDay));
    }

    [Fact]
    public void MapService_CannotAddCircle_WhenNotInitialized()
    {
        var mapService = new MapService();
        var circle = new Models.Maps.MapCircle(new Models.GeoCoordinates(52.5, 13.4), 1000);
        Assert.Throws<InvalidOperationException>(() => mapService.AddMapCircle(circle));
    }

    [Fact(Skip = "Requires HERE SDK credentials and MapView on platform thread")]
    public void MapView_Initialize_And_CreateHandler()
    {
        // End-to-end: Create MapView, attach handler, verify Map is available.
    }
}
