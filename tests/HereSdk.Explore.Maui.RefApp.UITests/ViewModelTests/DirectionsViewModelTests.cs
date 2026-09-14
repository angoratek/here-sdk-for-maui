using Xunit;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;
using Here.Explore.Maui.Models.Routing;
using Here.Explore.Maui.Models.Search;
using Here.Explore.Maui.Services;
using Here.Explore.Maui.RefApp.ViewModels;
using Location = Here.Explore.Maui.Models.Location;
using NSubstitute;

namespace Here.Explore.Maui.RefApp.UITests.ViewModelTests;

public class DirectionsViewModelTests
{
    private readonly IRoutingService _routingService;
    private readonly ISearchService _searchService;
    private readonly DirectionsViewModel _viewModel;

    public DirectionsViewModelTests()
    {
        _routingService = Substitute.For<IRoutingService>();
        _searchService = Substitute.For<ISearchService>();
        _viewModel = new DirectionsViewModel(_routingService, _searchService);
    }

    #region Initial State

    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        Assert.Equal("", _viewModel.OriginQuery);
        Assert.Equal("", _viewModel.DestinationQuery);
        Assert.False(_viewModel.HasOriginSuggestions);
        Assert.False(_viewModel.HasDestinationSuggestions);
        Assert.False(_viewModel.IsCalculating);
        Assert.False(_viewModel.IsRouteVisible);
        Assert.False(_viewModel.IsIsolineMode);
    }

    #endregion

    #region OriginQuery / DestinationQuery

    [Fact]
    public async Task OriginQuery_WhenAtLeastTwoChars_FetchesSuggestions()
    {
        var suggestions = new List<Suggestion> { new("Berlin", "id1", SuggestionType.Place) };
        _searchService.SuggestAsync(Arg.Any<TextQuery>(), Arg.Any<SearchOptions>())
            .Returns(new SuggestResult(SearchError.None, suggestions));

        _viewModel.OriginQuery = "Ber";

        await Task.Delay(400);

        await _searchService.Received(1).SuggestAsync(
            Arg.Any<TextQuery>(),
            Arg.Is<SearchOptions>(o => o.MaxItems == 5));
    }

    [Fact]
    public async Task DestinationQuery_WhenAtLeastTwoChars_FetchesSuggestions()
    {
        _searchService.SuggestAsync(Arg.Any<TextQuery>(), Arg.Any<SearchOptions>())
            .Returns(new SuggestResult(SearchError.None, new List<Suggestion>()));

        _viewModel.DestinationQuery = "Mun";

        await Task.Delay(400);

        await _searchService.Received(1).SuggestAsync(
            Arg.Any<TextQuery>(),
            Arg.Is<SearchOptions>(o => o.MaxItems == 5));
    }

    #endregion

    #region SelectOriginSuggestion

    [Fact]
    public async Task SelectOriginSuggestion_SetsOriginPlace()
    {
        var mockMap = Substitute.For<IMapService>();
        mockMap.SetCameraTargetAsync(Arg.Any<GeoCoordinates>(), Arg.Any<double>()).Returns(Task.CompletedTask);
        mockMap.AddMapMarker(Arg.Any<MapMarker>());
        _viewModel.SetMapService(mockMap);

        var suggestion = new Suggestion("Berlin", "id1", SuggestionType.Place);
        var place = new Place("id1", "Berlin", new GeoCoordinates(52.52, 13.405));
        _searchService.GetPlaceByIdAsync("id1").Returns(place);

        _viewModel.SelectOriginSuggestionCommand.Execute(suggestion);
        await Task.Delay(50);

        Assert.Equal("Berlin", _viewModel.OriginQuery);
        Assert.NotNull(_viewModel.OriginPlace);
    }

    [Fact]
    public async Task SelectOriginSuggestion_DoesNotReopenSuggestions_AfterDebounceWindow()
    {
        // Regression: picking a suggestion sets OriginQuery, whose change
        // handler scheduled a debounced suggest for the selected title — its
        // result used to repopulate the list after the clear, re-opening the
        // dropdown.
        var mockMap = Substitute.For<IMapService>();
        _viewModel.SetMapService(mockMap);

        _searchService.SuggestAsync(Arg.Any<TextQuery>(), Arg.Any<SearchOptions>())
            .Returns(new SuggestResult(SearchError.None,
                new List<Suggestion> { new("Berlin", "id1", SuggestionType.Place) }));

        var suggestion = new Suggestion("Berlin", "id1", SuggestionType.Place);
        _searchService.GetPlaceByIdAsync("id1").Returns(
            new Place("id1", "Berlin", new GeoCoordinates(52.52, 13.405)));

        _viewModel.SelectOriginSuggestionCommand.Execute(suggestion);
        await Task.Delay(400); // longer than the 300ms debounce

        Assert.False(_viewModel.HasOriginSuggestions);
        Assert.Empty(_viewModel.OriginSuggestions);
    }

    [Fact]
    public void MapTapped_DismissesOpenSuggestionDropdowns()
    {
        var mockMap = Substitute.For<IMapService>();
        _viewModel.SetMapService(mockMap);

        // Open both dropdowns, then tap the map — the VM subscribes to
        // MapTapped in SetMapService to close them.
        _viewModel.OriginSuggestions = new List<Suggestion> { new("Berlin", "id1", SuggestionType.Place) };
        _viewModel.HasOriginSuggestions = true;
        _viewModel.DestinationSuggestions = new List<Suggestion> { new("Munich", "id2", SuggestionType.Place) };
        _viewModel.HasDestinationSuggestions = true;

        mockMap.MapTapped += Raise.Event<EventHandler<MapTappedEventArgs>>(
            null, new MapTappedEventArgs(new GeoCoordinates(52.5, 13.4), new Point2D(10, 10)));

        Assert.False(_viewModel.HasOriginSuggestions);
        Assert.Empty(_viewModel.OriginSuggestions);
        Assert.False(_viewModel.HasDestinationSuggestions);
        Assert.Empty(_viewModel.DestinationSuggestions);
    }

    [Fact]
    public async Task SelectDestinationSuggestion_SetsDestinationPlace()
    {
        var mockMap = Substitute.For<IMapService>();
        mockMap.AddMapMarker(Arg.Any<MapMarker>());
        _viewModel.SetMapService(mockMap);

        var suggestion = new Suggestion("Munich", "id2", SuggestionType.Place);
        var place = new Place("id2", "Munich", new GeoCoordinates(48.135, 11.582));
        _searchService.GetPlaceByIdAsync("id2").Returns(place);

        _viewModel.SelectDestinationSuggestionCommand.Execute(suggestion);
        await Task.Delay(50);

        Assert.Equal("Munich", _viewModel.DestinationQuery);
        Assert.NotNull(_viewModel.DestinationPlace);
    }

    #endregion

    #region SwapLocations

    [Fact]
    public void SwapLocations_SwapsOriginAndDestination()
    {
        _viewModel.OriginQuery = "Origin";
        _viewModel.DestinationQuery = "Destination";

        _viewModel.SwapLocationsCommand.Execute(null);

        Assert.Equal("Destination", _viewModel.OriginQuery);
        Assert.Equal("Origin", _viewModel.DestinationQuery);
    }

    #endregion

    #region CalculateRoute

    [Fact]
    public async Task CalculateRoute_WithTwoPlaces_DrawsRoute()
    {
        var mockMap = Substitute.For<IMapService>();
        mockMap.SetCameraTargetAsync(Arg.Any<GeoCoordinates>(), Arg.Any<double>()).Returns(Task.CompletedTask);
        mockMap.AddMapPolyline(Arg.Any<MapPolyline>());
        mockMap.AddMapMarker(Arg.Any<MapMarker>());
        _viewModel.SetMapService(mockMap);

        // Set origin and destination places via reflection
        typeof(DirectionsViewModel).GetProperty(nameof(DirectionsViewModel.OriginPlace))!
            .SetValue(_viewModel, new Place("id1", "Origin", new GeoCoordinates(52.52, 13.405)));
        typeof(DirectionsViewModel).GetProperty(nameof(DirectionsViewModel.DestinationPlace))!
            .SetValue(_viewModel, new Place("id2", "Destination", new GeoCoordinates(48.135, 11.582)));

        var geometry = new List<GeoCoordinates>
        {
            new(52.52, 13.405),
            new(48.135, 11.582)
        };
        var section = new Section(0, new GeoCoordinates(52.52, 13.405), new GeoCoordinates(48.135, 11.582),
            new List<Maneuver>
            {
                new(new GeoCoordinates(52.52, 13.405), ManeuverAction.Depart),
                new(new GeoCoordinates(48.135, 11.582), ManeuverAction.Arrive)
            },
            SectionTransportMode.Car, 584000, 19800,
            Geometry: geometry);

        var route = new Route("route1", new List<Section> { section }, 584000, 19800);
        _routingService.CalculateRouteAsync(Arg.Any<List<Waypoint>>(), Arg.Any<RoutingOptions>())
            .Returns(new RoutingResult(RoutingError.None, new List<Route> { route }));

        _viewModel.CalculateRouteCommand.Execute(null);
        await Task.Delay(50);

        Assert.True(_viewModel.IsRouteVisible);
        Assert.NotNull(_viewModel.CurrentRoute);
        await _routingService.Received(1).CalculateRouteAsync(
            Arg.Any<List<Waypoint>>(),
            Arg.Any<RoutingOptions>());
        mockMap.Received(1).AddMapPolyline(Arg.Any<MapPolyline>());
    }

    [Fact]
    public void CalculateRoute_WithoutPlaces_DoesNothing()
    {
        _viewModel.CalculateRouteCommand.Execute(null);

        _routingService.DidNotReceiveWithAnyArgs().CalculateRouteAsync(default!, default!);
    }

    [Fact]
    public async Task CalculateRoute_RoutingError_SetsError()
    {
        var mockMap = Substitute.For<IMapService>();
        _viewModel.SetMapService(mockMap);

        typeof(DirectionsViewModel).GetProperty(nameof(DirectionsViewModel.OriginPlace))!
            .SetValue(_viewModel, new Place("id1", "Origin", new GeoCoordinates(0, 0)));
        typeof(DirectionsViewModel).GetProperty(nameof(DirectionsViewModel.DestinationPlace))!
            .SetValue(_viewModel, new Place("id2", "Dest", new GeoCoordinates(0, 0)));

        _routingService.CalculateRouteAsync(Arg.Any<List<Waypoint>>(), Arg.Any<RoutingOptions>())
            .Returns(new RoutingResult(RoutingError.NoRouteFound, new List<Route>()));

        _viewModel.CalculateRouteCommand.Execute(null);
        await Task.Delay(50);

        Assert.NotEqual("", _viewModel.RouteError);
        Assert.False(_viewModel.IsRouteVisible);
    }

    #endregion

    #region Route Summary

    [Fact]
    public async Task CalculateRoute_FormatsSummaryCorrectly()
    {
        var mockMap = Substitute.For<IMapService>();
        mockMap.SetCameraTargetAsync(Arg.Any<GeoCoordinates>(), Arg.Any<double>()).Returns(Task.CompletedTask);
        mockMap.AddMapPolyline(Arg.Any<MapPolyline>());
        _viewModel.SetMapService(mockMap);

        typeof(DirectionsViewModel).GetProperty(nameof(DirectionsViewModel.OriginPlace))!
            .SetValue(_viewModel, new Place("id1", "A", new GeoCoordinates(52.52, 13.405)));
        typeof(DirectionsViewModel).GetProperty(nameof(DirectionsViewModel.DestinationPlace))!
            .SetValue(_viewModel, new Place("id2", "B", new GeoCoordinates(48.135, 11.582)));

        var route = new Route("route1", new List<Section>
        {
            new(0, new GeoCoordinates(0, 0), new GeoCoordinates(0, 0), new List<Maneuver>(), SectionTransportMode.Car, 584000, 19800)
        }, 584000, 19800);
        _routingService.CalculateRouteAsync(Arg.Any<List<Waypoint>>(), Arg.Any<RoutingOptions>())
            .Returns(new RoutingResult(RoutingError.None, new List<Route> { route }));

        _viewModel.CalculateRouteCommand.Execute(null);
        await Task.Delay(50);

        Assert.Contains("km", _viewModel.RouteSummary);
        Assert.Contains("min", _viewModel.RouteSummary);
    }

    #endregion

    #region Transport Mode

    [Fact]
    public void SelectedTransportMode_DefaultsToZero()
    {
        Assert.Equal(0, _viewModel.SelectedTransportMode);
    }

    #endregion

    #region ClearRoute

    [Fact]
    public void ClearRoute_ResetsState()
    {
        _viewModel.ClearRouteCommand.Execute(null);

        Assert.False(_viewModel.IsRouteVisible);
        Assert.Null(_viewModel.CurrentRoute);
        Assert.Equal("", _viewModel.RouteSummary);
        Assert.Equal("", _viewModel.RouteError);
    }

    #endregion

    #region SetPoiDestination

    [Fact]
    public async Task SetPoiDestinationAsync_WithLocation_SetsDestinationAndOrigin()
    {
        var locationService = Substitute.For<ILocationService>();
        locationService.GetCurrentLocationAsync(Arg.Any<CancellationToken>())
            .Returns(new Location(new GeoCoordinates(52.52, 13.405), Source: LocationSource.Gps));
        var vm = new DirectionsViewModel(_routingService, _searchService, locationService);

        var poi = new Place("poi1", "Coffee Shop", new GeoCoordinates(52.53, 13.41));
        await vm.SetPoiDestinationAsync(poi);

        Assert.Equal(poi, vm.DestinationPlace);
        Assert.Equal("Coffee Shop", vm.DestinationQuery);
        Assert.NotNull(vm.OriginPlace);
        Assert.Equal(52.52, vm.OriginPlace!.Coordinates.Latitude);
        Assert.Equal("52.52000, 13.40500", vm.OriginQuery);
    }

    [Fact]
    public async Task SetPoiDestinationAsync_LocationUnavailable_ShowsRouteError()
    {
        var locationService = Substitute.For<ILocationService>();
        locationService.GetCurrentLocationAsync(Arg.Any<CancellationToken>())
            .Returns((Location?)null);
        var vm = new DirectionsViewModel(_routingService, _searchService, locationService);

        var poi = new Place("poi1", "Coffee Shop", new GeoCoordinates(52.53, 13.41));
        await vm.SetPoiDestinationAsync(poi);

        Assert.Equal(poi, vm.DestinationPlace);
        Assert.Contains("location", vm.RouteError, StringComparison.OrdinalIgnoreCase);
        Assert.Null(vm.OriginPlace);
    }

    [Fact]
    public async Task SetPoiDestinationAsync_ProgrammaticQueries_DoNotTriggerSuggest()
    {
        var locationService = Substitute.For<ILocationService>();
        locationService.GetCurrentLocationAsync(Arg.Any<CancellationToken>())
            .Returns(new Location(new GeoCoordinates(52.52, 13.405)));
        var vm = new DirectionsViewModel(_routingService, _searchService, locationService);

        var poi = new Place("poi1", "Coffee Shop Mitte", new GeoCoordinates(52.53, 13.41));
        await vm.SetPoiDestinationAsync(poi);

        await Task.Delay(400); // longer than the 300ms suggest debounce

        await _searchService.DidNotReceive().SuggestAsync(Arg.Any<TextQuery>(), Arg.Any<SearchOptions>());
        Assert.False(vm.HasDestinationSuggestions);
        Assert.False(vm.HasOriginSuggestions);
    }

    #endregion
}
