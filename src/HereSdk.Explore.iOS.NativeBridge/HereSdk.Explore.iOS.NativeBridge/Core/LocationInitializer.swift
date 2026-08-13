import Foundation

/// Stub class that satisfies the HERE SDK's optional `hsdk-initializeOptional`
/// reflection hook. The native SDK does an `NSClassFromString("LocationInitializer")`
/// lookup at engine init; if the class is missing, the hook logs
/// `class LocationInitializer not found` and the `LoadingView` never
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
    @objc public static func initializeLocation() {
        // Intentionally empty. Real positioning happens through MAUI's
        // Geolocation API (see `Here.Explore.Maui.Services.LocationService`).
    }
}
