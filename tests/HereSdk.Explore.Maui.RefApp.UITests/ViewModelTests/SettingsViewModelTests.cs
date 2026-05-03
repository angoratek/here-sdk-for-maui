using Xunit;
using Here.Explore.Maui.RefApp.ViewModels;

namespace Here.Explore.Maui.RefApp.UITests.ViewModelTests;

public class SettingsViewModelTests
{
    private readonly SettingsViewModel _viewModel;

    public SettingsViewModelTests()
    {
        _viewModel = new SettingsViewModel();
    }

    [Fact]
    public void AppName_IsNotEmpty()
    {
        Assert.False(string.IsNullOrEmpty(_viewModel.AppName));
    }

    [Fact]
    public void AppVersion_IsNotEmpty()
    {
        Assert.False(string.IsNullOrEmpty(_viewModel.AppVersion));
    }

    [Fact]
    public void HereSdkVersion_IsNotEmpty()
    {
        Assert.False(string.IsNullOrEmpty(_viewModel.HereSdkVersion));
        Assert.Contains("4.25", _viewModel.HereSdkVersion);
    }

    [Fact]
    public void OpenTermsCommand_ExistsAndCanExecute()
    {
        Assert.NotNull(_viewModel.OpenTermsCommand);
        Assert.True(_viewModel.OpenTermsCommand.CanExecute(null));
    }

    [Fact]
    public void OpenPrivacyCommand_ExistsAndCanExecute()
    {
        Assert.NotNull(_viewModel.OpenPrivacyCommand);
        Assert.True(_viewModel.OpenPrivacyCommand.CanExecute(null));
    }

    [Fact]
    public void ClearCacheCommand_ExistsAndCanExecute()
    {
        Assert.NotNull(_viewModel.ClearCacheCommand);
        Assert.True(_viewModel.ClearCacheCommand.CanExecute(null));
    }
}
