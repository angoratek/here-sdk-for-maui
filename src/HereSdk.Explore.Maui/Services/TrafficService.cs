using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Traffic;

namespace Here.Explore.Maui.Services;

/// <summary>
/// Shared partial of TrafficService — platform implementations are in TrafficService.Android.cs and TrafficService.iOS.cs.
/// Member definitions are provided by platform-specific partials when building for a platform,
/// and by stubs here when building for non-platform targets (e.g., unit tests).
/// </summary>
public partial class TrafficService : ITrafficService
{
    private bool _disposed;

#if !ANDROID && !IOS
    // Non-platform stub implementations for unit-test context
    public Task<TrafficFlowResult> QueryFlowAsync(GeoCircle area, TrafficFlowQueryOptions options) =>
        throw new NotImplementedException("Platform-specific implementation required.");
    public Task<TrafficIncidentsResult> QueryIncidentsAsync(GeoCircle area, TrafficIncidentsQueryOptions options) =>
        throw new NotImplementedException("Platform-specific implementation required.");
    public Task<TrafficIncident?> LookupIncidentAsync(string incidentId, TrafficIncidentLookupOptions options) =>
        throw new NotImplementedException("Platform-specific implementation required.");
#endif

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