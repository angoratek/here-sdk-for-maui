using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Search;
using Here.Explore.Maui.Services;
using Contact = Here.Explore.Maui.Models.Search.Contact;

namespace Here.Explore.Maui.DeviceTests.iOS;

public class SearchServiceiOSTests
{
    [Fact]
    public void SearchService_CanBeInstantiated()
    {
        var service = new SearchService();
        Assert.NotNull(service);
    }

    [Fact]
    public void TextQuery_CanBeCreated()
    {
        var query = new TextQuery("coffee near me");
        Assert.Equal("coffee near me", query.Query);
    }

    [Fact]
    public void Place_CanBeCreated()
    {
        var place = new Place("p1", "Cafe", new GeoCoordinates(52.5, 13.4));
        Assert.Equal("Cafe", place.Title);
    }

    [Fact]
    public void Place_WithFullDetails()
    {
        var address = new Address(Street: "Main St", City: "Berlin", CountryCode: "DE");
        var contact = new Contact(Phone: "+49 30 555000", Website: "https://cafe.de");
        var hours = new OpeningHours(IsOpenNow: true);
        var place = new Place("p2", "Coffee Shop", new GeoCoordinates(52.5, 13.4),
            Address: address, Contact: contact, OpeningHours: hours,
            DistanceInMeters: 250);
        Assert.Equal("Berlin", place.Address!.City);
        Assert.Equal("+49 30 555000", place.Contact!.Phone);
        Assert.True(place.OpeningHours!.IsOpenNow);
        Assert.Equal(250, place.DistanceInMeters);
    }

    [Fact]
    public void Suggestion_PlaceType()
    {
        var suggestion = new Suggestion("Brandenburger Tor", "p123", SuggestionType.Place);
        Assert.Equal(SuggestionType.Place, suggestion.Type);
    }

    [Fact]
    public void SearchResult_Success()
    {
        var places = new[] { new Place("p1", "Test", new GeoCoordinates(0, 0)) };
        var result = new SearchResult(SearchError.None, places);
        Assert.Single(result.Places!);
    }

    [Fact]
    public void SearchResult_Error()
    {
        var result = new SearchResult(SearchError.NetworkError, null);
        Assert.Equal(SearchError.NetworkError, result.Error);
    }

    [Fact]
    public void Address_FullAddress()
    {
        var address = new Address(
            Street: "Unter den Linden",
            HouseNumber: "77",
            City: "Berlin",
            District: "Mitte",
            State: "Berlin",
            CountryCode: "DE",
            CountryName: "Germany",
            PostalCode: "10117");
        Assert.Equal("Mitte", address.District);
        Assert.Equal("10117", address.PostalCode);
    }

    [Fact]
    public void PlaceCategory_WithAliases()
    {
        var category = new PlaceCategory("eat-drink", "Eat & Drink",
            new[] { "restaurant", "cafe", "bar" });
        Assert.Equal(3, category.Aliases!.Count);
    }
}
