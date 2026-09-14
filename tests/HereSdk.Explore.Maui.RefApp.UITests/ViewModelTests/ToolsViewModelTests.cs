using Xunit;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;
using Here.Explore.Maui.Services;
using Here.Explore.Maui.RefApp.ViewModels;
using Here.Explore.Maui.RefApp.Services;
using NSubstitute;

namespace Here.Explore.Maui.RefApp.UITests.ViewModelTests;

public class ToolsViewModelTests
{
    private readonly IThemeService _themeService;
    private readonly ToolsViewModel _viewModel;

    public ToolsViewModelTests()
    {
        _themeService = Substitute.For<IThemeService>();
        _viewModel = new ToolsViewModel(_themeService);
    }

    #region Initial State

    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        Assert.True(_viewModel.IsDrawingExpanded);
        Assert.False(_viewModel.IsGalleryExpanded);
        Assert.False(_viewModel.IsSettingsExpanded);
        Assert.False(_viewModel.IsDrawingActive);
        Assert.Equal("NormalDay", _viewModel.SelectedSchemeName);
        Assert.Equal(5, _viewModel.DemoPresets.Count);
    }

    [Fact]
    public void DemoPresets_HaveCommands()
    {
        foreach (var preset in _viewModel.DemoPresets)
        {
            Assert.NotNull(preset.Name);
            Assert.NotNull(preset.Description);
            Assert.NotNull(preset.ActivateCommand);
            Assert.True(preset.ActivateCommand.CanExecute(null));
        }
    }

    #endregion

    #region Section Toggles

    [Fact]
    public void ToggleDrawingExpanded_Toggles()
    {
        _viewModel.ToggleDrawingExpandedCommand.Execute(null);
        Assert.False(_viewModel.IsDrawingExpanded);

        _viewModel.ToggleDrawingExpandedCommand.Execute(null);
        Assert.True(_viewModel.IsDrawingExpanded);
    }

    [Fact]
    public void ToggleGalleryExpanded_Toggles()
    {
        _viewModel.ToggleGalleryExpandedCommand.Execute(null);
        Assert.True(_viewModel.IsGalleryExpanded);

        _viewModel.ToggleGalleryExpandedCommand.Execute(null);
        Assert.False(_viewModel.IsGalleryExpanded);
    }

    [Fact]
    public void ToggleSettingsExpanded_Toggles()
    {
        _viewModel.ToggleSettingsExpandedCommand.Execute(null);
        Assert.True(_viewModel.IsSettingsExpanded);

        _viewModel.ToggleSettingsExpandedCommand.Execute(null);
        Assert.False(_viewModel.IsSettingsExpanded);
    }

    #endregion

    #region Drawing Tools

    [Fact]
    public void SetDrawingTool_Marker_SetsState()
    {
        _viewModel.SetDrawingToolCommand.Execute("Marker");

        Assert.True(_viewModel.IsDrawingActive);
        Assert.Contains("Tap anywhere", _viewModel.DrawingHint);
    }

    [Fact]
    public void SetDrawingTool_Polyline_SetsState()
    {
        _viewModel.SetDrawingToolCommand.Execute("Polyline");

        Assert.True(_viewModel.IsDrawingActive);
        Assert.Contains("vertices", _viewModel.DrawingHint);
    }

    [Fact]
    public void SetDrawingTool_DeselectsIfSame()
    {
        _viewModel.SetDrawingToolCommand.Execute("Marker");
        _viewModel.SetDrawingToolCommand.Execute("Marker");

        Assert.False(_viewModel.IsDrawingActive);
    }

    [Fact]
    public void CancelDrawing_Resets()
    {
        _viewModel.SetDrawingToolCommand.Execute("Polyline");
        _viewModel.CancelDrawingCommand.Execute(null);

        Assert.False(_viewModel.IsDrawingActive);
        Assert.Equal("", _viewModel.DrawingHint);
    }

    [Fact]
    public void ClearAll_ResetsEverything()
    {
        var mockMap = Substitute.For<IMapService>();
        _viewModel.SetMapService(mockMap);

        _viewModel.ClearAllCommand.Execute(null);

        Assert.Equal(0, _viewModel.TotalObjectCount);
    }

    #endregion

    #region Undo / Finish / Delete (drawing session)

    private IMapService MapTap(ToolsViewModel vm, IMapService mockMap, double lat, double lon)
    {
        mockMap.MapTapped += Raise.Event<EventHandler<MapTappedEventArgs>>(
            null, new MapTappedEventArgs(new GeoCoordinates(lat, lon), new Point2D(100, 200)));
        return mockMap;
    }

    [Fact]
    public void SetDrawingTool_ResetsCanFinishAndCanUndo()
    {
        var mockMap = Substitute.For<IMapService>();
        _viewModel.SetMapService(mockMap);

        _viewModel.SetDrawingToolCommand.Execute("Polyline");
        MapTap(_viewModel, mockMap, 37.77, -122.42);
        MapTap(_viewModel, mockMap, 37.79, -122.44);
        Assert.True(_viewModel.CanFinish);
        Assert.True(_viewModel.CanUndo);

        // Switching tools resets the point stack — flags must drop
        _viewModel.SetDrawingToolCommand.Execute("Circle");
        Assert.False(_viewModel.CanFinish);
        Assert.False(_viewModel.CanUndo);
        Assert.Equal(0, _viewModel.DrawingPointCount);
    }

    [Fact]
    public void FinishDrawing_BelowMinPoints_DoesNotAddObject()
    {
        var mockMap = Substitute.For<IMapService>();
        _viewModel.SetMapService(mockMap);

        _viewModel.SetDrawingToolCommand.Execute("Polygon");
        MapTap(_viewModel, mockMap, 37.77, -122.42);
        MapTap(_viewModel, mockMap, 37.79, -122.44); // polygon needs 3

        _viewModel.FinishDrawingCommand.Execute(null);

        mockMap.DidNotReceive().AddMapPolygon(Arg.Any<MapPolygon>());
        // Session stays active — nothing silently discarded
        Assert.True(_viewModel.IsDrawingActive);
    }

    [Fact]
    public void FinishDrawing_Polygon_AddsObject_AndResets()
    {
        var mockMap = Substitute.For<IMapService>();
        _viewModel.SetMapService(mockMap);

        _viewModel.SetDrawingToolCommand.Execute("Polygon");
        MapTap(_viewModel, mockMap, 37.77, -122.42);
        MapTap(_viewModel, mockMap, 37.79, -122.44);
        MapTap(_viewModel, mockMap, 37.80, -122.40);

        // Ignore the preview-shape calls; count only the committed object.
        mockMap.ClearReceivedCalls();
        _viewModel.FinishDrawingCommand.Execute(null);

        mockMap.Received(1).AddMapPolygon(Arg.Any<MapPolygon>());
        Assert.False(_viewModel.IsDrawingActive);
        Assert.Equal(1, _viewModel.TotalObjectCount);
        Assert.Equal(1, _viewModel.DrawingObjects.Count);
        Assert.False(_viewModel.CanFinish);
    }

    [Fact]
    public void UndoLastPoint_RemovesLastVertex()
    {
        var mockMap = Substitute.For<IMapService>();
        _viewModel.SetMapService(mockMap);

        _viewModel.SetDrawingToolCommand.Execute("Polyline");
        MapTap(_viewModel, mockMap, 37.77, -122.42);
        MapTap(_viewModel, mockMap, 37.79, -122.44);
        Assert.Equal(2, _viewModel.DrawingPointCount);

        _viewModel.UndoLastPointCommand.Execute(null);

        Assert.Equal(1, _viewModel.DrawingPointCount);
        Assert.False(_viewModel.CanFinish);
        Assert.True(_viewModel.CanUndo);
        Assert.True(_viewModel.IsDrawingActive);
    }

    [Fact]
    public void UndoLastPoint_OnEmpty_CancelsDrawing()
    {
        _viewModel.SetDrawingToolCommand.Execute("Polyline");
        Assert.True(_viewModel.IsDrawingActive);

        _viewModel.UndoLastPointCommand.Execute(null);

        Assert.False(_viewModel.IsDrawingActive);
        Assert.Equal(DrawingTool.None, _viewModel.CurrentDrawingTool);
    }

    [Fact]
    public void UndoLastPoint_Marker_RemovesLastPlacedMarker()
    {
        var mockMap = Substitute.For<IMapService>();
        _viewModel.SetMapService(mockMap);

        _viewModel.SetDrawingToolCommand.Execute("Marker");
        MapTap(_viewModel, mockMap, 37.77, -122.42);
        MapTap(_viewModel, mockMap, 37.79, -122.44);
        Assert.Equal(2, _viewModel.TotalObjectCount);

        _viewModel.UndoLastPointCommand.Execute(null);

        Assert.Equal(1, _viewModel.TotalObjectCount);
        Assert.Equal(1, _viewModel.DrawingObjects.Count);
        mockMap.Received(1).RemoveMapMarker(Arg.Any<MapMarker>());
    }

    [Fact]
    public void DeleteObject_RemovesFromMapAndList()
    {
        var mockMap = Substitute.For<IMapService>();
        _viewModel.SetMapService(mockMap);

        _viewModel.SetDrawingToolCommand.Execute("Marker");
        MapTap(_viewModel, mockMap, 37.77, -122.42);

        var row = Assert.Single(_viewModel.DrawingObjects);
        row.DeleteCommand.Execute(null);

        Assert.Empty(_viewModel.DrawingObjects);
        Assert.Equal(0, _viewModel.TotalObjectCount);
        mockMap.Received(1).RemoveMapMarker(Arg.Any<MapMarker>());
    }

    [Fact]
    public void Marker_MultiDrop_PlacesOneMarkerPerTap()
    {
        var mockMap = Substitute.For<IMapService>();
        _viewModel.SetMapService(mockMap);

        _viewModel.SetDrawingToolCommand.Execute("Marker");
        MapTap(_viewModel, mockMap, 37.77, -122.42);
        MapTap(_viewModel, mockMap, 37.79, -122.44);
        MapTap(_viewModel, mockMap, 37.80, -122.40);

        mockMap.Received(3).AddMapMarker(Arg.Any<MapMarker>());
        Assert.Equal(3, _viewModel.TotalObjectCount);
        Assert.Equal(3, _viewModel.DrawingObjects.Count);
        Assert.True(_viewModel.IsDrawingActive);
    }

    #endregion

    #region Dark Mode

    [Fact]
    public void ToggleDarkMode_CallsThemeService()
    {
        _themeService.ClearReceivedCalls();

        _viewModel.ToggleDarkModeCommand.Execute(null);

        _themeService.Received(1).SetDarkMode(Arg.Any<bool>());
    }

    #endregion

    #region Scheme

    [Fact]
    public async Task ChangeScheme_LoadsScene()
    {
        var mockMap = Substitute.For<IMapService>();
        mockMap.LoadSceneAsync(Arg.Any<MapScheme>()).Returns(Task.CompletedTask);
        _viewModel.SetMapService(mockMap);

        _viewModel.ChangeSchemeCommand.Execute("HybridDay");
        await Task.Delay(50);

        await mockMap.Received(1).LoadSceneAsync(MapScheme.HybridDay);
        Assert.Equal("HybridDay", _viewModel.SelectedSchemeName);
    }

    #endregion

    #region SDK Version

    [Fact]
    public void SetMapService_SetsSdkVersion()
    {
        var mockMap = Substitute.For<IMapService>();
        _viewModel.SetMapService(mockMap);

        Assert.False(string.IsNullOrEmpty(_viewModel.SdkVersion));
    }

    #endregion
}
