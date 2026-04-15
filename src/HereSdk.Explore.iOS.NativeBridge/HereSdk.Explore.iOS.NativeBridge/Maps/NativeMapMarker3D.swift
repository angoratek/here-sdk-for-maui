import Foundation
import heresdk

/// ObjC-visible wrapper for MapMarker3D.
/// Supports both image-based and model-based 3D markers.
@objc(HereMapMarker3D)
public class HereMapMarker3D: NSObject {
    private var marker: MapMarker3D?

    @objc public var bearing: Double {
        get { marker?.bearing ?? 0 }
        set { marker?.bearing = newValue }
    }

    @objc public var scale: Double {
        get { marker?.scale ?? 1 }
        set { marker?.scale = newValue }
    }

    /// Create a 3D marker with an image (billboard-style).
    @objc public init(latitude: Double, longitude: Double, imageName: String, width: Int32, height: Int32, scale: Double) {
        super.init()
        let coords = GeoCoordinates(latitude: latitude, longitude: longitude)
        if let image = try? MapImage(named: imageName, width: width, height: height) {
            self.marker = MapMarker3D(at: coords, image: image, scale: scale, unit: .pixels)
        }
    }

    @objc public func addToMapView(_ mapView: HereMapBridgeView) {
        guard let marker = marker else { return }
        mapView.swiftMapView?.mapScene.addMapMarker3d(marker)
    }

    @objc public func removeFromMapView(_ mapView: HereMapBridgeView) {
        guard let marker = marker else { return }
        mapView.swiftMapView?.mapScene.removeMapMarker3d(marker)
    }

    var swiftMarker: MapMarker3D? { marker }
}