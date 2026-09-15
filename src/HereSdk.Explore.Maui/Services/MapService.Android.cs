#pragma warning disable CS1591
#if ANDROID
using Android.Runtime;
using ABitmap = Android.Graphics.Bitmap;
using ACanvas = Android.Graphics.Canvas;
using AColor = Android.Graphics.Color;
using APath = Android.Graphics.Path;
using APaint = Android.Graphics.Paint;
using ATextPaint = Android.Text.TextPaint;
using ATypeface = Android.Graphics.Typeface;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;
using Here.Explore.Maui.Helpers;

namespace Here.Explore.Maui.Services;

/// <summary>
/// Android-specific MapService implementation using HERE SDK Android bindings.
/// Key API differences from assumed signatures:
/// - Map item add/remove methods are on MapScene (not MapView)
/// - Method names use lowercase 'd': AddMapMarker3d, RemoveMapMarker3d
/// - Camera: LookAt() directly (not Update()), GetState() for state
/// - GeoCoordinatesUpdate takes Java.Lang.Double (not double)
/// - Core.Color uses (float green, float alpha, float red, float blue)
/// - MapPolyline.SolidRepresentation is MapPolylineSolidRepresentation
/// - MapScheme is a Java enum with static properties
/// - SDKNativeEngine: MakeSharedInstance(context, options), SharedInstance property
/// - SDKOptions takes AuthenticationMode in constructor
/// </summary>
public partial class MapService
{
    private Here.Explore.Maps.MapView? _mapView;
    private Here.Explore.Maps.MapCamera? _camera;
    private Here.Explore.Maps.MapScene? _mapScene;

    // Track platform map items for removal
    private readonly Dictionary<Here.Explore.Maui.Models.Maps.MapMarker, Here.Explore.Maps.MapMarker> _markers = new();
    private readonly Dictionary<Here.Explore.Maui.Models.Maps.MapPolyline, Here.Explore.Maps.MapPolyline> _polylines = new();
    private readonly Dictionary<Here.Explore.Maui.Models.Maps.MapPolygon, Here.Explore.Maps.MapPolygon> _polygons = new();
    private readonly Dictionary<Here.Explore.Maui.Models.Maps.MapArrow, Here.Explore.Maps.MapArrow> _arrows = new();
    private readonly Dictionary<Here.Explore.Maui.Models.Maps.MapMarker3D, Here.Explore.Maps.MapMarker3D> _markers3D = new();
    private readonly Dictionary<Here.Explore.Maui.Models.Maps.MapCircle, Here.Explore.Maps.MapPolygon> _circles = new();
    // Reference equality: MapMarkerCluster is a record, so value-equal clusters
    // (e.g. two created with defaults) must not collide as dictionary keys.
    private readonly Dictionary<Here.Explore.Maui.Models.Maps.MapMarkerCluster, Here.Explore.Maps.MapMarkerCluster> _markerClusters =
        new(ReferenceEqualityComparer.Instance);

    internal void Initialize(Here.Explore.Maps.MapView mapView)
    {
        _mapView = mapView;
        _camera = mapView.Camera;
        _mapScene = mapView.MapScene;
    }

    public double ZoomLevel => _camera?.GetState().ZoomLevel ?? 0;
    public double Bearing => _camera?.GetState().OrientationAtTarget.Bearing ?? 0;
    public double Tilt => _camera?.GetState().OrientationAtTarget.Tilt ?? 0;

    public Task<GeoCoordinates> GetCameraTargetAsync()
    {
        if (_camera is null) throw new InvalidOperationException("MapService not initialized.");
        var state = _camera.GetState();
        return Task.FromResult(new GeoCoordinates(state.TargetCoordinates.Latitude, state.TargetCoordinates.Longitude));
    }

    public Task SetCameraTargetAsync(GeoCoordinates target, double? zoomLevel = null)
    {
        if (_camera is null) throw new InvalidOperationException("MapService not initialized.");
        var geoUpdate = new Here.Explore.Core.GeoCoordinatesUpdate(
            (Java.Lang.Double?)target.Longitude, (Java.Lang.Double?)target.Latitude);
        if (zoomLevel.HasValue)
        {
            _camera.LookAt(new Here.Explore.Core.GeoCoordinates(target.Latitude, target.Longitude),
                new Here.Explore.Core.GeoOrientationUpdate(new Here.Explore.Core.GeoOrientation(0, 0)),
                new Here.Explore.Maps.MapMeasure(Here.Explore.Maps.MapMeasure.Kind.ZoomLevel!, zoomLevel.Value));
        }
        else
        {
            _camera.LookAt(new Here.Explore.Core.GeoCoordinates(target.Latitude, target.Longitude));
        }
        return Task.CompletedTask;
    }

    public Task AnimateCameraAsync(Here.Explore.Maui.Models.Maps.CameraAnimation animation)
    {
        if (_camera is null) throw new InvalidOperationException("MapService not initialized.");
        var geoUpdate = new Here.Explore.Core.GeoCoordinatesUpdate(
            (Java.Lang.Double?)animation.Target.Longitude, (Java.Lang.Double?)animation.Target.Latitude);
        var orientation = new Here.Explore.Core.GeoOrientationUpdate(
            new Here.Explore.Core.GeoOrientation(animation.Bearing ?? 0, animation.Tilt ?? 0));
        var mapMeasure = new Here.Explore.Maps.MapMeasure(
            Here.Explore.Maps.MapMeasure.Kind.ZoomLevel!, animation.ZoomLevel ?? _camera.GetState().ZoomLevel);
        var cameraUpdate = Here.Explore.Maps.MapCameraUpdateFactory.LookAt(geoUpdate, orientation, mapMeasure);
        var duration = Com.Here.Time.HereDuration.OfSeconds((long)animation.DurationInSeconds);
        var mapAnimation = Here.Explore.Maps.MapCameraAnimationFactory.CreateAnimation(
            cameraUpdate, duration!, new Here.Explore.Animation.Easing(Here.Explore.Animation.EasingFunction.Linear!));

        // The task completes when the animation ends (or is cancelled).
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        _camera.StartAnimation(mapAnimation, new CameraAnimationListener(tcs));
        return tcs.Task;
    }

    public async Task LoadSceneAsync(Here.Explore.Maui.Models.Maps.MapScheme scheme)
    {
        if (_mapScene is null) throw new InvalidOperationException("MapService not initialized.");
        var androidScheme = ToAndroidMapScheme(scheme);
        var tcs = new TaskCompletionSource<bool>();
        _mapScene.LoadScene(androidScheme, new SceneLoadCallback(tcs));
        await tcs.Task;
        CurrentScheme = scheme;
    }

    public void AddMapMarker(Here.Explore.Maui.Models.Maps.MapMarker marker)
    {
        Android.Util.Log.Debug("REFAPP_DIAG", $"AddMapMarker called: {marker.Coordinates.Latitude},{marker.Coordinates.Longitude}");
        if (_mapScene is null)
        {
            Android.Util.Log.Error("REFAPP_DIAG", "AddMapMarker: _mapScene is null");
            throw new InvalidOperationException("MapService not initialized.");
        }
        var androidCoords = new Here.Explore.Core.GeoCoordinates(marker.Coordinates.Latitude, marker.Coordinates.Longitude);
        // Glyph/Color pins are rendered programmatically (tinted teardrop +
        // icon glyph) so callers can make markers distinct per item type.
        // Otherwise MapImage requires an Android drawable resource — honor the
        // app-provided ImagePath name first, then the branded "marker_pin"
        // drawable, then the system compass icon as a last resort.
        var useGlyphPin = marker.Glyph is not null || marker.Color is not null;
        Here.Explore.Maps.MapImage? mapImage = null;
        if (useGlyphPin)
            mapImage = TryRenderGlyphPin(marker);
        else if (marker.ImagePath is not null)
            mapImage = TryLoadNamedDrawable(marker.ImagePath);

        try
        {
            mapImage ??= TryLoadNamedDrawable("marker_pin");
        }
        catch (Exception ex) { Android.Util.Log.Warn("REFAPP_DIAG", $"AddMapMarker: marker_pin load failed: {ex.Message}"); }

        try
        {
            mapImage ??= Here.Explore.Maps.MapImageFactory.FromResource(Platform.AppContext.Resources, global::Android.Resource.Drawable.IcMenuCompass);
        }
        catch (Exception ex)
        {
            Android.Util.Log.Error("REFAPP_DIAG", $"AddMapMarker: fallback marker load failed: {ex.Message}");
            throw;
        }

        // AnchorX/AnchorY are percentages (0–100) of the image size; Android's
        // Anchor2D is 0–1 normalized. Glyph pins are bottom-anchored by default
        // so the pin tip points at the coordinate.
        var anchorX = marker.AnchorX ?? (useGlyphPin ? 50 : null);
        var anchorY = marker.AnchorY ?? (useGlyphPin ? 100 : null);
        var androidMarker = anchorX is not null && anchorY is not null
            ? new Here.Explore.Maps.MapMarker(androidCoords, mapImage!,
                new Here.Explore.Core.Anchor2D(anchorX.Value / 100.0, anchorY.Value / 100.0))
            : new Here.Explore.Maps.MapMarker(androidCoords, mapImage!);
        _mapScene.AddMapMarker(androidMarker);
        _markers[marker] = androidMarker;
        Android.Util.Log.Debug("REFAPP_DIAG", "AddMapMarker: success");
    }

    public void RemoveMapMarker(Here.Explore.Maui.Models.Maps.MapMarker marker)
    {
        Android.Util.Log.Debug("REFAPP_DIAG", "RemoveMapMarker called");
        if (_mapScene is null) throw new InvalidOperationException("MapService not initialized.");
        if (_markers.TryGetValue(marker, out var androidMarker))
        {
            _mapScene.RemoveMapMarker(androidMarker);
            _markers.Remove(marker);
            Android.Util.Log.Debug("REFAPP_DIAG", "RemoveMapMarker: success");
        }
        else
        {
            Android.Util.Log.Warn("REFAPP_DIAG", "RemoveMapMarker: marker not found");
        }
    }

    public void AddMapPolyline(Here.Explore.Maui.Models.Maps.MapPolyline polyline)
    {
        Android.Util.Log.Debug("REFAPP_DIAG", $"AddMapPolyline called: {polyline.Vertices.Count} vertices");
        if (_mapScene is null)
        {
            Android.Util.Log.Error("REFAPP_DIAG", "AddMapPolyline: _mapScene is null");
            throw new InvalidOperationException("MapService not initialized.");
        }
        try
        {
            var vertices = polyline.Vertices.Select(v => new Here.Explore.Core.GeoCoordinates(v.Latitude, v.Longitude)).ToList();
            var geoPolyline = new Here.Explore.Core.GeoPolyline(vertices);
            var lineWidth = new Here.Explore.Maps.MapMeasureDependentRenderSize(
                Here.Explore.Maps.RenderSize.Unit.Pixels!,
                polyline.WidthInPixels);
            var color = ToCoreColor(polyline.Color);
            var cap = polyline.Cap switch
            {
                Models.Maps.LineCap.Square => Here.Explore.Maps.LineCap.Square,
                Models.Maps.LineCap.Butt => Here.Explore.Maps.LineCap.Butt,
                _ => Here.Explore.Maps.LineCap.Round,
            };
            Android.Util.Log.Debug("REFAPP_DIAG", $"lineWidth: sizeUnit={(lineWidth.SizeUnit?.ToString() ?? "null")}, measureKind={(lineWidth.MeasureKind?.ToString() ?? "null")}, sizes count={(lineWidth.Sizes is null ? -1 : lineWidth.Sizes.Count)}, width={polyline.WidthInPixels}");
            Android.Util.Log.Debug("REFAPP_DIAG", $"polyline color argb={polyline.Color:X}, cap={(cap?.ToString() ?? "null")}");
            if (cap is null) throw new InvalidOperationException("LineCap is null");
            var representation = new Here.Explore.Maps.MapPolyline.MapPolylineSolidRepresentation(lineWidth, color, cap);
            var androidPolyline = new Here.Explore.Maps.MapPolyline(geoPolyline, representation);
            _mapScene.AddMapPolyline(androidPolyline);
            _polylines[polyline] = androidPolyline;
            Android.Util.Log.Debug("REFAPP_DIAG", "AddMapPolyline: success");
        }
        catch (Exception ex)
        {
            Android.Util.Log.Error("REFAPP_DIAG", $"AddMapPolyline: exception: {ex}");
            throw;
        }
    }

    public void RemoveMapPolyline(Here.Explore.Maui.Models.Maps.MapPolyline polyline)
    {
        Android.Util.Log.Debug("REFAPP_DIAG", "RemoveMapPolyline called");
        if (_mapScene is null) throw new InvalidOperationException("MapService not initialized.");
        if (_polylines.TryGetValue(polyline, out var androidPolyline))
        {
            _mapScene.RemoveMapPolyline(androidPolyline);
            _polylines.Remove(polyline);
            Android.Util.Log.Debug("REFAPP_DIAG", "RemoveMapPolyline: success");
        }
        else
        {
            Android.Util.Log.Warn("REFAPP_DIAG", "RemoveMapPolyline: polyline not found");
        }
    }

    public void AddMapPolygon(Here.Explore.Maui.Models.Maps.MapPolygon polygon)
    {
        Android.Util.Log.Debug("REFAPP_DIAG", $"AddMapPolygon called: {polygon.Vertices.Count} vertices");
        if (_mapScene is null)
        {
            Android.Util.Log.Error("REFAPP_DIAG", "AddMapPolygon: _mapScene is null");
            throw new InvalidOperationException("MapService not initialized.");
        }
        try
        {
            var vertices = polygon.Vertices.Select(v => new Here.Explore.Core.GeoCoordinates(v.Latitude, v.Longitude)).ToList();
            var geoPolygon = new Here.Explore.Core.GeoPolygon(vertices);
            var fillColor = ToCoreColor(polygon.FillColor);
            // Outline is disabled by default (zero width) — same ctor shape
            // as the SDK's "outline visualization disabled" variant.
            var androidPolygon = polygon.StrokeWidthInPixels > 0
                ? new Here.Explore.Maps.MapPolygon(geoPolygon, fillColor,
                    ToCoreColor(polygon.StrokeColor), polygon.StrokeWidthInPixels)
                : new Here.Explore.Maps.MapPolygon(geoPolygon, fillColor);
            _mapScene.AddMapPolygon(androidPolygon);
            _polygons[polygon] = androidPolygon;
            Android.Util.Log.Debug("REFAPP_DIAG", "AddMapPolygon: success");
        }
        catch (Exception ex)
        {
            Android.Util.Log.Error("REFAPP_DIAG", $"AddMapPolygon: exception: {ex}");
            throw;
        }
    }

    public void RemoveMapPolygon(Here.Explore.Maui.Models.Maps.MapPolygon polygon)
    {
        Android.Util.Log.Debug("REFAPP_DIAG", "RemoveMapPolygon called");
        if (_mapScene is null) throw new InvalidOperationException("MapService not initialized.");
        if (_polygons.TryGetValue(polygon, out var androidPolygon))
        {
            _mapScene.RemoveMapPolygon(androidPolygon);
            _polygons.Remove(polygon);
            Android.Util.Log.Debug("REFAPP_DIAG", "RemoveMapPolygon: success");
        }
        else
        {
            Android.Util.Log.Warn("REFAPP_DIAG", "RemoveMapPolygon: polygon not found");
        }
    }

    public void AddMapArrow(Here.Explore.Maui.Models.Maps.MapArrow arrow)
    {
        if (_mapScene is null) throw new InvalidOperationException("MapService not initialized.");
        var vertices = arrow.Vertices.Select(v => new Here.Explore.Core.GeoCoordinates(v.Latitude, v.Longitude)).ToList();
        var geoPolyline = new Here.Explore.Core.GeoPolyline(vertices);
        var color = ToCoreColor(arrow.Color);
        var androidArrow = new Here.Explore.Maps.MapArrow(geoPolyline, arrow.WidthInPixels, color);
        _mapScene.AddMapArrow(androidArrow);
        _arrows[arrow] = androidArrow;
    }

    public void RemoveMapArrow(Here.Explore.Maui.Models.Maps.MapArrow arrow)
    {
        if (_mapScene is null) throw new InvalidOperationException("MapService not initialized.");
        if (_arrows.TryGetValue(arrow, out var androidArrow))
        {
            _mapScene.RemoveMapArrow(androidArrow);
            _arrows.Remove(arrow);
        }
    }

    public void AddMapMarker3D(Here.Explore.Maui.Models.Maps.MapMarker3D marker)
    {
        if (_mapScene is null) throw new InvalidOperationException("MapService not initialized.");

        // Flat 3D marker textured with the same drawable-based MapImage used
        // for 2D markers (ImagePath on the shared model is iOS-only — Android
        // markers load from app drawable resources).
        var androidCoords = new Here.Explore.Core.GeoCoordinates(marker.Coordinates.Latitude, marker.Coordinates.Longitude);
        var mapImage = TryLoadMarkerImage()
            ?? throw new InvalidOperationException("Failed to load a marker image for MapMarker3D.");

        var androidMarker3D = new Here.Explore.Maps.MapMarker3D(
            androidCoords, mapImage, marker.Scale, Here.Explore.Maps.RenderSize.Unit.Pixels!);
        _mapScene.AddMapMarker3d(androidMarker3D);
        _markers3D[marker] = androidMarker3D;
    }

    private Here.Explore.Maps.MapImage? TryLoadMarkerImage()
    {
        Here.Explore.Maps.MapImage? mapImage = TryLoadNamedDrawable("marker_pin");

        try
        {
            mapImage ??= Here.Explore.Maps.MapImageFactory.FromResource(Platform.AppContext.Resources, global::Android.Resource.Drawable.IcMenuCompass);
        }
        catch (Exception ex)
        {
            Android.Util.Log.Error("REFAPP_DIAG", $"TryLoadMarkerImage: fallback marker load failed: {ex.Message}");
        }
        return mapImage;
    }

    /// <summary>
    /// Loads a drawable by resource name (the shared model's ImagePath is a
    /// drawable name, not a file path, on Android). Returns null when no
    /// drawable with that name exists in the app package.
    /// </summary>
    private static Here.Explore.Maps.MapImage? TryLoadNamedDrawable(string name)
    {
        var resId = Platform.AppContext.Resources?.GetIdentifier(name, "drawable", Platform.AppContext.PackageName) ?? 0;
        return resId != 0
            ? Here.Explore.Maps.MapImageFactory.FromResource(Platform.AppContext.Resources, resId)
            : null;
    }

    // MaterialIcons typeface is expensive to load — cache it per process.
    private static ATypeface? _materialTypeface;
    private static bool _materialTypefaceResolved;

    /// <summary>
    /// Renders the shared model's Glyph/Color as a teardrop pin bitmap
    /// (white silhouette, tinted fill, white Material Icons glyph) so markers
    /// can be visually distinct per item type without shipping PNG assets.
    /// Returns null when rendering fails (no glyph font) — the caller then
    /// falls back to the plain drawable chain.
    /// </summary>
    private static Here.Explore.Maps.MapImage? TryRenderGlyphPin(Here.Explore.Maui.Models.Maps.MapMarker marker)
    {
        try
        {
            var density = Platform.AppContext.Resources?.DisplayMetrics?.Density ?? 1f;
            var width = (int)(44 * density);
            var height = (int)(56 * density);
            var bitmap = ABitmap.CreateBitmap(width, height, ABitmap.Config.Argb8888!)!;
            using var canvas = new ACanvas(bitmap);
            float headCx = width / 2f, headCy = height * 0.40f;
            float headRadius = width / 2f - 3 * density;
            float border = 2.5f * density;
            var paintColor = new AColor(unchecked((int)(marker.Color ?? 0xFFFF385C)));

            // White silhouette (border): circle + tapered tail.
            using var white = new APaint { AntiAlias = true };
            white.Color = AColor.White;
            canvas.DrawCircle(headCx, headCy, headRadius + border, white);
            using var tailWhite = new APath();
            tailWhite.MoveTo(headCx, height - 2 * density);
            tailWhite.LineTo(headCx - (headRadius + border) * 0.62f, headCy + headRadius * 0.70f);
            tailWhite.LineTo(headCx + (headRadius + border) * 0.62f, headCy + headRadius * 0.70f);
            tailWhite.Close();
            canvas.DrawPath(tailWhite, white);

            // Tinted fill: same shapes, slightly smaller.
            using var fill = new APaint { AntiAlias = true };
            fill.Color = paintColor;
            canvas.DrawCircle(headCx, headCy, headRadius, fill);
            using var tail = new APath();
            tail.MoveTo(headCx, height - 2 * density);
            tail.LineTo(headCx - headRadius * 0.55f, headCy + headRadius * 0.70f);
            tail.LineTo(headCx + headRadius * 0.55f, headCy + headRadius * 0.70f);
            tail.Close();
            canvas.DrawPath(tail, fill);

            // Glyph in white, vertically centered in the head.
            var glyph = marker.Glyph;
            if (!string.IsNullOrEmpty(glyph))
            {
                var typeface = ResolveMaterialTypeface();
                if (typeface is null)
                {
                    Android.Util.Log.Warn("REFAPP_DIAG", "Glyph pin skipped: no MaterialIcons typeface");
                    return null; // tofu boxes are worse than no glyph pin
                }
                using var tp = new ATextPaint { AntiAlias = true, TextAlign = APaint.Align.Center };
                tp.TextSize = (int)(15 * density);
                tp.Color = AColor.White;
                tp.SetTypeface(typeface);
                var fm = new APaint.FontMetrics();
                tp.GetFontMetrics(fm);
                canvas.DrawText(glyph, headCx, headCy - (fm.Ascent + fm.Descent) / 2f, tp);
            }

            return Here.Explore.Maps.MapImageFactory.FromBitmap(bitmap);
        }
        catch (Exception ex)
        {
            Android.Util.Log.Warn("REFAPP_DIAG", $"TryRenderGlyphPin failed: {ex.Message}");
            return null;
        }
    }

    private static ATypeface? ResolveMaterialTypeface()
    {
        if (_materialTypefaceResolved) return _materialTypeface;
        _materialTypefaceResolved = true;
        try
        {
            // MAUI's font asset path varies by project setup (assets root here,
            // assets/fonts/ in the default template) — try both.
            _materialTypeface = ATypeface.CreateFromAsset(Platform.AppContext.Assets, "fonts/MaterialIcons-Regular.ttf");
        }
        catch (Exception ex)
        {
            Android.Util.Log.Warn("REFAPP_DIAG", $"fonts/ asset miss: {ex.Message}");
            try
            {
                _materialTypeface = ATypeface.CreateFromAsset(Platform.AppContext.Assets, "MaterialIcons-Regular.ttf");
            }
            catch (Exception ex2)
            {
                Android.Util.Log.Warn("REFAPP_DIAG", $"MaterialIcons typeface load failed: {ex2.Message}");
            }
        }
        return _materialTypeface;
    }

    public void RemoveMapMarker3D(Here.Explore.Maui.Models.Maps.MapMarker3D marker)
    {
        if (_mapScene is null) throw new InvalidOperationException("MapService not initialized.");
        if (_markers3D.TryGetValue(marker, out var androidMarker3D))
        {
            _mapScene.RemoveMapMarker3d(androidMarker3D);
            _markers3D.Remove(marker);
        }
    }

    public async Task<Here.Explore.Maui.Models.Maps.MapPickResult?> PickAsync(Point2D screenPoint)
    {
        if (_mapView is null) throw new InvalidOperationException("MapService not initialized.");

        var tcs = new TaskCompletionSource<Here.Explore.Maui.Models.Maps.MapPickResult?>();
        var pickPoint = new Here.Explore.Core.Point2D(screenPoint.X, screenPoint.Y);
        var pickArea = new Here.Explore.Core.Rectangle2D(pickPoint, new Here.Explore.Core.Size2D(1, 1));
        // Create filter with MAP_ITEMS content type
        var filter = new Here.Explore.Maps.MapScene.MapPickFilter(
            new List<Here.Explore.Maps.MapScene.MapPickFilter.ContentType> { Here.Explore.Maps.MapScene.MapPickFilter.ContentType.MapItems! });

        _mapView.Pick(filter, pickArea, new MapPickCallback(tcs));
        return await tcs.Task;
    }

    public void AddMapCircle(Here.Explore.Maui.Models.Maps.MapCircle circle)
    {
        Android.Util.Log.Debug("REFAPP_DIAG", $"AddMapCircle called: center={circle.Center.Latitude},{circle.Center.Longitude} radius={circle.RadiusInMeters}");
        if (_mapScene is null)
        {
            Android.Util.Log.Error("REFAPP_DIAG", "AddMapCircle: _mapScene is null");
            throw new InvalidOperationException("MapService not initialized.");
        }
        try
        {
            var vertices = CircleGeometryHelper.GenerateCircleVertices(circle.Center, circle.RadiusInMeters);
            var geoPolygon = new Here.Explore.Core.GeoPolygon(vertices.Select(v => new Here.Explore.Core.GeoCoordinates(v.Latitude, v.Longitude)).ToList());
            var fillColor = ToCoreColor(circle.FillColor);
            // Circles are polygon approximations, so the outline works through
            // the same outline-taking polygon ctor.
            var androidPolygon = circle.StrokeWidthInPixels > 0
                ? new Here.Explore.Maps.MapPolygon(geoPolygon, fillColor,
                    ToCoreColor(circle.StrokeColor), circle.StrokeWidthInPixels)
                : new Here.Explore.Maps.MapPolygon(geoPolygon, fillColor);
            _mapScene.AddMapPolygon(androidPolygon);
            _circles[circle] = androidPolygon;
            Android.Util.Log.Debug("REFAPP_DIAG", "AddMapCircle: success");
        }
        catch (Exception ex)
        {
            Android.Util.Log.Error("REFAPP_DIAG", $"AddMapCircle: exception: {ex}");
            throw;
        }
    }

    public void RemoveMapCircle(Here.Explore.Maui.Models.Maps.MapCircle circle)
    {
        Android.Util.Log.Debug("REFAPP_DIAG", "RemoveMapCircle called");
        if (_mapScene is null) throw new InvalidOperationException("MapService not initialized.");
        if (_circles.TryGetValue(circle, out var androidPolygon))
        {
            _mapScene.RemoveMapPolygon(androidPolygon);
            _circles.Remove(circle);
            Android.Util.Log.Debug("REFAPP_DIAG", "RemoveMapCircle: success");
        }
        else
        {
            Android.Util.Log.Warn("REFAPP_DIAG", "RemoveMapCircle: circle not found");
        }
    }

    public void ClearAllMapItems()
    {
        if (_mapScene is null) return;

        // Remove all markers
        foreach (var androidMarker in _markers.Values)
            _mapScene.RemoveMapMarker(androidMarker);
        _markers.Clear();

        // Remove all polylines
        foreach (var androidPolyline in _polylines.Values)
            _mapScene.RemoveMapPolyline(androidPolyline);
        _polylines.Clear();

        // Remove all polygons
        foreach (var androidPolygon in _polygons.Values)
            _mapScene.RemoveMapPolygon(androidPolygon);
        _polygons.Clear();

        // Remove all arrows
        foreach (var androidArrow in _arrows.Values)
            _mapScene.RemoveMapArrow(androidArrow);
        _arrows.Clear();

        // Remove all 3D markers
        foreach (var androidMarker3D in _markers3D.Values)
            _mapScene.RemoveMapMarker3d(androidMarker3D);
        _markers3D.Clear();

        // Remove all clusters
        foreach (var androidCluster in _markerClusters.Values)
            _mapScene.RemoveMapMarkerCluster(androidCluster);
        _markerClusters.Clear();

        // Remove all circles
        foreach (var androidCircle in _circles.Values)
            _mapScene.RemoveMapPolygon(androidCircle);
        _circles.Clear();
    }

    private static Here.Explore.Core.Color ToCoreColor(uint argb)
    {
        // Use the factory method that accepts Android ARGB integer directly.
        return Here.Explore.Core.Color.ValueOf((int)argb);
    }

    private static Here.Explore.Maps.MapScheme ToAndroidMapScheme(Here.Explore.Maui.Models.Maps.MapScheme scheme) => scheme switch
    {
        Here.Explore.Maui.Models.Maps.MapScheme.NormalDay => Here.Explore.Maps.MapScheme.NormalDay!,
        Here.Explore.Maui.Models.Maps.MapScheme.NormalNight => Here.Explore.Maps.MapScheme.NormalNight!,
        Here.Explore.Maui.Models.Maps.MapScheme.HybridDay => Here.Explore.Maps.MapScheme.HybridDay!,
        Here.Explore.Maui.Models.Maps.MapScheme.SatelliteDay => Here.Explore.Maps.MapScheme.Satellite!, // Satellite (not SatelliteDay)
        Here.Explore.Maui.Models.Maps.MapScheme.TerrainDay => Here.Explore.Maps.MapScheme.NormalDay!, // No TerrainDay in binding
        _ => Here.Explore.Maps.MapScheme.NormalDay!,
    };

    public void AddMapMarkerCluster(MapMarkerCluster cluster, IEnumerable<MapMarker> markers)
    {
        if (_mapScene is null) throw new InvalidOperationException("MapService not initialized.");

        // Create ImageStyle with default cluster image
        var clusterImage = Here.Explore.Maps.MapImageFactory.FromResource(Platform.AppContext.Resources, global::Android.Resource.Drawable.IcMenuCompass)!;
        var imageStyle = new Here.Explore.Maps.MapMarkerCluster.ImageStyle(clusterImage);
        var androidCluster = new Here.Explore.Maps.MapMarkerCluster(imageStyle);

        foreach (var marker in markers)
        {
            var coords = new Here.Explore.Core.GeoCoordinates(marker.Coordinates.Latitude, marker.Coordinates.Longitude);
            var image = Here.Explore.Maps.MapImageFactory.FromResource(Platform.AppContext.Resources, global::Android.Resource.Drawable.IcMenuCompass)!;
            var androidMarker = new Here.Explore.Maps.MapMarker(coords, image);
            androidCluster.AddMapMarker(androidMarker);
        }

        _mapScene.AddMapMarkerCluster(androidCluster);
        _markerClusters[cluster] = androidCluster;
    }

    public void RemoveMapMarkerCluster(MapMarkerCluster cluster)
    {
        if (_mapScene is null) throw new InvalidOperationException("MapService not initialized.");

        // Remove only the tracked cluster — RemoveAllMapMarkers would wipe
        // individually added markers too.
        if (_markerClusters.TryGetValue(cluster, out var androidCluster))
        {
            _mapScene.RemoveMapMarkerCluster(androidCluster);
            _markerClusters.Remove(cluster);
        }
    }

    public void AddLocationIndicator(LocationIndicator indicator)
    {
        if (_mapView is null) throw new InvalidOperationException("MapService not initialized.");

        var androidIndicator = new Here.Explore.Maps.LocationIndicator(_mapView);
        var indicatorStyle = indicator.Style == LocationIndicatorStyle.Navigation
            ? Here.Explore.Maps.LocationIndicator.IndicatorStyle.Navigation!
            : Here.Explore.Maps.LocationIndicator.IndicatorStyle.Pedestrian!;
        androidIndicator.LocationIndicatorStyle = indicatorStyle;
        androidIndicator.Active = indicator.IsVisible;
        androidIndicator.Enable(_mapView);

        // Store reference for updates
        _locationIndicator = androidIndicator;

        // Update initial position
        UpdateLocationIndicator(indicator.Location, indicator.Bearing);
    }

    public void UpdateLocationIndicator(GeoCoordinates location, double? bearing = null)
    {
        if (_mapView is null || _locationIndicator is null) return;

        var androidCoords = new Here.Explore.Core.GeoCoordinates(location.Latitude, location.Longitude);
        var androidLocation = new Here.Explore.Core.Location(androidCoords);
        if (bearing.HasValue)
            androidLocation.BearingInDegrees = (Java.Lang.Double?)bearing.Value;
        _locationIndicator.UpdateLocation(androidLocation);
    }

    public void RemoveLocationIndicator()
    {
        if (_locationIndicator is null) return;
        _locationIndicator.Disable();
        _locationIndicator = null;
    }

    private Here.Explore.Maps.LocationIndicator? _locationIndicator;
}

internal class SceneLoadCallback : Java.Lang.Object, Here.Explore.Maps.MapScene.ILoadSceneCallback
{
    private readonly TaskCompletionSource<bool> _tcs;
    public SceneLoadCallback(TaskCompletionSource<bool> tcs) => _tcs = tcs;

    public void OnLoadScene(Here.Explore.Maps.MapError? error)
    {
        Android.Util.Log.Debug("REFAPP_DIAG", $"SceneLoadCallback.OnLoadScene called, error={(error is null ? "null" : error.Value.ToString())}");
        if (error is null)
            _tcs.SetResult(true);
        else
            _tcs.SetException(new Exception($"Scene load error: {error.Value}"));
    }
}

/// Bridges the SDK camera animation callback to the shared Task-based API.
internal class CameraAnimationListener : Java.Lang.Object, Here.Explore.Animation.IAnimationListener
{
    private readonly TaskCompletionSource<bool> _tcs;
    public CameraAnimationListener(TaskCompletionSource<bool> tcs) => _tcs = tcs;

    public void OnAnimationStateChanged(Here.Explore.Animation.AnimationState state)
    {
        if (state == Here.Explore.Animation.AnimationState.Completed)
            _tcs.TrySetResult(true);
        else if (state == Here.Explore.Animation.AnimationState.Cancelled)
            _tcs.TrySetResult(false);
    }
}

internal class MapPickCallback : Java.Lang.Object, Here.Explore.Maps.IMapViewBase.IMapPickCallback
{
    private readonly TaskCompletionSource<Here.Explore.Maui.Models.Maps.MapPickResult?> _tcs;
    public MapPickCallback(TaskCompletionSource<Here.Explore.Maui.Models.Maps.MapPickResult?> tcs) => _tcs = tcs;

    public void OnPickMap(Here.Explore.Maps.MapPickResult? result)
    {
        if (result?.MapItems is not { } mapItems || mapItems.Markers.Count == 0)
        {
            // Nothing picked — honor the null contract (never fabricate (0,0)).
            _tcs.SetResult(null);
            return;
        }

        // Coordinates from the first picked marker
        var marker = mapItems.Markers[0];
        var pickResult = new Here.Explore.Maui.Models.Maps.MapPickResult(
            new GeoCoordinates(marker.Coordinates.Latitude, marker.Coordinates.Longitude),
            result);
        _tcs.SetResult(pickResult);
    }
}
#endif