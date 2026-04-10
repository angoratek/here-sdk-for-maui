#if IOS
using Here.Explore.Maui.Models;

namespace Here.Explore.Maui.PlatformConverters;

/// <summary>
/// Converts between shared GeoBox and iOS NativeBridge type.
/// </summary>
internal static class GeoBoxConverter
{
    public static GeoBox ToShared(this HereGeoBox ios)
        => new(
            ios.SouthWest.ToShared(),
            ios.NorthEast.ToShared()
        );

    public static HereGeoBox ToiOS(this GeoBox shared)
        => new(
            shared.SouthWest.ToiOS(),
            shared.NorthEast.ToiOS()
        );
}
#endif