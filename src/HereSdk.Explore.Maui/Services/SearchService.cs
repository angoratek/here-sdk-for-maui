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
    // Non-platform stub implementations for unit-test context
    public Task<SearchResult> SearchAsync(TextQuery query, SearchOptions options) =>
        throw new NotImplementedException("Platform-specific implementation required.");
    public Task<SearchResult> SearchAsync(CategoryQuery query, SearchOptions options) =>
        throw new NotImplementedException("Platform-specific implementation required.");
    public Task<SuggestResult> SuggestAsync(TextQuery query, SearchOptions options) =>
        throw new NotImplementedException("Platform-specific implementation required.");
    public Task<Place?> GetPlaceByIdAsync(string placeId) =>
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