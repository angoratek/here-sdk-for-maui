#if ANDROID
namespace Here.Explore.Maui.PlatformConverters;

/// <summary>
/// Converts between shared GeoBox and Android binding type.
/// Shared type: Here.Explore.Maui.Models.GeoBox
/// Binding type: Here.Explore.Core.GeoBox
/// </summary>
internal static class GeoBoxConverter
{
    public static Here.Explore.Maui.Models.GeoBox ToShared(this Here.Explore.Core.GeoBox android)
        => new(
            android.SouthWestCorner.ToShared(),
            android.NorthEastCorner.ToShared()
        );

    public static Here.Explore.Core.GeoBox ToAndroid(this Here.Explore.Maui.Models.GeoBox shared)
        => new(
            shared.SouthWest.ToAndroid(),
            shared.NorthEast.ToAndroid()
        );
}
#endif