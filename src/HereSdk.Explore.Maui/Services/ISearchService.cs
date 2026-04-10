using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Search;

namespace Here.Explore.Maui.Services;

/// <summary>
/// Service for place search and discovery.
/// </summary>
public interface ISearchService : IHereSdkService
{
    Task<SearchResult> SearchAsync(TextQuery query, SearchOptions options);
    Task<SearchResult> SearchAsync(CategoryQuery query, SearchOptions options);
    Task<SuggestResult> SuggestAsync(TextQuery query, SearchOptions options);
    Task<Place?> GetPlaceByIdAsync(string placeId);
}