using Xunit;
using Here.Explore.Maui.RefApp.Controls;

namespace Here.Explore.Maui.RefApp.UITests.Controls;

public class EmptyStateViewTests
{
    [Fact]
    public void DefaultIcon_IsMailboxEmoji()
    {
        var view = new EmptyStateView();
        Assert.Equal("\U0001f4ed", view.Icon);
    }

    [Fact]
    public void Title_DefaultIsNull()
    {
        var view = new EmptyStateView();
        Assert.Null(view.Title);
    }

    [Fact]
    public void Subtitle_DefaultIsNull()
    {
        var view = new EmptyStateView();
        Assert.Null(view.Subtitle);
    }

    [Fact]
    public void DefaultVisibility_IsHidden()
    {
        var view = new EmptyStateView();
        Assert.False(view.IsVisible);
    }

    [Fact]
    public void IconViaBindableProperty_DefaultValue()
    {
        var defaultValue = EmptyStateView.IconProperty.DefaultValue;
        Assert.Equal("\U0001f4ed", defaultValue);
    }
}
