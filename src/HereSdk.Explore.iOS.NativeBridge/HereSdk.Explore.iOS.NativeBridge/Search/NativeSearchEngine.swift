import Foundation
import heresdk

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
        let area = TextQuery.Area(areaCenter: geoCoords)
        let textQuery = TextQuery(query, area: area)
        let options = SearchOptions()

        engine.searchByText(textQuery, options: options) { searchError, places in
            if let searchError = searchError {
                completion(nil, String(describing: searchError))
            } else if let places = places {
                let herePlaces = places.map { HerePlace.from($0) }
                completion(herePlaces, nil)
            } else {
                completion(nil, nil)
            }
        }
    }

    @objc public func searchByCategory(
        categoryId: String,
        latitude: Double,
        longitude: Double,
        completion: @escaping ([HerePlace]?, String?) -> Void
    ) {
        guard let engine = engine else {
            completion(nil, "SearchEngine not initialized")
            return
        }

        let geoCoords = GeoCoordinates(latitude: latitude, longitude: longitude)
        let category = PlaceCategory(id: categoryId)
        let area = CategoryQuery.Area(areaCenter: geoCoords)
        let categoryQuery = CategoryQuery(category, area: area)
        let options = SearchOptions()

        engine.searchByCategory(categoryQuery, options: options) { searchError, places in
            if let searchError = searchError {
                completion(nil, String(describing: searchError))
            } else if let places = places {
                let herePlaces = places.map { HerePlace.from($0) }
                completion(herePlaces, nil)
            } else {
                completion(nil, nil)
            }
        }
    }

    @objc public func suggest(
        query: String,
        latitude: Double,
        longitude: Double,
        completion: @escaping ([HereSuggestion]?, String?) -> Void
    ) {
        guard let engine = engine else {
            completion(nil, "SearchEngine not initialized")
            return
        }

        let geoCoords = GeoCoordinates(latitude: latitude, longitude: longitude)
        let area = TextQuery.Area(areaCenter: geoCoords)
        let textQuery = TextQuery(query, area: area)
        let options = SearchOptions()

        engine.suggestByText(textQuery, options: options) { searchError, suggestions in
            if let searchError = searchError {
                completion(nil, String(describing: searchError))
            } else if let suggestions = suggestions {
                let hereSuggestions = suggestions.map { HereSuggestion.from($0) }
                completion(hereSuggestions, nil)
            } else {
                completion(nil, nil)
            }
        }
    }

    @objc public func searchByPlaceId(
        placeId: String,
        completion: @escaping (HerePlace?, String?) -> Void
    ) {
        guard let engine = engine else {
            completion(nil, "SearchEngine not initialized")
            return
        }

        let placeIdQuery = PlaceIdQuery(placeId)
        engine.searchByPlaceId(placeIdQuery, languageCode: nil) { searchError, place in
            if let searchError = searchError {
                completion(nil, String(describing: searchError))
            } else if let place = place {
                completion(HerePlace.from(place), nil)
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
            latitude: swift.geoCoordinates?.latitude ?? 0,
            longitude: swift.geoCoordinates?.longitude ?? 0
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

    static func from(_ swift: Suggestion) -> HereSuggestion {
        let isPlace = swift.type == .place
        return HereSuggestion(
            title: swift.title ?? "",
            id: swift.id ?? "",
            isPlace: isPlace
        )
    }
}