using System.Windows.Input;
using Here.Explore.Maui.Models.Search;

namespace Here.Explore.Maui.RefApp.Controls;

public partial class DualSearchPanel : Border
{
    public static readonly BindableProperty OriginQueryProperty =
        BindableProperty.Create(nameof(OriginQuery), typeof(string), typeof(DualSearchPanel), string.Empty, BindingMode.TwoWay);

    public static readonly BindableProperty DestinationQueryProperty =
        BindableProperty.Create(nameof(DestinationQuery), typeof(string), typeof(DualSearchPanel), string.Empty, BindingMode.TwoWay);

    public static readonly BindableProperty OriginSuggestionsProperty =
        BindableProperty.Create(nameof(OriginSuggestions), typeof(IReadOnlyList<Suggestion>), typeof(DualSearchPanel), defaultValue: null);

    public static readonly BindableProperty DestinationSuggestionsProperty =
        BindableProperty.Create(nameof(DestinationSuggestions), typeof(IReadOnlyList<Suggestion>), typeof(DualSearchPanel), defaultValue: null);

    public static readonly BindableProperty SearchOriginCommandProperty =
        BindableProperty.Create(nameof(SearchOriginCommand), typeof(ICommand), typeof(DualSearchPanel));

    public static readonly BindableProperty SearchDestinationCommandProperty =
        BindableProperty.Create(nameof(SearchDestinationCommand), typeof(ICommand), typeof(DualSearchPanel));

    public static readonly BindableProperty SelectOriginCommandProperty =
        BindableProperty.Create(nameof(SelectOriginCommand), typeof(ICommand), typeof(DualSearchPanel));

    public static readonly BindableProperty SelectDestinationCommandProperty =
        BindableProperty.Create(nameof(SelectDestinationCommand), typeof(ICommand), typeof(DualSearchPanel));

    public static readonly BindableProperty ClearOriginCommandProperty =
        BindableProperty.Create(nameof(ClearOriginCommand), typeof(ICommand), typeof(DualSearchPanel));

    public static readonly BindableProperty ClearDestinationCommandProperty =
        BindableProperty.Create(nameof(ClearDestinationCommand), typeof(ICommand), typeof(DualSearchPanel));

    public static readonly BindableProperty SwapLocationsCommandProperty =
        BindableProperty.Create(nameof(SwapLocationsCommand), typeof(ICommand), typeof(DualSearchPanel));

    public string OriginQuery
    {
        get => (string)GetValue(OriginQueryProperty);
        set => SetValue(OriginQueryProperty, value);
    }

    public string DestinationQuery
    {
        get => (string)GetValue(DestinationQueryProperty);
        set => SetValue(DestinationQueryProperty, value);
    }

    public IReadOnlyList<Suggestion>? OriginSuggestions
    {
        get => (IReadOnlyList<Suggestion>?)GetValue(OriginSuggestionsProperty);
        set => SetValue(OriginSuggestionsProperty, value);
    }

    public IReadOnlyList<Suggestion>? DestinationSuggestions
    {
        get => (IReadOnlyList<Suggestion>?)GetValue(DestinationSuggestionsProperty);
        set => SetValue(DestinationSuggestionsProperty, value);
    }

    public ICommand? SearchOriginCommand
    {
        get => (ICommand?)GetValue(SearchOriginCommandProperty);
        set => SetValue(SearchOriginCommandProperty, value);
    }

    public ICommand? SearchDestinationCommand
    {
        get => (ICommand?)GetValue(SearchDestinationCommandProperty);
        set => SetValue(SearchDestinationCommandProperty, value);
    }

    public ICommand? SelectOriginCommand
    {
        get => (ICommand?)GetValue(SelectOriginCommandProperty);
        set => SetValue(SelectOriginCommandProperty, value);
    }

    public ICommand? SelectDestinationCommand
    {
        get => (ICommand?)GetValue(SelectDestinationCommandProperty);
        set => SetValue(SelectDestinationCommandProperty, value);
    }

    public ICommand? ClearOriginCommand
    {
        get => (ICommand?)GetValue(ClearOriginCommandProperty);
        set => SetValue(ClearOriginCommandProperty, value);
    }

    public ICommand? ClearDestinationCommand
    {
        get => (ICommand?)GetValue(ClearDestinationCommandProperty);
        set => SetValue(ClearDestinationCommandProperty, value);
    }

    public ICommand? SwapLocationsCommand
    {
        get => (ICommand?)GetValue(SwapLocationsCommandProperty);
        set => SetValue(SwapLocationsCommandProperty, value);
    }

    public DualSearchPanel()
    {
        InitializeComponent();
    }
}
