using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;
using Here.Explore.Maui.Models.Routing;

namespace Here.Explore.Maui.Helpers;

/// <summary>
/// Helper class for extracting route geometry and creating map polylines.
/// </summary>
public static class RouteGeometryHelper
{
    /// <summary>
    /// Extracts the full route geometry as a list of coordinates.
    /// Combines coordinates from all maneuvers in all sections.
    /// </summary>
    /// <param name="route">The route to extract geometry from.</param>
    /// <returns>List of geo coordinates representing the route path.</returns>
    public static IReadOnlyList<GeoCoordinates> ExtractGeometry(Route route)
    {
        var coordinates = new List<GeoCoordinates>();

        foreach (var section in route.Sections)
        {
            foreach (var maneuver in section.Maneuvers)
            {
                coordinates.Add(maneuver.Coordinates);
            }
        }

        return coordinates;
    }

    /// <summary>
    /// Creates a MapPolyline from route geometry.
    /// </summary>
    /// <param name="geometry">The geometry to create a polyline from.</param>
    /// <param name="color">ARGB color (default: red 0xFF0000FF).</param>
    /// <param name="widthInPixels">Line width in pixels (default: 6).</param>
    /// <returns>A new MapPolyline instance.</returns>
    /// <exception cref="ArgumentException">Thrown when geometry is empty.</exception>
    public static MapPolyline CreatePolyline(IReadOnlyList<GeoCoordinates> geometry, uint color = 0xFF0000FF, int widthInPixels = 6)
    {
        if (geometry.Count == 0)
            throw new ArgumentException("Geometry cannot be empty.", nameof(geometry));

        return new MapPolyline(geometry, color, widthInPixels);
    }
}
