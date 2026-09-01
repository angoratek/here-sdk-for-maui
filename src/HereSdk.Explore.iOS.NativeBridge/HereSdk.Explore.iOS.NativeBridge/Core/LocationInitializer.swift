import Foundation

/// Stub class that satisfies the HERE SDK's optional `hsdk-initializeOptional`
/// reflection hook. The native SDK does an `NSClassFromString("LocationInitializer")`
/// lookup at engine init and then calls a class method on the result. The exact
/// selector varies by SDK version and platform: the 4.25.x Explore SDK on iOS
/// calls `+opt_initialize:` (one argument). If the class is missing OR the
/// selector is missing, the hook logs `class LocationInitializer not found`
/// (or `unrecognized selector sent to class 0x…`) and the `LoadingView` never
/// dismisses, wedging the iOS RefApp on the init splash.
///
/// `ILocationService` in the C# layer uses
/// `Microsoft.Maui.Devices.Sensors.Geolocation` as the primary source
/// (see CLAUDE.md rule 17 — HERE native positioning is not exposed in
/// iOS NativeBridge yet), so the SDK's own positioning hook is
/// intentionally unused. This stub exists solely to satisfy the
/// reflection lookup so the init hook can complete.
///
/// The Objective-C class name is **exactly** `LocationInitializer` (no
/// `Here` prefix) because that is the literal string the native SDK
/// passes to `NSClassFromString`. Do not rename it.
@objc(LocationInitializer)
public final class LocationInitializer: NSObject {
    /// Class method that the HERE SDK's optional-init hook actually calls on
    /// the resolved `LocationInitializer` class. Takes one argument (the
    /// hook passes whatever it has — typically a settings/options object —
    /// and we ignore it). Selector name is **exactly** `opt_initialize:` —
    /// the trailing colon is part of the selector name.
    @objc public static func opt_initialize(_ arg: Any?) {
        // Intentionally empty. Real positioning happens through MAUI's
        // Geolocation API (see `Here.Explore.Maui.Services.LocationService`).
    }
}