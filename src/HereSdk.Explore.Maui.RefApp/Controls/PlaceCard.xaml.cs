using Here.Explore.Maui.Models.Search;
using Here.Explore.Maui.RefApp.Converters;
using Here.Explore.Maui.RefApp.Services;

namespace Here.Explore.Maui.RefApp.Controls;

public partial class PlaceCard : ContentView
{
    public event EventHandler? DirectionsClicked;

    private string? _phone;

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
            var categoryId = cat.Id ?? cat.Name ?? "";
            CategoryDisc.IsVisible = true;
            CategoryIconLabel.Text = CategoryVisuals.GlyphFor(categoryId);
            CategoryDisc.BackgroundColor =
                CategoryVisuals.ColorFor(categoryId) ?? Color.FromArgb("#59FFFFFF");
            CategoryLabel.Text = cat.Name?.ToUpperInvariant();
            CategoryLabel.IsVisible = true;
        }
        else
        {
            CategoryDisc.IsVisible = false;
            CategoryLabel.IsVisible = false;
        }

        var hours = place.OpeningHours;
        if (hours is not null)
        {
            OpenStatusRow.IsVisible = true;
            if (hours.IsOpenNow)
            {
                OpenDot.Color = TokenColor.Get("SuccessGreen", Color.FromArgb("#2FBF71"));
                OpenLabel.Text = "Open Now";
            }
            else
            {
                OpenDot.Color = TokenColor.Get("ErrorRed", Color.FromArgb("#E33B4E"));
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
            AddressRow.IsVisible = true;
        }
        else
        {
            AddressRow.IsVisible = false;
        }

        if (distanceKm.HasValue)
        {
            DistanceLabel.Text = distanceKm.Value < 1
                ? $"{distanceKm.Value * 1000:F0}m away"
                : $"{distanceKm.Value:F1}km away";
            DistanceRow.IsVisible = true;
        }
        else
        {
            DistanceRow.IsVisible = false;
        }

        var contact = place.Contact;
        if (contact is not null && (!string.IsNullOrEmpty(contact.Phone) || !string.IsNullOrEmpty(contact.Website)))
        {
            ContactRow.IsVisible = true;
            _phone = contact.Phone;
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
        if (!string.IsNullOrEmpty(_phone))
            _ = Launcher.OpenAsync($"tel:{_phone}");
    }

    private void OnWebsiteClicked(object? sender, EventArgs e)
    {
        // Opened via ViewModel
    }

    private void OnDirectionsClicked(object? sender, EventArgs e)
    {
        DirectionsClicked?.Invoke(this, EventArgs.Empty);
    }
}