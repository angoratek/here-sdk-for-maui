using Xunit;
using Here.Explore.Maui.RefApp.Controls;

namespace Here.Explore.Maui.RefApp.UITests.Controls;

public class ErrorBannerTests
{
    [Fact]
    public void DefaultRetryText_IsRetry()
    {
        var banner = new ErrorBanner();
        Assert.Equal("Retry", banner.RetryText);
    }

    [Fact]
    public void ErrorMessage_DefaultIsNull()
    {
        var banner = new ErrorBanner();
        Assert.Null(banner.ErrorMessage);
    }

    [Fact]
    public void RetryCommand_DefaultIsNull()
    {
        var banner = new ErrorBanner();
        Assert.Null(banner.RetryCommand);
    }

    [Fact]
    public void DefaultVisibility_IsHidden()
    {
        var banner = new ErrorBanner();
        Assert.False(banner.IsVisible);
    }

    [Fact]
    public void RetryTextViaBindableProperty_DefaultValue()
    {
        var defaultValue = ErrorBanner.RetryTextProperty.DefaultValue;
        Assert.Equal("Retry", defaultValue);
    }
}
