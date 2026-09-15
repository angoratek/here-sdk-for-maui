using Here.Explore.Maui.Models.Traffic;

namespace Here.Explore.Maui.RefApp.Services;

/// <summary>
/// Marker pin visuals per item type: a Material Icons glyph + tint color for
/// places, traffic incidents, and the fixed pins (origin, destination,
/// map-tap). Rendered by MapMarker.Glyph/Color as teardrop pins with a white
/// glyph, so every pin on the map reads as what it marks.
/// </summary>
public static class MarkerVisuals
{
    // Pin tint palette (ARGB).
    public const uint Accent = 0xFFFF385C;    // brand coral — selected place, destination
    public const uint Origin = 0xFF2FBF71;    // green — route origin
    public const uint Selected = 0xFF0A7AFF;  // blue — map-tap / long-press pins
    public const uint Neutral = 0xFF8E8E93;   // gray — query-center / isoline markers

    public const string GlyphPlace = "\ue55f";       // place
    public const string GlyphOrigin = "\ue57a";      // trip_origin
    public const string GlyphDestination = "\ue55f"; // place
    public const string GlyphTap = "\ue55f";         // place
    public const string GlyphLongPress = "\ue55c";   // my_location

    /// <summary>Pin for a search/category result or place-card pin.</summary>
    public static (string Glyph, uint Color) ForPlace(Here.Explore.Maui.Models.Search.Place place)
    {
        var category = place.PrimaryCategory ?? place.Categories?.FirstOrDefault();
        // Match on id AND name — HERE category ids are numeric
        // ("100-1000-0000" is restaurants) and only the name carries meaning.
        var categoryId = category is null ? "" : $"{category.Id} {category.Name}";
        return (CategoryVisuals.GlyphFor(categoryId),
                CategoryVisuals.ColorFor(categoryId)?.ToUint() ?? Accent);
    }

    /// <summary>Pin for a traffic incident: glyph by type, tint by impact severity.</summary>
    public static (string Glyph, uint Color) ForIncident(TrafficIncident incident) =>
        (IncidentGlyph(incident.Type), IncidentColor(incident.Impact));

    /// <summary>Incident glyph by type — single source shared with the incident list.</summary>
    public static string IncidentGlyph(TrafficIncidentType type) => type switch
    {
        TrafficIncidentType.Accident        => "\ue0c8",  // location_on
        TrafficIncidentType.Congestion      => "\ue531",  // directions_car
        TrafficIncidentType.Construction    => "\uea3c",  // construction
        TrafficIncidentType.RoadClosure     => "\ue5cd",  // close
        TrafficIncidentType.RoadHazard      => "\ue002",  // warning
        TrafficIncidentType.DisabledVehicle => "\ue0b9",  // build (wrench)
        TrafficIncidentType.LaneRestriction => "\ue565",  // traffic
        TrafficIncidentType.MassTransit     => "\ue530",  // directions_bus
        TrafficIncidentType.PlannedEvent    => "\ue53f",  // local_attraction
        TrafficIncidentType.Weather         => "\ue430",  // wb_sunny
        _                                   => "\ue002",  // warning
    };

    /// <summary>Incident pin tint by impact severity — mirrors TrafficIncidentImpactToColorConverter.</summary>
    public static uint IncidentColor(TrafficIncidentImpact impact) => impact switch
    {
        TrafficIncidentImpact.Closed   => 0xFFE33B4E,
        TrafficIncidentImpact.Major    => 0xFFF5A623,
        TrafficIncidentImpact.Moderate => 0xFFF5C518,
        TrafficIncidentImpact.Minor    => 0xFF2FBF71,
        _                              => 0xFFE33B4E,
    };
}