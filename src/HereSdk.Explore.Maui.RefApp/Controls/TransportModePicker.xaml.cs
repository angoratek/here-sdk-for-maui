using Here.Explore.Maui.RefApp.Extensions;

namespace Here.Explore.Maui.RefApp.Controls;

public partial class TransportModePicker : ContentView
{
    public record TransportModeOption(string Label, string Icon, int ModeKey)
    {
        public bool IsSelected { get; set; }
    }

    public event EventHandler<int>? ModeSelected;

    public static readonly TransportModeOption[] Modes =
    {
        // Icons are Material Icons glyphs (codepoints).
        new("Car", "\ue531", 0),          // directions_car
        new("Truck", "\ue558", 1),        // local_shipping
        new("Pedestrian", "\ue536", 2),   // directions_walk
        new("Bicycle", "\ue52f", 3),      // directions_bike
        new("Scooter", "\ueb1f", 4),      // electric_scooter
        new("Bus", "\ue530", 5),          // directions_bus
        new("Taxi", "\ue559", 6),         // local_taxi
        new("Transit", "\ue571", 7),      // tram
    };

    private readonly List<Border> _chipBorders = new();
    private readonly List<Label> _chipLabels = new();
    private readonly List<Label> _iconLabels = new();
    private int _selectedModeKey;

    public TransportModePicker()
    {
        InitializeComponent();
        Populate();
        SelectMode(0); // Default: Car
    }

    private void Populate()
    {
        foreach (var mode in Modes)
        {
            var content = new HorizontalStackLayout
            {
                Spacing = 7,
                VerticalOptions = LayoutOptions.Center
            };
            var icon = new Label
            {
                Text = mode.Icon,
                FontFamily = "MaterialIcons",
                FontSize = 17,
                VerticalOptions = LayoutOptions.Center
            };
            var label = new Label
            {
                Text = mode.Label,
                FontSize = 13,
                FontFamily = "InterMedium",
                VerticalOptions = LayoutOptions.Center
            };
            content.Children.Add(icon);
            content.Children.Add(label);

            var border = new Border
            {
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 19 },
                StrokeThickness = 0,
                Padding = new Thickness(13, 8),
                HeightRequest = 40,
                Content = content
            };
            ApplyUnselectedStyle(border, icon, label);

            var modeIndex = mode.ModeKey;
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => SelectMode(modeIndex);
            border.GestureRecognizers.Add(tap);

            _chipBorders.Add(border);
            _chipLabels.Add(label);
            _iconLabels.Add(icon);
            ModesContainer.Children.Add(border);
        }
    }

    /// <summary>
    /// Applies the unselected chip look with AppThemeColor bindings so the
    /// picker re-tints live when dark mode flips (previously the colors were
    /// snapshotted once at construction time).
    /// </summary>
    private void ApplyUnselectedStyle(Border border, Label icon, Label label)
    {
        var (stLight, stDark) = Application.Current.GetThemedPair("SurfaceTertiary", "SurfaceTertiaryDark");
        border.SetAppThemeColor(Border.BackgroundColorProperty, stLight, stDark);
        border.Stroke = border.BackgroundColor;

        var (tpLight, tpDark) = Application.Current.GetThemedPair("TextPrimary", "TextPrimaryDark");
        label.SetAppThemeColor(Label.TextColorProperty, tpLight, tpDark);

        var (secLight, secDark) = Application.Current.GetThemedPair("TextSecondary", "TextSecondaryDark");
        icon.SetAppThemeColor(Label.TextColorProperty, secLight, secDark);
    }

    public void SelectMode(int modeKey)
    {
        _selectedModeKey = modeKey;
        for (int i = 0; i < _chipBorders.Count; i++)
        {
            if (i == modeKey)
            {
                // Primary is coral in both themes; the selected state needs
                // no theme binding.
                var (pLight, pDark) = Application.Current.GetThemedPair("Primary", "PrimaryDark");
                _chipBorders[i].SetAppThemeColor(Border.BackgroundColorProperty, pLight, pDark);
                _chipBorders[i].Stroke = _chipBorders[i].BackgroundColor;
                _chipLabels[i].TextColor = Colors.White;
                _iconLabels[i].TextColor = Colors.White;
            }
            else
            {
                ApplyUnselectedStyle(_chipBorders[i], _iconLabels[i], _chipLabels[i]);
            }
        }
        ModeSelected?.Invoke(this, modeKey);
    }
}