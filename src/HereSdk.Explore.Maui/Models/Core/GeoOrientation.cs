namespace Here.Explore.Maui.Models;

/// <summary>
/// Geographic orientation update (partial update).
/// </summary>
public record GeoOrientationUpdate(
    double? Bearing = null,
    double? Tilt = null
)
{
    public GeoOrientation ApplyTo(GeoOrientation original)
    {
        return new GeoOrientation(
            Bearing ?? original.Bearing,
            Tilt ?? original.Tilt
        );
    }
}