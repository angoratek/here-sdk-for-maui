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
