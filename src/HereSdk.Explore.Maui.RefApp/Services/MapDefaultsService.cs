using Here.Explore.Maui.Models;
using Microsoft.Extensions.Configuration;

namespace Here.Explore.Maui.RefApp.Services;

public interface IMapDefaultsService
{
    GeoCoordinates Center { get; }
    double ZoomLevel { get; }
}

public class MapDefaultsService : IMapDefaultsService
{
    public GeoCoordinates Center { get; }
    public double ZoomLevel { get; }

    public MapDefaultsService(IConfiguration config)
    {
        var section = config.GetSection("DefaultCenter");
        var latStr = section["Latitude"];
        var lngStr = section["Longitude"];
        var zoomStr = section["ZoomLevel"];

        var lat = double.TryParse(latStr, out var l) ? l : 37.7749;
        var lng = double.TryParse(lngStr, out var ln) ? ln : -122.4194;
        Center = new GeoCoordinates(lat, lng);
        ZoomLevel = double.TryParse(zoomStr, out var z) ? z : 14.0;
    }
}
