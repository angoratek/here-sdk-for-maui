import Foundation
import heresdk

/// ObjC-visible wrapper for IsolineRoutingEngine.
@objc(HereIsolineRoutingEngine)
public class HereIsolineRoutingEngine: NSObject {
    private var engine: IsolineRoutingEngine?

    @objc public init(sdkEnginePointer: Int64) {
        super.init()
        // Initialize with shared SDKNativeEngine
        if let sdkEngine = SDKNativeEngine.sharedInstance {
            do {
                self.engine = try IsolineRoutingEngine(sdkEngine)
            } catch {
                NSLog("HereIsolineRoutingEngine: Failed to create IsolineRoutingEngine: \(error)")
            }
        }
    }

    /// Calculates an isoline around a center point.
    /// - Parameters:
    ///   - transportMode: 0=Car, 1=Truck, 2=Pedestrian, 3=Bicycle, 4=Scooter (see HereRoutingOptions)
    ///   - rangeInMeters: reachability range in meters
    ///   - maxPoints: maximum polygon points; 0 means SDK default
    @objc public func calculateIsoline(
        centerLatitude: Double,
        centerLongitude: Double,
        transportMode: Int,
        rangeInMeters: Int32,
        maxPoints: Int32,
        completion: @escaping (HereIsolineResult) -> Void
    ) {
        guard let engine = engine else {
            completion(HereIsolineResult(error: "IsolineRoutingEngine not initialized", isolines: nil))
            return
        }

        let rangeType = IsolineRangeType.distanceInMeters
        var calculation: IsolineOptions.Calculation
        if maxPoints > 0 {
            calculation = IsolineOptions.Calculation(
                rangeType: rangeType,
                rangeValues: [rangeInMeters],
                isolineCalculationMode: .balanced,
                maxPoints: maxPoints,
                isolineDirection: .departure)
        } else {
            calculation = IsolineOptions.Calculation(
                rangeType: rangeType,
                rangeValues: [rangeInMeters])
        }

        // Reuse the routing options wrapper for transport mode mapping
        let routingOptions = HereRoutingOptions(transportMode: transportMode).toSwift()
        let isolineOptions = IsolineOptions(calculationOptions: calculation, routingOptions: routingOptions)

        let center = Waypoint(coordinates: GeoCoordinates(latitude: centerLatitude, longitude: centerLongitude))

        engine.calculateIsoline(center: center, isolineOptions: isolineOptions) { error, isolines in
            if let error = error {
                completion(HereIsolineResult(error: String(describing: error), isolines: nil))
            } else if let isolines = isolines {
                completion(HereIsolineResult(error: nil, isolines: isolines.map { HereIsoline.from($0) }))
            } else {
                completion(HereIsolineResult(error: nil, isolines: nil))
            }
        }
    }
}

/// ObjC-visible isoline result.
@objc(HereIsolineResult)
public class HereIsolineResult: NSObject {
    @objc public var error: String?
    @objc public var isolines: [HereIsoline]?

    @objc public init(error: String?, isolines: [HereIsoline]?) {
        self.error = error
        self.isolines = isolines
        super.init()
    }
}

/// ObjC-visible wrapper for Isoline.
@objc(HereIsoline)
public class HereIsoline: NSObject {
    @objc public var rangeValue: Double
    // All polygon vertices flattened (outer boundaries only, matching Android binding behavior)
    @objc public var polygonVertices: [HereGeoCoordinates]

    @objc public init(rangeValue: Double, polygonVertices: [HereGeoCoordinates]) {
        self.rangeValue = rangeValue
        self.polygonVertices = polygonVertices
        super.init()
    }

    static func from(_ swift: Isoline) -> HereIsoline {
        var vertices: [HereGeoCoordinates] = []
        for polygon in swift.polygons {
            vertices.append(contentsOf: polygon.vertices.map { HereGeoCoordinates.from($0) })
        }
        return HereIsoline(rangeValue: swift.rangeValue, polygonVertices: vertices)
    }
}