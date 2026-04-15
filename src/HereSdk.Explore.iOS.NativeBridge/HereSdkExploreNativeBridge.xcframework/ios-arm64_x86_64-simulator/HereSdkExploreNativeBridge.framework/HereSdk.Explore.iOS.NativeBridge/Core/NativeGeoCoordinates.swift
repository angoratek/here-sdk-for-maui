import Foundation
import heresdk

/// ObjC-visible wrapper for GeoCoordinates (Swift struct → ObjC class).
@objc(HereGeoCoordinates)
public class HereGeoCoordinates: NSObject {
    @objc public var latitude: Double
    @objc public var longitude: Double

    @objc public init(latitude: Double, longitude: Double) {
        self.latitude = latitude
        self.longitude = longitude
        super.init()
    }

    func toSwift() -> GeoCoordinates {
        GeoCoordinates(latitude: latitude, longitude: longitude)
    }

    static func from(_ swift: GeoCoordinates) -> HereGeoCoordinates {
        HereGeoCoordinates(latitude: swift.latitude, longitude: swift.longitude)
    }
}