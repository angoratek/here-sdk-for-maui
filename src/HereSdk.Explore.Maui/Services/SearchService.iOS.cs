#pragma warning disable CS1591
#if IOS
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Search;
using Here.Explore.iOS;

namespace Here.Explore.Maui.Services;

/// <summary>
/// iOS-specific SearchService implementation using NativeBridge wrappers.
/// Uses HereSearchEngine for text search, category search, suggest, and place-by-id.
/// Passes SearchOptions (maxItems, languageCode) through to the native engine.
/// </summary>
public partial class SearchService
{
    private HereSearchEngine? _engine;

    internal void Initialize()
    {
        _engine = new HereSearchEngine(0);
    }

    partial void DisposePlatform(bool disposing)
    {
        _engine?.Dispose();
        _engine = null;
    }

    public async Task<SearchResult> SearchAsync(TextQuery query, SearchOptions options)
    {
        if (_engine is null) throw new InvalidOperationException("SearchService not initialized.");
        var tcs = new TaskCompletionSource<SearchResult>();

        var latitude = query.AreaCenter?.Latitude ?? 0;
        var longitude = query.AreaCenter?.Longitude ?? 0;
        var maxItems = options.MaxItems ?? 0;
        var languageCode = options.Language.HasValue ? (nint)options.Language.Value : -1;

        _engine.SearchByText(query.Query, latitude, longitude, (int)maxItems, languageCode, (places, error) =>
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
        var maxItems = options.MaxItems ?? 0;
        var languageCode = options.Language.HasValue ? (nint)options.Language.Value : -1;

        _engine.SearchByCategory(query.CategoryId, latitude, longitude, (int)maxItems, languageCode, (places, error) =>
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

    public async Task<SearchResult> SearchAsync(AddressQuery query, SearchOptions options)
    {
        if (_engine is null) throw new InvalidOperationException("SearchService not initialized.");
        var tcs = new TaskCompletionSource<SearchResult>();

        var latitude = query.AreaCenter?.Latitude ?? 0;
        var longitude = query.AreaCenter?.Longitude ?? 0;
        var maxItems = options.MaxItems ?? 0;
        var languageCode = options.Language.HasValue ? (nint)options.Language.Value : -1;

        void OnResult(HerePlace[]? places, string? error)
        {
            if (error is not null)
                tcs.SetResult(new SearchResult(ToSharedSearchError(error), null));
            else if (places is not null)
                tcs.SetResult(new SearchResult(SearchError.None, places.Select(ToSharedPlace).ToList()));
            else
                tcs.SetResult(new SearchResult(SearchError.None, null));
        }

        // Null AreaCenter uses the no-area variant — no geographic bias.
        if (query.AreaCenter is null)
            _engine.SearchByAddressNoArea(query.Query, (int)maxItems, languageCode, OnResult);
        else
            _engine.SearchByAddress(query.Query, latitude, longitude, (int)maxItems, languageCode, OnResult);

        return await tcs.Task;
    }

    public async Task<SearchResult> SearchAsync(GeoCoordinates coordinates, SearchOptions options)
    {
        if (_engine is null) throw new InvalidOperationException("SearchService not initialized.");
        var tcs = new TaskCompletionSource<SearchResult>();

        var maxItems = options.MaxItems ?? 0;
        var languageCode = options.Language.HasValue ? (nint)options.Language.Value : -1;

        _engine.SearchByCoordinates(coordinates.Latitude, coordinates.Longitude, (int)maxItems, languageCode, (places, error) =>
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
        var maxItems = options.MaxItems ?? 0;
        var languageCode = options.Language.HasValue ? (nint)options.Language.Value : -1;

        _engine.Suggest(query.Query, latitude, longitude, (int)maxItems, languageCode, (suggestions, error) =>
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
        var address = iosPlace.Address is null
            ? null
            : new Address(
                iosPlace.Address.Street,
                iosPlace.Address.HouseNumber,
                iosPlace.Address.City,
                iosPlace.Address.District,
                iosPlace.Address.State,
                iosPlace.Address.CountryCode,
                iosPlace.Address.CountryName,
                iosPlace.Address.PostalCode);

        var categories = iosPlace.PrimaryCategories?
            .Where(c => c is not null)
            .Select(c => new PlaceCategory(c.Id, c.Name ?? string.Empty))
            .ToList();

        return new Place(
            iosPlace.Id,
            iosPlace.Title,
            new GeoCoordinates(iosPlace.Latitude, iosPlace.Longitude),
            address,
            PrimaryCategory: categories?.FirstOrDefault(),
            Categories: categories);
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
