namespace Here.Explore.Maui.RefApp.Services;

/// <summary>
/// Maps a HERE place category (taxonomy id or display name) to the
/// visual language of the app: a Material Icons glyph and a tint color.
/// Shared by the category chips, the search-result carousel, and the
/// place card so all three stay consistent.
/// </summary>
public static class CategoryVisuals
{
    // Material Icons codepoints (PUA private-use area).
    private const string GlyphRestaurant = "\ue56c";       // restaurant
    private const string GlyphCafe = "\ue541";             // local_cafe
    private const string GlyphHotel = "\ue53a";            // hotel
    private const string GlyphFuel = "\ue546";             // local_gas_station
    private const string GlyphParking = "\ue54f";          // local_parking
    private const string GlyphHospital = "\ue548";         // local_hospital
    private const string GlyphBank = "\ue84f";             // account_balance
    private const string GlyphShopping = "\uf1cc";         // shopping_bag
    private const string GlyphMuseum = "\uea36";           // museum
    private const string GlyphAttraction = "\ue53f";       // local_attraction
    private const string GlyphPlace = "\ue55f";            // place

    /// <summary>Material Icons glyph (codepoint string) for the category.</summary>
    public static string GlyphFor(string categoryIdOrName)
    {
        var id = categoryIdOrName ?? "";
        if (Matches(id, "restaurant", "food", "eating")) return GlyphRestaurant;
        if (Matches(id, "cafe", "coffee")) return GlyphCafe;
        if (Matches(id, "hotel", "lodging", "accommodation")) return GlyphHotel;
        if (Matches(id, "fuel", "gas", "charging", "petrol")) return GlyphFuel;
        if (Matches(id, "parking")) return GlyphParking;
        if (Matches(id, "hospital", "medical", "health")) return GlyphHospital;
        if (Matches(id, "atm", "bank")) return GlyphBank;
        if (Matches(id, "shop", "retail", "mall", "store")) return GlyphShopping;
        if (Matches(id, "museum", "theatre")) return GlyphMuseum;
        if (Matches(id, "park", "attraction", "leisure", "sights")) return GlyphAttraction;
        return GlyphPlace;
    }

    /// <summary>Category tint color. Returns null for unmapped categories.</summary>
    public static Color? ColorFor(string categoryIdOrName)
    {
        var id = categoryIdOrName ?? "";
        if (Matches(id, "restaurant", "food", "cafe", "coffee", "eating")) return Color.FromArgb("#FF7A59");
        if (Matches(id, "hotel", "lodging", "accommodation")) return Color.FromArgb("#7C5CD6");
        if (Matches(id, "parking")) return Color.FromArgb("#8E8E93");
        if (Matches(id, "fuel", "gas", "charging", "petrol")) return Color.FromArgb("#0A7AFF");
        if (Matches(id, "hospital", "medical", "health")) return Color.FromArgb("#E33B4E");
        if (Matches(id, "atm", "bank")) return Color.FromArgb("#279E8F");
        if (Matches(id, "shop", "retail", "mall", "store")) return Color.FromArgb("#FF5B8A");
        if (Matches(id, "museum", "theatre")) return Color.FromArgb("#7C5CD6");
        if (Matches(id, "park", "attraction", "leisure", "sights")) return Color.FromArgb("#2FBF71");
        return null;
    }

    private static bool Matches(string id, params string[] keys) =>
        keys.Any(k => id.Contains(k, StringComparison.OrdinalIgnoreCase));
}