#if ANDROID
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Traffic;

namespace Here.Explore.Maui.Services;

/// <summary>
/// Android-specific TrafficService implementation.
/// Binding callback interfaces are renamed: TrafficFlowQueryHandler, TrafficIncidentsQueryHandler,
/// TrafficIncidentLookupHandler (Metadata.xml renames Callback → Handler).
/// </summary>
public partial class TrafficService
{
    private Here.Explore.Traffic.TrafficEngine? _engine;

    internal void Initialize()
    {
        if (Here.Explore.Core.Engine.SDKNativeEngine.SharedInstance is not null)
        {
            _engine = new Here.Explore.Traffic.TrafficEngine();
        }
    }

    public async Task<TrafficFlowResult> QueryFlowAsync(GeoCircle area, TrafficFlowQueryOptions options)
    {
        if (_engine is null) throw new InvalidOperationException("TrafficService not initialized.");
        var tcs = new TaskCompletionSource<TrafficFlowResult>();

        var androidCircle = new Here.Explore.Core.GeoCircle(
            new Here.Explore.Core.GeoCoordinates(area.Center.Latitude, area.Center.Longitude),
            area.RadiusInMeters);

        var androidOptions = new Here.Explore.Traffic.TrafficFlowQueryOptions();
        _engine.QueryForFlow(androidCircle, androidOptions, new FlowQueryCallback(tcs));
        return await tcs.Task;
    }

    public async Task<TrafficIncidentsResult> QueryIncidentsAsync(GeoCircle area, TrafficIncidentsQueryOptions options)
    {
        if (_engine is null) throw new InvalidOperationException("TrafficService not initialized.");
        var tcs = new TaskCompletionSource<TrafficIncidentsResult>();

        var androidCircle = new Here.Explore.Core.GeoCircle(
            new Here.Explore.Core.GeoCoordinates(area.Center.Latitude, area.Center.Longitude),
            area.RadiusInMeters);

        var androidOptions = new Here.Explore.Traffic.TrafficIncidentsQueryOptions();
        _engine.QueryForIncidents(androidCircle, androidOptions, new IncidentsQueryCallback(tcs));
        return await tcs.Task;
    }

    public async Task<TrafficIncident?> LookupIncidentAsync(string incidentId, TrafficIncidentLookupOptions options)
    {
        if (_engine is null) throw new InvalidOperationException("TrafficService not initialized.");
        var tcs = new TaskCompletionSource<TrafficIncident?>();

        var androidLookupOptions = new Here.Explore.Traffic.TrafficIncidentLookupOptions();
        _engine.LookupIncident(incidentId, androidLookupOptions, new IncidentLookupCallback(tcs));
        return await tcs.Task;
    }

    internal static TrafficIncidentType ToSharedIncidentType(Here.Explore.Traffic.TrafficIncidentType? type)
    {
        if (type is null) return TrafficIncidentType.Unknown;
        if (type.Equals(Here.Explore.Traffic.TrafficIncidentType.Accident)) return TrafficIncidentType.Accident;
        if (type.Equals(Here.Explore.Traffic.TrafficIncidentType.Congestion)) return TrafficIncidentType.Congestion;
        if (type.Equals(Here.Explore.Traffic.TrafficIncidentType.Construction)) return TrafficIncidentType.Construction;
        if (type.Equals(Here.Explore.Traffic.TrafficIncidentType.DisabledVehicle)) return TrafficIncidentType.DisabledVehicle;
        if (type.Equals(Here.Explore.Traffic.TrafficIncidentType.LaneRestriction)) return TrafficIncidentType.LaneRestriction;
        if (type.Equals(Here.Explore.Traffic.TrafficIncidentType.MassTransit)) return TrafficIncidentType.MassTransit;
        if (type.Equals(Here.Explore.Traffic.TrafficIncidentType.PlannedEvent)) return TrafficIncidentType.PlannedEvent;
        if (type.Equals(Here.Explore.Traffic.TrafficIncidentType.RoadClosure)) return TrafficIncidentType.RoadClosure;
        if (type.Equals(Here.Explore.Traffic.TrafficIncidentType.RoadHazard)) return TrafficIncidentType.RoadHazard;
        if (type.Equals(Here.Explore.Traffic.TrafficIncidentType.Weather)) return TrafficIncidentType.Weather;
        if (type.Equals(Here.Explore.Traffic.TrafficIncidentType.Other)) return TrafficIncidentType.Miscellaneous;
        return TrafficIncidentType.Unknown;
    }

    internal static TrafficIncidentImpact ToSharedIncidentImpact(Here.Explore.Traffic.TrafficIncidentImpact? impact)
    {
        if (impact is null) return TrafficIncidentImpact.Unknown;
        if (impact.Equals(Here.Explore.Traffic.TrafficIncidentImpact.Minor)) return TrafficIncidentImpact.Minor;
        if (impact.Equals(Here.Explore.Traffic.TrafficIncidentImpact.Low)) return TrafficIncidentImpact.Minor;
        if (impact.Equals(Here.Explore.Traffic.TrafficIncidentImpact.Major)) return TrafficIncidentImpact.Major;
        if (impact.Equals(Here.Explore.Traffic.TrafficIncidentImpact.Critical)) return TrafficIncidentImpact.Closed;
        return TrafficIncidentImpact.Unknown;
    }

    internal static TrafficQueryError ToSharedQueryError(Here.Explore.Traffic.TrafficQueryError? error)
    {
        if (error is null) return TrafficQueryError.None;
        if (error.Equals(Here.Explore.Traffic.TrafficQueryError.HttpError)) return TrafficQueryError.HttpError;
        if (error.Equals(Here.Explore.Traffic.TrafficQueryError.Offline)) return TrafficQueryError.NetworkError;
        if (error.Equals(Here.Explore.Traffic.TrafficQueryError.ServerUnreachable)) return TrafficQueryError.NetworkError;
        if (error.Equals(Here.Explore.Traffic.TrafficQueryError.TimedOut)) return TrafficQueryError.NetworkError;
        if (error.Equals(Here.Explore.Traffic.TrafficQueryError.BadRequest)) return TrafficQueryError.InvalidQuery;
        if (error.Equals(Here.Explore.Traffic.TrafficQueryError.IncidentIdNotFound)) return TrafficQueryError.NoResults;
        return TrafficQueryError.NetworkError;
    }
}

internal class FlowQueryCallback : Java.Lang.Object, Here.Explore.Traffic.TrafficFlowQueryHandler
{
    private readonly TaskCompletionSource<TrafficFlowResult> _tcs;
    public FlowQueryCallback(TaskCompletionSource<TrafficFlowResult> tcs) => _tcs = tcs;

    public void OnTrafficFlowFetched(Here.Explore.Traffic.TrafficQueryError? error, System.Collections.Generic.IList<Here.Explore.Traffic.TrafficFlowData>? flows)
    {
        if (error is not null)
            _tcs.SetResult(new TrafficFlowResult(TrafficService.ToSharedQueryError(error), null));
        else if (flows is not null)
            _tcs.SetResult(new TrafficFlowResult(TrafficQueryError.None,
                flows.Select(f => new TrafficFlow(f.JamFactor, (double)(f.SpeedInMetersPerSecond ?? (Java.Lang.Double)0.0),
                    new GeoPolyline(new List<GeoCoordinates>()),
                    FreeFlowSpeedInMetersPerSecond: f.FreeFlowSpeedInMetersPerSecond)).ToList()));
        else
            _tcs.SetResult(new TrafficFlowResult(TrafficQueryError.None, null));
    }
}

internal class IncidentsQueryCallback : Java.Lang.Object, Here.Explore.Traffic.TrafficIncidentsQueryHandler
{
    private readonly TaskCompletionSource<TrafficIncidentsResult> _tcs;
    public IncidentsQueryCallback(TaskCompletionSource<TrafficIncidentsResult> tcs) => _tcs = tcs;

    public void OnTrafficIncidentsFetched(Here.Explore.Traffic.TrafficQueryError? error, System.Collections.Generic.IList<Here.Explore.Traffic.TrafficIncident>? incidents)
    {
        if (error is not null)
            _tcs.SetResult(new TrafficIncidentsResult(ToSharedQueryError(error), null));
        else if (incidents is not null)
            _tcs.SetResult(new TrafficIncidentsResult(TrafficQueryError.None,
                incidents.Select(i => new TrafficIncident(i.Id, i.Description.Text,
                    TrafficService.ToSharedIncidentType(i.Type),
                    TrafficService.ToSharedIncidentImpact(i.Impact),
                    RoadClosed: i.IsRoadClosed)).ToList()));
        else
            _tcs.SetResult(new TrafficIncidentsResult(TrafficQueryError.None, null));
    }

    private static TrafficQueryError ToSharedQueryError(Here.Explore.Traffic.TrafficQueryError error)
    {
        if (error.Equals(Here.Explore.Traffic.TrafficQueryError.HttpError)) return TrafficQueryError.HttpError;
        if (error.Equals(Here.Explore.Traffic.TrafficQueryError.IncidentIdNotFound)) return TrafficQueryError.NoResults;
        if (error.Equals(Here.Explore.Traffic.TrafficQueryError.BadRequest)) return TrafficQueryError.InvalidQuery;
        return TrafficQueryError.NetworkError;
    }
}

internal class IncidentLookupCallback : Java.Lang.Object, Here.Explore.Traffic.TrafficIncidentLookupHandler
{
    private readonly TaskCompletionSource<TrafficIncident?> _tcs;
    public IncidentLookupCallback(TaskCompletionSource<TrafficIncident?> tcs) => _tcs = tcs;

    public void OnTrafficIncidentFetched(Here.Explore.Traffic.TrafficQueryError? error, Here.Explore.Traffic.TrafficIncident? incident)
    {
        if (error is not null || incident is null)
            _tcs.SetResult(null);
        else
            _tcs.SetResult(new TrafficIncident(incident.Id, incident.Description.Text,
                TrafficService.ToSharedIncidentType(incident.Type),
                TrafficService.ToSharedIncidentImpact(incident.Impact)));
    }
}
#endif