using System.Windows.Input;

namespace Here.Explore.Maui.RefApp.Controls;

public partial class SearchResultsList : Border
{
    public static readonly BindableProperty SuggestionsProperty =
        BindableProperty.Create(nameof(Suggestions), typeof(IReadOnlyList<object>), typeof(SearchResultsList));

    public static readonly BindableProperty SelectedItemProperty =
        BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(SearchResultsList), defaultBindingMode: BindingMode.TwoWay);

    public static readonly BindableProperty HasResultsProperty =
        BindableProperty.Create(nameof(HasResults), typeof(bool), typeof(SearchResultsList), false);

    public static readonly BindableProperty ItemSelectedCommandProperty =
        BindableProperty.Create(nameof(ItemSelectedCommand), typeof(ICommand), typeof(SearchResultsList));

    public IReadOnlyList<object>? Suggestions
    {
        get => (IReadOnlyList<object>?)GetValue(SuggestionsProperty);
        set => SetValue(SuggestionsProperty, value);
    }

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public bool HasResults
    {
        get => (bool)GetValue(HasResultsProperty);
        set => SetValue(HasResultsProperty, value);
    }

    public ICommand? ItemSelectedCommand
    {
        get => (ICommand?)GetValue(ItemSelectedCommandProperty);
        set => SetValue(ItemSelectedCommandProperty, value);
    }

    public SearchResultsList()
    {
        InitializeComponent();
        BindingContext = this;
        ResultsCollectionView.SelectionChanged += OnSelectionChanged;
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count > 0 && e.CurrentSelection[0] is object suggestion)
        {
            ItemSelectedCommand?.Execute(suggestion);
            // Clear selection after executing command
            ResultsCollectionView.SelectedItem = null;
        }
    }
}
