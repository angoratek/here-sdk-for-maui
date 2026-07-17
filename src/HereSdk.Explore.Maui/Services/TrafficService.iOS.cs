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
                        (TrafficIncidentType)i.TypeRawValue, (TrafficIncidentImpact)i.ImpactRawValue,
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