using Xunit;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;
using Here.Explore.Maui.Models.Routing;
using Here.Explore.Maui.Models.Search;
using Here.Explore.Maui.Services;
using Here.Explore.Maui.RefApp.ViewModels;
using NSubstitute;

namespace Here.Explore.Maui.RefApp.UITests.ViewModelTests;

public class DirectionsViewModelErrorStateTests
{
    private readonly IRoutingService _routingService;
    private readonly ISearchService _searchService;
    private readonly DirectionsViewModel _viewModel;

    public DirectionsViewModelErrorStateTests()
    {
        _routingService = Substitute.For<IRoutingService>();
        _searchService = Substitute.For<ISearchService>();
        _viewModel = new DirectionsViewModel(_routingService, _searchService);
    }

    #region Route Calculation — No Route Found

    [Fact]
    public async Task CalculateRoute_WhenNoRouteReturned_SetsEmptyState()
    {
        var mockMap = Substitute.For<IMapService>();
        _viewModel.SetMapService(mockMap);

        typeof(DirectionsViewModel).GetProperty(nameof(DirectionsViewModel.OriginPlace))!
            .SetValue(_viewModel, new Place("id1", "A", new GeoCoordinates(0, 0)));
        typeof(DirectionsViewModel).GetProperty(nameof(DirectionsViewModel.DestinationPlace))!
            .SetValue(_viewModel, new Place("id2", "B", new GeoCoordinates(0, 0)));

        _routingService.CalculateRouteAsync(Arg.Any<List<Waypoint>>(), Arg.Any<RoutingOptions>())
            .Returns(new RoutingResult(RoutingError.None, new List<Route>()));

        _viewModel.CalculateRouteCommand.Execute(null);
        await Task.Delay(50);

        Assert.Equal("No route found", _viewModel.EmptyStateTitle);
        Assert.NotNull(_viewModel.EmptyStateSubtitle);
        Assert.False(_viewModel.IsRouteVisible);
    }

    [Fact]
    public async Task CalculateRoute_WhenSuccessful_ClearsEmptyState()
    {
        var mockMap = Substitute.For<IMapService>();
        mockMap.SetCameraTargetAsync(Arg.Any<GeoCoordinates>(), Arg.Any<double>()).Returns(Task.CompletedTask);
        mockMap.AddMapPolyline(Arg.Any<MapPolyline>());
        _viewModel.SetMapService(mockMap);

        // Set previous empty state
        typeof(DirectionsViewModel).GetProperty(nameof(DirectionsViewModel.EmptyStateTitle))!
            .SetValue(_viewModel, "Previous empty");

        typeof(DirectionsViewModel).GetProperty(nameof(DirectionsViewModel.OriginPlace))!
            .SetValue(_viewModel, new Place("id1", "A", new GeoCoordinates(52.52, 13.405)));
        typeof(DirectionsViewModel).GetProperty(nameof(DirectionsViewModel.DestinationPlace))!
            .SetValue(_viewModel, new Place("id2", "B", new GeoCoordinates(48.135, 11.582)));

        var route = new Route("route1", new List<Section>(), 584000, 19800);
        _routingService.CalculateRouteAsync(Arg.Any<List<Waypoint>>(), Arg.Any<RoutingOptions>())
            .Returns(new RoutingResult(RoutingError.None, new List<Route> { route }));

        _viewModel.CalculateRouteCommand.Execute(null);
        await Task.Delay(50);

        Assert.Null(_viewModel.EmptyStateTitle);
    }

    #endregion

    #region Route Calculation — Service Exception

    [Fact]
    public async Task CalculateRoute_WhenExceptionThrown_SetsRouteError()
    {
        var mockMap = Substitute.For<IMapService>();
        _viewModel.SetMapService(mockMap);

        typeof(DirectionsViewModel).GetProperty(nameof(DirectionsViewModel.OriginPlace))!
            .SetValue(_viewModel, new Place("id1", "A", new GeoCoordinates(0, 0)));
        typeof(DirectionsViewModel).GetProperty(nameof(DirectionsViewModel.DestinationPlace))!
            .SetValue(_viewModel, new Place("id2", "B", new GeoCoordinates(0, 0)));

        _routingService.CalculateRouteAsync(Arg.Any<List<Waypoint>>(), Arg.Any<RoutingOptions>())
            .Returns(Task.FromException<RoutingResult>(new InvalidOperationException("Service unavailable")));

        _viewModel.CalculateRouteCommand.Execute(null);
        await Task.Delay(50);

        Assert.Contains("Service unavailable", _viewModel.RouteError);
        Assert.False(_viewModel.IsRouteVisible);
    }

    #endregion

    #region Isoline Error Handling

    [Fact]
    public async Task Isoline_WhenExceptionThrown_SetsRouteError()
    {
        var mockMap = Substitute.For<IMapService>();
        _viewModel.SetMapService(mockMap);

        _routingService.CalculateIsolineAsync(Arg.Any<GeoCoordinates>(), Arg.Any<IsolineOptions>())
            .Returns(Task.FromException<IsolineResult>(new InvalidOperationException("Isoline not supported")));

        _viewModel.ToggleIsolineModeCommand.Execute(null);
        await Task.Delay(50);

        Assert.Contains("Isoline error", _viewModel.RouteError);
        Assert.False(_viewModel.IsIsolineMode);
    }

    #endregion

    #region ClearRoute

    [Fact]
    public void ClearRoute_ClearsEmptyState()
    {
        typeof(DirectionsViewModel).GetProperty(nameof(DirectionsViewModel.EmptyStateTitle))!
            .SetValue(_viewModel, "No route found");

        _viewModel.ClearRouteCommand.Execute(null);

        Assert.Null(_viewModel.EmptyStateTitle);
    }

    #endregion

    #region Map Service Null Safety

    [Fact]
    public void CalculateRoute_WhenMapServiceNull_DoesNotCrash()
    {
        typeof(DirectionsViewModel).GetProperty(nameof(DirectionsViewModel.OriginPlace))!
            .SetValue(_viewModel, new Place("id1", "A", new GeoCoordinates(0, 0)));
        typeof(DirectionsViewModel).GetProperty(nameof(DirectionsViewModel.DestinationPlace))!
            .SetValue(_viewModel, new Place("id2", "B", new GeoCoordinates(0, 0)));

        // Don't set map service — should return early, not crash
        var exception = Record.Exception(() => _viewModel.CalculateRouteCommand.Execute(null));

        Assert.Null(exception);
    }

    #endregion
}
