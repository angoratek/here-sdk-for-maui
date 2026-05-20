using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;

namespace Here.Explore.Maui.Services;

/// <summary>
/// Shared partial of MapService — platform implementations are in MapService.Android.cs and MapService.iOS.cs.
/// Member definitions are provided by platform-specific partials when building for a platform,
/// and by stubs here when building for non-platform targets (e.g., unit tests).
/// </summary>
public partial class MapService : IMapService
{
    private bool _disposed;

    /// <summary>Raised when map camera state changes (position, zoom, tilt, bearing).</summary>
    public event EventHandler<CameraStateChangedEventArgs>? CameraStateChanged;
    /// <summary>Raised when the map becomes idle after user interaction.</summary>
    public event EventHandler? MapIdle;
    /// <summary>Raised when the user taps on the map.</summary>
    public event EventHandler<MapTappedEventArgs>? MapTapped;
    /// <summary>Raised when the user long-presses on the map.</summary>
    public event EventHandler<MapLongPressedEventArgs>? MapLongPressed;
    /// <summary>Raised when the user double-taps on the map.</summary>
    public event EventHandler<MapTappedEventArgs>? MapDoubleTapped;
    /// <summary>Raised when the user performs a pinch gesture on the map.</summary>
    public event EventHandler<MapPinchedEventArgs>? MapPinched;

    /// <summary>Gets the current map display scheme.</summary>
    public MapScheme CurrentScheme { get; protected set; } = MapScheme.NormalDay;

#if !ANDROID && !IOS
    // Non-platform stub implementations for unit-test context
    public double ZoomLevel => throw new NotImplementedException("Platform-specific implementation required.");
    public double Bearing => throw new NotImplementedException("Platform-specific implementation required.");
    public double Tilt => throw new NotImplementedException("Platform-specific implementation required.");

    public Task<GeoCoordinates> GetCameraTargetAsync() => throw new NotImplementedException("Platform-specific implementation required.");
    public Task SetCameraTargetAsync(GeoCoordinates target, double? zoomLevel = null) => throw new NotImplementedException("Platform-specific implementation required.");
    public Task AnimateCameraAsync(CameraAnimation animation) => throw new NotImplementedException("Platform-specific implementation required.");
    public Task LoadSceneAsync(MapScheme scheme) => throw new NotImplementedException("Platform-specific implementation required.");
    public void AddMapMarker(MapMarker marker) => throw new NotImplementedException("Platform-specific implementation required.");
    public void RemoveMapMarker(MapMarker marker) => throw new NotImplementedException("Platform-specific implementation required.");
    public void AddMapPolyline(MapPolyline polyline) => throw new NotImplementedException("Platform-specific implementation required.");
    public void RemoveMapPolyline(MapPolyline polyline) => throw new NotImplementedException("Platform-specific implementation required.");
    public void AddMapPolygon(MapPolygon polygon) => throw new NotImplementedException("Platform-specific implementation required.");
    public void RemoveMapPolygon(MapPolygon polygon) => throw new NotImplementedException("Platform-specific implementation required.");
    public void AddMapArrow(MapArrow arrow) => throw new NotImplementedException("Platform-specific implementation required.");
    public void RemoveMapArrow(MapArrow arrow) => throw new NotImplementedException("Platform-specific implementation required.");
    public void AddMapMarker3D(MapMarker3D marker) => throw new NotImplementedException("Platform-specific implementation required.");
    public void RemoveMapMarker3D(MapMarker3D marker) => throw new NotImplementedException("Platform-specific implementation required.");
    public void AddMapCircle(MapCircle circle) => throw new NotImplementedException("Platform-specific implementation required.");
    public void RemoveMapCircle(MapCircle circle) => throw new NotImplementedException("Platform-specific implementation required.");
    public Task<MapPickResult?> PickAsync(Point2D screenPoint) => throw new NotImplementedException("Platform-specific implementation required.");
    public void ClearAllMapItems() => throw new NotImplementedException("Platform-specific implementation required.");
    public void AddMapMarkerCluster(MapMarkerCluster cluster, IEnumerable<MapMarker> markers) => throw new NotImplementedException("Platform-specific implementation required.");
    public void RemoveMapMarkerCluster(MapMarkerCluster cluster) => throw new NotImplementedException("Platform-specific implementation required.");
    public void AddLocationIndicator(LocationIndicator indicator) => throw new NotImplementedException("Platform-specific implementation required.");
    public void UpdateLocationIndicator(GeoCoordinates location, double? bearing = null) => throw new NotImplementedException("Platform-specific implementation required.");
    public void RemoveLocationIndicator() => throw new NotImplementedException("Platform-specific implementation required.");
#endif

    internal void RaiseCameraStateChanged(CameraStateChangedEventArgs e) => CameraStateChanged?.Invoke(this, e);
    internal void RaiseMapIdle() => MapIdle?.Invoke(this, EventArgs.Empty);
    internal void RaiseMapTapped(MapTappedEventArgs e) => MapTapped?.Invoke(this, e);
    internal void RaiseMapLongPressed(MapLongPressedEventArgs e) => MapLongPressed?.Invoke(this, e);
    internal void RaiseMapDoubleTapped(MapTappedEventArgs e) => MapDoubleTapped?.Invoke(this, e);
    internal void RaiseMapPinched(MapPinchedEventArgs e) => MapPinched?.Invoke(this, e);

    /// <inheritdoc />
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Platform implementations will clear native resources
            }
            _disposed = true;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}