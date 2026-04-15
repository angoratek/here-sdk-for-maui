import Foundation
import UIKit
import heresdk

/// ObjC-visible wrapper for MapMarker.
@objc(HereMapMarker)
public class HereMapMarker: NSObject {
    @objc public var latitude: Double
    @objc public var longitude: Double
    @objc public var imageName: String?
    private var _swiftMarker: MapMarker?

    @objc public init(latitude: Double, longitude: Double) {
        self.latitude = latitude
        self.longitude = longitude
        super.init()
    }

    /// Create with image name (for MapScene-based add/remove).
    @objc public init(latitude: Double, longitude: Double, imageName: String) {
        self.latitude = latitude
        self.longitude = longitude
        self.imageName = imageName
        super.init()
    }

    /// Creates a MapImage from the imageName (if provided), used by MapScene.addMapMarker.
    func createMapImage() -> MapImage? {
        guard let imageName = imageName else {
            // Create a default marker image (1x1 green pixel) when no image is specified
            // This is a placeholder — in production, use a proper marker icon
            return try? MapImage(filePath: "marker", width: 32, height: 32)
        }
        return try? MapImage(named: imageName, width: 32, height: 32)
    }

    func setSwiftMarker(_ marker: MapMarker) {
        self._swiftMarker = marker
    }

    var swiftMarker: MapMarker? {
        return _swiftMarker
    }
}