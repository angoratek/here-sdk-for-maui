using Here.Explore.Maui.Models.Search;

namespace Here.Explore.Maui.RefApp.Controls;

public partial class PlaceCard : ContentView
{
    public event EventHandler? DirectionsClicked;

    public PlaceCard()
    {
        InitializeComponent();
    }

    public void LoadPlace(Place place, double? distanceKm = null)
    {
        TitleLabel.Text = place.Title;

        var cat = place.PrimaryCategory ?? place.Categories?.FirstOrDefault();
        if (cat is not null)
        {
            CategoryChip.IsVisible = true;
            var categoryId = cat.Id ?? cat.Name ?? "";
            CategoryIconLabel.Text = CategoryIconFor(categoryId);
            CategoryLabel.Text = cat.Name;
            CategoryChip.BackgroundColor = CategoryColorFor(categoryId);
        }
        else
        {
            CategoryChip.IsVisible = false;
        }

        var hours = place.OpeningHours;
        if (hours is not null)
        {
            OpenStatusRow.IsVisible = true;
            if (hours.IsOpenNow)
            {
                OpenDot.Color = Color.FromArgb("#34C759");
                OpenLabel.Text = "Open Now";
            }
            else
            {
                OpenDot.Color = Color.FromArgb("#FF3B30");
                OpenLabel.Text = "Closed";
            }
        }
        else
        {
            OpenStatusRow.IsVisible = false;
        }

        var addr = place.Address;
        if (addr is not null)
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(addr.Street)) parts.Add(addr.Street);
            if (!string.IsNullOrEmpty(addr.HouseNumber))
            {
                if (parts.Count == 0) parts.Add("");
                parts[^1] += " " + addr.HouseNumber;
            }
            var city = !string.IsNullOrEmpty(addr.District) ? addr.District : addr.City;
            var region = new List<string>();
            if (!string.IsNullOrEmpty(city)) region.Add(city);
            if (!string.IsNullOrEmpty(addr.State)) region.Add(addr.State);
            if (region.Count > 0) parts.Add(string.Join(", ", region));
            if (!string.IsNullOrEmpty(addr.PostalCode)) parts.Add(addr.PostalCode);
            AddressLabel.Text = string.Join("\n", parts);
            AddressLabel.IsVisible = true;
        }
        else
        {
            AddressLabel.IsVisible = false;
        }

        if (distanceKm.HasValue)
        {
            DistanceLabel.Text = distanceKm.Value < 1
                ? $"{distanceKm.Value * 1000:F0}m away"
                : $"{distanceKm.Value:F1}km away";
            DistanceLabel.IsVisible = true;
        }
        else
        {
            DistanceLabel.IsVisible = false;
        }

        var contact = place.Contact;
        if (contact is not null && (!string.IsNullOrEmpty(contact.Phone) || !string.IsNullOrEmpty(contact.Website)))
        {
            ContactRow.IsVisible = true;
            PhoneButton.IsVisible = !string.IsNullOrEmpty(contact.Phone);
            WebsiteButton.IsVisible = !string.IsNullOrEmpty(contact.Website);
        }
        else
        {
            ContactRow.IsVisible = false;
        }
    }

    private void OnPhoneClicked(object? sender, EventArgs e)
    {
        _ = Launcher.OpenAsync($"tel:{PhoneButton.Text}");
    }

    private void OnWebsiteClicked(object? sender, EventArgs e)
    {
        // Opened via ViewModel
    }

    private void OnDirectionsClicked(object? sender, EventArgs e)
    {
        DirectionsClicked?.Invoke(this, EventArgs.Empty);
    }

    private static Color CategoryColorFor(string categoryId)
    {
        if (categoryId.Contains("restaurant", StringComparison.OrdinalIgnoreCase) || categoryId.Contains("food", StringComparison.OrdinalIgnoreCase))
            return Color.FromArgb("#FF9500");
        if (categoryId.Contains("hotel", StringComparison.OrdinalIgnoreCase) || categoryId.Contains("lodging", StringComparison.OrdinalIgnoreCase))
            return Color.FromArgb("#5856D6");
        if (categoryId.Contains("shop", StringComparison.OrdinalIgnoreCase) || categoryId.Contains("retail", StringComparison.OrdinalIgnoreCase))
            return Color.FromArgb("#FF2D55");
        if (categoryId.Contains("fuel", StringComparison.OrdinalIgnoreCase) || categoryId.Contains("gas", StringComparison.OrdinalIgnoreCase) || categoryId.Contains("charging", StringComparison.OrdinalIgnoreCase))
            return Color.FromArgb("#007AFF");
        if (categoryId.Contains("hospital", StringComparison.OrdinalIgnoreCase) || categoryId.Contains("medical", StringComparison.OrdinalIgnoreCase))
            return Color.FromArgb("#FF3B30");
        if (categoryId.Contains("parking", StringComparison.OrdinalIgnoreCase))
            return Color.FromArgb("#8E8E93");
        if (categoryId.Contains("park", StringComparison.OrdinalIgnoreCase) || categoryId.Contains("museum", StringComparison.OrdinalIgnoreCase) || categoryId.Contains("attraction", StringComparison.OrdinalIgnoreCase))
            return Color.FromArgb("#34C759");
        if (categoryId.Contains("bank", StringComparison.OrdinalIgnoreCase) || categoryId.Contains("atm", StringComparison.OrdinalIgnoreCase))
            return Color.FromArgb("#34A853");
        return Color.FromArgb("#5AC8FA");
    }

    /// <summary>
    /// Maps a HERE Place Category id (or display name) to an emoji glyph
    /// shown in the category chip. Mirrors <see cref="CategoryColorFor"/>
    /// so the icon and colour are consistent.
    /// </summary>
    private static string CategoryIconFor(string categoryId)
    {
        if (categoryId.Contains("restaurant", StringComparison.OrdinalIgnoreCase) || categoryId.Contains("food", StringComparison.OrdinalIgnoreCase))
            return "🍽";
        if (categoryId.Contains("hotel", StringComparison.OrdinalIgnoreCase) || categoryId.Contains("lodging", StringComparison.OrdinalIgnoreCase))
            return "🏨";
        if (categoryId.Contains("shop", StringComparison.OrdinalIgnoreCase) || categoryId.Contains("retail", StringComparison.OrdinalIgnoreCase))
            return "🛍";
        if (categoryId.Contains("fuel", StringComparison.OrdinalIgnoreCase) || categoryId.Contains("gas", StringComparison.OrdinalIgnoreCase) || categoryId.Contains("charging", StringComparison.OrdinalIgnoreCase))
            return "⛽";
        if (categoryId.Contains("hospital", StringComparison.OrdinalIgnoreCase) || categoryId.Contains("medical", StringComparison.OrdinalIgnoreCase))
            return "🏥";
        if (categoryId.Contains("parking", StringComparison.OrdinalIgnoreCase))
            return "🅿";
        if (categoryId.Contains("park", StringComparison.OrdinalIgnoreCase) || categoryId.Contains("museum", StringComparison.OrdinalIgnoreCase) || categoryId.Contains("attraction", StringComparison.OrdinalIgnoreCase))
            return "🎯";
        if (categoryId.Contains("bank", StringComparison.OrdinalIgnoreCase) || categoryId.Contains("atm", StringComparison.OrdinalIgnoreCase))
            return "🏧";
        return "📍";
    }
}
