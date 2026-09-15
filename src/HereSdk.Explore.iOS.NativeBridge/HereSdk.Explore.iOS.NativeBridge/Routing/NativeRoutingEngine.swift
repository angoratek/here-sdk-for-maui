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

    /// Calculates traffic along a previously calculated route. Requires the
    /// HereRoute to still hold its underlying Swift Route (routeHandle alone
    /// is not enough — the SDK reuses the original calculation options).
    @objc(calculateTrafficOnRouteRoute:lastTraveledSectionIndex:traveledDistanceOnLastSectionInMeters:completion:)
    public func calculateTrafficOnRoute(
        route: HereRoute,
        lastTraveledSectionIndex: Int32,
        traveledDistanceOnLastSectionInMeters: Int32,
        completion: @escaping (HereTrafficOnRouteResult) -> Void
    ) {
        guard let engine = engine, let swiftRoute = route.swiftRoute else {
            completion(HereTrafficOnRouteResult(
                error: "RoutingEngine not initialized or route not retained",
                trafficOnRoute: nil))
            return
        }

        engine.calculateTrafficOnRoute(
            route: swiftRoute,
            lastTraveledSectionIndex: lastTraveledSectionIndex,
            traveledDistanceOnLastSectionInMeters: traveledDistanceOnLastSectionInMeters) { error, trafficOnRoute in
            if let error = error {
                completion(HereTrafficOnRouteResult(
                    error: String(describing: error),
                    trafficOnRoute: nil))
            } else if let trafficOnRoute = trafficOnRoute {
                completion(HereTrafficOnRouteResult(
                    error: nil,
                    trafficOnRoute: HereTrafficOnRoute.from(trafficOnRoute)))
            } else {
                completion(HereTrafficOnRouteResult(error: nil, trafficOnRoute: nil))
            }
        }
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

/// ObjC-visible wrapper for truck vehicle specifications.
/// Uses the 0/absent convention for optional numerics (a 0 value means
/// "not set" and is left at the SDK default).
@objc(HereTruckSpecifications)
public class HereTruckSpecifications: NSObject {
    @objc public var grossWeightInKilograms: Int
    @objc public var heightInCentimeters: Int
    @objc public var widthInCentimeters: Int
    @objc public var lengthInCentimeters: Int
    @objc public var axleCount: Int
    @objc public var trailerCount: Int

    @objc public init(
        grossWeightInKilograms: Int = 0,
        heightInCentimeters: Int = 0,
        widthInCentimeters: Int = 0,
        lengthInCentimeters: Int = 0,
        axleCount: Int = 0,
        trailerCount: Int = 0
    ) {
        self.grossWeightInKilograms = grossWeightInKilograms
        self.heightInCentimeters = heightInCentimeters
        self.widthInCentimeters = widthInCentimeters
        self.lengthInCentimeters = lengthInCentimeters
        self.axleCount = axleCount
        self.trailerCount = trailerCount
        super.init()
    }

    func toSwift() -> VehicleSpecification {
        var builder = VehicleSpecification.TruckBuilder()
        if grossWeightInKilograms > 0 {
            builder = builder.withGrossWeightInKilograms(Int32(grossWeightInKilograms))
        }
        if heightInCentimeters > 0 {
            builder = builder.withHeightInCentimeters(Int32(heightInCentimeters))
        }
        if widthInCentimeters > 0 {
            builder = builder.withWidthInCentimeters(Int32(widthInCentimeters))
        }
        if lengthInCentimeters > 0 {
            builder = builder.withLengthInCentimeters(Int32(lengthInCentimeters))
        }
        if axleCount > 0 {
            builder = builder.withAxleCount(Int32(axleCount))
        }
        if trailerCount > 0 {
            builder = builder.withTrailerCount(Int32(trailerCount))
        }
        return builder.build()
    }
}

/// ObjC-visible wrapper for RoutingOptions.
/// Uses TransportSpecification (the new v4.28+ pattern).
@objc(HereRoutingOptions)
public class HereRoutingOptions: NSObject {
    @objc public var transportMode: Int // 0=Car, 1=Truck, 2=Pedestrian, 3=Bicycle, 4=Scooter
    /// Maximum number of alternative routes in addition to the best one.
    /// 0 leaves the SDK default untouched.
    @objc public var maxAlternatives: Int32
    @objc public var truckSpecifications: HereTruckSpecifications?

    @objc public init(transportMode: Int = 0, maxAlternatives: Int32 = 0) {
        self.transportMode = transportMode
        self.maxAlternatives = maxAlternatives
        super.init()
    }

    func toSwift() -> RoutingOptions {
        let transportSpec: TransportSpecification
        switch transportMode {
        case 1:
            if let truckSpecifications = truckSpecifications {
                transportSpec = TransportSpecification.TruckBuilder()
                    .withVehicleSpecification(truckSpecifications.toSwift())
                    .build()
            } else {
                transportSpec = TransportSpecification.TruckBuilder().build()
            }
        case 2: transportSpec = TransportSpecification.PedestrianBuilder().build()
        case 3: transportSpec = TransportSpecification.BicycleBuilder().build()
        case 4: transportSpec = TransportSpecification.ScooterBuilder().build()
        default: transportSpec = TransportSpecification.CarBuilder().build()
        }

        var routeOptions = RouteOptions()
        if maxAlternatives > 0 {
            routeOptions.alternatives = maxAlternatives
        }
        return RoutingOptions(
            transportSpecification: transportSpec,
            routeOptions: routeOptions)
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
    /// The underlying Swift Route, retained so traffic-on-route can be
    /// calculated later (the SDK needs the original calculation options).
    /// Internal so HereRoutingEngine can access it within the module.
    var swiftRoute: Route?

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
        let route = HereRoute(
            lengthInMeters: swift.lengthInMeters,
            durationInSeconds: swift.duration,
            routeHandle: swift.routeHandle?.handle,
            sections: sections,
            geometry: geometry
        )
        route.swiftRoute = swift
        return route
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

/// ObjC-visible result of a traffic-on-route calculation.
@objc(HereTrafficOnRouteResult)
public class HereTrafficOnRouteResult: NSObject {
    @objc public var error: String?
    @objc public var trafficOnRoute: HereTrafficOnRoute?

    @objc public init(error: String?, trafficOnRoute: HereTrafficOnRoute?) {
        self.error = error
        self.trafficOnRoute = trafficOnRoute
        super.init()
    }
}

/// ObjC-visible wrapper for TrafficOnRoute.
@objc(HereTrafficOnRoute)
public class HereTrafficOnRoute: NSObject {
    @objc public var lastTraveledSectionIndex: Int32
    @objc public var traveledDistanceOnLastSectionInMeters: Int32
    @objc public var trafficSections: [HereTrafficOnSection]?

    @objc public init(
        lastTraveledSectionIndex: Int32,
        traveledDistanceOnLastSectionInMeters: Int32,
        trafficSections: [HereTrafficOnSection]?
    ) {
        self.lastTraveledSectionIndex = lastTraveledSectionIndex
        self.traveledDistanceOnLastSectionInMeters = traveledDistanceOnLastSectionInMeters
        self.trafficSections = trafficSections
        super.init()
    }

    static func from(_ swift: TrafficOnRoute) -> HereTrafficOnRoute {
        return HereTrafficOnRoute(
            lastTraveledSectionIndex: swift.lastTraveledSectionIndex,
            traveledDistanceOnLastSectionInMeters: swift.traveledDistanceOnLastSectionInMeters,
            trafficSections: swift.trafficSections.map { HereTrafficOnSection.from($0) }
        )
    }
}

/// ObjC-visible wrapper for TrafficOnSection.
@objc(HereTrafficOnSection)
public class HereTrafficOnSection: NSObject {
    @objc public var geometry: [HereGeoCoordinates]?
    @objc public var trafficSpans: [HereTrafficOnSpan]?
    @objc public var trafficIncidents: [HereTrafficIncidentOnRoute]?

    @objc public init(
        geometry: [HereGeoCoordinates]?,
        trafficSpans: [HereTrafficOnSpan]?,
        trafficIncidents: [HereTrafficIncidentOnRoute]?
    ) {
        self.geometry = geometry
        self.trafficSpans = trafficSpans
        self.trafficIncidents = trafficIncidents
        super.init()
    }

    static func from(_ swift: TrafficOnSection) -> HereTrafficOnSection {
        return HereTrafficOnSection(
            geometry: swift.geometry.map { HereGeoCoordinates.from($0) },
            trafficSpans: swift.trafficSpans.map { HereTrafficOnSpan.from($0) },
            trafficIncidents: swift.trafficIncidents.map { HereTrafficIncidentOnRoute.from($0) }
        )
    }
}

/// ObjC-visible wrapper for TrafficOnSpan.
@objc(HereTrafficOnSpan)
public class HereTrafficOnSpan: NSObject {
    @objc public var jamFactor: Double
    @objc public var lengthInMeters: Double
    @objc public var baseSpeedInMetersPerSecond: Double
    @objc public var trafficSpeedInMetersPerSecond: Double
    @objc public var trafficDelayInSeconds: Double
    @objc public var durationInSeconds: Double
    /// Index into HereTrafficOnSection.geometry where this span starts.
    @objc public var geometryOffset: Int32
    @objc public var incidentIndices: [Int32]?

    @objc public init(
        jamFactor: Double,
        lengthInMeters: Double,
        baseSpeedInMetersPerSecond: Double,
        trafficSpeedInMetersPerSecond: Double,
        trafficDelayInSeconds: Double,
        durationInSeconds: Double,
        geometryOffset: Int32,
        incidentIndices: [Int32]?
    ) {
        self.jamFactor = jamFactor
        self.lengthInMeters = lengthInMeters
        self.baseSpeedInMetersPerSecond = baseSpeedInMetersPerSecond
        self.trafficSpeedInMetersPerSecond = trafficSpeedInMetersPerSecond
        self.trafficDelayInSeconds = trafficDelayInSeconds
        self.durationInSeconds = durationInSeconds
        self.geometryOffset = geometryOffset
        self.incidentIndices = incidentIndices
        super.init()
    }

    static func from(_ swift: TrafficOnSpan) -> HereTrafficOnSpan {
        return HereTrafficOnSpan(
            jamFactor: swift.jamFactor,
            lengthInMeters: swift.lengthInMeters,
            baseSpeedInMetersPerSecond: swift.baseSpeedInMetersPerSecond,
            trafficSpeedInMetersPerSecond: swift.trafficSpeedInMetersPerSecond,
            trafficDelayInSeconds: swift.trafficDelay,
            durationInSeconds: swift.duration,
            geometryOffset: swift.trafficSectionPolylineOffset,
            incidentIndices: swift.incidentIndices
        )
    }
}

/// ObjC-visible wrapper for the routing TrafficIncidentOnRoute.
@objc(HereTrafficIncidentOnRoute)
public class HereTrafficIncidentOnRoute: NSObject {
    @objc public var id: String?
    @objc public var typeRawValue: Int32
    @objc public var impactRawValue: Int32
    @objc public var descriptionText: String?

    @objc public init(
        id: String?,
        typeRawValue: Int32,
        impactRawValue: Int32,
        descriptionText: String?
    ) {
        self.id = id
        self.typeRawValue = typeRawValue
        self.impactRawValue = impactRawValue
        self.descriptionText = descriptionText
        super.init()
    }

    static func from(_ swift: TrafficIncidentOnRoute) -> HereTrafficIncidentOnRoute {
        return HereTrafficIncidentOnRoute(
            id: swift.id,
            typeRawValue: Int32(swift.type.rawValue),
            impactRawValue: Int32(swift.impact.rawValue),
            descriptionText: swift.description.text
        )
    }
}
