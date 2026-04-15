using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Traffic;

namespace Here.Explore.Maui.Services;

/// <summary>
/// Service for real-time traffic information.
/// </summary>
public interface ITrafficService : IHereSdkService
{
    /// <summary>Queries real-time traffic flow data for a geographic area.</summary>
    /// <param name="area">The geographic circle defining the query area.</param>
    /// <param name="options">Flow query options.</param>
    /// <returns>The flow result containing speed and jam factor data, or an error.</returns>
    Task<TrafficFlowResult> QueryFlowAsync(GeoCircle area, TrafficFlowQueryOptions options);

    /// <summary>Queries traffic incidents for a geographic area.</summary>
    /// <param name="area">The geographic circle defining the query area.</param>
    /// <param name="options">Incidents query options.</param>
    /// <returns>The incidents result containing active incidents, or an error.</returns>
    Task<TrafficIncidentsResult> QueryIncidentsAsync(GeoCircle area, TrafficIncidentsQueryOptions options);

    /// <summary>Looks up a specific traffic incident by its ID.</summary>
    /// <param name="incidentId">The incident identifier from a previous query.</param>
    /// <param name="options">Lookup options.</param>
    /// <returns>The incident, or null if not found.</returns>
    Task<TrafficIncident?> LookupIncidentAsync(string incidentId, TrafficIncidentLookupOptions options);
}