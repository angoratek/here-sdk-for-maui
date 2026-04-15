#if IOS
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Routing;
using Here.Explore.iOS;

namespace Here.Explore.Maui.Services;

/// <summary>
/// iOS-specific RoutingService implementation using NativeBridge wrappers.
/// Uses HereRoutingEngine for route calculation.
/// Isoline and traffic-on-route are not yet available in NativeBridge.
/// </summary>
public partial class RoutingService
{
    private HereRoutingEngine? _engine;

    internal void Initialize()
    {
        _engine = new HereRoutingEngine(0);
    }

    public async Task<RoutingResult> CalculateRouteAsync(IReadOnlyList<Waypoint> waypoints, RoutingOptions options)
    {
        if (_engine is null) throw new InvalidOperationException("RoutingService not initialized.");
        var tcs = new TaskCompletionSource<RoutingResult>();

        var iosWaypoints = waypoints.Select(w => new HereWaypoint(
            w.Coordinates.Latitude,
            w.Coordinates.Longitude,
            (nint)ToIOSWaypointType(w.Type))).ToArray();

        var iosOptions = new HereRoutingOptions((nint)ToIOSTransportMode(options.TransportMode));

        _engine.CalculateRoute(iosWaypoints, iosOptions, result =>
        {
            if (result.Error is not null)
                tcs.SetResult(new RoutingResult(ToSharedRoutingError(result.Error), null));
            else if (result.Routes is not null)
                tcs.SetResult(new RoutingResult(RoutingError.None, result.Routes.Select(ToSharedRoute).ToList()));
            else
                tcs.SetResult(new RoutingResult(RoutingError.None, null));
        });

        return await tcs.Task;
    }

    public Task<IsolineResult> CalculateIsolineAsync(GeoCoordinates center, IsolineOptions options)
    {
        // Isoline routing is not yet available in NativeBridge.
        throw new NotImplementedException("Isoline calculation is not yet supported on iOS.");
    }

    public Task<TrafficOnRoute> GetTrafficOnRouteAsync(Route route)
    {
        // Traffic on route is not yet available in NativeBridge.
        throw new NotImplementedException("Traffic on route is not yet supported on iOS.");
    }

    private static SectionTransportMode ToIOSTransportMode(SectionTransportMode mode) => mode switch
    {
        SectionTransportMode.Truck => SectionTransportMode.Truck,
        SectionTransportMode.Pedestrian => SectionTransportMode.Pedestrian,
        SectionTransportMode.Bicycle => SectionTransportMode.Bicycle,
        SectionTransportMode.Scooter => SectionTransportMode.Scooter,
        _ => SectionTransportMode.Car,
    };

    // NativeBridge waypoint type: 0=Stopover, 1=PassThrough
    // Shared model: Stop=0, Start=1, Through=2
    private static int ToIOSWaypointType(WaypointType type) => type switch
    {
        WaypointType.Through => 1, // PassThrough
        _ => 0, // Stopover (both Stop and Start map to Stopover)
    };

    private static RoutingError ToSharedRoutingError(string? error)
    {
        if (string.IsNullOrEmpty(error)) return RoutingError.None;
        if (error.Contains("NoRouteFound", StringComparison.OrdinalIgnoreCase)) return RoutingError.NoRouteFound;
        if (error.Contains("HttpError", StringComparison.OrdinalIgnoreCase)) return RoutingError.HttpError;
        if (error.Contains("InvalidParameter", StringComparison.OrdinalIgnoreCase)) return RoutingError.InvalidParameters;
        return RoutingError.NetworkError;
    }

    private static Route ToSharedRoute(HereRoute iosRoute)
    {
        return new Route(
            string.Empty,
            new List<Section>(),
            iosRoute.LengthInMeters,
            (long)iosRoute.DurationInSeconds);
    }
}
#endif