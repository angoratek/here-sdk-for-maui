import Foundation
import heresdk

/// ObjC-visible wrapper for TrafficEngine.
@objc(HereTrafficEngine)
public class HereTrafficEngine: NSObject {
    private var engine: TrafficEngine?

    @objc public init(sdkEnginePointer: Int64) {
        super.init()
        if let sdkEngine = SDKNativeEngine.sharedInstance {
            do {
                self.engine = try TrafficEngine(sdkEngine)
            } catch {
                NSLog("HereTrafficEngine: Failed to create TrafficEngine: \(error)")
            }
        }
    }

    @objc public func queryFlow(
        latitude: Double,
        longitude: Double,
        radiusInMeters: Double,
        completion: @escaping ([HereTrafficFlow]?, String?) -> Void
    ) {
        guard let engine = engine else {
            completion(nil, "TrafficEngine not initialized")
            return
        }

        let circle = GeoCircle(center: GeoCoordinates(latitude: latitude, longitude: longitude),
                                radiusInMeters: radiusInMeters)
        let options = TrafficFlowQueryOptions()

        engine.queryForFlow(inside: circle, queryOptions: options) { queryError, flows in
            if let queryError = queryError {
                completion(nil, String(describing: queryError))
            } else if let flows = flows {
                let hereFlows = flows.map { HereTrafficFlow.from($0) }
                completion(hereFlows, nil)
            } else {
                completion(nil, nil)
            }
        }
    }

    @objc public func queryIncidents(
        latitude: Double,
        longitude: Double,
        radiusInMeters: Double,
        completion: @escaping ([HereTrafficIncident]?, String?) -> Void
    ) {
        guard let engine = engine else {
            completion(nil, "TrafficEngine not initialized")
            return
        }

        let circle = GeoCircle(center: GeoCoordinates(latitude: latitude, longitude: longitude),
                                radiusInMeters: radiusInMeters)
        let options = TrafficIncidentsQueryOptions()

        engine.queryForIncidents(inside: circle, queryOptions: options) { queryError, incidents in
            if let queryError = queryError {
                completion(nil, String(describing: queryError))
            } else if let incidents = incidents {
                let hereIncidents = incidents.map { HereTrafficIncident.from($0) }
                completion(hereIncidents, nil)
            } else {
                completion(nil, nil)
            }
        }
    }

    var swiftEngine: TrafficEngine? {
        return engine
    }
}

/// ObjC-visible wrapper for TrafficFlow.
@objc(HereTrafficFlow)
public class HereTrafficFlow: NSObject {
    @objc public var jamFactor: Double
    @objc public var speedInMetersPerSecond: Double
    @objc public var freeFlowSpeedInMetersPerSecond: Double

    @objc public init(jamFactor: Double, speedInMetersPerSecond: Double, freeFlowSpeedInMetersPerSecond: Double) {
        self.jamFactor = jamFactor
        self.speedInMetersPerSecond = speedInMetersPerSecond
        self.freeFlowSpeedInMetersPerSecond = freeFlowSpeedInMetersPerSecond
        super.init()
    }

    static func from(_ swift: TrafficFlow) -> HereTrafficFlow {
        return HereTrafficFlow(
            jamFactor: swift.jamFactor,
            speedInMetersPerSecond: swift.speedInMetersPerSecond ?? 0,
            freeFlowSpeedInMetersPerSecond: swift.freeFlowSpeedInMetersPerSecond
        )
    }
}

/// ObjC-visible wrapper for TrafficIncident.
@objc(HereTrafficIncident)
public class HereTrafficIncident: NSObject {
    @objc public var id: String
    @objc public var descriptionText: String
    @objc public var typeRawValue: Int
    @objc public var impactRawValue: Int
    @objc public var isRoadClosed: Bool

    @objc public init(id: String, descriptionText: String, typeRawValue: Int, impactRawValue: Int, isRoadClosed: Bool) {
        self.id = id
        self.descriptionText = descriptionText
        self.typeRawValue = typeRawValue
        self.impactRawValue = impactRawValue
        self.isRoadClosed = isRoadClosed
        super.init()
    }

    static func from(_ swift: TrafficIncident) -> HereTrafficIncident {
        return HereTrafficIncident(
            id: swift.id,
            descriptionText: swift.description.text,
            typeRawValue: Int(swift.type.rawValue),
            impactRawValue: Int(swift.impact.rawValue),
            isRoadClosed: swift.isRoadClosed
        )
    }
}