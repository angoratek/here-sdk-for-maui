using System.Windows.Input;

using Here.Explore.Maui.RefApp.Extensions;

namespace Here.Explore.Maui.RefApp.Controls;

/// <summary>Visual severity of the banner.</summary>
public enum BannerSeverity
{
    /// <summary>Soft red — something failed.</summary>
    Error,
    /// <summary>Soft green — status/success notification, not an error.</summary>
    Info
}

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

    public static readonly BindableProperty SeverityProperty =
        BindableProperty.Create(nameof(Severity), typeof(BannerSeverity), typeof(ErrorBanner),
            defaultValue: BannerSeverity.Error,
            propertyChanged: (b, _, _) => ((ErrorBanner)b).ApplySeverity());

    public ErrorBanner()
    {
        InitializeComponent();
        IsVisible = false;
        RetryButton.IsVisible = false;
        ApplySeverity();
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

    /// <summary>Error renders soft red; Info renders soft green with a
    /// check icon — success/status messages are not errors.</summary>
    public BannerSeverity Severity
    {
        get => (BannerSeverity)GetValue(SeverityProperty);
        set => SetValue(SeverityProperty, value);
    }

    private void OnErrorMessageChanged()
    {
        IsVisible = !string.IsNullOrEmpty(ErrorMessage);
    }

    private void OnRetryCommandChanged()
    {
        RetryButton.IsVisible = RetryCommand is not null;
    }

    private void ApplySeverity()
    {
        // Resource lookups (like the Themed markup extension) resolve once
        // here; MAUI's AppThemeColorBinding isn't available for code-set
        // properties without a SetAppThemeColor on each element.
        if (Application.Current is null) return; // unit-test construction
        var isInfo = Severity == BannerSeverity.Info;
        var bgKey = isInfo ? "BannerInfoBg" : "BannerErrorBg";
        var textKey = isInfo ? "BannerInfoText" : "BannerErrorText";
        var (bgLight, bgDark) = Application.Current.GetThemedPair(bgKey, bgKey + "Dark");
        var (textLight, textDark) = Application.Current.GetThemedPair(textKey, textKey + "Dark");
        var dark = Application.Current.RequestedTheme == AppTheme.Dark;

        var bg = dark ? bgDark : bgLight;
        var text = dark ? textDark : textLight;
        RootBorder.BackgroundColor = bg;
        IconLabel.TextColor = text;
        MessageLabel.TextColor = text;
        IconLabel.Text = isInfo ? "\ue86c" : "\ue001"; // check_circle vs error
        RetryButton.BackgroundColor = text;
    }
}