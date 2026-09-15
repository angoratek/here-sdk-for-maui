#pragma warning disable CS1591
#if IOS
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Routing;
using Here.Explore.iOS;

namespace Here.Explore.Maui.Services;

/// <summary>
/// iOS-specific RoutingService implementation using NativeBridge wrappers.
/// Uses HereRoutingEngine for route calculation and HereIsolineRoutingEngine for isolines.
/// Traffic-on-route is not yet available in NativeBridge.
/// </summary>
public partial class RoutingService
{
    private HereRoutingEngine? _engine;
    private HereIsolineRoutingEngine? _isolineEngine;

    // HereRoute wrappers of the most recent CalculateRouteAsync call, keyed by
    // route handle. CalculateTrafficOnRoute requires the wrapper to still hold
    // its underlying Swift Route, so the last calculation's routes are retained
    // for GetTrafficOnRouteAsync.
    private readonly Dictionary<string, HereRoute> _nativeRoutes = new();

    internal void Initialize()
    {
        _engine = new HereRoutingEngine(0);
        _isolineEngine = new HereIsolineRoutingEngine(0);
    }

    partial void DisposePlatform(bool disposing)
    {
        _engine?.Dispose();
        _engine = null;
        _isolineEngine?.Dispose();
        _isolineEngine = null;
    }

    public async Task<RoutingResult> CalculateRouteAsync(IReadOnlyList<Waypoint> waypoints, RoutingOptions options)
    {
        if (_engine is null) throw new InvalidOperationException("RoutingService not initialized.");
        var tcs = new TaskCompletionSource<RoutingResult>();

        var iosWaypoints = waypoints.Select(w => new HereWaypoint(
            w.Coordinates.Latitude,
            w.Coordinates.Longitude,
            (nint)ToIOSWaypointType(w.Type))).ToArray();

        var iosOptions = new HereRoutingOptions(
            (nint)ToIOSTransportMode(options.TransportMode),
            (int)(options.MaxAlternatives ?? 0));

        if (options.Truck is TruckVehicleSpecifications truck)
        {
            iosOptions.TruckSpecifications = new HereTruckSpecifications(
                truck.GrossWeightInKilograms ?? 0,
                truck.HeightInCentimeters ?? 0,
                truck.WidthInCentimeters ?? 0,
                truck.LengthInCentimeters ?? 0,
                truck.AxleCount ?? 0,
                truck.TrailerCount ?? 0);
        }

        _engine.CalculateRoute(iosWaypoints, iosOptions, result =>
        {
            if (result.Error is not null)
                tcs.SetResult(new RoutingResult(ToSharedRoutingError(result.Error), null));
            else if (result.Routes is not null)
            {
                // Retain the HereRoute wrappers for a later GetTrafficOnRouteAsync call.
                _nativeRoutes.Clear();
                foreach (var iosRoute in result.Routes)
                {
                    if (!string.IsNullOrEmpty(iosRoute.RouteHandle))
                        _nativeRoutes[iosRoute.RouteHandle!] = iosRoute;
                }

                tcs.SetResult(new RoutingResult(RoutingError.None, result.Routes.Select(ToSharedRoute).ToList()));
            }
            else
                tcs.SetResult(new RoutingResult(RoutingError.None, null));
        });

        return await tcs.Task;
    }

    public async Task<IsolineResult> CalculateIsolineAsync(GeoCoordinates center, IsolineOptions options)
    {
        if (_isolineEngine is null) throw new InvalidOperationException("RoutingService not initialized.");
        var tcs = new TaskCompletionSource<IsolineResult>();

        _isolineEngine.CalculateIsoline(
            center.Latitude,
            center.Longitude,
            (nint)ToIOSTransportMode(options.TransportMode),
            (int)options.RangeInMeters,
            options.MaxPoints ?? 0,
            result =>
            {
                if (result.Error is not null)
                    tcs.SetResult(new IsolineResult(ToSharedRoutingError(result.Error), null));
                else if (result.Isolines is not null)
                    tcs.SetResult(new IsolineResult(RoutingError.None,
                        result.Isolines.Select(i => new Isoline(
                            i.PolygonVertices.Select(v => new GeoCoordinates(v.Latitude, v.Longitude)).ToList(),
                            i.RangeValue)).ToList()));
                else
                    tcs.SetResult(new IsolineResult(RoutingError.None, null));
            });

        return await tcs.Task;
    }

    public async Task<TrafficOnRouteResult> GetTrafficOnRouteAsync(Route route)
    {
        if (_engine is null) throw new InvalidOperationException("RoutingService not initialized.");
        if (!_nativeRoutes.TryGetValue(route.Handle, out var iosRoute))
            throw new InvalidOperationException(
                "Unknown route: calculate it with CalculateRouteAsync first, then pass the returned Route.");

        var tcs = new TaskCompletionSource<TrafficOnRouteResult>();

        _engine.CalculateTrafficOnRoute(iosRoute, 0, 0, result =>
        {
            if (result.Error is not null)
                tcs.SetResult(new TrafficOnRouteResult(ToSharedRoutingError(result.Error), null));
            else if (result.TrafficOnRoute is not null)
                tcs.SetResult(new TrafficOnRouteResult(RoutingError.None, ToSharedTrafficOnRoute(result.TrafficOnRoute)));
            else
                tcs.SetResult(new TrafficOnRouteResult(RoutingError.None, null));
        });

        return await tcs.Task;
    }

    internal static TrafficOnRoute ToSharedTrafficOnRoute(HereTrafficOnRoute iosTrafficOnRoute)
    {
        var sections = new List<TrafficOnSection>();
        if (iosTrafficOnRoute.TrafficSections is not null)
        {
            foreach (var s in iosTrafficOnRoute.TrafficSections)
            {
                var geometry = (s.Geometry ?? Array.Empty<HereGeoCoordinates>())
                    .Select(v => new GeoCoordinates(v.Latitude, v.Longitude)).ToList();

                var spans = (s.TrafficSpans ?? Array.Empty<HereTrafficOnSpan>())
                    .Select(span => new TrafficOnSpan(
                        span.JamFactor,
                        span.LengthInMeters,
                        span.BaseSpeedInMetersPerSecond,
                        span.TrafficSpeedInMetersPerSecond,
                        span.TrafficDelayInSeconds,
                        span.DurationInSeconds,
                        span.GeometryOffset,
                        (span.IncidentIndices ?? Array.Empty<Foundation.NSNumber>())
                            .Select(n => n.Int32Value).ToList()))
                    .ToList();

                var incidents = (s.TrafficIncidents ?? Array.Empty<HereTrafficIncidentOnRoute>())
                    .Select(i => new TrafficIncidentOnRoute(
                        i.Id,
                        TrafficService.ToSharedIncidentType(i.TypeRawValue),
                        TrafficService.ToSharedIncidentImpact(i.ImpactRawValue),
                        i.DescriptionText))
                    .ToList();

                sections.Add(new TrafficOnSection(geometry, spans, incidents));
            }
        }

        return new TrafficOnRoute(
            iosTrafficOnRoute.LastTraveledSectionIndex,
            iosTrafficOnRoute.TraveledDistanceOnLastSectionInMeters,
            sections);
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
