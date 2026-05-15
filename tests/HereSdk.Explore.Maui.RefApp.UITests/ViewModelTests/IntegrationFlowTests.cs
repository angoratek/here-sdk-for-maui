using Xunit;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;
using Here.Explore.Maui.Models.Routing;
using Here.Explore.Maui.Models.Search;
using Here.Explore.Maui.Models.Traffic;
using Here.Explore.Maui.RefApp.Services;
using Here.Explore.Maui.Services;
using Here.Explore.Maui.RefApp.ViewModels;
using NSubstitute;

namespace Here.Explore.Maui.RefApp.UITests.ViewModelTests;

public class IntegrationFlowTests
{
    #region Explore → Directions Flow

    [Fact]
    public async Task Explore_SearchSelect_PlaceCardVisible()
    {
        // Full search → select flow
        var searchService = Substitute.For<ISearchService>();
        var locationService = Substitute.For<ILocationService>();
        var connectivityService = Substitute.For<IConnectivityService>();
        var permissionsService = Substitute.For<IPermissionsService>();
        connectivityService.IsConnected.Returns(true);
        permissionsService.RequestLocationPermissionAsync().Returns(true);

        var vm = new ExploreViewModel(searchService, locationService, connectivityService, permissionsService);

        var mockMap = Substitute.For<IMapService>();
        mockMap.GetCameraTargetAsync().Returns(new GeoCoordinates(37.77, -122.42));
        mockMap.SetCameraTargetAsync(Arg.Any<GeoCoordinates>(), Arg.Any<double>()).Returns(Task.CompletedTask);
        mockMap.AddMapMarker(Arg.Any<MapMarker>());
        vm.SetMapService(mockMap);

        // Step 1: Search query triggers suggest
        var suggestions = new List<Suggestion>
        {
            new("Golden Gate Bridge", "place1", SuggestionType.Place),
            new("San Francisco", "place2", SuggestionType.Place)
        };
        searchService.SuggestAsync(Arg.Any<TextQuery>(), Arg.Any<SearchOptions>())
            .Returns(new SuggestResult(SearchError.None, suggestions));

        vm.SearchQuery = "Golden";
        await Task.Delay(400);

        Assert.True(vm.HasSuggestions);
        Assert.Equal(2, vm.Suggestions.Count);

        // Step 2: Select suggestion → PlaceCard visible
        var place = new Place("place1", "Golden Gate Bridge", new GeoCoordinates(37.8199, -122.4783));
        searchService.GetPlaceByIdAsync("place1").Returns(place);

        vm.SelectSuggestionCommand.Execute(suggestions[0]);
        await Task.Delay(50);

        Assert.Equal("Golden Gate Bridge", vm.SearchQuery);
        Assert.True(vm.IsPlaceCardVisible);
        Assert.NotNull(vm.SelectedPlace);
        await mockMap.Received(1).SetCameraTargetAsync(
            Arg.Is<GeoCoordinates>(c => c.Latitude > 37 && c.Longitude < -122), 15);
    }

    [Fact]
    public async Task Directions_SetBothPlaces_CalculateRoute()
    {
        // Full routing flow
        var routingService = Substitute.For<IRoutingService>();
        var searchService = Substitute.For<ISearchService>();
        var vm = new DirectionsViewModel(routingService, searchService);

        var mockMap = Substitute.For<IMapService>();
        mockMap.SetCameraTargetAsync(Arg.Any<GeoCoordinates>(), Arg.Any<double>()).Returns(Task.CompletedTask);
        mockMap.AddMapPolyline(Arg.Any<MapPolyline>());
        mockMap.AddMapMarker(Arg.Any<MapMarker>());
        vm.SetMapService(mockMap);

        // Step 1: Set origin
        typeof(DirectionsViewModel).GetProperty(nameof(DirectionsViewModel.OriginPlace))!
            .SetValue(vm, new Place("id1", "SF", new GeoCoordinates(37.7749, -122.4194)));

        // Step 2: Set destination
        typeof(DirectionsViewModel).GetProperty(nameof(DirectionsViewModel.DestinationPlace))!
            .SetValue(vm, new Place("id2", "Oakland", new GeoCoordinates(37.8044, -122.2711)));

        // Step 3: Calculate route
        var sectionGeometry = new List<GeoCoordinates>
        {
            new(37.7749, -122.4194),
            new(37.8044, -122.2711)
        };
        var section = new Section(0,
            new GeoCoordinates(37.7749, -122.4194),
            new GeoCoordinates(37.8044, -122.2711),
            Array.Empty<Maneuver>(),
            SectionTransportMode.Car,
            18000, 1200,
            sectionGeometry);
        var route = new Route("route1",
            new List<Section> { section }, 18000, 1200);
        routingService.CalculateRouteAsync(Arg.Any<List<Waypoint>>(), Arg.Any<RoutingOptions>())
            .Returns(new RoutingResult(RoutingError.None, new List<Route> { route }));

        vm.CalculateRouteCommand.Execute(null);
        await Task.Delay(50);

        Assert.True(vm.IsRouteVisible);
        Assert.NotNull(vm.CurrentRoute);
        mockMap.Received(1).AddMapPolyline(Arg.Any<MapPolyline>());
    }

    [Fact]
    public async Task Explore_NavigateToDirections_SetsDestination()
    {
        var searchService = Substitute.For<ISearchService>();
        var locationService = Substitute.For<ILocationService>();
        var connectivityService = Substitute.For<IConnectivityService>();
        var permissionsService = Substitute.For<IPermissionsService>();
        connectivityService.IsConnected.Returns(true);
        permissionsService.RequestLocationPermissionAsync().Returns(true);

        var vm = new ExploreViewModel(searchService, locationService, connectivityService, permissionsService);

        // Set a selected place
        var place = new Place("place1", "Golden Gate Bridge", new GeoCoordinates(37.8199, -122.4783));
        typeof(ExploreViewModel).GetProperty(nameof(ExploreViewModel.SelectedPlace))!
            .SetValue(vm, place);

        // Navigate command should be available
        Assert.NotNull(vm.NavigateToDirectionsCommand);
        Assert.True(vm.NavigateToDirectionsCommand.CanExecute(null));
    }

    #endregion

    #region Tools Drawing Flow

    [Fact]
    public void Tools_Drawing_Marker_Flow()
    {
        var themeService = Substitute.For<Here.Explore.Maui.RefApp.Services.IThemeService>();
        var vm = new ToolsViewModel(themeService);

        var mockMap = Substitute.For<IMapService>();
        vm.SetMapService(mockMap);

        // Step 1: Select Marker tool
        vm.SetDrawingToolCommand.Execute("Marker");
        Assert.True(vm.IsDrawingActive);

        // Step 2: Simulate map tap (via event)
        mockMap.MapTapped += Raise.Event<EventHandler<MapTappedEventArgs>>(
            null, new MapTappedEventArgs(new GeoCoordinates(37.77, -122.42), new Point2D(100, 200)));

        // After marker placement (single tap = finish), tool is done
        Assert.False(vm.IsDrawingActive);
        mockMap.Received(1).AddMapMarker(Arg.Any<MapMarker>());
    }

    [Fact]
    public void Tools_Drawing_Polyline_Flow()
    {
        var themeService = Substitute.For<Here.Explore.Maui.RefApp.Services.IThemeService>();
        var vm = new ToolsViewModel(themeService);

        var mockMap = Substitute.For<IMapService>();
        vm.SetMapService(mockMap);

        // Step 1: Select Polyline tool
        vm.SetDrawingToolCommand.Execute("Polyline");
        Assert.True(vm.IsDrawingActive);

        // Step 2: Add vertices via map taps
        mockMap.MapTapped += Raise.Event<EventHandler<MapTappedEventArgs>>(
            null, new MapTappedEventArgs(new GeoCoordinates(37.77, -122.42), new Point2D(100, 200)));
        mockMap.MapTapped += Raise.Event<EventHandler<MapTappedEventArgs>>(
            null, new MapTappedEventArgs(new GeoCoordinates(37.79, -122.44), new Point2D(150, 250)));

        // Two vertices added, preview should be rendered
        Assert.Equal(2, vm.DrawingPointCount);
        mockMap.Received(1).AddMapPolyline(Arg.Any<MapPolyline>()); // preview

        // Step 3: Double-tap to finish
        mockMap.MapDoubleTapped += Raise.Event<EventHandler<MapTappedEventArgs>>(
            null, new MapTappedEventArgs(new GeoCoordinates(37.79, -122.44), new Point2D(150, 250)));

        Assert.False(vm.IsDrawingActive);
    }

    [Fact]
    public void Tools_ClearAll_RemovesAllObjects()
    {
        var themeService = Substitute.For<Here.Explore.Maui.RefApp.Services.IThemeService>();
        var vm = new ToolsViewModel(themeService);
        var mockMap = Substitute.For<IMapService>();
        vm.SetMapService(mockMap);

        // Add markers via drawing
        vm.SetDrawingToolCommand.Execute("Marker");
        mockMap.MapTapped += Raise.Event<EventHandler<MapTappedEventArgs>>(
            null, new MapTappedEventArgs(new GeoCoordinates(37.77, -122.42), new Point2D(100, 200)));

        Assert.Equal(1, vm.TotalObjectCount);

        // Clear all
        vm.ClearAllCommand.Execute(null);

        Assert.Equal(0, vm.TotalObjectCount);
        mockMap.Received(1).RemoveMapMarker(Arg.Any<MapMarker>());
    }

    #endregion

    #region Traffic Full Flow

    [Fact]
    public async Task Traffic_ToggleFlow_ToggleIncidents_Refresh()
    {
        var trafficService = Substitute.For<ITrafficService>();
        var vm = new TrafficViewModel(trafficService);

        var mockMap = Substitute.For<IMapService>();
        mockMap.GetCameraTargetAsync().Returns(new GeoCoordinates(52.53, 13.39));
        mockMap.AddMapPolyline(Arg.Any<MapPolyline>());
        mockMap.AddMapMarker(Arg.Any<MapMarker>());
        vm.SetMapService(mockMap);

        // Step 1: Toggle flow on
        var geometry = new GeoPolyline(new List<GeoCoordinates>
        {
            new(52.53, 13.39), new(52.54, 13.40)
        });
        var flow = new TrafficFlow(3.0, 15.0, geometry);
        trafficService.QueryFlowAsync(Arg.Any<GeoCircle>(), Arg.Any<TrafficFlowQueryOptions>())
            .Returns(new TrafficFlowResult(TrafficQueryError.None, new List<TrafficFlow> { flow }));

        vm.ToggleFlowCommand.Execute(null);
        await Task.Delay(50);

        Assert.True(vm.IsFlowVisible);
        Assert.True(vm.IsIncidentsVisible); // unchanged

        // Step 2: Toggle incidents off
        trafficService.ClearReceivedCalls();
        vm.ToggleIncidentsCommand.Execute(null);

        Assert.False(vm.IsIncidentsVisible);
        // No additional query since toggling off
        await trafficService.DidNotReceive().QueryIncidentsAsync(
            Arg.Any<GeoCircle>(), Arg.Any<TrafficIncidentsQueryOptions>());

        // Step 3: Select an incident
        var incident = new TrafficIncident("inc1", "Accident", TrafficIncidentType.Accident,
            TrafficIncidentImpact.Major);
        trafficService.LookupIncidentAsync("inc1", Arg.Any<TrafficIncidentLookupOptions>())
            .Returns(incident);

        vm.SelectIncidentCommand.Execute(incident);
        await Task.Delay(50);

        Assert.NotNull(vm.SelectedIncident);
        await trafficService.Received(1).LookupIncidentAsync("inc1",
            Arg.Any<TrafficIncidentLookupOptions>());
    }

    #endregion

    #region Directions Advanced Flows

    [Fact]
    public async Task Directions_RouteWithAlternatives_DrawsBothLines()
    {
        var routingService = Substitute.For<IRoutingService>();
        var searchService = Substitute.For<ISearchService>();
        var vm = new DirectionsViewModel(routingService, searchService);

        var mockMap = Substitute.For<IMapService>();
        mockMap.SetCameraTargetAsync(Arg.Any<GeoCoordinates>(), Arg.Any<double>()).Returns(Task.CompletedTask);
        mockMap.AddMapPolyline(Arg.Any<MapPolyline>());
        mockMap.AddMapMarker(Arg.Any<MapMarker>());
        vm.SetMapService(mockMap);

        typeof(DirectionsViewModel).GetProperty(nameof(DirectionsViewModel.OriginPlace))!
            .SetValue(vm, new Place("id1", "SF", new GeoCoordinates(37.7749, -122.4194)));
        typeof(DirectionsViewModel).GetProperty(nameof(DirectionsViewModel.DestinationPlace))!
            .SetValue(vm, new Place("id2", "Oakland", new GeoCoordinates(37.8044, -122.2711)));

        var mainGeom = new List<GeoCoordinates> { new(37.7749, -122.4194), new(37.78, -122.40), new(37.8044, -122.2711) };
        var altGeom = new List<GeoCoordinates> { new(37.7749, -122.4194), new(37.79, -122.35), new(37.8044, -122.2711) };

        var mainSection = new Section(0, mainGeom[0], mainGeom[^1], Array.Empty<Maneuver>(),
            SectionTransportMode.Car, 18000, 1200, mainGeom);
        var altSection = new Section(0, altGeom[0], altGeom[^1], Array.Empty<Maneuver>(),
            SectionTransportMode.Car, 20000, 1400, altGeom);

        routingService.CalculateRouteAsync(Arg.Any<List<Waypoint>>(), Arg.Any<RoutingOptions>())
            .Returns(new RoutingResult(RoutingError.None, new List<Route>
            {
                new("main", new List<Section> { mainSection }, 18000, 1200),
                new("alt", new List<Section> { altSection }, 20000, 1400),
            }));

        vm.CalculateRouteCommand.Execute(null);
        await Task.Delay(50);

        Assert.True(vm.IsRouteVisible);
        Assert.Single(vm.AlternativeRoutes);
        mockMap.Received(2).AddMapPolyline(Arg.Any<MapPolyline>()); // main + alt
    }

    [Fact]
    public async Task Directions_RoutingError_ShowsError()
    {
        var routingService = Substitute.For<IRoutingService>();
        var searchService = Substitute.For<ISearchService>();
        var vm = new DirectionsViewModel(routingService, searchService);

        var mockMap = Substitute.For<IMapService>();
        vm.SetMapService(mockMap);

        typeof(DirectionsViewModel).GetProperty(nameof(DirectionsViewModel.OriginPlace))!
            .SetValue(vm, new Place("id1", "SF", new GeoCoordinates(37.7749, -122.4194)));
        typeof(DirectionsViewModel).GetProperty(nameof(DirectionsViewModel.DestinationPlace))!
            .SetValue(vm, new Place("id2", "Oakland", new GeoCoordinates(37.8044, -122.2711)));

        routingService.CalculateRouteAsync(Arg.Any<List<Waypoint>>(), Arg.Any<RoutingOptions>())
            .Returns(new RoutingResult(RoutingError.NetworkError, new List<Route>()));

        vm.CalculateRouteCommand.Execute(null);
        await Task.Delay(50);

        Assert.False(vm.IsRouteVisible);
        Assert.NotEmpty(vm.RouteError);
        Assert.Contains("NetworkError", vm.RouteError);
    }

    [Fact]
    public async Task Directions_EmptyRoutes_ShowsEmptyState()
    {
        var routingService = Substitute.For<IRoutingService>();
        var searchService = Substitute.For<ISearchService>();
        var vm = new DirectionsViewModel(routingService, searchService);

        var mockMap = Substitute.For<IMapService>();
        vm.SetMapService(mockMap);

        typeof(DirectionsViewModel).GetProperty(nameof(DirectionsViewModel.OriginPlace))!
            .SetValue(vm, new Place("id1", "SF", new GeoCoordinates(37.7749, -122.4194)));
        typeof(DirectionsViewModel).GetProperty(nameof(DirectionsViewModel.DestinationPlace))!
            .SetValue(vm, new Place("id2", "Far", new GeoCoordinates(0, 0)));

        routingService.CalculateRouteAsync(Arg.Any<List<Waypoint>>(), Arg.Any<RoutingOptions>())
            .Returns(new RoutingResult(RoutingError.None, new List<Route>()));

        vm.CalculateRouteCommand.Execute(null);
        await Task.Delay(50);

        Assert.False(vm.IsRouteVisible);
        Assert.Null(vm.CurrentRoute);
        Assert.NotNull(vm.EmptyStateTitle);
    }

    [Fact]
    public void Directions_SwapLocations_ExchangesPlaces()
    {
        var routingService = Substitute.For<IRoutingService>();
        var searchService = Substitute.For<ISearchService>();
        var vm = new DirectionsViewModel(routingService, searchService);

        typeof(DirectionsViewModel).GetProperty(nameof(DirectionsViewModel.OriginPlace))!
            .SetValue(vm, new Place("id1", "SF", new GeoCoordinates(37.7749, -122.4194)));
        typeof(DirectionsViewModel).GetProperty(nameof(DirectionsViewModel.DestinationPlace))!
            .SetValue(vm, new Place("id2", "NYC", new GeoCoordinates(40.7128, -74.0060)));

        vm.SwapLocationsCommand.Execute(null);

        Assert.Equal("NYC", vm.OriginPlace?.Title);
        Assert.Equal("SF", vm.DestinationPlace?.Title);
    }

    [Fact]
    public void Directions_ClearRoute_ResetsState()
    {
        var routingService = Substitute.For<IRoutingService>();
        var searchService = Substitute.For<ISearchService>();
        var vm = new DirectionsViewModel(routingService, searchService);

        var mockMap = Substitute.For<IMapService>();
        vm.SetMapService(mockMap);

        // Set up route state
        typeof(DirectionsViewModel).GetProperty(nameof(DirectionsViewModel.IsRouteVisible))!
            .SetValue(vm, true);
        typeof(DirectionsViewModel).GetProperty(nameof(DirectionsViewModel.RouteSummary))!
            .SetValue(vm, "Test summary");

        vm.ClearRouteCommand.Execute(null);

        Assert.False(vm.IsRouteVisible);
        Assert.Equal("", vm.RouteSummary);
        Assert.Null(vm.CurrentRoute);
    }

    #endregion

    #region Explore Advanced Flows

    [Fact]
    public async Task Explore_CategorySearch_PlacesMarkers()
    {
        var searchService = Substitute.For<ISearchService>();
        var locationService = Substitute.For<ILocationService>();
        var connectivityService = Substitute.For<IConnectivityService>();
        var permissionsService = Substitute.For<IPermissionsService>();
        connectivityService.IsConnected.Returns(true);

        var vm = new ExploreViewModel(searchService, locationService, connectivityService, permissionsService);
        var mockMap = Substitute.For<IMapService>();
        mockMap.GetCameraTargetAsync().Returns(new GeoCoordinates(37.77, -122.42));
        mockMap.SetCameraTargetAsync(Arg.Any<GeoCoordinates>(), Arg.Any<double>()).Returns(Task.CompletedTask);
        mockMap.AddMapMarker(Arg.Any<MapMarker>());
        vm.SetMapService(mockMap);

        searchService.SearchAsync(Arg.Any<CategoryQuery>(), Arg.Any<SearchOptions>())
            .Returns(new SearchResult(SearchError.None, new List<Place>
            {
                new("id1", "Place1", new GeoCoordinates(37.77, -122.42)),
            }));

        vm.SearchCategoryCommand.Execute("restaurant");
        await Task.Delay(50);

        mockMap.Received().AddMapMarker(Arg.Any<MapMarker>());
    }

    [Fact]
    public async Task Explore_CategorySearch_NoResults_ShowsEmptyState()
    {
        var searchService = Substitute.For<ISearchService>();
        var locationService = Substitute.For<ILocationService>();
        var connectivityService = Substitute.For<IConnectivityService>();
        var permissionsService = Substitute.For<IPermissionsService>();
        connectivityService.IsConnected.Returns(true);

        var vm = new ExploreViewModel(searchService, locationService, connectivityService, permissionsService);
        var mockMap = Substitute.For<IMapService>();
        mockMap.GetCameraTargetAsync().Returns(new GeoCoordinates(37.77, -122.42));
        vm.SetMapService(mockMap);

        searchService.SearchAsync(Arg.Any<CategoryQuery>(), Arg.Any<SearchOptions>())
            .Returns(new SearchResult(SearchError.None, new List<Place>()));

        vm.SearchCategoryCommand.Execute("restaurant");
        await Task.Delay(50);

        Assert.NotNull(vm.EmptyStateTitle);
        Assert.Equal("No places found", vm.EmptyStateTitle);
        Assert.Contains("restaurant", vm.EmptyStateSubtitle!);
    }

    [Fact]
    public void Explore_ClearSearch_ResetsAllState()
    {
        var searchService = Substitute.For<ISearchService>();
        var locationService = Substitute.For<ILocationService>();
        var connectivityService = Substitute.For<IConnectivityService>();
        var permissionsService = Substitute.For<IPermissionsService>();
        connectivityService.IsConnected.Returns(true);

        var vm = new ExploreViewModel(searchService, locationService, connectivityService, permissionsService);
        var mockMap = Substitute.For<IMapService>();
        vm.SetMapService(mockMap);

        // Set all state
        vm.SearchQuery = "Test";
        typeof(ExploreViewModel).GetProperty(nameof(ExploreViewModel.SelectedPlace))!
            .SetValue(vm, new Place("id", "Title", new GeoCoordinates(0, 0)));
        typeof(ExploreViewModel).GetProperty(nameof(ExploreViewModel.IsPlaceCardVisible))!
            .SetValue(vm, true);

        vm.ClearSearchCommand.Execute(null);

        Assert.Equal("", vm.SearchQuery);
        Assert.Null(vm.SelectedPlace);
        Assert.False(vm.IsPlaceCardVisible);
    }

    [Fact]
    public void Explore_OfflineDetection_SetsOfflineState()
    {
        var searchService = Substitute.For<ISearchService>();
        var locationService = Substitute.For<ILocationService>();
        var connectivityService = Substitute.For<IConnectivityService>();
        var permissionsService = Substitute.For<IPermissionsService>();
        connectivityService.IsConnected.Returns(true);

        var vm = new ExploreViewModel(searchService, locationService, connectivityService, permissionsService);
        Assert.False(vm.IsOffline);

        // Simulate connectivity drop
        connectivityService.ConnectivityChanged += Raise.Event<EventHandler<bool>>(null, false);

        Assert.True(vm.IsOffline);
        Assert.Contains("offline", vm.SearchErrorMessage ?? "");
    }

    [Fact]
    public void Explore_ConnectivityRestored_ClearsOfflineState()
    {
        var searchService = Substitute.For<ISearchService>();
        var locationService = Substitute.For<ILocationService>();
        var connectivityService = Substitute.For<IConnectivityService>();
        var permissionsService = Substitute.For<IPermissionsService>();
        connectivityService.IsConnected.Returns(false);

        var vm = new ExploreViewModel(searchService, locationService, connectivityService, permissionsService);
        Assert.True(vm.IsOffline);

        connectivityService.ConnectivityChanged += Raise.Event<EventHandler<bool>>(null, true);

        Assert.False(vm.IsOffline);
        Assert.Null(vm.SearchErrorMessage);
    }

    #endregion

    #region Tools Advanced Flows

    [Fact]
    public void Tools_Drawing_Polygon_Flow()
    {
        var themeService = Substitute.For<IThemeService>();
        var vm = new ToolsViewModel(themeService);
        var mockMap = Substitute.For<IMapService>();
        vm.SetMapService(mockMap);

        vm.SetDrawingToolCommand.Execute("Polygon");
        Assert.True(vm.IsDrawingActive);

        // Add 3 vertices
        mockMap.MapTapped += Raise.Event<EventHandler<MapTappedEventArgs>>(
            null, new MapTappedEventArgs(new GeoCoordinates(37.77, -122.42), new Point2D(100, 200)));
        mockMap.MapTapped += Raise.Event<EventHandler<MapTappedEventArgs>>(
            null, new MapTappedEventArgs(new GeoCoordinates(37.78, -122.43), new Point2D(150, 250)));
        mockMap.MapTapped += Raise.Event<EventHandler<MapTappedEventArgs>>(
            null, new MapTappedEventArgs(new GeoCoordinates(37.79, -122.44), new Point2D(200, 300)));

        Assert.Equal(3, vm.DrawingPointCount);

        // Double-tap to finish
        mockMap.MapDoubleTapped += Raise.Event<EventHandler<MapTappedEventArgs>>(
            null, new MapTappedEventArgs(new GeoCoordinates(37.79, -122.44), new Point2D(200, 300)));

        Assert.False(vm.IsDrawingActive);
        mockMap.Received(2).AddMapPolygon(Arg.Any<MapPolygon>()); // preview + final
    }

    [Fact]
    public void Tools_Drawing_Circle_Flow()
    {
        var themeService = Substitute.For<IThemeService>();
        var vm = new ToolsViewModel(themeService);
        var mockMap = Substitute.For<IMapService>();
        vm.SetMapService(mockMap);

        vm.SetDrawingToolCommand.Execute("Circle");
        Assert.True(vm.IsDrawingActive);

        // First tap: center
        mockMap.MapTapped += Raise.Event<EventHandler<MapTappedEventArgs>>(
            null, new MapTappedEventArgs(new GeoCoordinates(37.77, -122.42), new Point2D(100, 200)));
        Assert.Equal(1, vm.DrawingPointCount);

        // Second tap: radius edge
        mockMap.MapTapped += Raise.Event<EventHandler<MapTappedEventArgs>>(
            null, new MapTappedEventArgs(new GeoCoordinates(37.78, -122.43), new Point2D(150, 250)));

        // Double-tap to finish
        mockMap.MapDoubleTapped += Raise.Event<EventHandler<MapTappedEventArgs>>(
            null, new MapTappedEventArgs(new GeoCoordinates(37.78, -122.43), new Point2D(150, 250)));

        Assert.False(vm.IsDrawingActive);
        mockMap.Received(2).AddMapCircle(Arg.Any<MapCircle>()); // preview + final
    }

    [Fact]
    public void Tools_DrawingTool_ToggleOff_WhenReselected()
    {
        var themeService = Substitute.For<IThemeService>();
        var vm = new ToolsViewModel(themeService);

        vm.SetDrawingToolCommand.Execute("Marker");
        Assert.True(vm.IsDrawingActive);

        // Select same tool again — should toggle off
        vm.SetDrawingToolCommand.Execute("Marker");
        Assert.False(vm.IsDrawingActive);
    }

    [Fact]
    public void Tools_Gallery_PresetsAllExist()
    {
        var themeService = Substitute.For<IThemeService>();
        var vm = new ToolsViewModel(themeService);
        Assert.Equal(5, vm.DemoPresets.Count);
        Assert.Contains(vm.DemoPresets, p => p.Name == "SF Landmarks");
        Assert.Contains(vm.DemoPresets, p => p.Name == "Route Network");
        Assert.Contains(vm.DemoPresets, p => p.Name == "District Boundaries");
        Assert.Contains(vm.DemoPresets, p => p.Name == "Concentric Circles");
        Assert.Contains(vm.DemoPresets, p => p.Name == "Marker Cluster");
    }

    #endregion
}
