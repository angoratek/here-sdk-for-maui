using System.Windows.Input;

namespace Here.Explore.Maui.RefApp.Controls;

public partial class BottomSheet : Border
{
    public enum SheetState
    {
        Collapsed,
        HalfExpanded,
        FullyExpanded
    }

    public static readonly BindableProperty CurrentStateProperty =
        BindableProperty.Create(nameof(CurrentState), typeof(SheetState), typeof(BottomSheet),
            SheetState.Collapsed, BindingMode.TwoWay, propertyChanged: OnStateChanged);

    public static readonly BindableProperty CollapsedHeightProperty =
        BindableProperty.Create(nameof(CollapsedHeight), typeof(double), typeof(BottomSheet), 80.0);

    public static readonly BindableProperty HalfExpandedHeightProperty =
        BindableProperty.Create(nameof(HalfExpandedHeight), typeof(double), typeof(BottomSheet), 280.0);

    public static readonly BindableProperty FullyExpandedHeightProperty =
        BindableProperty.Create(nameof(FullyExpandedHeight), typeof(double), typeof(BottomSheet), -1.0);

    public static readonly BindableProperty SheetContentProperty =
        BindableProperty.Create(nameof(SheetContent), typeof(View), typeof(BottomSheet),
            propertyChanged: (b, _, v) => ((BottomSheet)b)._contentSlot.Content = (View)v);

    public static readonly BindableProperty HeaderContentProperty =
        BindableProperty.Create(nameof(HeaderContent), typeof(View), typeof(BottomSheet),
            propertyChanged: (b, _, v) => ((BottomSheet)b)._headerSlot.Content = (View)v);

    private readonly Grid _rootGrid;
    private readonly ContentView _headerSlot;
    private readonly ContentView _contentSlot;
    private double _dragStartY;
    private double _sheetStartY;
    private double _availableHeight;

    public SheetState CurrentState
    {
        get => (SheetState)GetValue(CurrentStateProperty);
        set => SetValue(CurrentStateProperty, value);
    }

    public double CollapsedHeight
    {
        get => (double)GetValue(CollapsedHeightProperty);
        set => SetValue(CollapsedHeightProperty, value);
    }

    public double HalfExpandedHeight
    {
        get => (double)GetValue(HalfExpandedHeightProperty);
        set => SetValue(HalfExpandedHeightProperty, value);
    }

    public double FullyExpandedHeight
    {
        get => (double)GetValue(FullyExpandedHeightProperty);
        set => SetValue(FullyExpandedHeightProperty, value);
    }

    public View SheetContent
    {
        get => (View)GetValue(SheetContentProperty);
        set => SetValue(SheetContentProperty, value);
    }

    public View HeaderContent
    {
        get => (View)GetValue(HeaderContentProperty);
        set => SetValue(HeaderContentProperty, value);
    }

    public BottomSheet()
    {
        var handle = new BoxView
        {
            HeightRequest = 5,
            WidthRequest = 36,
            CornerRadius = 3,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 8, 0, 4)
        };
        handle.SetAppThemeColor(BoxView.ColorProperty, Color.FromArgb("#D1D1D6"), Color.FromArgb("#48484A"));

        var panGesture = new PanGestureRecognizer();
        panGesture.PanUpdated += OnPanUpdated;
        handle.GestureRecognizers.Add(panGesture);

        _headerSlot = new ContentView();
        _contentSlot = new ContentView();

        _rootGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            }
        };

        Grid.SetRow(handle, 0);
        _rootGrid.Children.Add(handle);

        Grid.SetRow(_headerSlot, 1);
        _rootGrid.Children.Add(_headerSlot);

        Grid.SetRow(_contentSlot, 2);
        _rootGrid.Children.Add(_contentSlot);

        Content = _rootGrid;
        VerticalOptions = LayoutOptions.End;
        HorizontalOptions = LayoutOptions.Fill;
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        var parentHeight = this.Parent is View pv ? pv.Height : 800;
        _availableHeight = parentHeight;
        UpdateSheetLayout();
    }

    private static void OnStateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((BottomSheet)bindable).UpdateSheetLayout();
    }

    private void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _dragStartY = e.TotalY;
                _sheetStartY = this.TranslationY;
                this.CancelAnimations();
                break;

            case GestureStatus.Running:
                var deltaY = e.TotalY - _dragStartY;
                var newY = _sheetStartY + deltaY;
                var maxY = GetTargetTranslation(SheetState.Collapsed);
                var minY = GetTargetTranslation(SheetState.FullyExpanded);
                newY = Math.Clamp(newY, minY, maxY);
                this.TranslationY = newY;
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                var velocity = e.TotalY;
                var state = DetermineSnapState(velocity);
                CurrentState = state;
                break;
        }
    }

    private SheetState DetermineSnapState(double velocityY)
    {
        var currentY = this.TranslationY;
        var collapsedY = GetTargetTranslation(SheetState.Collapsed);
        var halfY = GetTargetTranslation(SheetState.HalfExpanded);
        var fullY = GetTargetTranslation(SheetState.FullyExpanded);

        if (velocityY < -30)
            return CurrentState switch
            {
                SheetState.Collapsed => SheetState.HalfExpanded,
                SheetState.HalfExpanded => SheetState.FullyExpanded,
                _ => SheetState.FullyExpanded
            };

        if (velocityY > 30)
            return CurrentState switch
            {
                SheetState.FullyExpanded => SheetState.HalfExpanded,
                SheetState.HalfExpanded => SheetState.Collapsed,
                _ => SheetState.Collapsed
            };

        var distCollapsed = Math.Abs(currentY - collapsedY);
        var distHalf = Math.Abs(currentY - halfY);
        var distFull = Math.Abs(currentY - fullY);

        if (distCollapsed < distHalf && distCollapsed < distFull) return SheetState.Collapsed;
        if (distHalf < distFull) return SheetState.HalfExpanded;
        return SheetState.FullyExpanded;
    }

    private double GetTargetTranslation(SheetState state)
    {
        var fullHeight = FullyExpandedHeight > 0 ? FullyExpandedHeight : _availableHeight * 0.85;
        return state switch
        {
            SheetState.Collapsed => 0.0,
            SheetState.HalfExpanded => CollapsedHeight - HalfExpandedHeight,
            SheetState.FullyExpanded => CollapsedHeight - fullHeight,
            _ => 0.0
        };
    }

    private async void UpdateSheetLayout()
    {
        if (_availableHeight <= 0) return;

        var targetTranslation = GetTargetTranslation(CurrentState);
        await this.TranslateToAsync(0, targetTranslation, 350, Easing.CubicOut);

        var fullHeight = FullyExpandedHeight > 0 ? FullyExpandedHeight : _availableHeight * 0.85;
        var displayHeight = CurrentState switch
        {
            SheetState.Collapsed => CollapsedHeight,
            SheetState.HalfExpanded => HalfExpandedHeight,
            SheetState.FullyExpanded => fullHeight,
            _ => CollapsedHeight
        };
        this.HeightRequest = displayHeight;
    }
}
