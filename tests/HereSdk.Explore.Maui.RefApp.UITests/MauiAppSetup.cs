using System.Runtime.CompilerServices;
using Xunit;

namespace Here.Explore.Maui.RefApp.UITests;

/// <summary>
/// Sets up Application.Current with the resource keys needed by RefApp XAML controls.
/// Runs once before any test via ModuleInitializer.
/// </summary>
public static class MauiAppSetup
{
    private static readonly Lock _lock = new();
    private static bool _initialized;

    [ModuleInitializer]
    public static void Initialize()
    {
        lock (_lock)
        {
            if (_initialized) return;
            _initialized = true;

            if (Application.Current is not null) return;

            try
            {
                var app = new Application();
                var rd = app.Resources;

                // Colors referenced in XAML controls via AppThemeBinding
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
                rd.Add("Primary10", Color.FromArgb("#1A007AFF"));
                rd.Add("Primary30", Color.FromArgb("#4D007AFF"));
                rd.Add("PrimaryDark", Color.FromArgb("#007AFF"));

                // Styles referenced by XAML controls
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
            catch
            {
                // If setting up resources fails (e.g., dispatcher issues),
                // XAML-based control tests will handle the exception.
            }
        }
    }
}
