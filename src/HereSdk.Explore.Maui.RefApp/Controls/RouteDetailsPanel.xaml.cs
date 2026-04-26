using System.Windows.Input;

namespace Here.Explore.Maui.RefApp.Controls;

public partial class RouteDetailsPanel : Border
{
    public static readonly BindableProperty DistanceTextProperty =
        BindableProperty.Create(nameof(DistanceText), typeof(string), typeof(RouteDetailsPanel), "--");

    public static readonly BindableProperty DurationTextProperty =
        BindableProperty.Create(nameof(DurationText), typeof(string), typeof(RouteDetailsPanel), "--");

    public static readonly BindableProperty ManeuversProperty =
        BindableProperty.Create(nameof(Maneuvers), typeof(IReadOnlyList<object>), typeof(RouteDetailsPanel));

    public static readonly BindableProperty ClearRouteCommandProperty =
        BindableProperty.Create(nameof(ClearRouteCommand), typeof(ICommand), typeof(RouteDetailsPanel));

    public static readonly BindableProperty HasRouteProperty =
        BindableProperty.Create(nameof(HasRoute), typeof(bool), typeof(RouteDetailsPanel), false);

    public string DistanceText
    {
        get => (string)GetValue(DistanceTextProperty);
        set => SetValue(DistanceTextProperty, value);
    }

    public string DurationText
    {
        get => (string)GetValue(DurationTextProperty);
        set => SetValue(DurationTextProperty, value);
    }

    public IReadOnlyList<object>? Maneuvers
    {
        get => (IReadOnlyList<object>?)GetValue(ManeuversProperty);
        set => SetValue(ManeuversProperty, value);
    }

    public ICommand? ClearRouteCommand
    {
        get => (ICommand?)GetValue(ClearRouteCommandProperty);
        set => SetValue(ClearRouteCommandProperty, value);
    }

    public bool HasRoute
    {
        get => (bool)GetValue(HasRouteProperty);
        set => SetValue(HasRouteProperty, value);
    }

    public RouteDetailsPanel()
    {
        InitializeComponent();
    }
}
