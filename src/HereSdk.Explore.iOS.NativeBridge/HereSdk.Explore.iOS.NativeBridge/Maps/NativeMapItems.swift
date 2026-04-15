import Foundation
import UIKit
import heresdk

/// ObjC-visible wrapper for MapPolyline.
/// MapPolyline uses the Representation pattern for styling.
@objc(HereMapPolyline)
public class HereMapPolyline: NSObject {
    private var polyline: MapPolyline?

    @objc public var drawOrder: Int32 {
        get { polyline?.drawOrder ?? 0 }
        set { polyline?.drawOrder = newValue }
    }

    /// Creates a solid-color polyline with the given vertices, color, and width.
    @objc public init(vertices: [HereGeoCoordinates], color: UIColor, widthInPixels: Double) {
        super.init()
        do {
            let coords = vertices.map { $0.toSwift() }
            let geoPolyline = try GeoPolyline(vertices: coords)
            let lineWidth = try MapMeasureDependentRenderSize(sizeUnit: .pixels, size: widthInPixels)
            let representation = try MapPolyline.SolidRepresentation(
                lineWidth: lineWidth,
                color: color,
                capShape: .round
            )
            self.polyline = MapPolyline(geometry: geoPolyline, representation: representation)
        } catch {
            NSLog("HereMapPolyline: Failed to create: \(error)")
        }
    }

    @objc public func addToMapView(_ mapView: HereMapBridgeView) {
        guard let polyline = polyline else { return }
        mapView.swiftMapView?.mapScene.addMapPolyline(polyline)
    }

    @objc public func removeFromMapView(_ mapView: HereMapBridgeView) {
        guard let polyline = polyline else { return }
        mapView.swiftMapView?.mapScene.removeMapPolyline(polyline)
    }

    var swiftPolyline: MapPolyline? { polyline }
}

/// ObjC-visible wrapper for MapPolygon.
@objc(HereMapPolygon)
public class HereMapPolygon: NSObject {
    private var polygon: MapPolygon?

    @objc public var drawOrder: Int32 {
        get { polygon?.drawOrder ?? 0 }
        set { polygon?.drawOrder = newValue }
    }

    @objc public init(vertices: [HereGeoCoordinates], fillColor: UIColor) {
        super.init()
        do {
            let coords = vertices.map { $0.toSwift() }
            let geoPolygon = try GeoPolygon(vertices: coords)
            self.polygon = MapPolygon(geometry: geoPolygon, color: fillColor)
        } catch {
            NSLog("HereMapPolygon: Failed to create: \(error)")
        }
    }

    @objc public init(vertices: [HereGeoCoordinates], fillColor: UIColor, outlineColor: UIColor, outlineWidthInPixels: Double) {
        super.init()
        do {
            let coords = vertices.map { $0.toSwift() }
            let geoPolygon = try GeoPolygon(vertices: coords)
            self.polygon = MapPolygon(geometry: geoPolygon, color: fillColor, outlineColor: outlineColor, outlineWidthInPixels: outlineWidthInPixels)
        } catch {
            NSLog("HereMapPolygon: Failed to create: \(error)")
        }
    }

    @objc public func addToMapView(_ mapView: HereMapBridgeView) {
        guard let polygon = polygon else { return }
        mapView.swiftMapView?.mapScene.addMapPolygon(polygon)
    }

    @objc public func removeFromMapView(_ mapView: HereMapBridgeView) {
        guard let polygon = polygon else { return }
        mapView.swiftMapView?.mapScene.removeMapPolygon(polygon)
    }

    var swiftPolygon: MapPolygon? { polygon }
}

/// ObjC-visible wrapper for MapArrow.
@objc(HereMapArrow)
public class HereMapArrow: NSObject {
    private var arrow: MapArrow?

    @objc public init(vertices: [HereGeoCoordinates], widthInPixels: Double, color: UIColor) {
        super.init()
        do {
            let coords = vertices.map { $0.toSwift() }
            let geoPolyline = try GeoPolyline(vertices: coords)
            self.arrow = MapArrow(geometry: geoPolyline, widthInPixels: widthInPixels, color: color)
        } catch {
            NSLog("HereMapArrow: Failed to create: \(error)")
        }
    }

    @objc public func addToMapView(_ mapView: HereMapBridgeView) {
        guard let arrow = arrow else { return }
        mapView.swiftMapView?.mapScene.addMapArrow(arrow)
    }

    @objc public func removeFromMapView(_ mapView: HereMapBridgeView) {
        guard let arrow = arrow else { return }
        mapView.swiftMapView?.mapScene.removeMapArrow(arrow)
    }

    var swiftArrow: MapArrow? { arrow }
}