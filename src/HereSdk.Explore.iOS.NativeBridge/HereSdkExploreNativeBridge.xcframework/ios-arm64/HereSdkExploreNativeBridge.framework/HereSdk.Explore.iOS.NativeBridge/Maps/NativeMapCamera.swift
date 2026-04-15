import Foundation
import heresdk

/// ObjC-visible wrapper for MapCamera.State (nested struct).
@objc(HereCameraState)
public class HereCameraState: NSObject {
    @objc public var targetLatitude: Double
    @objc public var targetLongitude: Double
    @objc public var bearing: Double
    @objc public var tilt: Double
    @objc public var distanceToTargetInMeters: Double
    @objc public var zoomLevel: Double

    @objc public init(
        targetLatitude: Double,
        targetLongitude: Double,
        bearing: Double,
        tilt: Double,
        distanceToTargetInMeters: Double,
        zoomLevel: Double
    ) {
        self.targetLatitude = targetLatitude
        self.targetLongitude = targetLongitude
        self.bearing = bearing
        self.tilt = tilt
        self.distanceToTargetInMeters = distanceToTargetInMeters
        self.zoomLevel = zoomLevel
        super.init()
    }

    static func from(_ swift: MapCamera.State) -> HereCameraState {
        HereCameraState(
            targetLatitude: swift.targetCoordinates.latitude,
            targetLongitude: swift.targetCoordinates.longitude,
            bearing: swift.orientationAtTarget.bearing,
            tilt: swift.orientationAtTarget.tilt,
            distanceToTargetInMeters: swift.distanceToTargetInMeters,
            zoomLevel: swift.zoomLevel
        )
    }
}

/// ObjC-visible delegate for MapCamera updates.
@objc(HereMapCameraDelegate)
public protocol HereMapCameraDelegate: AnyObject {
    @objc optional func onMapCameraUpdated(_ cameraState: HereCameraState)
}

/// ObjC-visible wrapper for MapCamera.
@objc(HereMapCamera)
public class HereMapCamera: NSObject {
    private let camera: MapCamera
    private weak var delegate: HereMapCameraDelegate?

    @objc public init(_ camera: MapCamera) {
        self.camera = camera
        super.init()
    }

    @objc public var state: HereCameraState {
        return HereCameraState.from(camera.state)
    }

    @objc public func setTarget(_ coordinates: HereGeoCoordinates) {
        let update = MapCameraUpdateFactory.lookAt(point: GeoCoordinatesUpdate(latitude: coordinates.latitude, longitude: coordinates.longitude))
        camera.update(update)
    }

    @objc public func setTarget(_ coordinates: HereGeoCoordinates, zoomLevel: Double) {
        let geoUpdate = GeoCoordinatesUpdate(latitude: coordinates.latitude, longitude: coordinates.longitude)
        let measure = MapMeasure(kind: .zoomLevel, value: zoomLevel)
        let update = MapCameraUpdateFactory.lookAt(point: geoUpdate, measure: measure)
        camera.update(update)
    }

    @objc public func addDelegate(_ delegate: HereMapCameraDelegate) {
        self.delegate = delegate
        camera.addDelegate(self)
    }

    @objc public func removeDelegate() {
        camera.removeDelegate(self)
        self.delegate = nil
    }

    var swiftCamera: MapCamera {
        return camera
    }
}

// Conform to MapCameraDelegate to bridge callbacks
extension HereMapCamera: MapCameraDelegate {
    public func onMapCameraUpdated(_ cameraState: MapCamera.State) {
        delegate?.onMapCameraUpdated?(HereCameraState.from(cameraState))
    }
}