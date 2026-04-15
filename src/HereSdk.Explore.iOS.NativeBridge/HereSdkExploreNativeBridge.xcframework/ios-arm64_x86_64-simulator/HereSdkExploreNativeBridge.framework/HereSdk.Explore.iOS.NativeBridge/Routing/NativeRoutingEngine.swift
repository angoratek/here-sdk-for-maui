import Foundation
import heresdk

/// ObjC-visible callback protocol for route calculation results.
@objc(HereRouteCalculatedCallback)
public protocol HereRouteCalculatedCallback: AnyObject {
    @objc func onRouteCalculated(routes: [[String: Any]]?, error: String?)
}

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
                completion(HereRouteResult(error: error.localizedDescription, routes: nil))
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
    @objc public var type: Int // 0=Stop, 1=Start, 2=Through

    @objc public init(latitude: Double, longitude: Double, type: Int = 0) {
        self.latitude = latitude
        self.longitude = longitude
        self.type = type
        super.init()
    }

    func toSwift() -> Waypoint {
        let coordinates = GeoCoordinates(latitude: latitude, longitude: longitude)
        let waypointType: Waypoint.Type_
        switch type {
        case 1: waypointType = .start
        case 2: waypointType = .through
        default: waypointType = .stop
        }
        return Waypoint(coordinates: coordinates, type: waypointType)
    }
}

/// ObjC-visible wrapper for RoutingOptions (simplified).
@objc(HereRoutingOptions)
public class HereRoutingOptions: NSObject {
    @objc public var transportMode: Int // 0=Car, 1=Truck, 2=Pedestrian, 3=Bicycle, 4=Scooter

    @objc public init(transportMode: Int = 0) {
        self.transportMode = transportMode
        super.init()
    }

    func toSwift() -> RoutingOptions {
        let options = RoutingOptions()
        switch transportMode {
        case 1: return CarOptions() // Will be expanded
        case 2: return TruckOptions()
        case 3: return PedestrianOptions()
        case 4: return BicycleOptions()
        default: return CarOptions()
        }
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
    @objc public var lengthInMeters: Double
    @objc public var durationInSeconds: Double

    @objc public init(lengthInMeters: Double, durationInSeconds: Double) {
        self.lengthInMeters = lengthInMeters
        self.durationInSeconds = durationInSeconds
        super.init()
    }

    static func from(_ swift: Route) -> HereRoute {
        return HereRoute(
            lengthInMeters: swift.lengthInMeters,
            durationInSeconds: swift.durationInSeconds
        )
    }
}