using Xunit;
using Here.Explore.Maui.RefApp.Controls;
using static Here.Explore.Maui.RefApp.Controls.BottomSheet;

namespace Here.Explore.Maui.RefApp.UITests.Controls;

public class BottomSheetTests
{
    [Fact]
    public void DefaultState_IsCollapsed()
    {
        var sheet = new BottomSheet();
        Assert.Equal(SheetState.Collapsed, sheet.CurrentState);
    }

    [Fact]
    public void DefaultHeights_HaveExpectedValues()
    {
        var sheet = new BottomSheet();
        Assert.Equal(80.0, sheet.CollapsedHeight);
        Assert.Equal(280.0, sheet.HalfExpandedHeight);
        Assert.Equal(-1.0, sheet.FullyExpandedHeight); // -1 means "auto" (% of parent)
    }

    [Fact]
    public void CurrentState_TransitionToHalfExpanded()
    {
        var sheet = new BottomSheet();
        sheet.CurrentState = SheetState.HalfExpanded;
        Assert.Equal(SheetState.HalfExpanded, sheet.CurrentState);
    }

    [Fact]
    public void CurrentState_TransitionToFullyExpanded()
    {
        var sheet = new BottomSheet();
        sheet.CurrentState = SheetState.FullyExpanded;
        Assert.Equal(SheetState.FullyExpanded, sheet.CurrentState);
    }

    [Fact]
    public void CurrentState_FullCycle_CollapsedToExpandedAndBack()
    {
        var sheet = new BottomSheet();

        sheet.CurrentState = SheetState.HalfExpanded;
        Assert.Equal(SheetState.HalfExpanded, sheet.CurrentState);

        sheet.CurrentState = SheetState.FullyExpanded;
        Assert.Equal(SheetState.FullyExpanded, sheet.CurrentState);

        sheet.CurrentState = SheetState.Collapsed;
        Assert.Equal(SheetState.Collapsed, sheet.CurrentState);
    }

    [Fact]
    public void CollapsedHeight_CustomValue()
    {
        var sheet = new BottomSheet { CollapsedHeight = 120.0 };
        Assert.Equal(120.0, sheet.CollapsedHeight);
    }

    [Fact]
    public void HalfExpandedHeight_CustomValue()
    {
        var sheet = new BottomSheet { HalfExpandedHeight = 350.0 };
        Assert.Equal(350.0, sheet.HalfExpandedHeight);
    }

    [Fact]
    public void FullyExpandedHeight_CustomValue()
    {
        var sheet = new BottomSheet { FullyExpandedHeight = 600.0 };
        Assert.Equal(600.0, sheet.FullyExpandedHeight);
    }

    [Fact]
    public void SheetContent_SetAndGet()
    {
        var sheet = new BottomSheet();
        var content = new Label { Text = "Hello" };
        sheet.SheetContent = content;
        Assert.Equal(content, sheet.SheetContent);
    }

    [Fact]
    public void HeaderContent_SetAndGet()
    {
        var sheet = new BottomSheet();
        var header = new Label { Text = "Header" };
        sheet.HeaderContent = header;
        Assert.Equal(header, sheet.HeaderContent);
    }

    [Fact]
    public void SheetContent_Replaced_UpdatesCorrectly()
    {
        var sheet = new BottomSheet();
        var first = new Label { Text = "First" };
        var second = new Label { Text = "Second" };

        sheet.SheetContent = first;
        Assert.Equal(first, sheet.SheetContent);

        sheet.SheetContent = second;
        Assert.Equal(second, sheet.SheetContent);
    }

    [Fact]
    public void CurrentState_SameState_NoException()
    {
        var sheet = new BottomSheet();
        sheet.CurrentState = SheetState.Collapsed; // Already collapsed
        Assert.Equal(SheetState.Collapsed, sheet.CurrentState);
    }

    [Fact]
    public void HeightConfiguration_MultipleHeights_SetIndependently()
    {
        var sheet = new BottomSheet
        {
            CollapsedHeight = 100,
            HalfExpandedHeight = 300,
            FullyExpandedHeight = 500
        };
        Assert.Equal(100.0, sheet.CollapsedHeight);
        Assert.Equal(300.0, sheet.HalfExpandedHeight);
        Assert.Equal(500.0, sheet.FullyExpandedHeight);
    }

    [Fact]
    public void Chrome_HasRoundedTopCornersAndBackground()
    {
        var sheet = new BottomSheet();
        var shape = Assert.IsType<Microsoft.Maui.Controls.Shapes.RoundRectangle>(sheet.StrokeShape);
        Assert.Equal(20.0, shape.CornerRadius.TopLeft);
        Assert.Equal(20.0, shape.CornerRadius.TopRight);
        Assert.Equal(0.0, shape.CornerRadius.BottomLeft);
        Assert.NotEqual(Colors.Transparent, sheet.BackgroundColor);
        Assert.NotNull(sheet.Shadow);
    }

    [Fact]
    public void BodyDragAndScrim_DefaultOff()
    {
        var sheet = new BottomSheet();
        Assert.False(sheet.BodyDragEnabled);
        Assert.False(sheet.IsScrimEnabled);
    }

    [Fact]
    public void Scrim_TogglesVisibilityWithState()
    {
        var sheet = new BottomSheet { IsScrimEnabled = true };
        var scrim = FindScrim(sheet);
        Assert.NotNull(scrim);
        Assert.False(scrim!.IsVisible);

        sheet.CurrentState = SheetState.HalfExpanded;
        Assert.True(scrim.IsVisible);
        Assert.False(scrim.InputTransparent);

        sheet.CurrentState = SheetState.Collapsed;
        Assert.False(scrim.IsVisible);
        Assert.True(scrim.InputTransparent);
    }

    [Fact]
    public void Scrim_HasTapToCollapseRecognizer()
    {
        var sheet = new BottomSheet { IsScrimEnabled = true };
        var scrim = FindScrim(sheet);
        Assert.Single(scrim!.GestureRecognizers);
        Assert.IsType<TapGestureRecognizer>(scrim.GestureRecognizers[0]);
    }

    private static Border? FindScrim(BottomSheet sheet)
        => sheet.Content is Grid grid
            ? grid.Children.OfType<Border>().FirstOrDefault()
            : null;
}
