import Foundation
import heresdk

/// Factory for creating MapView instances.
/// MapView is already ObjC-visible from the SDK, so we just provide creation helpers.
@objc(HereMapBridgeView)
public class HereMapBridgeView: NSObject {
    @objc public static func createMapView() -> MapView {
        return MapView()
    }
}