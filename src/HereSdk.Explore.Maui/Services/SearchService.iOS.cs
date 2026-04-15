#if IOS
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Search;
using Here.Explore.iOS;

namespace Here.Explore.Maui.Services;

/// <summary>
/// iOS-specific SearchService implementation using NativeBridge wrappers.
/// Uses HereSearchEngine for text search, category search, suggest, and place-by-id.
/// </summary>
public partial class SearchService
{
    private HereSearchEngine? _engine;

    internal void Initialize()
    {
        _engine = new HereSearchEngine(0);
    }

    public async Task<SearchResult> SearchAsync(TextQuery query, SearchOptions options)
    {
        if (_engine is null) throw new InvalidOperationException("SearchService not initialized.");
        var tcs = new TaskCompletionSource<SearchResult>();

        var latitude = query.AreaCenter?.Latitude ?? 0;
        var longitude = query.AreaCenter?.Longitude ?? 0;

        _engine.SearchByText(query.Query, latitude, longitude, (places, error) =>
        {
            if (error is not null)
                tcs.SetResult(new SearchResult(ToSharedSearchError(error), null));
            else if (places is not null)
                tcs.SetResult(new SearchResult(SearchError.None, places.Select(ToSharedPlace).ToList()));
            else
                tcs.SetResult(new SearchResult(SearchError.None, null));
        });

        return await tcs.Task;
    }

    public async Task<SearchResult> SearchAsync(CategoryQuery query, SearchOptions options)
    {
        if (_engine is null) throw new InvalidOperationException("SearchService not initialized.");
        var tcs = new TaskCompletionSource<SearchResult>();

        var latitude = query.AreaCenter?.Latitude ?? 0;
        var longitude = query.AreaCenter?.Longitude ?? 0;

        _engine.SearchByCategory(query.CategoryId, latitude, longitude, (places, error) =>
        {
            if (error is not null)
                tcs.SetResult(new SearchResult(ToSharedSearchError(error), null));
            else if (places is not null)
                tcs.SetResult(new SearchResult(SearchError.None, places.Select(ToSharedPlace).ToList()));
            else
                tcs.SetResult(new SearchResult(SearchError.None, null));
        });

        return await tcs.Task;
    }

    public async Task<SuggestResult> SuggestAsync(TextQuery query, SearchOptions options)
    {
        if (_engine is null) throw new InvalidOperationException("SearchService not initialized.");
        var tcs = new TaskCompletionSource<SuggestResult>();

        var latitude = query.AreaCenter?.Latitude ?? 0;
        var longitude = query.AreaCenter?.Longitude ?? 0;

        _engine.Suggest(query.Query, latitude, longitude, (suggestions, error) =>
        {
            if (error is not null)
                tcs.SetResult(new SuggestResult(ToSharedSearchError(error), null));
            else if (suggestions is not null)
                tcs.SetResult(new SuggestResult(SearchError.None, suggestions.Select(ToSharedSuggestion).ToList()));
            else
                tcs.SetResult(new SuggestResult(SearchError.None, null));
        });

        return await tcs.Task;
    }

    public async Task<Place?> GetPlaceByIdAsync(string placeId)
    {
        if (_engine is null) throw new InvalidOperationException("SearchService not initialized.");
        var tcs = new TaskCompletionSource<Place?>();

        _engine.SearchByPlaceId(placeId, (place, error) =>
        {
            if (error is not null || place is null)
                tcs.SetResult(null);
            else
                tcs.SetResult(ToSharedPlace(place));
        });

        return await tcs.Task;
    }

    private static SearchError ToSharedSearchError(string? error)
    {
        if (string.IsNullOrEmpty(error)) return SearchError.None;
        if (error.Contains("NoResults", StringComparison.OrdinalIgnoreCase)) return SearchError.NoResults;
        if (error.Contains("BadRequest", StringComparison.OrdinalIgnoreCase)) return SearchError.InvalidQuery;
        if (error.Contains("HttpError", StringComparison.OrdinalIgnoreCase)) return SearchError.HttpError;
        return SearchError.NetworkError;
    }

    private static Place ToSharedPlace(HerePlace iosPlace)
    {
        return new Place(
            iosPlace.Id,
            iosPlace.Title,
            new GeoCoordinates(iosPlace.Latitude, iosPlace.Longitude));
    }

    private static Suggestion ToSharedSuggestion(HereSuggestion iosSuggestion)
    {
        var type = iosSuggestion.IsPlace ? SuggestionType.Place : SuggestionType.Category;
        return new Suggestion(
            iosSuggestion.Title,
            iosSuggestion.Id,
            type,
            type == SuggestionType.Category);
    }
}
#endif