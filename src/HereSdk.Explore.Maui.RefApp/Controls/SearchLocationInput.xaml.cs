using System.Windows.Input;
using Here.Explore.Maui.Models.Search;

namespace Here.Explore.Maui.RefApp.Controls;

public partial class SearchLocationInput : Border
{
    public static readonly BindableProperty QueryProperty =
        BindableProperty.Create(nameof(Query), typeof(string), typeof(SearchLocationInput), string.Empty, BindingMode.TwoWay);

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(SearchLocationInput), "Search...");

    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(string), typeof(SearchLocationInput), "📍");

    public static readonly BindableProperty SuggestionsProperty =
        BindableProperty.Create(nameof(Suggestions), typeof(IReadOnlyList<Suggestion>), typeof(SearchLocationInput), defaultValue: null);

    public static readonly BindableProperty SearchCommandProperty =
        BindableProperty.Create(nameof(SearchCommand), typeof(ICommand), typeof(SearchLocationInput));

    public static readonly BindableProperty SelectSuggestionCommandProperty =
        BindableProperty.Create(nameof(SelectSuggestionCommand), typeof(ICommand), typeof(SearchLocationInput));

    public static readonly BindableProperty ClearCommandProperty =
        BindableProperty.Create(nameof(ClearCommand), typeof(ICommand), typeof(SearchLocationInput));

    public static readonly BindableProperty IsActiveProperty =
        BindableProperty.Create(nameof(IsActive), typeof(bool), typeof(SearchLocationInput), false, BindingMode.TwoWay);

    public string Query
    {
        get => (string)GetValue(QueryProperty);
        set => SetValue(QueryProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public IReadOnlyList<Suggestion>? Suggestions
    {
        get => (IReadOnlyList<Suggestion>?)GetValue(SuggestionsProperty);
        set => SetValue(SuggestionsProperty, value);
    }

    public ICommand? SearchCommand
    {
        get => (ICommand?)GetValue(SearchCommandProperty);
        set => SetValue(SearchCommandProperty, value);
    }

    public ICommand? SelectSuggestionCommand
    {
        get => (ICommand?)GetValue(SelectSuggestionCommandProperty);
        set => SetValue(SelectSuggestionCommandProperty, value);
    }

    public ICommand? ClearCommand
    {
        get => (ICommand?)GetValue(ClearCommandProperty);
        set => SetValue(ClearCommandProperty, value);
    }

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public bool HasSuggestions => Suggestions is not null && Suggestions.Count > 0;

    public SearchLocationInput()
    {
        InitializeComponent();
    }

    private void OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (SearchCommand?.CanExecute(null) == true)
            SearchCommand.Execute(null);
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == nameof(Suggestions))
            OnPropertyChanged(nameof(HasSuggestions));
    }
}
