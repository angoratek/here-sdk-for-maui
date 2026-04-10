#if IOS
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Traffic;

namespace Here.Explore.Maui.Services;

/// <summary>
/// iOS-specific TrafficService implementation using NativeBridge.
/// </summary>
public partial class TrafficService : ITrafficService
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
                        new GeoPolyline(new List<GeoCoordinates>()),
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
}
#endif