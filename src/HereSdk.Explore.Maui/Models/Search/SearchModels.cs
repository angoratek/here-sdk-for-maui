using Here.Explore.Maui.Models;

namespace Here.Explore.Maui.Models.Search;

/// <summary>
/// A text-based search query. Note: when <see cref="AreaCenter"/> is null the
/// underlying SDK still requires an area center on both platforms, so results
/// are biased toward (0, 0) — pass a center for location-relevant results.
/// </summary>
public record TextQuery(string Query, GeoCoordinates? AreaCenter = null);

/// <summary>
/// An address-based search (forward geocoding) query, e.g.
/// "Invalidenstraße 116, Berlin". Excludes POI names — use
/// <see cref="TextQuery"/> when a POI name is included. When
/// <see cref="AreaCenter"/> is null the query is unbiased.
/// </summary>
public record AddressQuery(string Query, GeoCoordinates? AreaCenter = null);

/// <summary>
/// A category-based search query. Note: when <see cref="AreaCenter"/> is null
/// the underlying SDK still requires an area center on both platforms, so
/// results are biased toward (0, 0) — pass a center for location-relevant results.
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
    string? Category = null,
    PlaceCategory? PrimaryCategory = null,
    IReadOnlyList<PlaceCategory>? Categories = null,
    Contact? Contact = null,
    OpeningHours? OpeningHours = null
);

/// <summary>
/// Place category information.
/// </summary>
public record PlaceCategory(
    string Id,
    string Name,
    IReadOnlyList<string>? Aliases = null
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
/// Contact information for a place.
/// </summary>
public record Contact(
    string? Phone = null,
    string? Website = null,
    string? Email = null
);

/// <summary>
/// Opening hours for a place.
/// </summary>
public record OpeningHours(
    bool IsOpenNow = false,
    IReadOnlyList<TimeRange>? TimeRanges = null,
    string? RawText = null
);

/// <summary>
/// A time range for opening hours.
/// </summary>
public record TimeRange(
    DayOfWeek StartDay,
    TimeSpan StartTime,
    DayOfWeek EndDay,
    TimeSpan EndTime
);

/// <summary>
/// Search error codes.
/// </summary>
public enum SearchError
{
    /// <summary>No error — search succeeded.</summary>
    None,
    /// <summary>Network error during search.</summary>
    NetworkError,
    /// <summary>HTTP error from the search service.</summary>
    HttpError,
    /// <summary>Search completed but returned no results.</summary>
    NoResults,
    /// <summary>The search query is invalid.</summary>
    InvalidQuery,
    /// <summary>Insufficient memory for the search operation.</summary>
    InsufficientMemory,
    /// <summary>Search engine is not initialized.</summary>
    EngineNotInitialized,
    /// <summary>The requested place was not found.</summary>
    PlaceNotFound,
    /// <summary>Error serializing or deserializing the response.</summary>
    SerializationError,
    /// <summary>Invalid parameter provided.</summary>
    InvalidParameter
}

/// <summary>
/// Type of search suggestion.
/// </summary>
public enum SuggestionType
{
    /// <summary>A specific place (POI or address).</summary>
    Place,
    /// <summary>A category of places.</summary>
    Category,
    /// <summary>A business chain.</summary>
    Chain
}

/// <summary>
/// Language for search results.
/// </summary>
public enum SearchLanguage
{
    /// <summary>English.</summary>
    En,
    /// <summary>German.</summary>
    De,
    /// <summary>French.</summary>
    Fr,
    /// <summary>Spanish.</summary>
    Es,
    /// <summary>Italian.</summary>
    It,
    /// <summary>Portuguese.</summary>
    Pt,
    /// <summary>Dutch.</summary>
    Nl,
    /// <summary>Polish.</summary>
    Pl,
    /// <summary>Russian.</summary>
    Ru,
    /// <summary>Chinese.</summary>
    Zh,
    /// <summary>Japanese.</summary>
    Ja,
    /// <summary>Korean.</summary>
    Ko
}