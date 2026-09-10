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
    // Non-device stub: tracks initialization in-memory so unit/integration
    // tests can assert that DI factories call Initialize() correctly.
    private bool _stubInitialized;
    internal void Initialize() => _stubInitialized = true;
#endif

    /// <summary>
    /// Gets whether the service's native traffic engine has been created.
    /// False until <see cref="Initialize"/> runs. Used by integration tests
    /// to confirm the DI factory wired initialization correctly.
    /// </summary>
    public bool IsInitialized =>
#if ANDROID || IOS
        _engine is not null;
#else
        _stubInitialized;
#endif

#if !ANDROID && !IOS
    // Non-platform stub implementations for unit-test context
    public Task<TrafficFlowResult> QueryFlowAsync(GeoCircle area, TrafficFlowQueryOptions options) =>
        throw new NotImplementedException("Platform-specific implementation required.");
    public Task<TrafficIncidentsResult> QueryIncidentsAsync(GeoCircle area, TrafficIncidentsQueryOptions options) =>
        throw new NotImplementedException("Platform-specific implementation required.");
    public Task<TrafficIncident?> LookupIncidentAsync(string incidentId, TrafficIncidentLookupOptions options) =>
        throw new NotImplementedException("Platform-specific implementation required.");
#endif

    /// <inheritdoc />
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                DisposePlatform(disposing);
            }
            _disposed = true;
        }
    }

    /// <summary>Platform hook that disposes the native engine(s).</summary>
    // Implemented by the platform partials; elided when no platform part
    // exists (unit-test builds).
    partial void DisposePlatform(bool disposing);

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}