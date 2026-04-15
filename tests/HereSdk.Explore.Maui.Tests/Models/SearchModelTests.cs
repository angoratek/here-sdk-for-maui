using Xunit;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Search;

namespace Here.Explore.Maui.Tests.Models;

public class SearchModelTests
{
    [Fact]
    public void TextQuery_CreatedWithQueryOnly()
    {
        var query = new TextQuery("Berlin");

        Assert.Equal("Berlin", query.Query);
        Assert.Null(query.AreaCenter);
    }

    [Fact]
    public void TextQuery_CreatedWithAreaCenter()
    {
        var query = new TextQuery("Pizza", new GeoCoordinates(52.5, 13.4));

        Assert.Equal("Pizza", query.Query);
        Assert.NotNull(query.AreaCenter);
        Assert.Equal(52.5, query.AreaCenter!.Latitude);
        Assert.Equal(13.4, query.AreaCenter.Longitude);
    }

    [Fact]
    public void CategoryQuery_CreatedWithId()
    {
        var query = new CategoryQuery("100-1000"); // Restaurant category

        Assert.Equal("100-1000", query.CategoryId);
        Assert.Null(query.AreaCenter);
    }

    [Fact]
    public void CategoryQuery_CreatedWithAreaCenter()
    {
        var query = new CategoryQuery("100-1000", new GeoCoordinates(52.5, 13.4));

        Assert.NotNull(query.AreaCenter);
    }

    [Fact]
    public void SearchOptions_Defaults()
    {
        var opts = new SearchOptions();

        Assert.Null(opts.MaxItems);
        Assert.Null(opts.Language);
    }

    [Fact]
    public void SearchOptions_WithMaxItems()
    {
        var opts = new SearchOptions(MaxItems: 10, Language: SearchLanguage.De);

        Assert.Equal(10, opts.MaxItems);
        Assert.Equal(SearchLanguage.De, opts.Language);
    }

    [Fact]
    public void SearchResult_WithPlaces()
    {
        var places = new List<Place>
        {
            new("id1", "Berlin Hauptbahnhof", new GeoCoordinates(52.5, 13.4)),
            new("id2", "Berlin Alexanderplatz", new GeoCoordinates(52.52, 13.41))
        };
        var result = new SearchResult(SearchError.None, places);

        Assert.Equal(SearchError.None, result.Error);
        Assert.Equal(2, result.Places!.Count);
    }

    [Fact]
    public void SearchResult_WithError()
    {
        var result = new SearchResult(SearchError.NoResults, null);

        Assert.Equal(SearchError.NoResults, result.Error);
        Assert.Null(result.Places);
    }

    [Fact]
    public void Place_CreatedWithAllFields()
    {
        var address = new Address(City: "Berlin", CountryCode: "DEU");
        var place = new Place("pid1", "Brandenburg Gate", new GeoCoordinates(52.5163, 13.3777),
            address, DistanceInMeters: 500.0);

        Assert.Equal("pid1", place.Id);
        Assert.Equal("Brandenburg Gate", place.Title);
        Assert.NotNull(place.Address);
        Assert.Equal("Berlin", place.Address!.City);
        Assert.Equal("DEU", place.Address.CountryCode);
        Assert.Equal(500.0, place.DistanceInMeters);
    }

    [Fact]
    public void Place_CreatedWithMinimalFields()
    {
        var place = new Place("pid1", "Some Place", new GeoCoordinates(0, 0));

        Assert.Null(place.Address);
        Assert.Null(place.DistanceInMeters);
    }

    [Fact]
    public void Suggestion_Created()
    {
        var sug = new Suggestion("Ber", "sug1", SuggestionType.Place);

        Assert.Equal("Ber", sug.Title);
        Assert.Equal("sug1", sug.Id);
        Assert.Equal(SuggestionType.Place, sug.Type);
        Assert.False(sug.IsCategory);
    }

    [Fact]
    public void Suggestion_CategoryType()
    {
        var sug = new Suggestion("Restaurants", "cat1", SuggestionType.Category, IsCategory: true);

        Assert.Equal(SuggestionType.Category, sug.Type);
        Assert.True(sug.IsCategory);
    }

    [Fact]
    public void Address_CreatedWithAllFields()
    {
        var addr = new Address(
            Street: "Unter den Linden",
            HouseNumber: "1",
            City: "Berlin",
            District: "Mitte",
            State: "Berlin",
            CountryCode: "DEU",
            CountryName: "Germany",
            PostalCode: "10117"
        );

        Assert.Equal("Unter den Linden", addr.Street);
        Assert.Equal("1", addr.HouseNumber);
        Assert.Equal("Berlin", addr.City);
        Assert.Equal("DEU", addr.CountryCode);
        Assert.Equal("Germany", addr.CountryName);
        Assert.Equal("10117", addr.PostalCode);
    }

    [Fact]
    public void SearchError_Values()
    {
        Assert.Equal(0, (int)SearchError.None);
        Assert.Equal(1, (int)SearchError.NetworkError);
        Assert.Equal(2, (int)SearchError.HttpError);
        Assert.Equal(3, (int)SearchError.NoResults);
    }

    [Fact]
    public void SuggestionType_Values()
    {
        Assert.Equal(0, (int)SuggestionType.Place);
        Assert.Equal(1, (int)SuggestionType.Category);
        Assert.Equal(2, (int)SuggestionType.Chain);
    }

    [Fact]
    public void SearchLanguage_Values()
    {
        Assert.Equal(0, (int)SearchLanguage.En);
        Assert.Equal(1, (int)SearchLanguage.De);
        Assert.Equal(2, (int)SearchLanguage.Fr);
    }

    [Fact]
    public void SuggestResult_WithSuggestions()
    {
        var suggestions = new List<Suggestion>
        {
            new("Ber", "s1", SuggestionType.Place),
            new("Berlin", "s2", SuggestionType.Place)
        };
        var result = new SuggestResult(SearchError.None, suggestions);

        Assert.Equal(SearchError.None, result.Error);
        Assert.Equal(2, result.Suggestions!.Count);
    }
}