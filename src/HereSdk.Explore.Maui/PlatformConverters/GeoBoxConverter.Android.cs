#if ANDROID
using Here.Explore.Maui.Models;

namespace Here.Explore.Maui.PlatformConverters;

/// <summary>
/// Converts between shared GeoBox and Android binding type.
/// </summary>
internal static class GeoBoxConverter
{
    public static GeoBox ToShared(this Com.Here.Sdk.Core.GeoBox android)
        => new(
            android.SouthWestCorner.ToShared(),
            android.NorthEastCorner.ToShared()
        );

    public static Com.Here.Sdk.Core.GeoBox ToAndroid(this GeoBox shared)
        => new(
            shared.SouthWest.ToAndroid(),
            shared.NorthEast.ToAndroid()
        );
}
#endif