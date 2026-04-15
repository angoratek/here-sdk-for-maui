namespace Here.Explore.Maui.Models;

/// <summary>
/// Update for geographic coordinates (partial update).
/// </summary>
public record GeoCoordinatesUpdate(
    double? Latitude = null,
    double? Longitude = null
)
{
    /// <summary>Applies this partial update to an existing GeoCoordinates, replacing only non-null values.</summary>
    /// <param name="original">The original coordinates to update.</param>
    /// <returns>A new GeoCoordinates with the specified fields replaced.</returns>
    public GeoCoordinates ApplyTo(GeoCoordinates original)
    {
        return new GeoCoordinates(
            Latitude ?? original.Latitude,
            Longitude ?? original.Longitude
        );
    }
}