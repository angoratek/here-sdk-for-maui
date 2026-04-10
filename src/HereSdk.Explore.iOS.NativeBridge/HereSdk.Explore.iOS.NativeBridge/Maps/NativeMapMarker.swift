import Foundation
import heresdk

/// ObjC-visible wrapper for MapMarker.
@objc(HereMapMarker)
public class HereMapMarker: NSObject {
    private var marker: MapMarker?

    @objc public var latitude: Double
    @objc public var longitude: Double

    @objc public init(latitude: Double, longitude: Double) {
        self.latitude = latitude
        self.longitude = longitude
        super.init()
    }

    /// Create with a MapImage (requires the actual MapImage from the SDK).
    /// This will be called from C# after getting a MapImage via the factory.
    func createMarker(with mapImage: MapImage) -> MapMarker {
        let coordinates = GeoCoordinates(latitude: latitude, longitude: longitude)
        let marker = MapMarker(at: coordinates, image: mapImage)
        self.marker = marker
        return marker
    }

    var swiftMarker: MapMarker? {
        return marker
    }
}