#if ANDROID
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Traffic;
using Com.Here.Sdk.Traffic;

namespace Here.Explore.Maui.Services;

/// <summary>
/// Android-specific TrafficService implementation.
/// Note: Android SDK method names are queryForFlow/queryForIncidents (with "For").
/// Callback interfaces are TrafficFlowQueryCallback/TrafficIncidentsQueryCallback.
/// </summary>
public partial class TrafficService : ITrafficService
{
    private TrafficEngine? _engine;

    internal void Initialize()
    {
        if (Com.Here.Sdk.Core.Engine.SDKNativeEngine.Instance is not null)
        {
            _engine = new TrafficEngine();
        }
    }

    public async Task<TrafficFlowResult> QueryFlowAsync(GeoCircle area, TrafficFlowQueryOptions options)
    {
        if (_engine is null) throw new InvalidOperationException("TrafficService not initialized.");
        var tcs = new TaskCompletionSource<TrafficFlowResult>();

        var androidCircle = new Com.Here.Sdk.Core.GeoCircle(
            new Com.Here.Sdk.Core.GeoCoordinates(area.Center.Latitude, area.Center.Longitude),
            area.RadiusInMeters);

        var androidOptions = new TrafficFlowQueryOptions();
        _engine.QueryForFlow(androidCircle, androidOptions, new FlowQueryCallback(tcs));
        return await tcs.Task;
    }

    public async Task<TrafficIncidentsResult> QueryIncidentsAsync(GeoCircle area, TrafficIncidentsQueryOptions options)
    {
        if (_engine is null) throw new InvalidOperationException("TrafficService not initialized.");
        var tcs = new TaskCompletionSource<TrafficIncidentsResult>();

        var androidCircle = new Com.Here.Sdk.Core.GeoCircle(
            new Com.Here.Sdk.Core.GeoCoordinates(area.Center.Latitude, area.Center.Longitude),
            area.RadiusInMeters);

        var androidOptions = new TrafficIncidentsQueryOptions();
        _engine.QueryForIncidents(androidCircle, androidOptions, new IncidentsQueryCallback(tcs));
        return await tcs.Task;
    }

    public async Task<TrafficIncident?> LookupIncidentAsync(string incidentId, TrafficIncidentLookupOptions options)
    {
        if (_engine is null) throw new InvalidOperationException("TrafficService not initialized.");
        var tcs = new TaskCompletionSource<TrafficIncident?>();

        var androidLookupOptions = new TrafficIncidentLookupOptions();
        _engine.LookupIncident(incidentId, androidLookupOptions, new IncidentLookupCallback(tcs));
        return await tcs.Task;
    }
}

internal class FlowQueryCallback : Java.Lang.Object, TrafficEngine.ITrafficFlowQueryCallback
{
    private readonly TaskCompletionSource<TrafficFlowResult> _tcs;
    public FlowQueryCallback(TaskCompletionSource<TrafficFlowResult> tcs) => _tcs = tcs;

    public void OnTrafficFlowFetched(TrafficQueryError? error, IList<Com.Here.Sdk.Traffic.TrafficFlow>? flows)
    {
        if (error is not null && error.Value != TrafficQueryError.None)
            _tcs.SetResult(new TrafficFlowResult(TrafficQueryError.NetworkError, null));
        else if (flows is not null)
            _tcs.SetResult(new TrafficFlowResult(TrafficQueryError.None,
                flows.Select(f => new TrafficFlow(f.JamFactor, f.SpeedInMetersPerSecond ?? 0,
                    new GeoPolyline(new List<GeoCoordinates>()),
                    FreeFlowSpeedInMetersPerSecond: f.FreeFlowSpeedInMetersPerSecond)).ToList()));
        else
            _tcs.SetResult(new TrafficFlowResult(TrafficQueryError.None, null));
    }
}

internal class IncidentsQueryCallback : Java.Lang.Object, TrafficEngine.ITrafficIncidentsQueryCallback
{
    private readonly TaskCompletionSource<TrafficIncidentsResult> _tcs;
    public IncidentsQueryCallback(TaskCompletionSource<TrafficIncidentsResult> tcs) => _tcs = tcs;

    public void OnTrafficIncidentsFetched(TrafficQueryError? error, IList<Com.Here.Sdk.Traffic.TrafficIncident>? incidents)
    {
        if (error is not null && error.Value != TrafficQueryError.None)
            _tcs.SetResult(new TrafficIncidentsResult(TrafficQueryError.NetworkError, null));
        else if (incidents is not null)
            _tcs.SetResult(new TrafficIncidentsResult(TrafficQueryError.None,
                incidents.Select(i => new TrafficIncident(i.Id, i.Description.Text,
                    (TrafficIncidentType)(int)i.Type, (TrafficIncidentImpact)(int)i.Impact,
                    RoadClosed: i.IsRoadClosed)).ToList()));
        else
            _tcs.SetResult(new TrafficIncidentsResult(TrafficQueryError.None, null));
    }
}

internal class IncidentLookupCallback : Java.Lang.Object, TrafficEngine.ITrafficIncidentLookupCallback
{
    private readonly TaskCompletionSource<TrafficIncident?> _tcs;
    public IncidentLookupCallback(TaskCompletionSource<TrafficIncident?> tcs) => _tcs = tcs;

    public void OnTrafficIncidentLookupCompleted(TrafficQueryError? error, Com.Here.Sdk.Traffic.TrafficIncident? incident)
    {
        if (error is not null && error.Value != TrafficQueryError.None)
            _tcs.SetResult(null);
        else if (incident is not null)
            _tcs.SetResult(new TrafficIncident(incident.Id, incident.Description.Text,
                (TrafficIncidentType)(int)incident.Type, (TrafficIncidentImpact)(int)incident.Impact));
        else
            _tcs.SetResult(null);
    }
}
#endif