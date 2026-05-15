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
using HereLocation = Here.Explore.Maui.Models.Location;

namespace Here.Explore.Maui.RefApp.UITests.ViewModelTests;

public class EdgeCaseTests
{
    #region Rapid Search Input — Debounce

    [Fact]
    public async Task RapidSearchInput_OnlyLastQueryExecutes()
    {
        var searchService = Substitute.For<ISearchService>();
        var locationService = Substitute.For<ILocationService>();
        var connectivityService = Substitute.For<IConnectivityService>();
        var permissionsService = Substitute.For<IPermissionsService>();
        connectivityService.IsConnected.Returns(true);

        var vm = new ExploreViewModel(searchService, locationService, connectivityService, permissionsService);

        searchService.SuggestAsync(Arg.Any<TextQuery>(), Arg.Any<SearchOptions>())
            .Returns(new SuggestResult(SearchError.None, new List<Suggestion>()));

        // Rapidly change query multiple times
        vm.SearchQuery = "S";
        vm.SearchQuery = "Sa";
        vm.SearchQuery = "San";
        vm.SearchQuery = "San ";

        await Task.Delay(500);

        // Only the last suggest should execute (debounce cancels earlier)
        await searchService.Received(1).SuggestAsync(
            Arg.Any<TextQuery>(),
            Arg.Any<SearchOptions>());
    }

    [Fact]
    public async Task RapidSearchInput_CancelledDuringDelay()
    {
        var searchService = Substitute.For<ISearchService>();
        var locationService = Substitute.For<ILocationService>();
        var connectivityService = Substitute.For<IConnectivityService>();
        var permissionsService = Substitute.For<IPermissionsService>();
        connectivityService.IsConnected.Returns(true);

        searchService.SuggestAsync(Arg.Any<TextQuery>(), Arg.Any<SearchOptions>())
            .Returns(async call =>
            {
                await Task.Delay(200);
                return new SuggestResult(SearchError.None, new List<Suggestion>());
            });

        var vm = new ExploreViewModel(searchService, locationService, connectivityService, permissionsService);

        vm.SearchQuery = "San Francisco";
        await Task.Delay(100);
        // Clear before result returns
        vm.SearchQuery = "";
        await Task.Delay(500);

        // Should not have suggestions from cancelled query
        Assert.False(vm.HasSuggestions);
    }

    #endregion

    #region Concurrent SubmitSearch

    [Fact]
    public async Task ConcurrentSubmitSearch_HandlesOverlapGracefully()
    {
        var searchService = Substitute.For<ISearchService>();
        var locationService = Substitute.For<ILocationService>();
        var connectivityService = Substitute.For<IConnectivityService>();
        var permissionsService = Substitute.For<IPermissionsService>();
        connectivityService.IsConnected.Returns(true);

        var vm = new ExploreViewModel(searchService, locationService, connectivityService, permissionsService);
        var mockMap = Substitute.For<IMapService>();
        mockMap.GetCameraTargetAsync().Returns(new GeoCoordinates(37.77, -122.42));
        mockMap.AddMapMarker(Arg.Any<MapMarker>());
        vm.SetMapService(mockMap);

        // First search takes longer, second returns immediately
        var tcs1 = new TaskCompletionSource<SearchResult>();
        var tcs2 = new TaskCompletionSource<SearchResult>();

        searchService.SearchAsync(
                Arg.Is<TextQuery>(q => q.Query == "query1"), Arg.Any<SearchOptions>())
            .Returns(tcs1.Task);
        searchService.SearchAsync(
                Arg.Is<TextQuery>(q => q.Query == "query2"), Arg.Any<SearchOptions>())
            .Returns(tcs2.Task);

        vm.SearchQuery = "query1";
        vm.SubmitSearchCommand.Execute(null);

        vm.SearchQuery = "query2";
        vm.SubmitSearchCommand.Execute(null);

        // First search finishes late
        tcs1.SetResult(new SearchResult(SearchError.None, new List<Place> { new("id1", "Old", new GeoCoordinates(0, 0)) }));

        // But second search result is what we see
        var place = new Place("id2", "Current", new GeoCoordinates(37.77, -122.42));
        tcs2.SetResult(new SearchResult(SearchError.None, new List<Place> { place }));

        await Task.Delay(50);

        // Second search markers rendered
        mockMap.Received().AddMapMarker(Arg.Any<MapMarker>());
    }

    #endregion

    #region Empty String Queries

    [Fact]
    public void SubmitSearch_WithEmptyString_DoesNothing()
    {
        var searchService = Substitute.For<ISearchService>();
        var locationService = Substitute.For<ILocationService>();
        var connectivityService = Substitute.For<IConnectivityService>();
        var permissionsService = Substitute.For<IPermissionsService>();

        var vm = new ExploreViewModel(searchService, locationService, connectivityService, permissionsService);
        vm.SubmitSearchCommand.Execute(null);

        searchService.DidNotReceiveWithAnyArgs().SearchAsync((TextQuery)default!, (SearchOptions)default!);
    }

    [Fact]
    public void SubmitSearch_WithWhitespaceOnly_DoesNothing()
    {
        var searchService = Substitute.For<ISearchService>();
        var locationService = Substitute.For<ILocationService>();
        var connectivityService = Substitute.For<IConnectivityService>();
        var permissionsService = Substitute.For<IPermissionsService>();

        var vm = new ExploreViewModel(searchService, locationService, connectivityService, permissionsService);
        vm.SearchQuery = "   ";
        vm.SubmitSearchCommand.Execute(null);

        searchService.DidNotReceiveWithAnyArgs().SearchAsync((TextQuery)default!, (SearchOptions)default!);
    }

    #endregion

    #region Null Service During Command

    [Fact]
    public async Task ExploreViewModel_SubmitSearch_WithNullSearchService_DoesNotCrash()
    {
        // ViewModel requires non-null services in constructor, but we test
        // null return from services, not null service injection
        var searchService = Substitute.For<ISearchService>();
        var locationService = Substitute.For<ILocationService>();
        var connectivityService = Substitute.For<IConnectivityService>();
        var permissionsService = Substitute.For<IPermissionsService>();
        connectivityService.IsConnected.Returns(true);

        var vm = new ExploreViewModel(searchService, locationService, connectivityService, permissionsService);

        // Map service is null — SubmitSearch should return early
        vm.SearchQuery = "SF";
        var exception = Record.Exception(() => vm.SubmitSearchCommand.Execute(null));

        Assert.Null(exception);
    }

    [Fact]
    public void ChangeScheme_WithNullMapService_DoesNotCrash()
    {
        var searchService = Substitute.For<ISearchService>();
        var locationService = Substitute.For<ILocationService>();
        var connectivityService = Substitute.For<IConnectivityService>();
        var permissionsService = Substitute.For<IPermissionsService>();
        connectivityService.IsConnected.Returns(true);

        var vm = new ExploreViewModel(searchService, locationService, connectivityService, permissionsService);

        // Map service not set
        var exception = Record.Exception(() => vm.ChangeSchemeCommand.Execute("SatelliteDay"));

        Assert.Null(exception);
    }

    [Fact]
    public async Task Zoom_WithNullMapService_DoesNotCrash()
    {
        var searchService = Substitute.For<ISearchService>();
        var locationService = Substitute.For<ILocationService>();
        var connectivityService = Substitute.For<IConnectivityService>();
        var permissionsService = Substitute.For<IPermissionsService>();
        connectivityService.IsConnected.Returns(true);

        var vm = new ExploreViewModel(searchService, locationService, connectivityService, permissionsService);

        var exception = await Record.ExceptionAsync(async () =>
        {
            vm.ZoomInCommand.Execute(null);
            vm.ZoomOutCommand.Execute(null);
            await Task.Delay(50);
        });

        Assert.Null(exception);
    }

    #endregion

    #region Tools Edge Cases

    [Fact]
    public void SetDrawingTool_InvalidToolName_DoesNotCrash()
    {
        var themeService = Substitute.For<IThemeService>();
        var vm = new ToolsViewModel(themeService);

        var exception = Record.Exception(() => vm.SetDrawingToolCommand.Execute("InvalidTool"));

        Assert.Null(exception);
        Assert.False(vm.IsDrawingActive);
    }

    [Fact]
    public void FinishDrawing_WithoutEnoughPoints_DoesNotCrash()
    {
        var themeService = Substitute.For<IThemeService>();
        var vm = new ToolsViewModel(themeService);
        var mockMap = Substitute.For<IMapService>();
        vm.SetMapService(mockMap);

        // Set polyline but only one point
        vm.SetDrawingToolCommand.Execute("Polyline");
        mockMap.MapTapped += Raise.Event<EventHandler<MapTappedEventArgs>>(
            null, new MapTappedEventArgs(new GeoCoordinates(37.77, -122.42), new Point2D(100, 200)));

        // Try to finish with only 1 point
        var exception = Record.Exception(() => vm.CancelDrawingCommand.Execute(null));
        Assert.Null(exception);
    }

    [Fact]
    public async Task ChangeScheme_InvalidName_DoesNotCrash()
    {
        var themeService = Substitute.For<IThemeService>();
        var vm = new ToolsViewModel(themeService);
        var mockMap = Substitute.For<IMapService>();
        mockMap.LoadSceneAsync(Arg.Any<MapScheme>()).Returns(Task.CompletedTask);
        vm.SetMapService(mockMap);

        var exception = await Record.ExceptionAsync(async () =>
        {
            vm.ChangeSchemeCommand.Execute("InvalidScheme");
            await Task.Delay(50);
        });

        Assert.Null(exception);
    }

    #endregion

    #region Traffic Edge Cases

    [Fact]
    public async Task SelectIncident_LookupServiceFails_DoesNotCrash()
    {
        var trafficService = Substitute.For<ITrafficService>();
        trafficService.LookupIncidentAsync("inc1", Arg.Any<TrafficIncidentLookupOptions>())
            .Returns(Task.FromException<TrafficIncident>(new InvalidOperationException("Lookup failed")));

        var vm = new TrafficViewModel(trafficService);
        var incident = new TrafficIncident("inc1", "Accident", TrafficIncidentType.Accident,
            TrafficIncidentImpact.Major);

        var exception = await Record.ExceptionAsync(async () =>
        {
            vm.SelectIncidentCommand.Execute(incident);
            await Task.Delay(50);
        });

        Assert.Null(exception);
        // Original incident should still be set
        Assert.NotNull(vm.SelectedIncident);
    }

    [Fact]
    public async Task FlowGeometry_InvalidVertices_SkipsRendering()
    {
        var mockMap = Substitute.For<IMapService>();
        mockMap.GetCameraTargetAsync().Returns(new GeoCoordinates(52.53, 13.39));
        var trafficService = Substitute.For<ITrafficService>();
        var vm = new TrafficViewModel(trafficService);
        vm.SetMapService(mockMap);

        // Flow with only 1 vertex (invalid for polyline)
        var geometry = new GeoPolyline(new List<GeoCoordinates> { new(52.53, 13.39) });
        var flow = new TrafficFlow(3.0, 15.0, geometry);
        trafficService.QueryFlowAsync(Arg.Any<GeoCircle>(), Arg.Any<TrafficFlowQueryOptions>())
            .Returns(new TrafficFlowResult(TrafficQueryError.None, new List<TrafficFlow> { flow }));

        vm.ToggleFlowCommand.Execute(null);
        await Task.Delay(50);

        // Flow count is 1, but no polyline rendered due to insufficient vertices
        Assert.Equal(1, vm.FlowCount);
        mockMap.DidNotReceive().AddMapPolyline(Arg.Any<MapPolyline>());
    }

    #endregion

    #region Directions Edge Cases

    [Fact]
    public async Task CalculateRoute_WithoutOrigin_DoesNothing()
    {
        var routingService = Substitute.For<IRoutingService>();
        var searchService = Substitute.For<ISearchService>();
        var vm = new DirectionsViewModel(routingService, searchService);

        var mockMap = Substitute.For<IMapService>();
        vm.SetMapService(mockMap);

        // Only destination set, no origin
        typeof(DirectionsViewModel).GetProperty(nameof(DirectionsViewModel.DestinationPlace))!
            .SetValue(vm, new Place("id2", "Oakland", new GeoCoordinates(37.8044, -122.2711)));

        vm.CalculateRouteCommand.Execute(null);
        await Task.Delay(50);

        await routingService.DidNotReceive().CalculateRouteAsync(
            Arg.Any<List<Waypoint>>(), Arg.Any<RoutingOptions>());
    }

    [Fact]
    public async Task CalculateRoute_WithoutDestination_DoesNothing()
    {
        var routingService = Substitute.For<IRoutingService>();
        var searchService = Substitute.For<ISearchService>();
        var vm = new DirectionsViewModel(routingService, searchService);

        var mockMap = Substitute.For<IMapService>();
        vm.SetMapService(mockMap);

        typeof(DirectionsViewModel).GetProperty(nameof(DirectionsViewModel.OriginPlace))!
            .SetValue(vm, new Place("id1", "SF", new GeoCoordinates(37.7749, -122.4194)));

        vm.CalculateRouteCommand.Execute(null);
        await Task.Delay(50);

        await routingService.DidNotReceive().CalculateRouteAsync(
            Arg.Any<List<Waypoint>>(), Arg.Any<RoutingOptions>());
    }

    [Fact]
    public async Task CalculateRoute_ServiceThrowsException_ShowsError()
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
            .Returns(Task.FromException<RoutingResult>(new InvalidOperationException("Network failure")));

        vm.CalculateRouteCommand.Execute(null);
        await Task.Delay(50);

        Assert.False(vm.IsRouteVisible);
        Assert.NotEmpty(vm.RouteError);
        Assert.Contains("Network failure", vm.RouteError);
    }

    [Fact]
    public async Task IsolineMode_WithoutMapService_DoesNotCrash()
    {
        var routingService = Substitute.For<IRoutingService>();
        var searchService = Substitute.For<ISearchService>();
        var vm = new DirectionsViewModel(routingService, searchService);

        // No map service set
        var ex = await Record.ExceptionAsync(async () =>
        {
            vm.ToggleIsolineModeCommand.Execute(null);
            await Task.Delay(50);
        });

        Assert.Null(ex);
        // IsIsolineMode is toggled to true, but mapService is null so nothing renders
        Assert.True(vm.IsIsolineMode);
    }

    [Fact]
    public async Task TransportModeSelection_AllModes_DoNotCrash()
    {
        var routingService = Substitute.For<IRoutingService>();
        var searchService = Substitute.For<ISearchService>();
        var vm = new DirectionsViewModel(routingService, searchService);

        for (int i = 0; i <= 7; i++)
        {
            var ex = Record.Exception(() => vm.SelectedTransportMode = i);
            Assert.Null(ex);
        }
    }

    #endregion

    #region Explore Edge Cases

    [Fact]
    public async Task Search_ServiceThrowsException_ShowsError()
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

        searchService.SearchAsync(Arg.Any<TextQuery>(), Arg.Any<SearchOptions>())
            .Returns(Task.FromException<SearchResult>(new InvalidOperationException("Search service down")));

        vm.SearchQuery = "test";
        vm.SubmitSearchCommand.Execute(null);
        await Task.Delay(50);

        Assert.NotNull(vm.SearchErrorMessage);
        Assert.Contains("Search service down", vm.SearchErrorMessage);
    }

    [Fact]
    public async Task Search_ShortQuery_DoesNotExecute()
    {
        var searchService = Substitute.For<ISearchService>();
        var locationService = Substitute.For<ILocationService>();
        var connectivityService = Substitute.For<IConnectivityService>();
        var permissionsService = Substitute.For<IPermissionsService>();

        var vm = new ExploreViewModel(searchService, locationService, connectivityService, permissionsService);
        vm.SearchQuery = "A"; // Single character

        // Submit with short query
        vm.SubmitSearchCommand.Execute(null);
        await Task.Delay(50);

        await searchService.DidNotReceive().SearchAsync(
            Arg.Any<TextQuery>(), Arg.Any<SearchOptions>());
    }

    [Fact]
    public async Task CenterOnLocation_PermissionDenied_ShowsError()
    {
        var searchService = Substitute.For<ISearchService>();
        var locationService = Substitute.For<ILocationService>();
        var connectivityService = Substitute.For<IConnectivityService>();
        var permissionsService = Substitute.For<IPermissionsService>();
        connectivityService.IsConnected.Returns(true);
        permissionsService.RequestLocationPermissionAsync().Returns(false);

        var vm = new ExploreViewModel(searchService, locationService, connectivityService, permissionsService);
        var mockMap = Substitute.For<IMapService>();
        vm.SetMapService(mockMap);

        vm.CenterOnLocationCommand.Execute(null);
        await Task.Delay(50);

        Assert.NotNull(vm.SearchErrorMessage);
        Assert.Contains("permission", vm.SearchErrorMessage ?? "", StringComparison.OrdinalIgnoreCase);
        Assert.False(vm.IsLocationTracking);
    }

    [Fact]
    public async Task CenterOnLocation_LocationNull_ShowsError()
    {
        var searchService = Substitute.For<ISearchService>();
        var locationService = Substitute.For<ILocationService>();
        var connectivityService = Substitute.For<IConnectivityService>();
        var permissionsService = Substitute.For<IPermissionsService>();
        connectivityService.IsConnected.Returns(true);
        permissionsService.RequestLocationPermissionAsync().Returns(true);
        locationService.GetCurrentLocationAsync().Returns((HereLocation?)null);

        var vm = new ExploreViewModel(searchService, locationService, connectivityService, permissionsService);
        var mockMap = Substitute.For<IMapService>();
        vm.SetMapService(mockMap);

        vm.CenterOnLocationCommand.Execute(null);
        await Task.Delay(50);

        Assert.NotNull(vm.SearchErrorMessage);
        Assert.Contains("GPS", vm.SearchErrorMessage ?? "");
    }

    [Fact]
    public async Task CenterOnLocation_ServiceThrows_ShowsError()
    {
        var searchService = Substitute.For<ISearchService>();
        var locationService = Substitute.For<ILocationService>();
        var connectivityService = Substitute.For<IConnectivityService>();
        var permissionsService = Substitute.For<IPermissionsService>();
        connectivityService.IsConnected.Returns(true);
        permissionsService.RequestLocationPermissionAsync().Returns(true);
        locationService.GetCurrentLocationAsync()
            .Returns(Task.FromException<HereLocation?>(new InvalidOperationException("GPS unavailable")));

        var vm = new ExploreViewModel(searchService, locationService, connectivityService, permissionsService);
        var mockMap = Substitute.For<IMapService>();
        vm.SetMapService(mockMap);

        vm.CenterOnLocationCommand.Execute(null);
        await Task.Delay(50);

        Assert.NotNull(vm.SearchErrorMessage);
        Assert.Contains("GPS unavailable", vm.SearchErrorMessage);
        Assert.False(vm.IsLocationTracking);
    }

    [Fact]
    public async Task SubmitSearch_ZeroResults_ShowsEmptyState()
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

        searchService.SearchAsync(Arg.Any<TextQuery>(), Arg.Any<SearchOptions>())
            .Returns(new SearchResult(SearchError.None, new List<Place>()));

        vm.SearchQuery = "nonexistentplace";
        vm.SubmitSearchCommand.Execute(null);
        await Task.Delay(50);

        Assert.NotNull(vm.EmptyStateTitle);
        Assert.Contains("No places found", vm.EmptyStateTitle);
    }

    #endregion

    #region Traffic Edge Cases

    [Fact]
    public async Task ToggleFlow_WithoutMapService_DoesNotCrash()
    {
        var trafficService = Substitute.For<ITrafficService>();
        var vm = new TrafficViewModel(trafficService);

        var ex = await Record.ExceptionAsync(async () =>
        {
            vm.ToggleFlowCommand.Execute(null);
            await Task.Delay(50);
        });

        Assert.Null(ex);
    }

    [Fact]
    public async Task ToggleIncidents_WithoutMapService_DoesNotCrash()
    {
        var trafficService = Substitute.For<ITrafficService>();
        var vm = new TrafficViewModel(trafficService);

        var ex = await Record.ExceptionAsync(async () =>
        {
            vm.ToggleIncidentsCommand.Execute(null);
            await Task.Delay(50);
        });

        Assert.Null(ex);
    }

    [Fact]
    public async Task TrafficQuery_ServiceError_DoesNotCrash()
    {
        var trafficService = Substitute.For<ITrafficService>();
        var vm = new TrafficViewModel(trafficService);
        var mockMap = Substitute.For<IMapService>();
        mockMap.GetCameraTargetAsync().Returns(new GeoCoordinates(52.53, 13.39));
        vm.SetMapService(mockMap);

        trafficService.QueryFlowAsync(Arg.Any<GeoCircle>(), Arg.Any<TrafficFlowQueryOptions>())
            .Returns(new TrafficFlowResult(TrafficQueryError.NetworkError, new List<TrafficFlow>()));

        vm.ToggleFlowCommand.Execute(null);
        await Task.Delay(50);

        // Should still be visible (toggle was flipped) but flow count is 0
        Assert.True(vm.IsFlowVisible);
        Assert.Equal(0, vm.FlowCount);
    }

    [Fact]
    public async Task SelectIncident_NullParameter_DoesNotCrash()
    {
        var trafficService = Substitute.For<ITrafficService>();
        var vm = new TrafficViewModel(trafficService);

        var ex = await Record.ExceptionAsync(async () =>
        {
            vm.SelectIncidentCommand.Execute(null);
            await Task.Delay(50);
        });

        Assert.Null(ex);
    }

    [Fact]
    public async Task Traffic_ToggleFlowOff_DoesNotQueryAgain()
    {
        var trafficService = Substitute.For<ITrafficService>();
        var vm = new TrafficViewModel(trafficService);
        var mockMap = Substitute.For<IMapService>();
        mockMap.GetCameraTargetAsync().Returns(new GeoCoordinates(52.53, 13.39));
        vm.SetMapService(mockMap);

        // Set up flow already visible
        typeof(TrafficViewModel).GetProperty(nameof(TrafficViewModel.IsFlowVisible))!
            .SetValue(vm, true);

        vm.ToggleFlowCommand.Execute(null);
        await Task.Delay(50);

        Assert.False(vm.IsFlowVisible);
        // No query when toggling off
        await trafficService.DidNotReceive().QueryFlowAsync(
            Arg.Any<GeoCircle>(), Arg.Any<TrafficFlowQueryOptions>());
    }

    #endregion

    #region Tools Edge Cases

    [Fact]
    public void Tools_MapTapped_WithoutActiveTool_DoesNothing()
    {
        var themeService = Substitute.For<IThemeService>();
        var vm = new ToolsViewModel(themeService);
        var mockMap = Substitute.For<IMapService>();
        vm.SetMapService(mockMap);

        // Tap without active tool
        mockMap.MapTapped += Raise.Event<EventHandler<MapTappedEventArgs>>(
            null, new MapTappedEventArgs(new GeoCoordinates(37.77, -122.42), new Point2D(100, 200)));

        Assert.Equal(0, vm.TotalObjectCount);
        mockMap.DidNotReceive().AddMapMarker(Arg.Any<MapMarker>());
    }

    [Fact]
    public void Tools_DoubleTap_NothingActive_DoesNothing()
    {
        var themeService = Substitute.For<IThemeService>();
        var vm = new ToolsViewModel(themeService);
        var mockMap = Substitute.For<IMapService>();
        vm.SetMapService(mockMap);

        // Double-tap without active tool
        var ex = Record.Exception(() =>
        {
            mockMap.MapDoubleTapped += Raise.Event<EventHandler<MapTappedEventArgs>>(
                null, new MapTappedEventArgs(new GeoCoordinates(37.77, -122.42), new Point2D(100, 200)));
        });

        Assert.Null(ex);
    }

    [Fact]
    public void Tools_ClearAll_NoObjects_DoesNotCrash()
    {
        var themeService = Substitute.For<IThemeService>();
        var vm = new ToolsViewModel(themeService);
        var mockMap = Substitute.For<IMapService>();
        vm.SetMapService(mockMap);

        // Clear without adding anything
        var ex = Record.Exception(() => vm.ClearAllCommand.Execute(null));
        Assert.Null(ex);
        Assert.Equal(0, vm.TotalObjectCount);
    }

    [Fact]
    public void Tools_SectionToggles_StateTransitions()
    {
        var themeService = Substitute.For<IThemeService>();
        var vm = new ToolsViewModel(themeService);

        Assert.True(vm.IsDrawingExpanded); // default open
        Assert.False(vm.IsGalleryExpanded);
        Assert.False(vm.IsSettingsExpanded);

        vm.ToggleDrawingExpandedCommand.Execute(null);
        Assert.False(vm.IsDrawingExpanded);

        vm.ToggleGalleryExpandedCommand.Execute(null);
        Assert.True(vm.IsGalleryExpanded);

        vm.ToggleSettingsExpandedCommand.Execute(null);
        Assert.True(vm.IsSettingsExpanded);
    }

    [Fact]
    public async Task Tools_DemoPreset_ConcentricCircles_Works()
    {
        var themeService = Substitute.For<IThemeService>();
        var vm = new ToolsViewModel(themeService);
        var mockMap = Substitute.For<IMapService>();
        mockMap.SetCameraTargetAsync(Arg.Any<GeoCoordinates>(), Arg.Any<double>()).Returns(Task.CompletedTask);
        mockMap.AddMapCircle(Arg.Any<MapCircle>());
        vm.SetMapService(mockMap);

        var preset = vm.DemoPresets.First(p => p.Name == "Concentric Circles");
        preset.ActivateCommand.Execute(null);
        await Task.Delay(50);

        mockMap.Received(5).AddMapCircle(Arg.Any<MapCircle>());
    }

    [Fact]
    public async Task Tools_DemoPreset_MarkerCluster_Works()
    {
        var themeService = Substitute.For<IThemeService>();
        var vm = new ToolsViewModel(themeService);
        var mockMap = Substitute.For<IMapService>();
        mockMap.SetCameraTargetAsync(Arg.Any<GeoCoordinates>(), Arg.Any<double>()).Returns(Task.CompletedTask);
        mockMap.AddMapMarker(Arg.Any<MapMarker>());
        vm.SetMapService(mockMap);

        var preset = vm.DemoPresets.First(p => p.Name == "Marker Cluster");
        preset.ActivateCommand.Execute(null);
        await Task.Delay(50);

        mockMap.Received(30).AddMapMarker(Arg.Any<MapMarker>());
    }

    #endregion
}
