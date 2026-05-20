#pragma warning disable CS1591
#if ANDROID
namespace Here.Explore.Maui.PlatformConverters;

/// <summary>
/// Converts between shared GeoCoordinates and Android binding type.
/// Shared type: Here.Explore.Maui.Models.GeoCoordinates
/// Binding type: Here.Explore.Core.GeoCoordinates
/// </summary>
internal static class GeoCoordinatesConverter
{
    public static Here.Explore.Maui.Models.GeoCoordinates ToShared(this Here.Explore.Core.GeoCoordinates android)
        => new(android.Latitude, android.Longitude);

    public static Here.Explore.Core.GeoCoordinates ToAndroid(this Here.Explore.Maui.Models.GeoCoordinates shared)
        => new(shared.Latitude, shared.Longitude);
}
#endif