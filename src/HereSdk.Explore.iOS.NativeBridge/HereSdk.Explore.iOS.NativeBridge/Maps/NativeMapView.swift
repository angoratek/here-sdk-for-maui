import Foundation
import UIKit
import heresdk

/// ObjC-visible UIView wrapper that owns a HERE MapView.
/// C# uses this as the platform view for the MAUI handler.
/// Other NativeBridge wrappers (HereMapCamera, HereMapScene, HereGestures)
/// extract MapView/mapScene/camera/gestures from this bridge view.
/// Bridges the SDK's MapIdleDelegate to a plain closure.
private final class MapIdleDelegateBridge: NSObject, MapIdleDelegate {
    private let handler: () -> Void

    init(_ handler: @escaping () -> Void) {
        self.handler = handler
    }

    func onMapBusy() {}

    func onMapIdle() {
        handler()
    }
}

@objc(HereMapBridgeView)
public class HereMapBridgeView: NSObject {
    private(set) var mapView: MapView?
    private var idleDelegateBridge: MapIdleDelegateBridge?

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

    /// Convert screen coordinates to geo coordinates.
    @objc public func viewToGeoCoordinates(originX: Double, originY: Double) -> HereGeoCoordinates? {
        guard let mapView = mapView else { return nil }
        let point = Point2D(x: originX, y: originY)
        if let geo = mapView.viewToGeoCoordinates(viewCoordinates: point) {
            return HereGeoCoordinates.from(geo)
        }
        return nil
    }

    /// Picks map items at a screen point (1x1 px area, MAP_ITEMS filter) and
    /// reports the first picked marker's coordinates — nil when nothing was picked.
    @objc public func pickFirstMarker(originX: Double, originY: Double,
                                      completion: @escaping (HereGeoCoordinates?, String?) -> Void) {
        guard let mapView = mapView else {
            completion(nil, "MapView not initialized")
            return
        }
        let point = Point2D(x: originX, y: originY)
        let area = Rectangle2D(origin: point, size: Size2D(width: 1, height: 1))
        let filter = MapScene.MapPickFilter(filter: [.mapItems])
        mapView.pick(filter: filter, inside: area) { result in
            if let marker = result?.mapItems?.markers.first {
                completion(HereGeoCoordinates.from(marker.coordinates), nil)
            } else {
                completion(nil, nil)
            }
        }
    }

    /// Registers a callback invoked when the map becomes idle after loading
    /// completes (rendering and map data loaded).
    @objc public func setMapIdleHandler(_ handler: @escaping () -> Void) {
        guard let mapView = mapView else { return }
        if idleDelegateBridge == nil {
            let bridge = MapIdleDelegateBridge(handler)
            idleDelegateBridge = bridge
            mapView.hereMap.addMapIdleDelegate(bridge)
        }
    }

    /// Non-@objc accessor for Swift code within the framework.
    var swiftMapView: MapView? { mapView }
}