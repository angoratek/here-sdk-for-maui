using Xunit;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Search;
using Here.Explore.Maui.Services;
using NSubstitute;

namespace Here.Explore.Maui.Tests.Services;

public class SearchServiceExtendedTests
{
    [Fact]
    public async Task SearchAsync_WithCategoryQuery_ReturnsResults()
    {
        var searchService = Substitute.For<ISearchService>();
        var expected = new SearchResult(SearchError.None, new List<Place>
        {
            new("p1", "Italian Restaurant", new GeoCoordinates(52.5, 13.4))
        });
        searchService.SearchAsync(Arg.Any<CategoryQuery>(), Arg.Any<SearchOptions>())
            .Returns(Task.FromResult(expected));

        var result = await searchService.SearchAsync(
            new CategoryQuery("100-1000", new GeoCoordinates(52.5, 13.4)),
            new SearchOptions(MaxItems: 5));

        Assert.Equal(SearchError.None, result.Error);
        Assert.NotNull(result.Places);
        Assert.Single(result.Places);
        Assert.Equal("Italian Restaurant", result.Places![0].Title);
    }

    [Fact]
    public async Task SearchAsync_ReturnsError_WhenNetworkUnavailable()
    {
        var searchService = Substitute.For<ISearchService>();
        searchService.SearchAsync(Arg.Any<TextQuery>(), Arg.Any<SearchOptions>())
            .Returns(Task.FromResult(new SearchResult(SearchError.NetworkError, null)));

        var result = await searchService.SearchAsync(new TextQuery("test"), new SearchOptions());

        Assert.Equal(SearchError.NetworkError, result.Error);
        Assert.Null(result.Places);
    }

    [Fact]
    public async Task SuggestAsync_ReturnsSuggestions()
    {
        var searchService = Substitute.For<ISearchService>();
        var expected = new SuggestResult(SearchError.None, new List<Suggestion>
        {
            new("Ber", "s1", SuggestionType.Place),
            new("Berlin", "s2", SuggestionType.Place),
            new("Berg", "s3", SuggestionType.Chain)
        });
        searchService.SuggestAsync(Arg.Any<TextQuery>(), Arg.Any<SearchOptions>())
            .Returns(Task.FromResult(expected));

        var result = await searchService.SuggestAsync(new TextQuery("Ber"), new SearchOptions());

        Assert.Equal(SearchError.None, result.Error);
        Assert.Equal(3, result.Suggestions!.Count);
    }

    [Fact]
    public async Task GetPlaceByIdAsync_ReturnsPlace()
    {
        var searchService = Substitute.For<ISearchService>();
        var expected = new Place("pid-123", "Brandenburg Gate", new GeoCoordinates(52.5163, 13.3777));
        searchService.GetPlaceByIdAsync("pid-123")
            .Returns(Task.FromResult<Place?>(expected));

        var result = await searchService.GetPlaceByIdAsync("pid-123");

        Assert.NotNull(result);
        Assert.Equal("Brandenburg Gate", result!.Title);
    }

    [Fact]
    public async Task GetPlaceByIdAsync_ReturnsNull_WhenNotFound()
    {
        var searchService = Substitute.For<ISearchService>();
        searchService.GetPlaceByIdAsync("nonexistent")
            .Returns(Task.FromResult<Place?>(null));

        var result = await searchService.GetPlaceByIdAsync("nonexistent");

        Assert.Null(result);
    }

    [Fact]
    public async Task SearchAsync_WithLanguageOption_PassesOption()
    {
        var searchService = Substitute.For<ISearchService>();
        var options = new SearchOptions(MaxItems: 20, Language: SearchLanguage.De);
        searchService.SearchAsync(Arg.Any<TextQuery>(), options)
            .Returns(Task.FromResult(new SearchResult(SearchError.None, null)));

        await searchService.SearchAsync(new TextQuery("Berlin"), options);

        await searchService.Received(1).SearchAsync(Arg.Any<TextQuery>(), options);
    }
}