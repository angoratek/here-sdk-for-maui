using Xunit;

namespace Here.Explore.Maui.RefApp.UITests;

/// <summary>
/// Sets up Application.Current with the resource keys needed by RefApp XAML controls.
/// Must run before any control that calls InitializeComponent() with StaticResource references.
/// </summary>
public sealed class MauiAppTestFixture : IDisposable
{
    private static readonly Lock _lock = new();
    private static bool _initialized;

    public MauiAppTestFixture()
    {
        Initialize();
    }

    internal static void Initialize()
    {
        lock (_lock)
        {
            if (_initialized) return;
            _initialized = true;

            if (Application.Current is not null) return;

            var app = new Application();
            var rd = app.Resources;

            // Colors referenced in XAML via AppThemeBinding
            rd.Add("TextPrimary", Color.FromArgb("#000000"));
            rd.Add("TextPrimaryDark", Color.FromArgb("#FFFFFF"));
            rd.Add("TextSecondary", Color.FromArgb("#6E6E73"));
            rd.Add("TextSecondaryDark", Color.FromArgb("#AEAEB2"));
            rd.Add("TextTertiary", Color.FromArgb("#C7C7CC"));
            rd.Add("TextTertiaryDark", Color.FromArgb("#48484A"));
            rd.Add("Surface", Color.FromArgb("#FFFFFF"));
            rd.Add("SurfaceSecondaryDark", Color.FromArgb("#2C2C2E"));
            rd.Add("SurfaceTertiary", Color.FromArgb("#E5E5EA"));
            rd.Add("SurfaceTertiaryDark", Color.FromArgb("#3A3A3C"));
            rd.Add("SurfaceSecondary", Color.FromArgb("#F2F2F7"));
            rd.Add("ShadowColor", Color.FromArgb("#40000000"));
            rd.Add("ShadowColorDark", Color.FromArgb("#80000000"));
            rd.Add("Primary", Color.FromArgb("#007AFF"));

            // Styles
            var cardBorder = new Style(typeof(Border))
            {
                Setters =
                {
                    new Setter { Property = Border.StrokeShapeProperty, Value = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 } },
                    new Setter { Property = Border.StrokeThicknessProperty, Value = 0.0 },
                    new Setter { Property = Border.PaddingProperty, Value = new Thickness(16) },
                }
            };
            rd.Add("CardBorder", cardBorder);

            Application.Current = app;
        }
    }

    public void Dispose() { }
}

[CollectionDefinition("MauiAppCollection")]
public class MauiAppCollection : ICollectionFixture<MauiAppTestFixture>
{
}
