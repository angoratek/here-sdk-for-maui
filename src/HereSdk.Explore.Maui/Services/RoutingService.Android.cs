#if ANDROID
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Routing;
using SharedManeuverAction = Here.Explore.Maui.Models.Routing.ManeuverAction;

namespace Here.Explore.Maui.Services;

/// <summary>
/// Android-specific RoutingService implementation using HERE SDK Android bindings.
/// Uses RoutingEngine.CalculateRoute() with RoutingOptions (unified, not deprecated per-transport overloads).
/// Uses IsolineRoutingEngine.CalculateIsoline() with Waypoint center point.
/// Java enum comparisons use if/else if since Java.Lang.Enum can't be used in C# switch.
/// Duration type is Com.Here.Time.HereDuration with .Seconds property.
/// ManeuverAction uses PascalCase: LeftTurn, RightTurn, SharpLeftTurn, LeftUTurn, etc.
/// </summary>
public partial class RoutingService
{
    private Here.Explore.Routing.RoutingEngine? _engine;
    private Here.Explore.Routing.IsolineRoutingEngine? _isolineEngine;

    internal void Initialize()
    {
        if (Here.Explore.Core.Engine.SDKNativeEngine.SharedInstance is not null)
        {
            _engine = new Here.Explore.Routing.RoutingEngine();
            _isolineEngine = new Here.Explore.Routing.IsolineRoutingEngine();
        }
    }

    public async Task<RoutingResult> CalculateRouteAsync(IReadOnlyList<Waypoint> waypoints, RoutingOptions options)
    {
        if (_engine is null) throw new InvalidOperationException("RoutingService not initialized.");
        var tcs = new TaskCompletionSource<RoutingResult>();

        var androidWaypoints = waypoints.Select(w => new Here.Explore.Routing.Waypoint(
            new Here.Explore.Core.GeoCoordinates(w.Coordinates.Latitude, w.Coordinates.Longitude))).ToList();

        var androidOptions = ToAndroidRoutingOptions(options);
        _engine.CalculateRoute(androidWaypoints, androidOptions, new RouteCalculatedCallback(tcs));
        return await tcs.Task;
    }

    public async Task<IsolineResult> CalculateIsolineAsync(GeoCoordinates center, IsolineOptions options)
    {
        if (_isolineEngine is null) throw new InvalidOperationException("RoutingService not initialized.");
        var tcs = new TaskCompletionSource<IsolineResult>();

        var androidWaypoint = new Here.Explore.Routing.Waypoint(
            new Here.Explore.Core.GeoCoordinates(center.Latitude, center.Longitude));
        var androidOptions = ToAndroidIsolineOptions(options);
        _isolineEngine.CalculateIsoline(androidWaypoint, androidOptions, new IsolineCalculatedCallback(tcs));
        return await tcs.Task;
    }

    public Task<TrafficOnRoute> GetTrafficOnRouteAsync(Route route)
    {
        // TrafficOnRoute requires an Android Route object — must be calculated first
        throw new InvalidOperationException(
            "Use the overload that accepts a calculated Android route. " +
            "Call CalculateRouteAsync first, then pass the result to GetTrafficOnRouteAsync.");
    }

    private static Here.Explore.Routing.RoutingOptions ToAndroidRoutingOptions(RoutingOptions options)
    {
        var androidOptions = Here.Explore.Routing.RoutingOptions.FromDefaultParameterConfiguration()!;

        if (options.Optimization == OptimizationMode.Shortest &&
            Here.Explore.Routing.OptimizationMode.Shortest is not null)
            androidOptions.RouteOptions.OptimizationMode = Here.Explore.Routing.OptimizationMode.Shortest!;
        else if (Here.Explore.Routing.OptimizationMode.Fastest is not null)
            androidOptions.RouteOptions.OptimizationMode = Here.Explore.Routing.OptimizationMode.Fastest!;

        var transportSpec = BuildTransportSpecification(options.TransportMode);
        androidOptions.TransportSpecification = transportSpec;

        return androidOptions;
    }

    private static Here.Explore.Transport.TransportSpecification BuildTransportSpecification(SectionTransportMode mode)
    {
        return mode switch
        {
            SectionTransportMode.Truck => new Here.Explore.Transport.TransportSpecification.TruckBuilder().Build(),
            SectionTransportMode.Pedestrian => new Here.Explore.Transport.TransportSpecification.PedestrianBuilder().Build(),
            SectionTransportMode.Bicycle => new Here.Explore.Transport.TransportSpecification.BicycleBuilder().Build(),
            SectionTransportMode.Scooter => new Here.Explore.Transport.TransportSpecification.ScooterBuilder().Build(),
            SectionTransportMode.Bus => new Here.Explore.Transport.TransportSpecification.BusBuilder().Build(),
            SectionTransportMode.Taxi => new Here.Explore.Transport.TransportSpecification.TaxiBuilder().Build(),
            _ => new Here.Explore.Transport.TransportSpecification.CarBuilder().Build(),
        };
    }

    private static Here.Explore.Routing.IsolineOptions ToAndroidIsolineOptions(IsolineOptions options)
    {
        var rangeType = Here.Explore.Routing.IsolineRangeType.DistanceInMeters!;
        var rangeValues = new System.Collections.Generic.List<Java.Lang.Integer>
        {
            (Java.Lang.Integer)(int)options.RangeInMeters
        };

        var calculation = options.MaxPoints.HasValue
            ? new Here.Explore.Routing.IsolineOptions.Calculation(rangeType, rangeValues,
                Here.Explore.Routing.IsolineCalculationMode.Balanced!,
                (Java.Lang.Integer)options.MaxPoints.Value,
                Here.Explore.Routing.RoutePlaceDirection.Departure!)
            : new Here.Explore.Routing.IsolineOptions.Calculation(rangeType, rangeValues);

        var transportSpec = BuildTransportSpecification(options.TransportMode);
        var routingOptions = Here.Explore.Routing.RoutingOptions.FromDefaultParameterConfiguration()!;
        routingOptions.TransportSpecification = transportSpec;

        return new Here.Explore.Routing.IsolineOptions(calculation, routingOptions);
    }

    internal static RoutingError ToSharedRoutingError(Here.Explore.Routing.RoutingError? error)
    {
        if (error is null) return RoutingError.None;
        if (error.Equals(Here.Explore.Routing.RoutingError.HttpError)) return RoutingError.HttpError;
        if (error.Equals(Here.Explore.Routing.RoutingError.NoRouteFound)) return RoutingError.NoRouteFound;
        if (error.Equals(Here.Explore.Routing.RoutingError.CouldNotMatchOrigin)) return RoutingError.NoMatchingMapData;
        if (error.Equals(Here.Explore.Routing.RoutingError.CouldNotMatchDestination)) return RoutingError.NoMatchingMapData;
        if (error.Equals(Here.Explore.Routing.RoutingError.InvalidParameter)) return RoutingError.InvalidParameters;
        if (error.Equals(Here.Explore.Routing.RoutingError.NoIsolineFound)) return RoutingError.NoRouteFound;
        if (error.Equals(Here.Explore.Routing.RoutingError.RouteCalculationFailed)) return RoutingError.NoAcceptableRoute;
        return RoutingError.NetworkError;
    }

    internal static Route ToSharedRoute(Here.Explore.Routing.Route androidRoute)
    {
        var handle = androidRoute.RouteHandle?.Handle ?? string.Empty;
        var length = androidRoute.LengthInMeters;
        var duration = androidRoute.Duration?.Seconds ?? 0;

        var sections = new List<Section>();
        if (androidRoute.Sections is not null)
        {
            var sectionIndex = 0;
            foreach (var s in androidRoute.Sections)
            {
                var departureCoords = s.DeparturePlace?.MapMatchedCoordinates;
                var departure = departureCoords is not null
                    ? new GeoCoordinates(departureCoords.Latitude, departureCoords.Longitude)
                    : new GeoCoordinates(0, 0);

                var arrivalCoords = s.ArrivalPlace?.MapMatchedCoordinates;
                var arrival = arrivalCoords is not null
                    ? new GeoCoordinates(arrivalCoords.Latitude, arrivalCoords.Longitude)
                    : new GeoCoordinates(0, 0);

                var transportMode = ToSharedTransportMode(s.SectionTransportMode);

                var maneuvers = new List<Maneuver>();
                if (s.Maneuvers is not null)
                {
                    foreach (var m in s.Maneuvers)
                    {
                        var mCoords = m.Coordinates is not null
                            ? new GeoCoordinates(m.Coordinates.Latitude, m.Coordinates.Longitude)
                            : new GeoCoordinates(0, 0);

                        var action = ToSharedManeuverAction(m.Action);
                        var mDuration = m.Duration?.Seconds;

                        string? nextRoadName = m.NextRoadTexts?.Names?.DefaultValue;
                        string? nextRoadNumber = m.NextRoadTexts?.NumbersWithDirection?.DefaultValue;
                        double? turnAngle = m.TurnAngleInDegrees is not null
                            ? (double)m.TurnAngleInDegrees
                            : null;

                        maneuvers.Add(new Maneuver(
                            mCoords,
                            action,
                            m.Text,
                            turnAngle,
                            null,
                            nextRoadName,
                            nextRoadNumber,
                            (double?)m.LengthInMeters,
                            mDuration));
                    }
                }

                IReadOnlyList<GeoCoordinates>? geometry = null;
                if (s.Geometry is not null && s.Geometry.Vertices is not null)
                {
                    var geoList = new List<GeoCoordinates>();
                    foreach (var v in s.Geometry.Vertices)
                    {
                        if (v is Here.Explore.Core.GeoCoordinates coords)
                            geoList.Add(new GeoCoordinates(coords.Latitude, coords.Longitude));
                    }
                    geometry = geoList;
                }

                sections.Add(new Section(
                    sectionIndex++,
                    departure,
                    arrival,
                    maneuvers,
                    transportMode,
                    s.LengthInMeters,
                    s.Duration?.Seconds ?? 0,
                    geometry));
            }
        }

        return new Route(handle, sections, length, duration);
    }

    private static SectionTransportMode ToSharedTransportMode(Here.Explore.Routing.SectionTransportMode? mode)
    {
        if (mode is null) return SectionTransportMode.Car;
        if (mode.Equals(Here.Explore.Routing.SectionTransportMode.Truck)) return SectionTransportMode.Truck;
        if (mode.Equals(Here.Explore.Routing.SectionTransportMode.Pedestrian)) return SectionTransportMode.Pedestrian;
        if (mode.Equals(Here.Explore.Routing.SectionTransportMode.Bicycle)) return SectionTransportMode.Bicycle;
        if (mode.Equals(Here.Explore.Routing.SectionTransportMode.Scooter)) return SectionTransportMode.Scooter;
        if (mode.Equals(Here.Explore.Routing.SectionTransportMode.Bus)) return SectionTransportMode.Bus;
        if (mode.Equals(Here.Explore.Routing.SectionTransportMode.Taxi)) return SectionTransportMode.Taxi;
        if (mode.Equals(Here.Explore.Routing.SectionTransportMode.PublicTransit)) return SectionTransportMode.Transit;
        return SectionTransportMode.Car;
    }

    private static SharedManeuverAction ToSharedManeuverAction(Here.Explore.Routing.ManeuverAction? action)
    {
        if (action is null) return SharedManeuverAction.Straight;
        if (action.Equals(Here.Explore.Routing.ManeuverAction.Depart)) return SharedManeuverAction.Depart;
        if (action.Equals(Here.Explore.Routing.ManeuverAction.Arrive)) return SharedManeuverAction.Arrive;
        if (action.Equals(Here.Explore.Routing.ManeuverAction.LeftTurn)) return SharedManeuverAction.Left;
        if (action.Equals(Here.Explore.Routing.ManeuverAction.RightTurn)) return SharedManeuverAction.Right;
        if (action.Equals(Here.Explore.Routing.ManeuverAction.SharpLeftTurn)) return SharedManeuverAction.SharpLeft;
        if (action.Equals(Here.Explore.Routing.ManeuverAction.SharpRightTurn)) return SharedManeuverAction.SharpRight;
        if (action.Equals(Here.Explore.Routing.ManeuverAction.SlightLeftTurn)) return SharedManeuverAction.SlightLeft;
        if (action.Equals(Here.Explore.Routing.ManeuverAction.SlightRightTurn)) return SharedManeuverAction.SlightRight;
        if (action.Equals(Here.Explore.Routing.ManeuverAction.LeftUTurn)) return SharedManeuverAction.UTurnLeft;
        if (action.Equals(Here.Explore.Routing.ManeuverAction.RightUTurn)) return SharedManeuverAction.UTurnRight;
        if (action.Equals(Here.Explore.Routing.ManeuverAction.LeftRamp)) return SharedManeuverAction.LeftRamp;
        if (action.Equals(Here.Explore.Routing.ManeuverAction.RightRamp)) return SharedManeuverAction.RightRamp;
        if (action.Equals(Here.Explore.Routing.ManeuverAction.LeftExit)) return SharedManeuverAction.LeftExit;
        if (action.Equals(Here.Explore.Routing.ManeuverAction.RightExit)) return SharedManeuverAction.RightExit;
        if (action.Equals(Here.Explore.Routing.ManeuverAction.LeftRoundaboutEnter)) return SharedManeuverAction.Roundabout;
        if (action.Equals(Here.Explore.Routing.ManeuverAction.RightRoundaboutEnter)) return SharedManeuverAction.Roundabout;
        if (action.Equals(Here.Explore.Routing.ManeuverAction.ContinueOn)) return SharedManeuverAction.Straight;
        return SharedManeuverAction.Straight;
    }
}

internal class RouteCalculatedCallback : Java.Lang.Object, Here.Explore.Routing.RouteCalculatedHandler
{
    private readonly TaskCompletionSource<RoutingResult> _tcs;
    public RouteCalculatedCallback(TaskCompletionSource<RoutingResult> tcs) => _tcs = tcs;

    public void OnRouteCalculated(Here.Explore.Routing.RoutingError? error, System.Collections.Generic.IList<Here.Explore.Routing.Route>? routes)
    {
        if (error is not null)
            _tcs.SetResult(new RoutingResult(RoutingService.ToSharedRoutingError(error), null));
        else if (routes is not null)
            _tcs.SetResult(new RoutingResult(RoutingError.None,
                routes.Select(RoutingService.ToSharedRoute).ToList()));
        else
            _tcs.SetResult(new RoutingResult(RoutingError.None, null));
    }
}

internal class IsolineCalculatedCallback : Java.Lang.Object, Here.Explore.Routing.IsolineCalculatedHandler
{
    private readonly TaskCompletionSource<IsolineResult> _tcs;
    public IsolineCalculatedCallback(TaskCompletionSource<IsolineResult> tcs) => _tcs = tcs;

    public void OnIsolineCalculated(Here.Explore.Routing.RoutingError? error, System.Collections.Generic.IList<Here.Explore.Routing.Isoline>? isolines)
    {
        if (error is not null)
            _tcs.SetResult(new IsolineResult(RoutingService.ToSharedRoutingError(error), null));
        else if (isolines is not null)
            _tcs.SetResult(new IsolineResult(RoutingError.None,
                isolines.Select(i =>
                {
                    var polygons = new List<GeoCoordinates>();
                    if (i.Polygons is not null)
                    {
                        foreach (var polygon in i.Polygons)
                        {
                            if (polygon.Vertices is not null)
                            {
                                foreach (var v in polygon.Vertices)
                                {
                                    if (v is Here.Explore.Core.GeoCoordinates coords)
                                        polygons.Add(new GeoCoordinates(coords.Latitude, coords.Longitude));
                                }
                            }
                        }
                    }
                    return new Isoline(polygons, i.RangeValue);
                }).ToList()));
        else
            _tcs.SetResult(new IsolineResult(RoutingError.None, null));
    }
}
#endif