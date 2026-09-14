using System.Diagnostics;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Here.Explore.Maui.RefApp.Extensions;

/// <summary>
/// Resolves a color token pair (<c>Key</c> / <c>KeyDark</c>) from the application
/// resources into an AppThemeBinding: <c>TextColor="{ext:Themed TextPrimary}"</c>.
/// Falls back to the light value when no <c>KeyDark</c> sibling exists, so tokens
/// that are theme-invariant (status colors, brand colors) need no Dark twin.
/// </summary>
/// <remarks>
/// Declares <see cref="RequireServiceAttribute"/> for <c>IProvideValueTarget</c>
/// because the returned AppThemeBinding is produced by
/// <see cref="AppThemeBindingExtension"/>, which throws without that service.
/// (AppThemeBinding itself is internal to MAUI, so the extension must delegate.)
/// </remarks>
[ContentProperty(nameof(Key))]
[RequireService(new[] { typeof(IProvideValueTarget) })]
public class ThemedExtension : IMarkupExtension<BindingBase>
{
    public string Key { get; set; } = string.Empty;

    public BindingBase ProvideValue(IServiceProvider serviceProvider)
    {
        var resources = Application.Current?.Resources;
        if (resources is null || !resources.TryGetValue(Key, out var light))
        {
            Debug.WriteLine($"[Themed] Resource key '{Key}' not found in application resources.");
            light = Colors.Transparent;
        }

        if (resources is null || !resources.TryGetValue(Key + "Dark", out var dark))
            dark = light;

        return new AppThemeBindingExtension { Light = light, Dark = dark }
            .ProvideValue(serviceProvider) as BindingBase ?? new Binding();
    }

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        => ProvideValue(serviceProvider);
}