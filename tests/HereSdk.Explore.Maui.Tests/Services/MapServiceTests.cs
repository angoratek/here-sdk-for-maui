using Xunit;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;
using Here.Explore.Maui.Services;
using NSubstitute;

namespace Here.Explore.Maui.Tests.Services;

public class MapServiceTests
{
    [Fact]
    public async Task GetCameraTargetAsync_ReturnsCoordinates()
    {
        var expected = new GeoCoordinates(52.531268, 13.387659);
        var mapService = Substitute.For<IMapService>();
        mapService.GetCameraTargetAsync().Returns(Task.FromResult(expected));

        var result = await mapService.GetCameraTargetAsync();

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task SetCameraTargetAsync_CallsService()
    {
        var mapService = Substitute.For<IMapService>();
        var target = new GeoCoordinates(48.8566, 2.3522);

        await mapService.SetCameraTargetAsync(target, zoomLevel: 10.0);

        await mapService.Received(1).SetCameraTargetAsync(target, 10.0);
    }

    [Fact]
    public async Task LoadSceneAsync_CallsService()
    {
        var mapService = Substitute.For<IMapService>();
        await mapService.LoadSceneAsync(MapScheme.NormalDay);
        await mapService.Received(1).LoadSceneAsync(MapScheme.NormalDay);
    }

    [Fact]
    public void AddMapMarker_CallsService()
    {
        var mapService = Substitute.For<IMapService>();
        var marker = new MapMarker(new GeoCoordinates(52.5, 13.4));
        mapService.AddMapMarker(marker);
        mapService.Received(1).AddMapMarker(marker);
    }

    [Fact]
    public void AddMapPolyline_CallsService()
    {
        var mapService = Substitute.For<IMapService>();
        var polyline = new MapPolyline(
            new List<GeoCoordinates> { new(52.5, 13.4), new(52.6, 13.5) },
            Color: 0xFF0000FF,
            WidthInPixels: 5
        );
        mapService.AddMapPolyline(polyline);
        mapService.Received(1).AddMapPolyline(polyline);
    }

    [Fact]
    public void AddMapPolygon_CallsService()
    {
        var mapService = Substitute.For<IMapService>();
        var polygon = new MapPolygon(
            new List<GeoCoordinates> { new(52.5, 13.4), new(52.6, 13.5), new(52.55, 13.3) },
            FillColor: 0x44FF0000
        );
        mapService.AddMapPolygon(polygon);
        mapService.Received(1).AddMapPolygon(polygon);
    }

    [Fact]
    public void AddMapArrow_CallsService()
    {
        var mapService = Substitute.For<IMapService>();
        var arrow = new MapArrow(
            new List<GeoCoordinates> { new(52.5, 13.4), new(52.6, 13.5) },
            Color: 0xFF00FF00,
            WidthInPixels: 8
        );
        mapService.AddMapArrow(arrow);
        mapService.Received(1).AddMapArrow(arrow);
    }

    [Fact]
    public void AddMapMarker3D_CallsService()
    {
        var mapService = Substitute.For<IMapService>();
        var marker3D = new MapMarker3D(new GeoCoordinates(52.5, 13.4), Scale: 2.0);
        mapService.AddMapMarker3D(marker3D);
        mapService.Received(1).AddMapMarker3D(marker3D);
    }

    [Fact]
    public void RemoveMapMarker_CallsService()
    {
        var mapService = Substitute.For<IMapService>();
        var marker = new MapMarker(new GeoCoordinates(52.5, 13.4));
        mapService.RemoveMapMarker(marker);
        mapService.Received(1).RemoveMapMarker(marker);
    }

    [Fact]
    public async Task AnimateCameraAsync_CallsService()
    {
        var mapService = Substitute.For<IMapService>();
        var animation = new CameraAnimation(
            new GeoCoordinates(48.8566, 2.3522),
            ZoomLevel: 12.0,
            DurationInSeconds: 2.0
        );

        await mapService.AnimateCameraAsync(animation);
        await mapService.Received(1).AnimateCameraAsync(animation);
    }
}