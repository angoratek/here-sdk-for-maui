import Foundation
import heresdk

/// ObjC-visible NSInteger enum wrapper for MapScheme (which is UInt32-backed in Swift).
@objc(HereMapScheme)
public enum HereMapScheme: NSInteger {
    case normalDay = 0
    case normalNight = 1
    case satellite = 2
    case hybridDay = 3
    case hybridNight = 4
    case liteDay = 5
    case liteNight = 6
    case liteHybridDay = 7
    case liteHybridNight = 8
    case logisticsDay = 9
    case logisticsNight = 10
    case logisticsHybridDay = 11
    case logisticsHybridNight = 12
    case roadNetworkDay = 13
    case roadNetworkNight = 14

    func toSwift() -> MapScheme? {
        switch self {
        case .normalDay: return .normalDay
        case .normalNight: return .normalNight
        case .satellite: return .satellite
        case .hybridDay: return .hybridDay
        case .hybridNight: return .hybridNight
        case .liteDay: return .liteDay
        case .liteNight: return .liteNight
        case .liteHybridDay: return .liteHybridDay
        case .liteHybridNight: return .liteHybridNight
        case .logisticsDay: return .logisticsDay
        case .logisticsNight: return .logisticsNight
        case .logisticsHybridDay: return .logisticsHybridDay
        case .logisticsHybridNight: return .logisticsHybridNight
        case .roadNetworkDay: return .roadNetworkDay
        case .roadNetworkNight: return .roadNetworkNight
        }
    }

    static func from(_ swift: MapScheme) -> HereMapScheme {
        switch swift {
        case .normalDay: return .normalDay
        case .normalNight: return .normalNight
        case .satellite: return .satellite
        case .hybridDay: return .hybridDay
        case .hybridNight: return .hybridNight
        case .liteDay: return .liteDay
        case .liteNight: return .liteNight
        case .liteHybridDay: return .liteHybridDay
        case .liteHybridNight: return .liteHybridNight
        case .logisticsDay: return .logisticsDay
        case .logisticsNight: return .logisticsNight
        case .logisticsHybridDay: return .logisticsHybridDay
        case .logisticsHybridNight: return .logisticsHybridNight
        case .roadNetworkDay: return .roadNetworkDay
        case .roadNetworkNight: return .roadNetworkNight
        @unknown default: return .normalDay
        }
    }
}

/// Callback for scene load completion.
@objc(HereSceneLoadCallback)
public protocol HereSceneLoadCallback: AnyObject {
    func onSceneLoaded(_ scheme: HereMapScheme)
    func onSceneLoadFailed(_ error: String)
}

/// ObjC-visible wrapper for MapScene.
@objc(HereMapScene)
public class HereMapScene: NSObject {
    private let mapScene: MapScene
    private weak var callback: HereSceneLoadCallback?

    /// Non-@objc init — ObjC can't provide a MapScene argument.
    public init(_ mapScene: MapScene) {
        self.mapScene = mapScene
        super.init()
    }

    /// @objc factory — creates from the bridge view which holds the MapView.
    @objc public convenience init(bridgeView: HereMapBridgeView) {
        self.init(bridgeView.swiftMapView!.mapScene)
    }

    @objc public func loadScene(_ scheme: HereMapScheme) {
        guard let swiftScheme = scheme.toSwift() else { return }
        mapScene.loadScene(mapScheme: swiftScheme) { [weak self] loadSceneError in
            if let error = loadSceneError {
                self?.callback?.onSceneLoadFailed(String(describing: error))
            } else {
                self?.callback?.onSceneLoaded(scheme)
            }
        }
    }

    @objc public func setCallback(_ callback: HereSceneLoadCallback) {
        self.callback = callback
    }

    // --- Marker management ---

    @objc public func addMapMarker(_ marker: HereMapMarker) {
        guard let mapImage = marker.createMapImage() else { return }
        let coordinates = GeoCoordinates(latitude: marker.latitude, longitude: marker.longitude)
        let swiftMarker = MapMarker(at: coordinates, image: mapImage)
        marker.setSwiftMarker(swiftMarker)
        mapScene.addMapMarker(swiftMarker)
    }

    @objc public func removeMapMarker(_ marker: HereMapMarker) {
        guard let swiftMarker = marker.swiftMarker else { return }
        mapScene.removeMapMarker(swiftMarker)
    }

    // --- 3D marker management ---

    @objc public func addMapMarker3D(_ marker: HereMapMarker3D) {
        guard let swiftMarker = marker.swiftMarker else { return }
        mapScene.addMapMarker3d(swiftMarker)
    }

    @objc public func removeMapMarker3D(_ marker: HereMapMarker3D) {
        guard let swiftMarker = marker.swiftMarker else { return }
        mapScene.removeMapMarker3d(swiftMarker)
    }

    var swiftMapScene: MapScene {
        return mapScene
    }
}