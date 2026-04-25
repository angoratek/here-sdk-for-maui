using System.Windows.Input;

namespace Here.Explore.Maui.RefApp.Controls;

public partial class SearchResultsList : Border
{
    public static readonly BindableProperty SuggestionsProperty =
        BindableProperty.Create(nameof(Suggestions), typeof(IReadOnlyList<object>), typeof(SearchResultsList));

    public static readonly BindableProperty SelectedSuggestionProperty =
        BindableProperty.Create(nameof(SelectedSuggestion), typeof(object), typeof(SearchResultsList), defaultBindingMode: BindingMode.TwoWay);

    public static readonly BindableProperty HasResultsProperty =
        BindableProperty.Create(nameof(HasResults), typeof(bool), typeof(SearchResultsList), false);

    public IReadOnlyList<object>? Suggestions
    {
        get => (IReadOnlyList<object>?)GetValue(SuggestionsProperty);
        set => SetValue(SuggestionsProperty, value);
    }

    public object? SelectedSuggestion
    {
        get => GetValue(SelectedSuggestionProperty);
        set => SetValue(SelectedSuggestionProperty, value);
    }

    public bool HasResults
    {
        get => (bool)GetValue(HasResultsProperty);
        set => SetValue(HasResultsProperty, value);
    }

    public SearchResultsList()
    {
        InitializeComponent();
        BindingContext = this;
    }
}
