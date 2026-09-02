using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Search;

namespace Here.Explore.Maui.Services;

/// <summary>
/// Service for place search and discovery.
/// </summary>
public interface ISearchService : IHereSdkService
{
    /// <summary>Searches for places matching a text query (e.g., "pizza in Berlin").</summary>
    /// <param name="query">The text query with optional area center for bias.</param>
    /// <param name="options">Search options (max items, language).</param>
    /// <returns>The search result containing matched places or an error.</returns>
    Task<SearchResult> SearchAsync(TextQuery query, SearchOptions options);

    /// <summary>Searches for places in a specific category (e.g., restaurants, hotels).</summary>
    /// <param name="query">The category query with optional area center.</param>
    /// <param name="options">Search options (max items, language).</param>
    /// <returns>The search result containing matched places or an error.</returns>
    Task<SearchResult> SearchAsync(CategoryQuery query, SearchOptions options);

    /// <summary>Forward-geocodes an address string (e.g., "Invalidenstraße 116, Berlin") to places with coordinates.</summary>
    /// <param name="query">The address query with optional area center for bias.</param>
    /// <param name="options">Search options (max items, language).</param>
    /// <returns>The search result containing matched addresses or an error.</returns>
    Task<SearchResult> SearchAsync(AddressQuery query, SearchOptions options);

    /// <summary>Reverse-geocodes coordinates to the place/address at that location.</summary>
    /// <param name="coordinates">The coordinates to look up.</param>
    /// <param name="options">Search options (max items, language).</param>
    /// <returns>The search result containing the matched place (with address) or an error.</returns>
    Task<SearchResult> SearchAsync(GeoCoordinates coordinates, SearchOptions options);

    /// <summary>Gets auto-suggest results for a text query (lightweight, for type-ahead).</summary>
    /// <param name="query">The partial text query.</param>
    /// <param name="options">Search options (max items, language).</param>
    /// <returns>The suggest result containing suggestions or an error.</returns>
    Task<SuggestResult> SuggestAsync(TextQuery query, SearchOptions options);

    /// <summary>Retrieves a place by its ID (from a suggestion or previous search result).</summary>
    /// <param name="placeId">The place identifier returned by search or suggest.</param>
    /// <returns>The place, or null if not found.</returns>
    Task<Place?> GetPlaceByIdAsync(string placeId);
}