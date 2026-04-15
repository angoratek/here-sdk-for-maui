import Foundation
import heresdk

/// ObjC-visible wrapper for GeoPolyline.
@objc(HereGeoPolyline)
public class HereGeoPolyline: NSObject {
    @objc public var vertices: [HereGeoCoordinates]

    @objc public init(vertices: [HereGeoCoordinates]) {
        self.vertices = vertices
        super.init()
    }

    func toSwift() -> GeoPolyline {
        let coords = vertices.map { $0.toSwift() }
        return (try? GeoPolyline(vertices: coords)) ?? GeoPolyline(vertices: [])
    }

    static func from(_ swift: GeoPolyline) -> HereGeoPolyline {
        HereGeoPolyline(vertices: swift.vertices.map { HereGeoCoordinates.from($0) })
    }
}

/// ObjC-visible wrapper for GeoPolygon.
@objc(HereGeoPolygon)
public class HereGeoPolygon: NSObject {
    @objc public var vertices: [HereGeoCoordinates]

    @objc public init(vertices: [HereGeoCoordinates]) {
        self.vertices = vertices
        super.init()
    }

    func toSwift() -> GeoPolygon {
        let coords = vertices.map { $0.toSwift() }
        return (try? GeoPolygon(vertices: coords)) ?? GeoPolygon(vertices: [])
    }

    static func from(_ swift: GeoPolygon) -> HereGeoPolygon {
        HereGeoPolygon(vertices: swift.vertices.map { HereGeoCoordinates.from($0) })
    }
}