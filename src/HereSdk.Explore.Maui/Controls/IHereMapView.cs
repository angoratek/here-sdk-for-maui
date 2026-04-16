using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;
using Here.Explore.Maui.Services;
using Microsoft.Maui;

namespace Here.Explore.Maui.Controls;

/// <summary>
/// Interface for the cross-platform map view.
/// Map events are exposed via <see cref="IMapService"/> (accessed through <see cref="Map"/>).
/// </summary>
public interface IHereMapView : IView
{
    /// <summary>Gets or sets the geographic coordinates the camera is centered on.</summary>
    GeoCoordinates CameraTarget { get; set; }

    /// <summary>Gets or sets the map visual scheme (day, night, satellite, etc.).</summary>
    MapScheme MapScheme { get; set; }

    /// <summary>Gets the map service for camera control, markers, scene management, and events.</summary>
    IMapService Map { get; }
}