# How to Search

This guide follows the pattern used in `ExplorePage` and `DirectionsPage` in the reference app.

## Dual-Input Search

The ref app uses two search inputs — one for origin (optional) and one for destination:

```csharp
public partial class ExploreViewModel : ObservableObject
{
    private readonly ISearchService _searchService;

    public ExploreViewModel(ISearchService searchService)
    {
        _searchService = searchService;
    }

    // Called on every keystroke with debounce
    [RelayCommand]
    private async Task SubmitSearch(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return;

        var result = await _searchService.SuggestAsync(
            new TextQuery(query),
            new SearchOptions { MaxItems = 10 });

        Suggestions = result.Suggestions?
            .Select(s => new SuggestionItem(s.Title, s))
            .ToList();
    }
}
```

## Selecting a Suggestion

When a user taps a suggestion, get its coordinates and create a `Waypoint`:

```csharp
[RelayCommand]
private async Task SelectSuggestion(SuggestionItem item)
{
    var place = await _searchService.GetPlaceByIdAsync(item.Suggestion.PlaceId);
    if (place is null) return;

    var waypoint = new Waypoint(place.Coordinates, WaypointType.Stop);

    if (IsOriginMode)
        OriginWaypoint = waypoint;
    else
        DestinationWaypoint = waypoint;
}
```

## Search with Area Restriction

Restrict search to a geographic area:

```csharp
var searchArea = new GeoCircle(
    center: new GeoCoordinates(52.5200, 13.4050),
    radiusInMeters: 3000);

var result = await _searchService.SearchAsync(
    new TextQuery("restaurant"),
    new SearchOptions { SearchArea = searchArea, MaxItems = 20 });
```

## Category Search

```csharp
// Search by predefined category
var result = await _searchService.SearchAsync(
    new CategoryQuery(PlaceCategory.Parking),
    new SearchOptions());

// Place categories include:
// Restaurant, Hotel, Parking, GasStation, Hospital,
// Shopping, Museum, Bank, Airport, TrainStation, etc.
```

## Error Handling

```csharp
try
{
    var result = await _searchService.SearchAsync(query, options);
    if (result.Error == SearchError.None)
    {
        Places = result.Places;
    }
    else
    {
        ErrorMessage = result.Error switch
        {
            SearchError.NoResultsFound => "No places found.",
            SearchError.NetworkError => "Check your connection.",
            _ => $"Search failed: {result.Error}"
        };
    }
}
catch (Exception ex)
{
    ErrorMessage = $"Search error: {ex.Message}";
}
```

## Displaying Results

```csharp
// Place has: Id, Title, Coordinates, Address, Contacts, OpeningHours, Categories
var place = result.Places![0];
Console.WriteLine($"{place.Title}");
Console.WriteLine($"{place.Address?.City}, {place.Address?.CountryCode}");
Console.WriteLine($"{place.Contacts?.Phones?.FirstOrDefault()?.Number}");

// Opening hours
if (place.OpeningHours?.IsOpenNow == true)
    Console.WriteLine("Open now");
```

## Debounce Pattern

To avoid excessive API calls on text input, use debounce:

```csharp
private CancellationTokenSource? _debounceCts;

private async Task OnSearchTextChanged(string text)
{
    _debounceCts?.Cancel();
    _debounceCts = new CancellationTokenSource();

    try
    {
        await Task.Delay(300, _debounceCts.Token);
        await SubmitSearch(text);
    }
    catch (TaskCanceledException) { }
}
```
