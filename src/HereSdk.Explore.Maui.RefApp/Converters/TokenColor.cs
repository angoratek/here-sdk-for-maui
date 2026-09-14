namespace Here.Explore.Maui.RefApp.Converters;

/// <summary>
/// Shared token-color lookup for converters: status colors are theme-invariant,
/// so they resolve once from the application resources instead of hard-coding
/// hex literals that drift from Colors.xaml.
/// </summary>
internal static class TokenColor
{
    internal static Color Get(string key, Color fallback)
        => Application.Current?.Resources.TryGetValue(key, out var value) == true &&
           value is Color color
            ? color
            : fallback;
}