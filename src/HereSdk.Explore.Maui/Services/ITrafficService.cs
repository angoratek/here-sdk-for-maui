using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Traffic;

namespace Here.Explore.Maui.Services;

/// <summary>
/// Service for real-time traffic information.
/// </summary>
public interface ITrafficService : IHereSdkService
{
    Task<TrafficFlowResult> QueryFlowAsync(GeoCircle area, TrafficFlowQueryOptions options);
    Task<TrafficIncidentsResult> QueryIncidentsAsync(GeoCircle area, TrafficIncidentsQueryOptions options);
    Task<TrafficIncident?> LookupIncidentAsync(string incidentId, TrafficIncidentLookupOptions options);
}