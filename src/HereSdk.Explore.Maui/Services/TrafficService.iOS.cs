#pragma warning disable CS1591
#if IOS
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Traffic;
using Here.Explore.iOS;

namespace Here.Explore.Maui.Services;

/// <summary>
/// iOS-specific TrafficService implementation using NativeBridge.
/// </summary>
public partial class TrafficService
{
    private HereTrafficEngine? _engine;

    internal void Initialize()
    {
        _engine = new HereTrafficEngine(0);
    }

    partial void DisposePlatform(bool disposing)
    {
        _engine?.Dispose();
        _engine = null;
    }

    public async Task<TrafficFlowResult> QueryFlowAsync(GeoCircle area, TrafficFlowQueryOptions options)
    {
        if (_engine is null) throw new InvalidOperationException("TrafficService not initialized.");
        var tcs = new TaskCompletionSource<TrafficFlowResult>();

        _engine.QueryFlow(area.Center.Latitude, area.Center.Longitude, area.RadiusInMeters, (flows, error) =>
        {
            if (error is not null)
                tcs.SetResult(new TrafficFlowResult(TrafficQueryError.NetworkError, null));
            else if (flows is not null)
                tcs.SetResult(new TrafficFlowResult(TrafficQueryError.None,
                    flows.Select(f => new TrafficFlow(f.JamFactor, f.SpeedInMetersPerSecond,
                        ToSharedPolyline(f.Location),
                        FreeFlowSpeedInMetersPerSecond: f.FreeFlowSpeedInMetersPerSecond)).ToList()));
            else
                tcs.SetResult(new TrafficFlowResult(TrafficQueryError.None, null));
        });

        return await tcs.Task;
    }

    public async Task<TrafficIncidentsResult> QueryIncidentsAsync(GeoCircle area, TrafficIncidentsQueryOptions options)
    {
        if (_engine is null) throw new InvalidOperationException("TrafficService not initialized.");
        var tcs = new TaskCompletionSource<TrafficIncidentsResult>();

        _engine.QueryIncidents(area.Center.Latitude, area.Center.Longitude, area.RadiusInMeters, (incidents, error) =>
        {
            if (error is not null)
                tcs.SetResult(new TrafficIncidentsResult(TrafficQueryError.NetworkError, null));
            else if (incidents is not null)
                tcs.SetResult(new TrafficIncidentsResult(TrafficQueryError.None,
                    incidents.Select(i => new TrafficIncident(i.Id, i.DescriptionText,
                        ToSharedIncidentType((int)i.TypeRawValue), ToSharedIncidentImpact((int)i.ImpactRawValue),
                        Location: FirstVertex(i.Location),
                        RoadClosed: i.IsRoadClosed)).ToList()));
            else
                tcs.SetResult(new TrafficIncidentsResult(TrafficQueryError.None, null));
        });

        return await tcs.Task;
    }

    public async Task<TrafficIncident?> LookupIncidentAsync(string incidentId, TrafficIncidentLookupOptions options)
    {
        // Will be expanded with lookup API
        return null;
    }

    /// <summary>
    /// Maps an iOS bridge <c>HereGeoPolyline</c> to a shared
    /// <see cref="GeoPolyline"/>. Returns an empty polyline if the
    /// bridge type is null (the underlying iOS TrafficFlow may have
    /// no location for some rows).
    /// </summary>
    private static GeoPolyline ToSharedPolyline(Here.Explore.iOS.HereGeoPolyline? polyline)
    {
        if (polyline?.Vertices is null || polyline.Vertices.Length == 0)
            return new GeoPolyline(new List<GeoCoordinates>());
        var vertices = polyline.Vertices
            .Select(v => new GeoCoordinates(v.Latitude, v.Longitude))
            .ToList();
        return new GeoPolyline(vertices);
    }

    // iOS TrafficIncidentType raw values (UInt32-backed enum, order differs
    // from the shared enum — a direct cast corrupts the values):
    // accident=0, congestion=1, construction=2, disabledVehicle=3, massTransit=4,
    // plannedEvent=5, roadHazard=6, weather=7, roadClosure=8, laneRestriction=9,
    // other=10, unknown=11.
    internal static TrafficIncidentType ToSharedIncidentType(int rawValue) => rawValue switch
    {
        0 => TrafficIncidentType.Accident,
        1 => TrafficIncidentType.Congestion,
        2 => TrafficIncidentType.Construction,
        3 => TrafficIncidentType.DisabledVehicle,
        4 => TrafficIncidentType.MassTransit,
        5 => TrafficIncidentType.PlannedEvent,
        6 => TrafficIncidentType.RoadHazard,
        7 => TrafficIncidentType.Weather,
        8 => TrafficIncidentType.RoadClosure,
        9 => TrafficIncidentType.LaneRestriction,
        10 => TrafficIncidentType.Miscellaneous,
        _ => TrafficIncidentType.Unknown,
    };

    // iOS TrafficIncidentImpact raw values: critical=0, major=1, minor=2, low=3,
    // unknown=4. Mapped consistently with the Android implementation
    // (Critical → Closed, Low → Minor).
    internal static TrafficIncidentImpact ToSharedIncidentImpact(int rawValue) => rawValue switch
    {
        0 => TrafficIncidentImpact.Closed,
        1 => TrafficIncidentImpact.Major,
        2 => TrafficIncidentImpact.Minor,
        3 => TrafficIncidentImpact.Minor,
        _ => TrafficIncidentImpact.Unknown,
    };

    /// <summary>
    /// Returns the first vertex of the bridge polyline as a
    /// <see cref="GeoCoordinates"/>, or null if the polyline is missing
    /// or empty. The RefApp uses this to place a marker on the map.
    /// </summary>
    private static GeoCoordinates? FirstVertex(Here.Explore.iOS.HereGeoPolyline? polyline)
    {
        if (polyline?.Vertices is null || polyline.Vertices.Length == 0)
            return null;
        var first = polyline.Vertices[0];
        return new GeoCoordinates(first.Latitude, first.Longitude);
    }
}
#endif