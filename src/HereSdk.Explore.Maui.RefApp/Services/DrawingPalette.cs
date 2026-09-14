namespace Here.Explore.Maui.RefApp.Services;

/// <summary>
/// Drawn-object color constants for the Tools panel. Single source of truth
/// so the drawing tools, previews and demo objects share one visual language
/// (coral accent from the design tokens in Colors.xaml).
/// </summary>
public static class DrawingPalette
{
    /// <summary>Brand coral #FF385C, opaque — committed strokes and marker pins.</summary>
    public const uint Stroke = 0xFFFF385C;

    /// <summary>Coral at 30% — committed polygon fills.</summary>
    public const uint Fill = 0x4DFF385C;

    /// <summary>Coral at 55% — live drawing preview strokes.</summary>
    public const uint PreviewStroke = 0x8CFF385C;

    /// <summary>Coral at 15% — live drawing preview fills.</summary>
    public const uint PreviewFill = 0x26FF385C;

    /// <summary>Committed polyline width in pixels.</summary>
    public const int StrokeWidthPixels = 4;

    /// <summary>Live preview polyline width in pixels (thinner than committed).</summary>
    public const int PreviewWidthPixels = 3;

    /// <summary>Committed circle stroke width in pixels.</summary>
    public const int CircleStrokeWidthPixels = 3;

    /// <summary>Palette for demo/dataset objects that need distinct hues.</summary>
    public static readonly uint[] SeriesColors =
    {
        0xFFFF385C, // coral
        0xFFFF9500, // orange
        0xFF34C759, // green
        0xFFFF3B30, // red
    };

    /// <summary>Fill variants for demo polygons (same hues at ~20%).</summary>
    public static readonly uint[] SeriesFills =
    {
        0x33FF385C,
        0x335850FF,
        0x33FF3B30,
        0x33FF9500,
        0x3334C759,
    };
}