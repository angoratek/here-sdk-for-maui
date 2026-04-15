import Foundation
import UIKit
import heresdk

/// ObjC-visible UIView wrapper that owns a HERE MapView.
/// C# uses this as the platform view for the MAUI handler.
/// Other NativeBridge wrappers (HereMapCamera, HereMapScene, HereGestures)
/// extract MapView/mapScene/camera/gestures from this bridge view.
@objc(HereMapBridgeView)
public class HereMapBridgeView: NSObject {
    private(set) var mapView: MapView?

    /// @objc factory — C# calls this to create a bridge that owns the MapView.
    @objc public static func create() -> HereMapBridgeView {
        let bridge = HereMapBridgeView()
        bridge.mapView = MapView()
        return bridge
    }

    /// Expose the MapView as a UIView for C# handlers.
    @objc public var platformView: UIView? {
        return mapView
    }

    /// Non-@objc accessor for Swift code within the framework.
    var swiftMapView: MapView? { mapView }
}