using Here.Explore.Maui.Models;

namespace Here.Explore.Maui.Models.Search;

/// <summary>
/// A text-based search query.
/// </summary>
public record TextQuery(string Query, GeoCoordinates? AreaCenter = null);

/// <summary>
/// A category-based search query.
/// </summary>
public record CategoryQuery(string CategoryId, GeoCoordinates? AreaCenter = null);

/// <summary>
/// Options for search queries.
/// </summary>
public record SearchOptions(int? MaxItems = null, SearchLanguage? Language = null);

/// <summary>
/// Result of a search query.
/// </summary>
public record SearchResult(SearchError Error, IReadOnlyList<Place>? Places);

/// <summary>
/// Result of an auto-suggest query.
/// </summary>
public record SuggestResult(SearchError Error, IReadOnlyList<Suggestion>? Suggestions);

/// <summary>
/// A place (POI) returned by search.
/// </summary>
public record Place(
    string Id,
    string Title,
    GeoCoordinates Coordinates,
    Address? Address = null,
    double? DistanceInMeters = null,
    string? Category = null
);

/// <summary>
/// A search suggestion (autocomplete).
/// </summary>
public record Suggestion(
    string Title,
    string Id,
    SuggestionType Type,
    bool IsCategory = false
);

/// <summary>
/// Address information.
/// </summary>
public record Address(
    string? Street = null,
    string? HouseNumber = null,
    string? City = null,
    string? District = null,
    string? State = null,
    string? CountryCode = null,
    string? CountryName = null,
    string? PostalCode = null
);

/// <summary>
/// Search error codes.
/// </summary>
public enum SearchError
{
    None,
    NetworkError,
    HttpError,
    NoResults,
    InvalidQuery,
    InsufficientMemory,
    EngineNotInitialized,
    PlaceNotFound,
    SerializationError,
    InvalidParameter
}

/// <summary>
/// Type of search suggestion.
/// </summary>
public enum SuggestionType
{
    Place,
    Category,
    Chain
}

/// <summary>
/// Language for search results.
/// </summary>
public enum SearchLanguage
{
    En,
    De,
    Fr,
    Es,
    It,
    Pt,
    Nl,
    Pl,
    Ru,
    Zh,
    Ja,
    Ko
}