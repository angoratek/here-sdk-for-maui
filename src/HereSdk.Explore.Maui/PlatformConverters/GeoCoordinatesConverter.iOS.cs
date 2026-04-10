#if IOS
using Here.Explore.Maui.Models;

namespace Here.Explore.Maui.PlatformConverters;

/// <summary>
/// Converts between shared GeoCoordinates and iOS NativeBridge type.
/// </summary>
internal static class GeoCoordinatesConverter
{
    public static GeoCoordinates ToShared(this HereGeoCoordinates ios)
        => new(ios.Latitude, ios.Longitude);

    public static HereGeoCoordinates ToiOS(this GeoCoordinates shared)
        => new(shared.Latitude, shared.Longitude);
}
#endif