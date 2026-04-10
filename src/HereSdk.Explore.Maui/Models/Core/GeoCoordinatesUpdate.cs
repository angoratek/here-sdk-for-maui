namespace Here.Explore.Maui.Models;

/// <summary>
/// Update for geographic coordinates (partial update).
/// </summary>
public record GeoCoordinatesUpdate(
    double? Latitude = null,
    double? Longitude = null
)
{
    public GeoCoordinates ApplyTo(GeoCoordinates original)
    {
        return new GeoCoordinates(
            Latitude ?? original.Latitude,
            Longitude ?? original.Longitude
        );
    }
}