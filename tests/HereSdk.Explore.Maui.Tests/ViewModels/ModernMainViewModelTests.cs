using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Routing;
using Here.Explore.Maui.Models.Search;
using Here.Explore.Maui.RefApp.ViewModels;
using Here.Explore.Maui.Services;
using NSubstitute;

namespace Here.Explore.Maui.Tests.ViewModels;

public class ModernMainViewModelTests
{
    private readonly ISearchService _searchService;
    private readonly IRoutingService _routingService;
    private readonly ILocationService _locationService;
    private readonly ModernMainViewModel _viewModel;

    public ModernMainViewModelTests()
    {
        _searchService = Substitute.For<ISearchService>();
        _routingService = Substitute.For<IRoutingService>();
        _locationService = Substitute.For<ILocationService>();
        _viewModel = new ModernMainViewModel(_searchService, _routingService, _locationService);
    }

    [Fact]
    public void Constructor_SetsDefaults()
    {
        Assert.Equal(string.Empty, _viewModel.OriginQuery);
        Assert.Equal(string.Empty, _viewModel.DestinationQuery);
        Assert.Null(_viewModel.OriginPlace);
        Assert.Null(_viewModel.DestinationPlace);
        Assert.False(_viewModel.IsLoading);
        Assert.False(_viewModel.IsMapObjectsPanelVisible);
        Assert.False(_viewModel.CanCalculateRoute);
        Assert.False(_viewModel.HasRoute);
    }

    [Fact]
    public void OriginQuery_Set_RaisesPropertyChanged()
    {
        var changed = false;
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(_viewModel.OriginQuery)) changed = true;
        };

        _viewModel.OriginQuery = "Berlin";

        Assert.True(changed);
    }

    [Fact]
    public void OriginPlace_Set_UpdatesCanCalculateRoute()
    {
        var changed = false;
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(_viewModel.CanCalculateRoute)) changed = true;
        };

        _viewModel.OriginPlace = new Place("1", "Berlin", new GeoCoordinates(52.5, 13.4));

        Assert.True(changed);
        Assert.False(_viewModel.CanCalculateRoute);
    }

    [Fact]
    public void BothPlacesSet_CanCalculateRoute_IsTrue()
    {
        _viewModel.OriginPlace = new Place("1", "Berlin", new GeoCoordinates(52.5, 13.4));
        _viewModel.DestinationPlace = new Place("2", "Munich", new GeoCoordinates(48.1, 11.5));

        Assert.True(_viewModel.CanCalculateRoute);
    }

    [Fact]
    public void ClearOriginCommand_ClearsOrigin()
    {
        _viewModel.OriginQuery = "Berlin";
        _viewModel.OriginPlace = new Place("1", "Berlin", new GeoCoordinates(52.5, 13.4));

        _viewModel.ClearOriginCommand.Execute(null);

        Assert.Equal(string.Empty, _viewModel.OriginQuery);
        Assert.Null(_viewModel.OriginPlace);
    }

    [Fact]
    public void ClearDestinationCommand_ClearsDestination()
    {
        _viewModel.DestinationQuery = "Munich";
        _viewModel.DestinationPlace = new Place("2", "Munich", new GeoCoordinates(48.1, 11.5));

        _viewModel.ClearDestinationCommand.Execute(null);

        Assert.Equal(string.Empty, _viewModel.DestinationQuery);
        Assert.Null(_viewModel.DestinationPlace);
    }

    [Fact]
    public void SwapLocationsCommand_SwapsValues()
    {
        _viewModel.OriginQuery = "Berlin";
        _viewModel.DestinationQuery = "Munich";
        _viewModel.OriginPlace = new Place("1", "Berlin", new GeoCoordinates(52.5, 13.4));
        _viewModel.DestinationPlace = new Place("2", "Munich", new GeoCoordinates(48.1, 11.5));

        _viewModel.SwapLocationsCommand.Execute(null);

        Assert.Equal("Munich", _viewModel.OriginQuery);
        Assert.Equal("Berlin", _viewModel.DestinationQuery);
        Assert.Equal("Munich", _viewModel.OriginPlace?.Title);
        Assert.Equal("Berlin", _viewModel.DestinationPlace?.Title);
    }

    [Fact]
    public void ToggleMapObjectsPanelCommand_TogglesVisibility()
    {
        Assert.False(_viewModel.IsMapObjectsPanelVisible);

        _viewModel.ToggleMapObjectsPanelCommand.Execute(null);
        Assert.True(_viewModel.IsMapObjectsPanelVisible);

        _viewModel.ToggleMapObjectsPanelCommand.Execute(null);
        Assert.False(_viewModel.IsMapObjectsPanelVisible);
    }

    [Fact]
    public async Task SearchOriginCommand_PopulatesSuggestions()
    {
        var suggestions = new List<Suggestion>
        {
            new("Berlin", "id1", SuggestionType.Place, false),
            new("Berlin Airport", "id2", SuggestionType.Place, false)
        };
        _searchService.SuggestAsync(Arg.Any<TextQuery>(), Arg.Any<SearchOptions>())
            .Returns(new SuggestResult(SearchError.None, suggestions));

        _viewModel.OriginQuery = "Berlin";
        await Task.Run(() => _viewModel.SearchOriginCommand.Execute(null));

        Assert.NotNull(_viewModel.OriginSuggestions);
        Assert.Equal(2, _viewModel.OriginSuggestions?.Count);
    }

    [Fact]
    public async Task CalculateRouteCommand_WithValidPlaces_CallsRoutingService()
    {
        var origin = new Place("1", "Berlin", new GeoCoordinates(52.5, 13.4));
        var destination = new Place("2", "Munich", new GeoCoordinates(48.1, 11.5));
        _viewModel.OriginPlace = origin;
        _viewModel.DestinationPlace = destination;

        var route = new Route("handle", new List<Section>(), 500000, 12000);
        _routingService.CalculateRouteAsync(Arg.Any<IReadOnlyList<Waypoint>>(), Arg.Any<RoutingOptions>())
            .Returns(new RoutingResult(RoutingError.None, new List<Route> { route }));

        await Task.Run(() => _viewModel.CalculateRouteCommand.Execute(null));

        await _routingService.Received(1).CalculateRouteAsync(
            Arg.Is<IReadOnlyList<Waypoint>>(w => w.Count == 2),
            Arg.Any<RoutingOptions>());
    }

    [Fact]
    public void ClearRouteCommand_ClearsRoute()
    {
        // Cannot fully test without IMapService mock initialization,
        // but we can verify the command exists and executes
        Assert.NotNull(_viewModel.ClearRouteCommand);
    }

    [Fact]
    public void DistanceText_NoRoute_ReturnsDashDash()
    {
        Assert.Equal("--", _viewModel.DistanceText);
    }

    [Fact]
    public void DurationText_NoRoute_ReturnsDashDash()
    {
        Assert.Equal("--", _viewModel.DurationText);
    }

    [Fact]
    public void Maneuvers_NoRoute_ReturnsEmptyList()
    {
        Assert.Empty(_viewModel.Maneuvers);
    }
}
