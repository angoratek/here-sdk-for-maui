import Foundation
import heresdk

/// ObjC-visible wrapper for RoutingEngine.
@objc(HereRoutingEngine)
public class HereRoutingEngine: NSObject {
    private var engine: RoutingEngine?

    @objc public init(sdkEnginePointer: Int64) {
        super.init()
        // Initialize with shared SDKNativeEngine
        if let sdkEngine = SDKNativeEngine.sharedInstance {
            do {
                self.engine = try RoutingEngine(sdkEngine)
            } catch {
                NSLog("HereRoutingEngine: Failed to create RoutingEngine: \(error)")
            }
        }
    }

    @objc public func calculateRoute(
        waypoints: [HereWaypoint],
        options: HereRoutingOptions,
        completion: @escaping (HereRouteResult) -> Void
    ) {
        guard let engine = engine else {
            completion(HereRouteResult(error: "RoutingEngine not initialized", routes: nil))
            return
        }

        let swiftWaypoints = waypoints.map { $0.toSwift() }
        let swiftOptions = options.toSwift()

        engine.calculateRoute(with: swiftWaypoints, options: swiftOptions) { error, routes in
            if let error = error {
                completion(HereRouteResult(error: String(describing: error), routes: nil))
            } else if let routes = routes {
                let routeResults = routes.map { HereRoute.from($0) }
                completion(HereRouteResult(error: nil, routes: routeResults))
            } else {
                completion(HereRouteResult(error: nil, routes: nil))
            }
        }
    }

    var swiftEngine: RoutingEngine? {
        return engine
    }
}

/// ObjC-visible wrapper for Waypoint (Swift struct).
@objc(HereWaypoint)
public class HereWaypoint: NSObject {
    @objc public var latitude: Double
    @objc public var longitude: Double
    @objc public var type: Int // 0=Stopover, 1=PassThrough

    @objc public init(latitude: Double, longitude: Double, type: Int = 0) {
        self.latitude = latitude
        self.longitude = longitude
        self.type = type
        super.init()
    }

    func toSwift() -> Waypoint {
        let coordinates = GeoCoordinates(latitude: latitude, longitude: longitude)
        let waypointType: WaypointType
        switch type {
        case 1: waypointType = .passThrough
        default: waypointType = .stopover
        }
        return Waypoint(coordinates: coordinates, type: waypointType)
    }
}

/// ObjC-visible wrapper for RoutingOptions.
/// Uses TransportSpecification (the new v4.28+ pattern).
@objc(HereRoutingOptions)
public class HereRoutingOptions: NSObject {
    @objc public var transportMode: Int // 0=Car, 1=Truck, 2=Pedestrian, 3=Bicycle, 4=Scooter

    @objc public init(transportMode: Int = 0) {
        self.transportMode = transportMode
        super.init()
    }

    func toSwift() -> RoutingOptions {
        let transportSpec: TransportSpecification
        switch transportMode {
        case 1: transportSpec = TransportSpecification.TruckBuilder().build()
        case 2: transportSpec = TransportSpecification.PedestrianBuilder().build()
        case 3: transportSpec = TransportSpecification.BicycleBuilder().build()
        case 4: transportSpec = TransportSpecification.ScooterBuilder().build()
        default: transportSpec = TransportSpecification.CarBuilder().build()
        }
        return RoutingOptions(transportSpecification: transportSpec)
    }
}

/// ObjC-visible route result.
@objc(HereRouteResult)
public class HereRouteResult: NSObject {
    @objc public var error: String?
    @objc public var routes: [HereRoute]?

    @objc public init(error: String?, routes: [HereRoute]?) {
        self.error = error
        self.routes = routes
        super.init()
    }
}

/// ObjC-visible wrapper for Route.
@objc(HereRoute)
public class HereRoute: NSObject {
    @objc public var lengthInMeters: Int32
    @objc public var durationInSeconds: Double
    @objc public var routeHandle: String?
    @objc public var sections: [HereSection]?
    @objc public var geometry: HereGeoPolyline?

    @objc public init(
        lengthInMeters: Int32,
        durationInSeconds: Double,
        routeHandle: String? = nil,
        sections: [HereSection]? = nil,
        geometry: HereGeoPolyline? = nil
    ) {
        self.lengthInMeters = lengthInMeters
        self.durationInSeconds = durationInSeconds
        self.routeHandle = routeHandle
        self.sections = sections
        self.geometry = geometry
        super.init()
    }

    static func from(_ swift: Route) -> HereRoute {
        let sections = swift.sections.map { HereSection.from($0) }
        let geometry = HereGeoPolyline.from(swift.geometry)
        return HereRoute(
            lengthInMeters: swift.lengthInMeters,
            durationInSeconds: swift.duration,
            routeHandle: swift.routeHandle?.handle,
            sections: sections,
            geometry: geometry
        )
    }
}

/// ObjC-visible wrapper for Section.
@objc(HereSection)
public class HereSection: NSObject {
    @objc public var departure: HereGeoCoordinates
    @objc public var arrival: HereGeoCoordinates
    @objc public var maneuvers: [HereManeuver]?
    @objc public var transportMode: Int // maps to SectionTransportMode raw value
    @objc public var lengthInMeters: Int32
    @objc public var durationInSeconds: Double
    @objc public var geometry: HereGeoPolyline?

    @objc public init(
        departure: HereGeoCoordinates,
        arrival: HereGeoCoordinates,
        maneuvers: [HereManeuver]? = nil,
        transportMode: Int = 0,
        lengthInMeters: Int32 = 0,
        durationInSeconds: Double = 0,
        geometry: HereGeoPolyline? = nil
    ) {
        self.departure = departure
        self.arrival = arrival
        self.maneuvers = maneuvers
        self.transportMode = transportMode
        self.lengthInMeters = lengthInMeters
        self.durationInSeconds = durationInSeconds
        self.geometry = geometry
        super.init()
    }

    static func from(_ swift: Section) -> HereSection {
        let maneuvers = swift.maneuvers.map { HereManeuver.from($0) }
        let departure = HereGeoCoordinates.from(swift.departurePlace.mapMatchedCoordinates)
        let arrival = HereGeoCoordinates.from(swift.arrivalPlace.mapMatchedCoordinates)
        let geometry = HereGeoPolyline.from(swift.geometry)
        return HereSection(
            departure: departure,
            arrival: arrival,
            maneuvers: maneuvers,
            transportMode: Int(swift.sectionTransportMode.rawValue),
            lengthInMeters: swift.lengthInMeters,
            durationInSeconds: swift.duration,
            geometry: geometry
        )
    }
}

/// ObjC-visible wrapper for Maneuver.
@objc(HereManeuver)
public class HereManeuver: NSObject {
    @objc public var coordinates: HereGeoCoordinates
    @objc public var action: Int // maps to ManeuverAction raw value
    @objc public var text: String?
    @objc public var lengthInMeters: Int32
    @objc public var durationInSeconds: Double
    @objc public var turnAngleInDegrees: Double

    @objc public init(
        coordinates: HereGeoCoordinates,
        action: Int = 0,
        text: String? = nil,
        lengthInMeters: Int32 = 0,
        durationInSeconds: Double = 0,
        turnAngleInDegrees: Double = 0
    ) {
        self.coordinates = coordinates
        self.action = action
        self.text = text
        self.lengthInMeters = lengthInMeters
        self.durationInSeconds = durationInSeconds
        self.turnAngleInDegrees = turnAngleInDegrees
        super.init()
    }

    static func from(_ swift: Maneuver) -> HereManeuver {
        let turnAngle: Double
        if let angle = swift.turnAngleInDegrees {
            turnAngle = angle
        } else {
            turnAngle = 0
        }
        return HereManeuver(
            coordinates: HereGeoCoordinates.from(swift.coordinates),
            action: Int(swift.action.rawValue),
            text: swift.text,
            lengthInMeters: swift.lengthInMeters,
            durationInSeconds: swift.duration,
            turnAngleInDegrees: turnAngle
        )
    }
}
