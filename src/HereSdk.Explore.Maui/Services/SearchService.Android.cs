#pragma warning disable CS1591
#if ANDROID
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Search;

namespace Here.Explore.Maui.Services;

/// <summary>
/// Android-specific SearchService implementation using HERE SDK Android bindings.
/// Uses SearchEngine.SearchByText() (SearchCompletedHandler) for text search,
/// SearchByCategory() for category search, and SuggestByText() (SuggestCompletedHandler) for auto-suggest.
/// Java enum comparisons use if/else if since Java.Lang.Enum can't be used in C# switch.
/// </summary>
public partial class SearchService
{
    private Here.Explore.Search.SearchEngine? _engine;

    internal void Initialize()
    {
        if (Here.Explore.Core.Engine.SDKNativeEngine.SharedInstance is not null)
        {
            _engine = new Here.Explore.Search.SearchEngine();
        }
    }

    public async Task<SearchResult> SearchAsync(TextQuery query, SearchOptions options)
    {
        if (_engine is null) throw new InvalidOperationException("SearchService not initialized.");
        var tcs = new TaskCompletionSource<SearchResult>();

        var androidArea = query.AreaCenter is not null
            ? new Here.Explore.Search.TextQuery.Area(
                new Here.Explore.Core.GeoCoordinates(query.AreaCenter.Latitude, query.AreaCenter.Longitude))
            : new Here.Explore.Search.TextQuery.Area(
                new Here.Explore.Core.GeoCoordinates(0, 0));

        var androidQuery = new Here.Explore.Search.TextQuery(query.Query, androidArea);
        var androidOptions = ToAndroidSearchOptions(options);
        _engine.SearchByText(androidQuery, androidOptions, new SearchCallback(tcs));
        return await tcs.Task;
    }

    public async Task<SearchResult> SearchAsync(CategoryQuery query, SearchOptions options)
    {
        if (_engine is null) throw new InvalidOperationException("SearchService not initialized.");
        var tcs = new TaskCompletionSource<SearchResult>();

        var category = new Here.Explore.Search.PlaceCategory(query.CategoryId);
        var area = query.AreaCenter is not null
            ? new Here.Explore.Search.CategoryQuery.Area(
                new Here.Explore.Core.GeoCoordinates(query.AreaCenter.Latitude, query.AreaCenter.Longitude))
            : new Here.Explore.Search.CategoryQuery.Area(
                new Here.Explore.Core.GeoCoordinates(0, 0));

        var androidQuery = new Here.Explore.Search.CategoryQuery(category, area);
        var androidOptions = ToAndroidSearchOptions(options);
        _engine.SearchByCategory(androidQuery, androidOptions, new SearchCallback(tcs));
        return await tcs.Task;
    }

    public async Task<SearchResult> SearchAsync(AddressQuery query, SearchOptions options)
    {
        if (_engine is null) throw new InvalidOperationException("SearchService not initialized.");
        var tcs = new TaskCompletionSource<SearchResult>();

        var androidArea = query.AreaCenter is not null
            ? new Here.Explore.Core.GeoCoordinates(query.AreaCenter.Latitude, query.AreaCenter.Longitude)
            : new Here.Explore.Core.GeoCoordinates(0, 0);

        var androidQuery = new Here.Explore.Search.AddressQuery(query.Query, androidArea);
        var androidOptions = ToAndroidSearchOptions(options);
        _engine.SearchByAddress(androidQuery, androidOptions, new SearchCallback(tcs));
        return await tcs.Task;
    }

    public async Task<SearchResult> SearchAsync(GeoCoordinates coordinates, SearchOptions options)
    {
        if (_engine is null) throw new InvalidOperationException("SearchService not initialized.");
        var tcs = new TaskCompletionSource<SearchResult>();

        var androidCoords = new Here.Explore.Core.GeoCoordinates(coordinates.Latitude, coordinates.Longitude);
        var androidOptions = ToAndroidSearchOptions(options);
        _engine.SearchByCoordinates(androidCoords, androidOptions, new SearchCallback(tcs));
        return await tcs.Task;
    }

    public async Task<SuggestResult> SuggestAsync(TextQuery query, SearchOptions options)
    {
        if (_engine is null) throw new InvalidOperationException("SearchService not initialized.");
        var tcs = new TaskCompletionSource<SuggestResult>();

        var androidArea = query.AreaCenter is not null
            ? new Here.Explore.Search.TextQuery.Area(
                new Here.Explore.Core.GeoCoordinates(query.AreaCenter.Latitude, query.AreaCenter.Longitude))
            : new Here.Explore.Search.TextQuery.Area(
                new Here.Explore.Core.GeoCoordinates(0, 0));

        var androidQuery = new Here.Explore.Search.TextQuery(query.Query, androidArea);
        var androidOptions = ToAndroidSearchOptions(options);
        _engine.SuggestByText(androidQuery, androidOptions, new SuggestCallback(tcs));
        return await tcs.Task;
    }

    public async Task<Place?> GetPlaceByIdAsync(string placeId)
    {
        if (_engine is null) throw new InvalidOperationException("SearchService not initialized.");
        var tcs = new TaskCompletionSource<Place?>();

        var placeIdQuery = new Here.Explore.Search.PlaceIdQuery(placeId);
        _engine.SearchByPlaceId(placeIdQuery, null, new PlaceIdCallback(tcs));
        return await tcs.Task;
    }

    private static Here.Explore.Search.SearchOptions ToAndroidSearchOptions(SearchOptions options)
    {
        var androidOptions = new Here.Explore.Search.SearchOptions();
        if (options.MaxItems.HasValue)
            androidOptions.MaxItems = (Java.Lang.Integer)options.MaxItems.Value;
        if (options.Language.HasValue)
            androidOptions.LanguageCode = ToAndroidLanguageCode(options.Language.Value);
        return androidOptions;
    }

    private static Here.Explore.Core.LanguageCode ToAndroidLanguageCode(SearchLanguage language)
    {
        if (language == SearchLanguage.De) return Here.Explore.Core.LanguageCode.DeDe!;
        if (language == SearchLanguage.Fr) return Here.Explore.Core.LanguageCode.FrFr!;
        if (language == SearchLanguage.Es) return Here.Explore.Core.LanguageCode.EsEs!;
        if (language == SearchLanguage.It) return Here.Explore.Core.LanguageCode.ItIt!;
        if (language == SearchLanguage.Pt) return Here.Explore.Core.LanguageCode.PtBr!;
        if (language == SearchLanguage.Nl) return Here.Explore.Core.LanguageCode.NlNl!;
        if (language == SearchLanguage.Pl) return Here.Explore.Core.LanguageCode.PlPl!;
        if (language == SearchLanguage.Ru) return Here.Explore.Core.LanguageCode.RuRu!;
        if (language == SearchLanguage.Zh) return Here.Explore.Core.LanguageCode.ZhCn!;
        if (language == SearchLanguage.Ja) return Here.Explore.Core.LanguageCode.JaJp!;
        if (language == SearchLanguage.Ko) return Here.Explore.Core.LanguageCode.KoKr!;
        return Here.Explore.Core.LanguageCode.EnUs!;
    }

    internal static SearchError ToSharedSearchError(Here.Explore.Search.SearchError? error)
    {
        if (error is null) return SearchError.None;
        if (error.Equals(Here.Explore.Search.SearchError.NoResultsFound)) return SearchError.NoResults;
        if (error.Equals(Here.Explore.Search.SearchError.BadRequest)) return SearchError.InvalidQuery;
        if (error.Equals(Here.Explore.Search.SearchError.QueryEmpty)) return SearchError.InvalidQuery;
        if (error.Equals(Here.Explore.Search.SearchError.QueryTooLong)) return SearchError.InvalidQuery;
        if (error.Equals(Here.Explore.Search.SearchError.FilterEmpty)) return SearchError.InvalidQuery;
        if (error.Equals(Here.Explore.Search.SearchError.HttpError)) return SearchError.HttpError;
        if (error.Equals(Here.Explore.Search.SearchError.ParsingError)) return SearchError.SerializationError;
        if (error.Equals(Here.Explore.Search.SearchError.InvalidCustomOptionFormat)) return SearchError.InvalidParameter;
        return SearchError.NetworkError;
    }

    internal static Place ToSharedPlace(Here.Explore.Search.Place androidPlace)
    {
        var coords = androidPlace.GeoCoordinates is not null
            ? new GeoCoordinates(androidPlace.GeoCoordinates.Latitude, androidPlace.GeoCoordinates.Longitude)
            : new GeoCoordinates(0, 0);

        var address = androidPlace.Address is not null
            ? new Address(
                androidPlace.Address.Street,
                androidPlace.Address.HouseNumOrName,
                androidPlace.Address.City,
                androidPlace.Address.District,
                androidPlace.Address.State,
                androidPlace.Address.CountryCode,
                androidPlace.Address.Country,
                androidPlace.Address.PostalCode)
            : null;

        double? distance = null;
        if (androidPlace.DistanceInMeters is not null)
            distance = (int)androidPlace.DistanceInMeters;

        return new Place(androidPlace.Id, androidPlace.Title, coords, address, distance);
    }

    internal static Suggestion ToSharedSuggestion(Here.Explore.Search.Suggestion androidSuggestion)
    {
        var type = SuggestionType.Place;
        if (androidSuggestion.Type is not null &&
            androidSuggestion.Type.Equals(Here.Explore.Search.SuggestionType.Category))
            type = SuggestionType.Category;
        else if (androidSuggestion.Type is not null &&
            androidSuggestion.Type.Equals(Here.Explore.Search.SuggestionType.Chain))
            type = SuggestionType.Chain;

        return new Suggestion(
            androidSuggestion.Title,
            androidSuggestion.Id ?? string.Empty,
            type,
            type == SuggestionType.Category);
    }
}

internal class SearchCallback : Java.Lang.Object, Here.Explore.Search.SearchCompletedHandler
{
    private readonly TaskCompletionSource<SearchResult> _tcs;
    public SearchCallback(TaskCompletionSource<SearchResult> tcs) => _tcs = tcs;

    public void OnSearchCompleted(Here.Explore.Search.SearchError? error, System.Collections.Generic.IList<Here.Explore.Search.Place>? places)
    {
        if (error is not null)
            _tcs.SetResult(new SearchResult(SearchService.ToSharedSearchError(error), null));
        else if (places is not null)
            _tcs.SetResult(new SearchResult(SearchError.None, places.Select(SearchService.ToSharedPlace).ToList()));
        else
            _tcs.SetResult(new SearchResult(SearchError.None, null));
    }
}

internal class SuggestCallback : Java.Lang.Object, Here.Explore.Search.SuggestCompletedHandler
{
    private readonly TaskCompletionSource<SuggestResult> _tcs;
    public SuggestCallback(TaskCompletionSource<SuggestResult> tcs) => _tcs = tcs;

    public void OnSuggestCompleted(Here.Explore.Search.SearchError? error, System.Collections.Generic.IList<Here.Explore.Search.Suggestion>? suggestions)
    {
        if (error is not null)
            _tcs.SetResult(new SuggestResult(SearchService.ToSharedSearchError(error), null));
        else if (suggestions is not null)
            _tcs.SetResult(new SuggestResult(SearchError.None,
                suggestions.Select(SearchService.ToSharedSuggestion).ToList()));
        else
            _tcs.SetResult(new SuggestResult(SearchError.None, null));
    }
}

internal class PlaceIdCallback : Java.Lang.Object, Here.Explore.Search.IPlaceIdSearchCallback
{
    private readonly TaskCompletionSource<Place?> _tcs;
    public PlaceIdCallback(TaskCompletionSource<Place?> tcs) => _tcs = tcs;

    public void OnPlaceIdSearchCompleted(Here.Explore.Search.SearchError? error, Here.Explore.Search.Place? place)
    {
        if (error is not null || place is null)
            _tcs.SetResult(null);
        else
            _tcs.SetResult(SearchService.ToSharedPlace(place));
    }
}
#endif