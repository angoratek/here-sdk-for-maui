using System.Windows.Input;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Search;
using Here.Explore.Maui.Services;

namespace Here.Explore.Maui.RefApp.ViewModels;

public class SearchViewModel
{
    private readonly ISearchService? _searchService;

    public string SearchQuery { get; set; } = string.Empty;
    public IReadOnlyList<Place>? Results { get; private set; }

    public SearchViewModel() { }

    public SearchViewModel(ISearchService searchService)
    {
        _searchService = searchService;
    }

    public ICommand SearchCommand => new Command(async () => await SearchAsync());

    private async Task SearchAsync()
    {
        if (_searchService is null || string.IsNullOrWhiteSpace(SearchQuery)) return;

        try
        {
            var result = await _searchService.SearchAsync(
                new TextQuery(SearchQuery),
                new SearchOptions());
            Results = result.Places;
        }
        catch (Exception ex)
        {
            // Handle search error
            Results = null;
        }
    }
}