using System.Windows.Input;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Search;
using Here.Explore.Maui.Services;

namespace Here.Explore.Maui.RefApp.ViewModels;

public class SearchViewModel : ViewModelBase
{
    private readonly ISearchService? _searchService;
    private string _searchQuery = string.Empty;
    private IReadOnlyList<Place>? _results;

    public string SearchQuery
    {
        get => _searchQuery;
        set => SetProperty(ref _searchQuery, value);
    }

    public IReadOnlyList<Place>? Results
    {
        get => _results;
        private set => SetProperty(ref _results, value);
    }

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
        catch (Exception)
        {
            Results = null;
        }
    }
}