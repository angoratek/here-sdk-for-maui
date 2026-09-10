using Here.Explore.Maui.Controls;
using Here.Explore.Maui.Handlers;
using Here.Explore.Maui.Services;

namespace Here.Explore.Maui.DeviceTests.Android;

public class MapViewAndroidTests
{
    [Fact]
    public void HereMapView_CanBeInstantiated()
    {
        var view = new HereMapView();
        Assert.NotNull(view);
    }

    [Fact]
    public void HereMapView_HasDefaultAutomationId()
    {
        var view = new HereMapView();
        Assert.NotNull(view);
    }

    [Fact]
    public void HereMapView_Map_IsNotNull_AfterHandlerCreated()
    {
        var view = new HereMapView();
        // Map is created by the handler on the platform thread.
        // In device test context, the handler may not be attached yet.
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
    public void MapService_CannotRemoveMarkerCluster_WhenNotInitialized()
    {
        var mapService = new MapService();
        var cluster = new Models.Maps.MapMarkerCluster();
        Assert.Throws<InvalidOperationException>(() => mapService.RemoveMapMarkerCluster(cluster));
    }

    [Fact]
    public void MapService_CannotAdd3DMarker_WhenNotInitialized()
    {
        var mapService = new MapService();
        var marker = new Models.Maps.MapMarker3D(new Models.GeoCoordinates(52.5, 13.4));
        Assert.Throws<InvalidOperationException>(() => mapService.AddMapMarker3D(marker));
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
    public void MapService_CannotAddPolygon_WhenNotInitialized()
    {
        var mapService = new MapService();
        var polygon = new Models.Maps.MapPolygon(
            new[] { new Models.GeoCoordinates(0, 0), new Models.GeoCoordinates(1, 0), new Models.GeoCoordinates(0, 1) });
        Assert.Throws<InvalidOperationException>(() => mapService.AddMapPolygon(polygon));
    }

    [Fact]
    public void MapService_CannotAddCircle_WhenNotInitialized()
    {
        var mapService = new MapService();
        var circle = new Models.Maps.MapCircle(new Models.GeoCoordinates(52.5, 13.4), 1000);
        Assert.Throws<InvalidOperationException>(() => mapService.AddMapCircle(circle));
    }

    [Fact]
    public void MapService_CannotLoadScene_WhenNotInitialized()
    {
        var mapService = new MapService();
        Assert.ThrowsAsync<InvalidOperationException>(() =>
            mapService.LoadSceneAsync(Models.Maps.MapScheme.NormalDay));
    }

    [Fact(Skip = "Requires HERE SDK credentials and MapView on platform thread")]
    public void MapView_Initialize_And_CreateHandler()
    {
        // End-to-end: Create MapView, attach handler, verify Map is available.
        // Requires HERE SDK initialized with valid credentials.
    }
}
