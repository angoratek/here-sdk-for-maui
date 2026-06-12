using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Search;
using Here.Explore.Maui.Services;
using Contact = Here.Explore.Maui.Models.Search.Contact;

namespace Here.Explore.Maui.DeviceTests.Android;

public class SearchServiceAndroidTests
{
    // ================================================================
    // Service lifecycle
    // ================================================================

    [Fact]
    public void SearchService_CanBeInstantiated()
    {
        var service = new SearchService();
        Assert.NotNull(service);
    }

    // ================================================================
    // TextQuery model
    // ================================================================

    [Fact]
    public void TextQuery_CanBeCreated()
    {
        var query = new TextQuery("pizza in Berlin");
        Assert.Equal("pizza in Berlin", query.Query);
        Assert.Null(query.AreaCenter);
    }

    [Fact]
    public void TextQuery_WithAreaCenter()
    {
        var query = new TextQuery("restaurant", new GeoCoordinates(52.5, 13.4));
        Assert.Equal(52.5, query.AreaCenter!.Latitude);
    }

    // ================================================================
    // CategoryQuery model
    // ================================================================

    [Fact]
    public void CategoryQuery_CanBeCreated()
    {
        var query = new CategoryQuery("restaurant-123");
        Assert.Equal("restaurant-123", query.CategoryId);
    }

    [Fact]
    public void CategoryQuery_WithAreaCenter()
    {
        var query = new CategoryQuery("hotel-abc", new GeoCoordinates(48.8, 2.3));
        Assert.NotNull(query.AreaCenter);
    }

    // ================================================================
    // SearchOptions model
    // ================================================================

    [Fact]
    public void SearchOptions_Defaults()
    {
        var options = new SearchOptions();
        Assert.Null(options.MaxItems);
        Assert.Null(options.Language);
    }

    [Fact]
    public void SearchOptions_Custom()
    {
        var options = new SearchOptions(MaxItems: 20, Language: SearchLanguage.De);
        Assert.Equal(20, options.MaxItems);
        Assert.Equal(SearchLanguage.De, options.Language);
    }

    // ================================================================
    // Place model
    // ================================================================

    [Fact]
    public void Place_CanBeCreated()
    {
        var place = new Place("p1", "Berliner Dom", new GeoCoordinates(52.5, 13.4));
        Assert.Equal("p1", place.Id);
        Assert.Equal("Berliner Dom", place.Title);
    }

    [Fact]
    public void Place_WithAddress()
    {
        var address = new Address(Street: "Unter den Linden", City: "Berlin", CountryCode: "DE");
        var place = new Place("p2", "Brandenburger Tor", new GeoCoordinates(52.5, 13.4), Address: address);
        Assert.Equal("Berlin", place.Address!.City);
    }

    [Fact]
    public void Place_WithDistance()
    {
        var place = new Place("p3", "Cafe", new GeoCoordinates(52.5, 13.4), DistanceInMeters: 500);
        Assert.Equal(500, place.DistanceInMeters);
    }

    [Fact]
    public void Place_WithContact()
    {
        var contact = new Contact(Phone: "+49 30 123456", Website: "https://example.com");
        var place = new Place("p4", "Restaurant", new GeoCoordinates(52.5, 13.4), Contact: contact);
        Assert.Equal("+49 30 123456", place.Contact!.Phone);
    }

    [Fact]
    public void Place_WithOpeningHours()
    {
        var hours = new OpeningHours(IsOpenNow: true);
        var place = new Place("p5", "Store", new GeoCoordinates(0, 0), OpeningHours: hours);
        Assert.True(place.OpeningHours!.IsOpenNow);
    }

    [Fact]
    public void Place_WithCategories()
    {
        var categories = new[] { new PlaceCategory("restaurant", "Restaurant") };
        var place = new Place("p6", "Pizza Place", new GeoCoordinates(0, 0), Categories: categories);
        Assert.Single(place.Categories!);
    }

    // ================================================================
    // PlaceCategory model
    // ================================================================

    [Fact]
    public void PlaceCategory_CanBeCreated()
    {
        var cat = new PlaceCategory("restaurant", "Restaurant");
        Assert.Equal("restaurant", cat.Id);
        Assert.Equal("Restaurant", cat.Name);
    }

    [Fact]
    public void PlaceCategory_WithAliases()
    {
        var cat = new PlaceCategory("fuel", "Fuel Station", new[] { "gas station", "petrol" });
        Assert.Equal(2, cat.Aliases!.Count);
    }

    // ================================================================
    // Address model
    // ================================================================

    [Fact]
    public void Address_FullAddress()
    {
        var address = new Address(
            Street: "Friedrichstr",
            HouseNumber: "142",
            City: "Berlin",
            State: "Berlin",
            CountryCode: "DE",
            CountryName: "Germany",
            PostalCode: "10117");
        Assert.Equal("Friedrichstr", address.Street);
        Assert.Equal("DE", address.CountryCode);
    }

    [Fact]
    public void Address_Minimal()
    {
        var address = new Address(City: "Munich");
        Assert.Equal("Munich", address.City);
        Assert.Null(address.Street);
    }

    // ================================================================
    // Contact model
    // ================================================================

    [Fact]
    public void Contact_AllFields()
    {
        var contact = new Contact(
            Phone: "+1 555 0199",
            Website: "https://example.com",
            Email: "info@example.com");
        Assert.NotNull(contact.Phone);
        Assert.NotNull(contact.Website);
    }

    [Fact]
    public void Contact_Empty()
    {
        var contact = new Contact();
        Assert.Null(contact.Phone);
    }

    // ================================================================
    // OpeningHours model
    // ================================================================

    [Fact]
    public void OpeningHours_ClosedNow()
    {
        var hours = new OpeningHours(IsOpenNow: false);
        Assert.False(hours.IsOpenNow);
    }

    [Fact]
    public void OpeningHours_WithTimeRanges()
    {
        var range = new TimeRange(DayOfWeek.Monday, new TimeSpan(9, 0, 0),
                                  DayOfWeek.Monday, new TimeSpan(17, 0, 0));
        var hours = new OpeningHours(IsOpenNow: true, TimeRanges: new[] { range });
        Assert.Single(hours.TimeRanges!);
    }

    // ================================================================
    // Suggestion model
    // ================================================================

    [Fact]
    public void Suggestion_Place()
    {
        var suggestion = new Suggestion("Berlin Hbf", "p123", SuggestionType.Place);
        Assert.Equal("Berlin Hbf", suggestion.Title);
        Assert.Equal(SuggestionType.Place, suggestion.Type);
        Assert.False(suggestion.IsCategory);
    }

    [Fact]
    public void Suggestion_Category()
    {
        var suggestion = new Suggestion("Restaurants", "cat-1", SuggestionType.Category, IsCategory: true);
        Assert.True(suggestion.IsCategory);
    }

    [Fact]
    public void Suggestion_Equality_Works()
    {
        var a = new Suggestion("Test", "id1", SuggestionType.Place);
        var b = new Suggestion("Test", "id1", SuggestionType.Place);
        Assert.Equal(a, b);
    }

    // ================================================================
    // SearchResult model
    // ================================================================

    [Fact]
    public void SearchResult_Success()
    {
        var places = new[] { new Place("p1", "Place", new GeoCoordinates(0, 0)) };
        var result = new SearchResult(SearchError.None, places);
        Assert.True(result.Error == SearchError.None);
        Assert.Single(result.Places!);
    }

    [Fact]
    public void SearchResult_NoResults()
    {
        var result = new SearchResult(SearchError.NoResults, null);
        Assert.Null(result.Places);
    }

    [Fact]
    public void SearchResult_NetworkError()
    {
        var result = new SearchResult(SearchError.NetworkError, null);
        Assert.True(result.Error == SearchError.NetworkError);
    }

    // ================================================================
    // SuggestResult model
    // ================================================================

    [Fact]
    public void SuggestResult_Success()
    {
        var suggestions = new[] { new Suggestion("Test", "s1", SuggestionType.Place) };
        var result = new SuggestResult(SearchError.None, suggestions);
        Assert.Single(result.Suggestions!);
    }

    // ================================================================
    // SearchError enum
    // ================================================================

    [Fact]
    public void SearchError_Values_AreDefined()
    {
        var errors = Enum.GetValues<SearchError>();
        Assert.Contains(SearchError.None, errors);
        Assert.Contains(SearchError.NetworkError, errors);
        Assert.Contains(SearchError.NoResults, errors);
        Assert.Contains(SearchError.HttpError, errors);
        Assert.Contains(SearchError.InvalidQuery, errors);
    }

    // ================================================================
    // SearchLanguage enum
    // ================================================================

    [Fact]
    public void SearchLanguage_HasSupportedLanguages()
    {
        var langs = Enum.GetValues<SearchLanguage>();
        Assert.Contains(SearchLanguage.En, langs);
        Assert.Contains(SearchLanguage.De, langs);
        Assert.Contains(SearchLanguage.Fr, langs);
        Assert.Contains(SearchLanguage.Ja, langs);
    }

    // ================================================================
    // SuggestionType enum
    // ================================================================

    [Fact]
    public void SuggestionType_Values_AreDefined()
    {
        var types = Enum.GetValues<SuggestionType>();
        Assert.Contains(SuggestionType.Place, types);
        Assert.Contains(SuggestionType.Category, types);
        Assert.Contains(SuggestionType.Chain, types);
    }
}
