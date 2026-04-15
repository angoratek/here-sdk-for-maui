import Foundation
import heresdk

/// ObjC-visible callback for search results.
@objc(HereSearchCallback)
public protocol HereSearchCallback: AnyObject {
    @objc func onSearchCompleted(places: [HerePlace]?, error: String?)
    @objc func onSuggestCompleted(suggestions: [HereSuggestion]?, error: String?)
}

/// ObjC-visible wrapper for SearchEngine.
@objc(HereSearchEngine)
public class HereSearchEngine: NSObject {
    private var engine: SearchEngine?

    @objc public init(sdkEnginePointer: Int64) {
        super.init()
        if let sdkEngine = SDKNativeEngine.sharedInstance {
            do {
                self.engine = try SearchEngine(sdkEngine)
            } catch {
                NSLog("HereSearchEngine: Failed to create SearchEngine: \(error)")
            }
        }
    }

    @objc public func searchByText(
        query: String,
        latitude: Double,
        longitude: Double,
        completion: @escaping ([HerePlace]?, String?) -> Void
    ) {
        guard let engine = engine else {
            completion(nil, "SearchEngine not initialized")
            return
        }

        let geoCoords = GeoCoordinates(latitude: latitude, longitude: longitude)
        let textQuery = TextQuery(query, areaCenter: geoCoords)
        let options = SearchOptions()

        engine.searchByText(textQuery, options: options) { error, places in
            if let error = error {
                completion(nil, error.localizedDescription)
            } else if let places = places {
                let herePlaces = places.map { HerePlace.from($0) }
                completion(herePlaces, nil)
            } else {
                completion(nil, nil)
            }
        }
    }

    var swiftEngine: SearchEngine? {
        return engine
    }
}

/// ObjC-visible wrapper for Place (search result).
@objc(HerePlace)
public class HerePlace: NSObject {
    @objc public var id: String
    @objc public var title: String
    @objc public var latitude: Double
    @objc public var longitude: Double

    @objc public init(id: String, title: String, latitude: Double, longitude: Double) {
        self.id = id
        self.title = title
        self.latitude = latitude
        self.longitude = longitude
        super.init()
    }

    static func from(_ swift: Place) -> HerePlace {
        return HerePlace(
            id: swift.id ?? "",
            title: swift.title ?? "",
            latitude: swift.coordinates.latitude,
            longitude: swift.coordinates.longitude
        )
    }
}

/// ObjC-visible wrapper for Suggestion.
@objc(HereSuggestion)
public class HereSuggestion: NSObject {
    @objc public var title: String
    @objc public var id: String
    @objc public var isPlace: Bool

    @objc public init(title: String, id: String, isPlace: Bool) {
        self.title = title
        self.id = id
        self.isPlace = isPlace
        super.init()
    }
}