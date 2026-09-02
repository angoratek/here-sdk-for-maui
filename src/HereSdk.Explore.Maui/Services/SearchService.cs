using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Search;

namespace Here.Explore.Maui.Services;

/// <summary>
/// Shared partial of SearchService — platform implementations are in SearchService.Android.cs and SearchService.iOS.cs.
/// </summary>
public partial class SearchService : ISearchService
{
    private bool _disposed;

#if !ANDROID && !IOS
    // Non-device stub: tracks initialization in-memory so unit/integration
    // tests can assert that DI factories call Initialize() correctly.
    private bool _stubInitialized;
    internal void Initialize() => _stubInitialized = true;
#endif

    /// <summary>
    /// Gets whether the service's native search engine has been created.
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
    public Task<SearchResult> SearchAsync(TextQuery query, SearchOptions options) =>
        throw new NotImplementedException("Platform-specific implementation required.");
    public Task<SearchResult> SearchAsync(CategoryQuery query, SearchOptions options) =>
        throw new NotImplementedException("Platform-specific implementation required.");
    public Task<SearchResult> SearchAsync(AddressQuery query, SearchOptions options) =>
        throw new NotImplementedException("Platform-specific implementation required.");
    public Task<SearchResult> SearchAsync(GeoCoordinates coordinates, SearchOptions options) =>
        throw new NotImplementedException("Platform-specific implementation required.");
    public Task<SuggestResult> SuggestAsync(TextQuery query, SearchOptions options) =>
        throw new NotImplementedException("Platform-specific implementation required.");
    public Task<Place?> GetPlaceByIdAsync(string placeId) =>
        throw new NotImplementedException("Platform-specific implementation required.");
#endif

    /// <inheritdoc />
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

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}