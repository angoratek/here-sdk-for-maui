import Foundation
import heresdk

/// ObjC-visible wrapper for Point2D (Swift struct → ObjC class).
@objc(HerePoint2D)
public class HerePoint2D: NSObject {
    @objc public var x: Double
    @objc public var y: Double

    @objc public init(x: Double, y: Double) {
        self.x = x
        self.y = y
        super.init()
    }

    func toSwift() -> Point2D {
        Point2D(x: x, y: y)
    }

    static func from(_ swift: Point2D) -> HerePoint2D {
        HerePoint2D(x: swift.x, y: swift.y)
    }
}

/// ObjC-visible wrapper for Size2D (Swift struct → ObjC class).
@objc(HereSize2D)
public class HereSize2D: NSObject {
    @objc public var width: Double
    @objc public var height: Double

    @objc public init(width: Double, height: Double) {
        self.width = width
        self.height = height
        super.init()
    }

    func toSwift() -> Size2D {
        Size2D(width: width, height: height)
    }

    static func from(_ swift: Size2D) -> HereSize2D {
        HereSize2D(width: swift.width, height: swift.height)
    }
}

/// ObjC-visible wrapper for Location (Swift struct → ObjC class).
@objc(HereLocation)
public class HereLocation: NSObject {
    @objc public var latitude: Double
    @objc public var longitude: Double
    @objc public var speedInMetersPerSecond: Double
    @objc public var bearingInDegrees: Double
    @objc public var timestampInMilliseconds: Int64

    @objc public init(
        latitude: Double,
        longitude: Double,
        speedInMetersPerSecond: Double = 0,
        bearingInDegrees: Double = 0,
        timestampInMilliseconds: Int64 = 0
    ) {
        self.latitude = latitude
        self.longitude = longitude
        self.speedInMetersPerSecond = speedInMetersPerSecond
        self.bearingInDegrees = bearingInDegrees
        self.timestampInMilliseconds = timestampInMilliseconds
        super.init()
    }

    func toSwift() -> Location {
        return Location(
            coordinates: GeoCoordinates(latitude: latitude, longitude: longitude),
            bearingInDegrees: bearingInDegrees != 0 ? bearingInDegrees : nil,
            speedInMetersPerSecond: speedInMetersPerSecond != 0 ? speedInMetersPerSecond : nil
        )
    }

    static func from(_ swift: Location) -> HereLocation {
        HereLocation(
            latitude: swift.coordinates.latitude,
            longitude: swift.coordinates.longitude,
            speedInMetersPerSecond: swift.speedInMetersPerSecond ?? 0,
            bearingInDegrees: swift.bearingInDegrees ?? 0
        )
    }
}

/// ObjC-visible wrapper for InstantiationErrorCode (UInt32 → NSInteger).
@objc(HereInstantiationErrorCode)
public enum HereInstantiationErrorCode: NSInteger {
    case none = 0
    case networkError = 1
    case invalidCredentials = 2
    case invalidOptions = 3
    case alreadyInitialized = 4
    case engineDisposed = 5
    case internalError = 6
}