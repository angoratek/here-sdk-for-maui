using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Traffic;

namespace Here.Explore.Maui.Services;

/// <summary>
/// Shared partial of TrafficService — platform implementations are in TrafficService.Android.cs and TrafficService.iOS.cs.
/// </summary>
public partial class TrafficService : ITrafficService
{
    private bool _disposed;

    public Task<TrafficFlowResult> QueryFlowAsync(GeoCircle area, TrafficFlowQueryOptions options) =>
        throw new NotImplementedException("Platform-specific implementation required.");
    public Task<TrafficIncidentsResult> QueryIncidentsAsync(GeoCircle area, TrafficIncidentsQueryOptions options) =>
        throw new NotImplementedException("Platform-specific implementation required.");
    public Task<TrafficIncident?> LookupIncidentAsync(string incidentId, TrafficIncidentLookupOptions options) =>
        throw new NotImplementedException("Platform-specific implementation required.");

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Platform implementations will dispose native engine
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}