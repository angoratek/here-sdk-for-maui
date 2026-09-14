using Here.Explore.Maui.RefApp.Extensions;
using Here.Explore.Maui.RefApp.Services;

namespace Here.Explore.Maui.RefApp.Controls;

public partial class CategoryChipBar : ContentView
{
    public record CategoryChip(string CategoryId, string Label, string Icon);

    public event EventHandler<CategoryChip>? CategorySelected;

    private CategoryChip? _selectedChip;
    private readonly List<Border> _chipBorders = new();

    public static readonly CategoryChip[] DefaultCategories =
    {
        // HERE Place Category IDs from the HERE Places Category System
        // (https://developer.here.com/documentation places API). The IDs are
        // taxonomy codes such as "100-1000" for restaurants; the previous
        // values ("restaurant", "hotel", …) were placeholders and caused
        // the API to return 400 Illegal input for parameter 'categories'.
        // Icons are Material Icons glyphs (see CategoryVisuals).
        new("100-1000", "Restaurants", "\ue56c"),
        new("500-5000", "Hotels", "\ue53a"),
        new("700-7600-0116", "Gas Stations", "\ue546"),
        new("800-8500", "Parking", "\ue54f"),
        new("700-7010", "ATMs", "\ue84f"),
        new("800-8000", "Hospitals", "\ue548"),
        new("600", "Shopping", "\uf1cc"),
        new("300", "Attractions", "\ue53f"),
    };

    public CategoryChipBar()
    {
        InitializeComponent();
        Populate();
    }

    private void Populate()
    {
        foreach (var chip in DefaultCategories)
        {
            var chipContent = new HorizontalStackLayout
            {
                Spacing = 6,
                VerticalOptions = LayoutOptions.Center
            };
            var iconLabel = new Label
            {
                Text = chip.Icon,
                FontFamily = "MaterialIcons",
                FontSize = 15,
                VerticalOptions = LayoutOptions.Center
            };
            var textLabel = new Label
            {
                Text = chip.Label,
                FontSize = 13,
                FontFamily = "InterMedium",
                VerticalOptions = LayoutOptions.Center
            };
            chipContent.Children.Add(iconLabel);
            chipContent.Children.Add(textLabel);

            var chipBorder = new Border
            {
                // Per-chip AutomationId so Appium can target each one
                // without relying on the icon-prefixed label text.
                AutomationId = $"ExploreCategory{ChipIdFromLabel(chip.Label)}",
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 18 },
                StrokeThickness = 0,
                Padding = new Thickness(12, 7),
                HeightRequest = 36,
                Content = chipContent
            };

            ApplyUnselectedStyle(chipBorder, iconLabel, textLabel, chip.CategoryId);

            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => OnChipTapped(chip, chipBorder, iconLabel, textLabel);
            chipBorder.GestureRecognizers.Add(tap);

            _chipBorders.Add(chipBorder);
            ChipsContainer.Children.Add(chipBorder);
        }
    }

    // AppTheme resource keys for chip chrome.
    private const string SurfaceTertiaryLight = "SurfaceTertiary";
    private const string SurfaceTertiaryDark = "SurfaceTertiaryDark";

    /// <summary>
    /// Applies the unselected chip look with AppThemeColor bindings so the
    /// chips re-tint live when dark mode flips (previously the colors were
    /// snapshotted once at construction time).
    /// </summary>
    private void ApplyUnselectedStyle(Border border, Label iconLabel, Label textLabel, string categoryId)
    {
        var (stLight, stDark) = Application.Current.GetThemedPair(SurfaceTertiaryLight, SurfaceTertiaryDark);
        border.SetAppThemeColor(Border.BackgroundColorProperty, stLight, stDark);

        var (tsLight, tsDark) = Application.Current.GetThemedPair("TextPrimary", "TextPrimaryDark");
        textLabel.SetAppThemeColor(Label.TextColorProperty, tsLight, tsDark);

        var categoryColor = CategoryVisuals.ColorFor(categoryId);
        var (secLight, secDark) = Application.Current.GetThemedPair("TextSecondary", "TextSecondaryDark");
        if (categoryColor is not null)
            iconLabel.TextColor = categoryColor;
        else
            iconLabel.SetAppThemeColor(Label.TextColorProperty, secLight, secDark);
    }

    /// <summary>
    /// Maps a chip's human-readable label (e.g. "Restaurants") to the
    /// suffix used in its AutomationId (e.g. "Restaurants"). One-to-one
    /// with the labels in <see cref="DefaultCategories"/>; labels with
    /// spaces are stripped (none currently do).
    /// </summary>
    private static string ChipIdFromLabel(string label) =>
        label.Replace(" ", string.Empty, StringComparison.Ordinal);

    private void OnChipTapped(CategoryChip chip, Border border, Label iconLabel, Label textLabel)
    {
        // Deselect all — surface chip, category-tinted icon
        foreach (var b in _chipBorders)
        {
            if (b.Content is HorizontalStackLayout content)
            {
                var icon = (Label)content.Children[0];
                var text = (Label)content.Children[1];
                var other = DefaultCategories.First(c => ChipIdFromLabel(c.Label) ==
                    b.AutomationId.Replace("ExploreCategory", string.Empty, StringComparison.Ordinal));
                ApplyUnselectedStyle(b, icon, text, other.CategoryId);
            }
        }

        // Select tapped — solid coral pill, white icon + text.
        // Primary is coral in both themes (PrimaryDark is a lighter tint),
        // so the selected state itself needs no theme binding.
        var (pLight, pDark) = Application.Current.GetThemedPair("Primary", "PrimaryDark");
        border.SetAppThemeColor(Border.BackgroundColorProperty, pLight, pDark);
        iconLabel.TextColor = Colors.White;
        textLabel.TextColor = Colors.White;

        _selectedChip = chip;
        CategorySelected?.Invoke(this, chip);
    }
}