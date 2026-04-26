using Here.Explore.Maui.Models;

namespace Here.Explore.Maui.Helpers;

/// <summary>
/// Helper for generating circle geometry as a polygon approximation.
/// </summary>
public static class CircleGeometryHelper
{
    private const int DefaultSegments = 64;
    private const double EarthRadiusMeters = 6371000.0;

    /// <summary>
    /// Generates polygon vertices approximating a circle on the Earth's surface.
    /// Uses a simple equirectangular approximation suitable for small-to-medium radii.
    /// </summary>
    public static IReadOnlyList<GeoCoordinates> GenerateCircleVertices(GeoCoordinates center, double radiusInMeters, int segments = DefaultSegments)
    {
        var vertices = new List<GeoCoordinates>();
        if (radiusInMeters <= 0) return vertices;

        var latRad = center.Latitude * Math.PI / 180.0;
        var cosLat = Math.Cos(latRad);
        var metersPerDegreeLat = 111320.0;
        var metersPerDegreeLon = 111320.0 * cosLat;

        for (int i = 0; i <= segments; i++)
        {
            var angle = 2.0 * Math.PI * i / segments;
            var deltaLat = radiusInMeters * Math.Cos(angle) / metersPerDegreeLat;
            var deltaLon = radiusInMeters * Math.Sin(angle) / metersPerDegreeLon;
            vertices.Add(new GeoCoordinates(center.Latitude + deltaLat, center.Longitude + deltaLon));
        }

        return vertices;
    }
}
