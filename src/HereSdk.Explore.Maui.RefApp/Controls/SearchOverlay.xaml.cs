using System.Windows.Input;

namespace Here.Explore.Maui.RefApp.Controls;

public partial class SearchOverlay : Border
{
    public static readonly BindableProperty SearchQueryProperty =
        BindableProperty.Create(nameof(SearchQuery), typeof(string), typeof(SearchOverlay), string.Empty, BindingMode.TwoWay);

    public static readonly BindableProperty SearchCommandProperty =
        BindableProperty.Create(nameof(SearchCommand), typeof(ICommand), typeof(SearchOverlay));

    public static readonly BindableProperty ClearSearchCommandProperty =
        BindableProperty.Create(nameof(ClearSearchCommand), typeof(ICommand), typeof(SearchOverlay));

    public static readonly BindableProperty FocusSearchCommandProperty =
        BindableProperty.Create(nameof(FocusSearchCommand), typeof(ICommand), typeof(SearchOverlay));

    public static readonly BindableProperty SelectSuggestionCommandProperty =
        BindableProperty.Create(nameof(SelectSuggestionCommand), typeof(ICommand), typeof(SearchOverlay));

    public string SearchQuery
    {
        get => (string)GetValue(SearchQueryProperty);
        set => SetValue(SearchQueryProperty, value);
    }

    public ICommand? SearchCommand
    {
        get => (ICommand?)GetValue(SearchCommandProperty);
        set => SetValue(SearchCommandProperty, value);
    }

    public ICommand? ClearSearchCommand
    {
        get => (ICommand?)GetValue(ClearSearchCommandProperty);
        set => SetValue(ClearSearchCommandProperty, value);
    }

    public ICommand? FocusSearchCommand
    {
        get => (ICommand?)GetValue(FocusSearchCommandProperty);
        set => SetValue(FocusSearchCommandProperty, value);
    }

    public ICommand? SelectSuggestionCommand
    {
        get => (ICommand?)GetValue(SelectSuggestionCommandProperty);
        set => SetValue(SelectSuggestionCommandProperty, value);
    }

    public SearchOverlay()
    {
        InitializeComponent();
        BindingContext = this;
    }
}
