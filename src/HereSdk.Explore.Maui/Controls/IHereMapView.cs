using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;
using Microsoft.Maui;

namespace Here.Explore.Maui.Controls;

/// <summary>
/// Interface for the cross-platform map view.
/// </summary>
public interface IHereMapView : IView
{
    /// <summary>Gets or sets the geographic coordinates the camera is centered on.</summary>
    GeoCoordinates CameraTarget { get; set; }

    /// <summary>Gets or sets the map visual scheme (day, night, satellite, etc.).</summary>
    MapScheme MapScheme { get; set; }

    /// <summary>Gets the map service for camera control, markers, and scene management.</summary>
    Services.IMapService Map { get; }

    /// <summary>Raised when the camera position changes.</summary>
    event EventHandler<CameraStateChangedEventArgs>? CameraStateChanged;

    /// <summary>Raised when the map is idle (rendering and loading complete).</summary>
    event EventHandler? MapIdle;

    /// <summary>Raised when the user taps on the map.</summary>
    event EventHandler<MapTappedEventArgs>? MapTapped;

    /// <summary>Raised when the user double-taps on the map.</summary>
    event EventHandler<MapDoubleTappedEventArgs>? MapDoubleTapped;

    /// <summary>Raised when the user long-presses on the map.</summary>
    event EventHandler<MapLongPressedEventArgs>? MapLongPressed;

    /// <summary>Raised when the user pans (drags) the map.</summary>
    event EventHandler<MapPannedEventArgs>? MapPanned;

    /// <summary>Raised when the user pinch-zooms or rotates the map.</summary>
    event EventHandler<MapPinchRotatedEventArgs>? MapPinchRotated;
}