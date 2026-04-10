using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;

namespace Here.Explore.Maui.Controls;

/// <summary>
/// Interface for the cross-platform map view.
/// </summary>
public interface IHereMapView
{
    GeoCoordinates CameraTarget { get; set; }
    MapScheme MapScheme { get; set; }
    Services.IMapService Map { get; }
}