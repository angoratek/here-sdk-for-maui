using System.Windows.Input;

namespace Here.Explore.Maui.RefApp.Controls;

public partial class ErrorBanner : ContentView
{
    public static readonly BindableProperty ErrorMessageProperty =
        BindableProperty.Create(nameof(ErrorMessage), typeof(string), typeof(ErrorBanner), defaultValue: null,
            propertyChanged: (b, _, _) => ((ErrorBanner)b).OnErrorMessageChanged());

    public static readonly BindableProperty RetryCommandProperty =
        BindableProperty.Create(nameof(RetryCommand), typeof(ICommand), typeof(ErrorBanner), defaultValue: null,
            propertyChanged: (b, _, _) => ((ErrorBanner)b).OnRetryCommandChanged());

    public static readonly BindableProperty RetryTextProperty =
        BindableProperty.Create(nameof(RetryText), typeof(string), typeof(ErrorBanner), defaultValue: "Retry");

    public ErrorBanner()
    {
        InitializeComponent();
        IsVisible = false;
        RetryButton.IsVisible = false;
    }

    public string? ErrorMessage
    {
        get => (string?)GetValue(ErrorMessageProperty);
        set => SetValue(ErrorMessageProperty, value);
    }

    public ICommand? RetryCommand
    {
        get => (ICommand?)GetValue(RetryCommandProperty);
        set => SetValue(RetryCommandProperty, value);
    }

    public string RetryText
    {
        get => (string)GetValue(RetryTextProperty);
        set => SetValue(RetryTextProperty, value);
    }

    private void OnErrorMessageChanged()
    {
        IsVisible = !string.IsNullOrEmpty(ErrorMessage);
    }

    private void OnRetryCommandChanged()
    {
        RetryButton.IsVisible = RetryCommand is not null;
    }
}
