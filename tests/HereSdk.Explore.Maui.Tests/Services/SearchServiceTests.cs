using Xunit;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Search;
using Here.Explore.Maui.Services;
using NSubstitute;

namespace Here.Explore.Maui.Tests.Services;

public class SearchServiceTests
{
    [Fact]
    public async Task SearchAsync_WithTextQuery_ReturnsResults()
    {
        var searchService = Substitute.For<ISearchService>();
        var expected = new SearchResult(SearchError.None, new List<Place>
        {
            new("id1", "Berlin", new GeoCoordinates(52.5, 13.4))
        });
        searchService.SearchAsync(Arg.Any<TextQuery>(), Arg.Any<SearchOptions>())
            .Returns(Task.FromResult(expected));

        var result = await searchService.SearchAsync(
            new TextQuery("Berlin"),
            new SearchOptions());

        Assert.Equal(SearchError.None, result.Error);
        Assert.NotNull(result.Places);
        Assert.Single(result.Places);
    }
}