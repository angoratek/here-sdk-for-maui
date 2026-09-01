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
    private double _deviceHeight;

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

        // On iOS, the first navigation to a page hosting this BottomSheet can
        // land before OnSizeAllocated gets a real parent height, leaving the
        // sheet stuck at CollapsedHeight even when CurrentState is set to
        // FullyExpanded in XAML. The Loaded event fires when the visual tree
        // is actually attached, so re-applying the layout there fixes the
        // initial-expand-on-Shell-tab issue.
        Loaded += OnLoaded;

        // Re-apply layout on every SizeChanged. On iOS Shell-tab navigation
        // the visual tree can resize AFTER the first Loaded + UpdateSheetLayout
        // run, leaving the sheet stuck at a stale TranslationY. The
        // SizeChanged signal is the canonical "I actually have a real size
        // now" hook, and re-running the layout there is idempotent.
        SizeChanged += OnSizeChanged;

        // DeviceDisplay is reliable on iOS even before the first layout pass,
        // so cache it here as a fallback when Parent.Height is still 0
        // (Shell-tab navigation can land before OnSizeAllocated reports a
        // valid parent height).
        _deviceHeight = DeviceDisplay.Current.MainDisplayInfo.Height /
            DeviceDisplay.Current.MainDisplayInfo.Density;

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
        _availableHeight = ResolveAvailableHeight();
        UpdateSheetLayout();
    }

    /// <summary>
    /// On iOS, Shell-tab content pages can land on the Tools tab before
    /// <see cref="OnSizeAllocated"/> is called with a non-zero parent height,
    /// which leaves the BottomSheet stuck at its CollapsedHeight even though
    /// <c>CurrentState</c> is set to <see cref="SheetState.FullyExpanded"/>.
    /// Re-applying the layout on the Loaded event (fired when the visual
    /// tree is attached) forces the snap to the configured state once the
    /// page is actually on screen. Subscribed from the constructor so it
    /// fires every time the BottomSheet is attached to a parent.
    /// </summary>
    private void OnLoaded(object? sender, EventArgs e)
    {
        _availableHeight = ResolveAvailableHeight();
        UpdateSheetLayout();
    }

    /// <summary>
    /// Re-runs the layout on every SizeChanged signal. On iOS Shell-tab
    /// navigation the visual tree can resize AFTER the first Loaded +
    /// UpdateSheetLayout run, leaving the sheet stuck at a stale
    /// TranslationY (0, collapsed). Subscribed from the constructor.
    /// </summary>
    private void OnSizeChanged(object? sender, EventArgs e)
    {
        _availableHeight = ResolveAvailableHeight();
        UpdateSheetLayout();
    }

    /// <summary>
    /// Returns the height we should treat as available for the sheet to
    /// draw into. Prefer the parent View's reported height; on iOS Shell
    /// tabs that value can be 0 until the first layout pass, so fall back
    /// to the cached <see cref="DeviceDisplay"/> height in that case.
    /// </summary>
    private double ResolveAvailableHeight()
    {
        if (this.Parent is View pv && pv.Height > 0)
        {
            return pv.Height;
        }
        return _deviceHeight > 0 ? _deviceHeight : 800.0;
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
        // On iOS, the Loaded event can fire before the parent has been laid
        // out (so Parent.Height is still 0). If we bail here we leave the
        // sheet stuck at CollapsedHeight for the lifetime of the page, even
        // though CurrentState is set to FullyExpanded in XAML. Poll a few
        // times for a real parent height, and fall back to the cached
        // DeviceDisplay height so the sheet always snaps to its target
        // state instead of staying collapsed.
        if (_availableHeight <= 0)
        {
            for (var i = 0; i < 10; i++)
            {
                await Task.Delay(50);
                _availableHeight = ResolveAvailableHeight();
                if (_availableHeight > 0) break;
            }
        }

        if (_availableHeight <= 0)
        {
            _availableHeight = _deviceHeight > 0 ? _deviceHeight : 800.0;
        }

        // On iOS Shell tab content pages, MAUI's iOS handler re-applies
        // AutoLayout constraints after layout and discards TranslationY,
        // leaving the sheet stuck at TranslationY=0 (collapsed position).
        // We work around this by changing the sheet's HeightRequest only —
        // `VerticalOptions = End` anchors it to the parent bottom, and a
        // larger HeightRequest makes the sheet grow upward into the
        // available space. Visibility of the SheetContent is also gated on
        // CurrentState so the collapsed sheet doesn't render its content
        // (which would push the layout taller than CollapsedHeight).
        var fullHeight = FullyExpandedHeight > 0 ? FullyExpandedHeight : _availableHeight * 0.85;
        var displayHeight = CurrentState switch
        {
            SheetState.Collapsed => CollapsedHeight,
            SheetState.HalfExpanded => HalfExpandedHeight,
            SheetState.FullyExpanded => fullHeight,
            _ => CollapsedHeight
        };
        this.HeightRequest = displayHeight;

        if (_contentSlot is not null)
        {
            _contentSlot.IsVisible = CurrentState != SheetState.Collapsed;
        }
    }
}
