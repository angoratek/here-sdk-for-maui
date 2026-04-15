namespace Here.Explore.Maui.Models;

/// <summary>
/// Geographic orientation update (partial update).
/// </summary>
public record GeoOrientationUpdate(
    double? Bearing = null,
    double? Tilt = null
)
{
    /// <summary>Applies this partial update to an existing GeoOrientation, replacing only non-null values.</summary>
    /// <param name="original">The original orientation to update.</param>
    /// <returns>A new GeoOrientation with the specified fields replaced.</returns>
    public GeoOrientation ApplyTo(GeoOrientation original)
    {
        return new GeoOrientation(
            Bearing ?? original.Bearing,
            Tilt ?? original.Tilt
        );
    }
}