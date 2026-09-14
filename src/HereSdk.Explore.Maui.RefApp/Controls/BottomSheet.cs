using System.Windows.Input;

using Here.Explore.Maui.RefApp.Extensions;

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

    public static readonly BindableProperty BodyDragEnabledProperty =
        BindableProperty.Create(nameof(BodyDragEnabled), typeof(bool), typeof(BottomSheet), false,
            propertyChanged: (b, _, v) => ((BottomSheet)b).OnBodyDragEnabledChanged((bool)v));

    public static readonly BindableProperty IsScrimEnabledProperty =
        BindableProperty.Create(nameof(IsScrimEnabled), typeof(bool), typeof(BottomSheet), false,
            propertyChanged: (b, _, _) => ((BottomSheet)b).UpdateSheetLayout());

    private readonly Grid _rootGrid;
    private readonly ContentView _headerSlot;
    private readonly ContentView _contentSlot;
    private readonly Border _scrim;
    // Pointer pressed/released positions are the authoritative drag signal.
    private double _pressY;
    private bool _pressActive;
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

    /// <summary>
    /// Also attach the drag gesture to the sheet body, not just the handle
    /// and header. Opt-in: a container-level pan recognizer fights the
    /// scroll gesture on Android, so only enable it for sheets whose body
    /// is short (e.g. the Explore place-card sheet).
    /// </summary>
    public bool BodyDragEnabled
    {
        get => (bool)GetValue(BodyDragEnabledProperty);
        set => SetValue(BodyDragEnabledProperty, value);
    }

    /// <summary>
    /// Shows a dim scrim over the sheet's own footprint while expanded;
    /// tapping it collapses the sheet.
    /// </summary>
    public bool IsScrimEnabled
    {
        get => (bool)GetValue(IsScrimEnabledProperty);
        set => SetValue(IsScrimEnabledProperty, value);
    }

    public BottomSheet()
    {
        // Sheet chrome — the control owns its own surface so the panels
        // don't have to hand-roll backgrounds, radius, or shadows.
        StrokeThickness = 0;
        StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
        {
            CornerRadius = new CornerRadius(20, 20, 0, 0)
        };
        Padding = 0;
        var (surfaceLight, surfaceDark) = Application.Current.GetThemedPair("Surface", "SurfaceDark");
        this.SetAppThemeColor(BackgroundColorProperty, surfaceLight, surfaceDark);
        var (shadowLight, shadowDark) = Application.Current.GetThemedPair("ShadowColor", "ShadowColorDark");
        Shadow = new Shadow
        {
            Offset = new Point(0, -2),
            Radius = 16,
            // Shadow brush is resolved per current theme at construction; a
            // live shadow re-tint on dark-mode flip is not worth an event
            // subscription.
            Brush = new SolidColorBrush(Application.Current?.RequestedTheme == AppTheme.Dark ? shadowDark : shadowLight)
        };

        // Handle: small visual bar inside a ≥36dp-tall touch target, so
        // dragging (and Appium handle taps) are reliable.
        var handle = new BoxView
        {
            HeightRequest = 4,
            WidthRequest = 36,
            CornerRadius = 2,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        handle.SetAppThemeColor(BoxView.ColorProperty, Color.FromArgb("#E1E1E4"), Color.FromArgb("#48484A"));

        var handleRow = new Grid { HeightRequest = 36, Padding = new Thickness(0, 6, 0, 2) };
        handleRow.Children.Add(handle);

        // Drag handling uses a PointerGestureRecognizer only. A
        // PanGestureRecognizer cannot be used for the snap decision on
        // Android: its cumulative TotalY stalls once the finger leaves the
        // view's bounds (a 600px drag past the sheet edge tracks as ~120px)
        // and it fires two Running events per move, and attaching one
        // alongside a pointer recognizer suppresses the pointer events
        // entirely. Pointer Pressed/Released positions are absolute and
        // bounds-independent — the cost is that the sheet snaps at release
        // instead of following the finger live (pointer Moved never fires
        // for touch on Android).
        AttachDragHandlers(handleRow);

        // Dragging from the header row too (title area) — same handlers,
        // own recognizer instance (attached below, after _headerSlot).

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

        _scrim = new Border { IsVisible = false, InputTransparent = true };
        var scrimColor = Application.Current.GetThemedPair("Scrim", "Scrim").Light;
        _scrim.BackgroundColor = scrimColor;
        var scrimTap = new TapGestureRecognizer();
        scrimTap.Tapped += (_, _) => CurrentState = SheetState.Collapsed;
        _scrim.GestureRecognizers.Add(scrimTap);

        _rootGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            }
        };

        _rootGrid.Children.Add(_scrim);
        Grid.SetRowSpan(_scrim, 3);

        Grid.SetRow(handleRow, 0);
        _rootGrid.Children.Add(handleRow);

        Grid.SetRow(_headerSlot, 1);
        AttachDragHandlers(_headerSlot);
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
        var sheet = (BottomSheet)bindable;
        // Scrim visibility is cheap and synchronous — apply it immediately so
        // state changes are observable without waiting for the async layout
        // path (which awaits when no parent height is available, e.g. in
        // unit tests).
        sheet.UpdateScrim();
        sheet.UpdateSheetLayout();
    }

    private void UpdateScrim()
    {
        if (_scrim is null)
        {
            return;
        }
        var showScrim = IsScrimEnabled && CurrentState != SheetState.Collapsed;
        _scrim.IsVisible = showScrim;
        _scrim.InputTransparent = !showScrim;
    }

    private void AttachDragHandlers(View target)
    {
        var drag = new PointerGestureRecognizer();
        drag.PointerPressed += OnDragPointerPressed;
        drag.PointerReleased += OnDragPointerReleased;
        target.GestureRecognizers.Add(drag);
    }

    private void OnDragPointerPressed(object? sender, PointerEventArgs e)
    {
        var y = e.GetPosition(null)?.Y;
        if (y is null) return;
        _pressY = y.Value;
        _pressActive = true;
        this.CancelAnimations();
    }

    private void OnDragPointerReleased(object? sender, PointerEventArgs e)
    {
        if (!_pressActive) return;
        _pressActive = false;
        var y = e.GetPosition(null)?.Y;
        if (y is null) return;
        CurrentState = DetermineSnapState(y.Value - _pressY);
    }

    /// <summary>
    /// Snaps to the state nearest to where the finger released. When the
    /// nearest state is the one the drag started from but the finger still
    /// travelled a deliberate distance, advance one state in the drag
    /// direction (flick responsiveness — pointer events carry no velocity).
    /// </summary>
    private SheetState DetermineSnapState(double dragDelta)
    {
        var minY = GetTargetTranslation(SheetState.FullyExpanded);
        var maxY = GetTargetTranslation(SheetState.Collapsed);
        var start = GetTargetTranslation(CurrentState);
        var releasedY = Math.Clamp(start + dragDelta, minY, maxY);

        var collapsedY = GetTargetTranslation(SheetState.Collapsed);
        var halfY = GetTargetTranslation(SheetState.HalfExpanded);
        var fullY = GetTargetTranslation(SheetState.FullyExpanded);

        var distCollapsed = Math.Abs(releasedY - collapsedY);
        var distHalf = Math.Abs(releasedY - halfY);
        var distFull = Math.Abs(releasedY - fullY);

        var nearest = distCollapsed < distHalf && distCollapsed < distFull ? SheetState.Collapsed
            : distHalf < distFull ? SheetState.HalfExpanded
            : SheetState.FullyExpanded;

        if (nearest == CurrentState && Math.Abs(dragDelta) >= 60)
        {
            return dragDelta < 0
                ? (CurrentState == SheetState.Collapsed ? SheetState.HalfExpanded : SheetState.FullyExpanded)
                : (CurrentState == SheetState.FullyExpanded ? SheetState.HalfExpanded : SheetState.Collapsed);
        }
        return nearest;
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
        // A null parent means the sheet is not attached to a visual tree
        // (unit-test construction) — there is nothing to poll for, so fall
        // back synchronously instead of leaving an async continuation that
        // would mutate HeightRequest off the caller's thread.
        if (_availableHeight <= 0 && Parent is not null)
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

        UpdateScrim();
    }

    private void OnBodyDragEnabledChanged(bool enabled)
    {
        if (_rootGrid is null) return;
        var existing = _rootGrid.GestureRecognizers.OfType<PointerGestureRecognizer>().ToList();
        if (enabled && existing.Count == 0)
        {
            AttachDragHandlers(_rootGrid);
        }
        else if (!enabled)
        {
            foreach (var g in existing)
                _rootGrid.GestureRecognizers.Remove(g);
        }
    }
}
