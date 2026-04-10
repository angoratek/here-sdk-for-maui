#if ANDROID
using Here.Explore.Maui.Models;

namespace Here.Explore.Maui.PlatformConverters;

/// <summary>
/// Converts between shared GeoCoordinates and Android binding type.
/// </summary>
internal static class GeoCoordinatesConverter
{
    public static GeoCoordinates ToShared(this Com.Here.Sdk.Core.GeoCoordinates android)
        => new(android.Latitude, android.Longitude);

    public static Com.Here.Sdk.Core.GeoCoordinates ToAndroid(this GeoCoordinates shared)
        => new(shared.Latitude, shared.Longitude);
}
#endif