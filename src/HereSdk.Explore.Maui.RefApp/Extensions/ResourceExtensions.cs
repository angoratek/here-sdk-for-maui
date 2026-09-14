namespace Here.Explore.Maui.RefApp.Extensions;

/// <summary>
/// Resource-dictionary helpers for code-built views, which cannot use
/// <c>{ext:Themed}</c> markup. Pair with <see cref="BindableObjectExtensions"/>
/// <c>SetAppThemeColor</c> to keep constructed UI theme-reactive.
/// </summary>
public static class ResourceExtensions
{
    /// <summary>
    /// Resolves a <c>Key</c>/<c>KeyDark</c> sibling pair into explicit light/dark
    /// colors, falling back to the light value (then <paramref name="fallback"/>)
    /// when a key is missing.
    /// </summary>
    public static (Color Light, Color Dark) GetThemedPair(
        this Application? app, string lightKey, string darkKey, Color? fallback = null)
    {
        var fb = fallback ?? Colors.Transparent;
        if (app?.Resources is not { } resources)
            return (fb, fb);

        resources.TryGetValue(lightKey, out var light);
        if (!resources.TryGetValue(darkKey, out var dark))
            dark = light;
        return (light as Color ?? fb, dark as Color ?? fb);
    }
}