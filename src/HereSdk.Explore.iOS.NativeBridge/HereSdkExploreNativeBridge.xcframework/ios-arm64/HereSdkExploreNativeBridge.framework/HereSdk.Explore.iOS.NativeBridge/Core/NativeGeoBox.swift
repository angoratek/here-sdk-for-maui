import Foundation
import heresdk

/// ObjC-visible wrapper for GeoBox.
@objc(HereGeoBox)
public class HereGeoBox: NSObject {
    @objc public var southWest: HereGeoCoordinates
    @objc public var northEast: HereGeoCoordinates

    @objc public init(southWest: HereGeoCoordinates, northEast: HereGeoCoordinates) {
        self.southWest = southWest
        self.northEast = northEast
        super.init()
    }

    func toSwift() -> GeoBox {
        GeoBox(southWestCorner: southWest.toSwift(), northEastCorner: northEast.toSwift())
    }

    static func from(_ swift: GeoBox) -> HereGeoBox {
        HereGeoBox(
            southWest: HereGeoCoordinates.from(swift.southWestCorner),
            northEast: HereGeoCoordinates.from(swift.northEastCorner)
        )
    }
}