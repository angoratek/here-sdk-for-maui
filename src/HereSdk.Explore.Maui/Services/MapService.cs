using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;

namespace Here.Explore.Maui.Services;

/// <summary>
/// Shared partial of MapService — platform implementations are in MapService.Android.cs and MapService.iOS.cs.
/// </summary>
public partial class MapService : IMapService
{
    private bool _disposed;

    public event EventHandler<CameraStateChangedEventArgs>? CameraStateChanged;
    public event EventHandler? MapIdle;

    public MapScheme CurrentScheme { get; private set; } = MapScheme.NormalDay;

    public virtual double ZoomLevel => throw new NotImplementedException("Platform-specific implementation required.");
    public virtual double Bearing => throw new NotImplementedException("Platform-specific implementation required.");
    public virtual double Tilt => throw new NotImplementedException("Platform-specific implementation required.");

    // Platform-specific implementations
    public virtual Task<GeoCoordinates> GetCameraTargetAsync() => throw new NotImplementedException("Platform-specific implementation required.");
    public virtual Task SetCameraTargetAsync(GeoCoordinates target, double? zoomLevel = null) => throw new NotImplementedException("Platform-specific implementation required.");
    public virtual Task AnimateCameraAsync(CameraAnimation animation) => throw new NotImplementedException("Platform-specific implementation required.");
    public virtual Task LoadSceneAsync(MapScheme scheme) => throw new NotImplementedException("Platform-specific implementation required.");
    public virtual void AddMapMarker(MapMarker marker) => throw new NotImplementedException("Platform-specific implementation required.");
    public virtual void RemoveMapMarker(MapMarker marker) => throw new NotImplementedException("Platform-specific implementation required.");
    public virtual void AddMapPolyline(MapPolyline polyline) => throw new NotImplementedException("Platform-specific implementation required.");
    public virtual void RemoveMapPolyline(MapPolyline polyline) => throw new NotImplementedException("Platform-specific implementation required.");
    public virtual void AddMapPolygon(MapPolygon polygon) => throw new NotImplementedException("Platform-specific implementation required.");
    public virtual void RemoveMapPolygon(MapPolygon polygon) => throw new NotImplementedException("Platform-specific implementation required.");
    public virtual void AddMapArrow(MapArrow arrow) => throw new NotImplementedException("Platform-specific implementation required.");
    public virtual void RemoveMapArrow(MapArrow arrow) => throw new NotImplementedException("Platform-specific implementation required.");
    public virtual void AddMapMarker3D(MapMarker3D marker) => throw new NotImplementedException("Platform-specific implementation required.");
    public virtual void RemoveMapMarker3D(MapMarker3D marker) => throw new NotImplementedException("Platform-specific implementation required.");
    public virtual Task<MapPickResult?> PickAsync(Point2D screenPoint) => throw new NotImplementedException("Platform-specific implementation required.");

    protected void RaiseCameraStateChanged(CameraStateChangedEventArgs e) => CameraStateChanged?.Invoke(this, e);
    protected void RaiseMapIdle() => MapIdle?.Invoke(this, EventArgs.Empty);

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

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}