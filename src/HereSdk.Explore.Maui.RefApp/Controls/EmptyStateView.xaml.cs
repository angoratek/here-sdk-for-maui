namespace Here.Explore.Maui.RefApp.Controls;

public partial class EmptyStateView : ContentView
{
    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(string), typeof(EmptyStateView), defaultValue: "📭");

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(EmptyStateView), defaultValue: null,
            propertyChanged: (b, _, _) => ((EmptyStateView)b).OnVisibilityChanged());

    public static readonly BindableProperty SubtitleProperty =
        BindableProperty.Create(nameof(Subtitle), typeof(string), typeof(EmptyStateView), defaultValue: null);

    public EmptyStateView()
    {
        InitializeComponent();
        IsVisible = false;
    }

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Subtitle
    {
        get => (string?)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    private void OnVisibilityChanged()
    {
        IsVisible = !string.IsNullOrEmpty(Title);
    }
}
