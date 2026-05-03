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
        var handle = iosRoute.RouteHandle ?? string.Empty;
        var sections = new List<Section>();

        if (iosRoute.Sections is not null)
        {
            var sectionIndex = 0;
            foreach (var s in iosRoute.Sections)
            {
                var departure = new GeoCoordinates(s.Departure.Latitude, s.Departure.Longitude);
                var arrival = new GeoCoordinates(s.Arrival.Latitude, s.Arrival.Longitude);
                var transportMode = ToSharedTransportMode((uint)s.TransportMode);

                var maneuvers = new List<Maneuver>();
                if (s.Maneuvers is not null)
                {
                    foreach (var m in s.Maneuvers)
                    {
                        var mCoords = new GeoCoordinates(m.Coordinates.Latitude, m.Coordinates.Longitude);
                        var action = ToSharedManeuverAction((uint)m.Action);
                        maneuvers.Add(new Maneuver(
                            mCoords,
                            action,
                            m.Text,
                            m.TurnAngleInDegrees,
                            null,
                            null,
                            null,
                            m.LengthInMeters,
                            (long)m.DurationInSeconds));
                    }
                }

                IReadOnlyList<GeoCoordinates>? geometry = null;
                if (s.Geometry is not null)
                {
                    geometry = s.Geometry.Vertices
                        .Select(v => new GeoCoordinates(v.Latitude, v.Longitude))
                        .ToList();
                }

                sections.Add(new Section(
                    sectionIndex++,
                    departure,
                    arrival,
                    maneuvers,
                    transportMode,
                    s.LengthInMeters,
                    (long)s.DurationInSeconds,
                    geometry));
            }
        }

        return new Route(handle, sections, iosRoute.LengthInMeters, (long)iosRoute.DurationInSeconds);
    }

    private static SectionTransportMode ToSharedTransportMode(uint rawValue)
    {
        // SectionTransportMode raw values from HERE SDK iOS:
        // car=0, truck=1, pedestrian=2, ferry=3, carShuttleTrain=4, scooter=5, bicycle=6,
        // publicTransit=7, taxi=8, bus=9, privateBus=10
        return rawValue switch
        {
            1 => SectionTransportMode.Truck,
            2 => SectionTransportMode.Pedestrian,
            5 => SectionTransportMode.Scooter,
            6 => SectionTransportMode.Bicycle,
            7 => SectionTransportMode.Transit,
            8 => SectionTransportMode.Taxi,
            9 => SectionTransportMode.Bus,
            _ => SectionTransportMode.Car,
        };
    }

    private static ManeuverAction ToSharedManeuverAction(uint rawValue)
    {
        // ManeuverAction raw values from HERE SDK iOS:
        // depart=0, arrive=1, leftUTurn=2, sharpLeftTurn=3, leftTurn=4, slightLeftTurn=5,
        // continueOn=6, slightRightTurn=7, rightTurn=8, sharpRightTurn=9, rightUTurn=10,
        // leftExit=11, rightExit=12, leftRamp=13, rightRamp=14, leftFork=15, middleFork=16,
        // rightFork=17, enterHighwayFromLeft=18, enterHighwayFromRight=19,
        // leftRoundaboutEnter=20, rightRoundaboutEnter=21, ...
        return rawValue switch
        {
            0 => ManeuverAction.Depart,
            1 => ManeuverAction.Arrive,
            2 => ManeuverAction.UTurnLeft,
            3 => ManeuverAction.SharpLeft,
            4 => ManeuverAction.Left,
            5 => ManeuverAction.SlightLeft,
            6 => ManeuverAction.Straight,
            7 => ManeuverAction.SlightRight,
            8 => ManeuverAction.Right,
            9 => ManeuverAction.SharpRight,
            10 => ManeuverAction.UTurnRight,
            11 => ManeuverAction.LeftExit,
            12 => ManeuverAction.RightExit,
            13 => ManeuverAction.LeftRamp,
            14 => ManeuverAction.RightRamp,
            20 => ManeuverAction.Roundabout,
            21 => ManeuverAction.Roundabout,
            _ => ManeuverAction.Straight,
        };
    }
}
#endif
