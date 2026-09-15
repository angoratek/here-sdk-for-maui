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
        maxItems: Int32,
        languageCode: Int, // -1 = none, otherwise SearchLanguage raw value
        completion: @escaping ([HerePlace]?, String?) -> Void
    ) {
        guard let engine = engine else {
            completion(nil, "SearchEngine not initialized")
            return
        }

        let geoCoords = GeoCoordinates(latitude: latitude, longitude: longitude)
        let area = TextQuery.Area(areaCenter: geoCoords)
        let textQuery = TextQuery(query, area: area)
        let options = buildSearchOptions(maxItems: maxItems, languageRawValue: languageCode)

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
        maxItems: Int32,
        languageCode: Int,
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
        let options = buildSearchOptions(maxItems: maxItems, languageRawValue: languageCode)

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
        maxItems: Int32,
        languageCode: Int,
        completion: @escaping ([HereSuggestion]?, String?) -> Void
    ) {
        guard let engine = engine else {
            completion(nil, "SearchEngine not initialized")
            return
        }

        let geoCoords = GeoCoordinates(latitude: latitude, longitude: longitude)
        let area = TextQuery.Area(areaCenter: geoCoords)
        let textQuery = TextQuery(query, area: area)
        let options = buildSearchOptions(maxItems: maxItems, languageRawValue: languageCode)

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

    @objc public func searchByAddress(
        query: String,
        latitude: Double,
        longitude: Double,
        maxItems: Int32,
        languageCode: Int,
        completion: @escaping ([HerePlace]?, String?) -> Void
    ) {
        guard let engine = engine else {
            completion(nil, "SearchEngine not initialized")
            return
        }

        let geoCoords = GeoCoordinates(latitude: latitude, longitude: longitude)
        let addressQuery = AddressQuery(query, near: geoCoords)
        let options = buildSearchOptions(maxItems: maxItems, languageRawValue: languageCode)

        engine.searchByAddress(addressQuery, options: options) { searchError, places in
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

    /// Address search without an area center — no geographic bias.
    @objc public func searchByAddressNoArea(
        query: String,
        maxItems: Int32,
        languageCode: Int,
        completion: @escaping ([HerePlace]?, String?) -> Void
    ) {
        guard let engine = engine else {
            completion(nil, "SearchEngine not initialized")
            return
        }

        let addressQuery = AddressQuery(query)
        let options = buildSearchOptions(maxItems: maxItems, languageRawValue: languageCode)

        engine.searchByAddress(addressQuery, options: options) { searchError, places in
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

    /// Reverse geocoding: look up the place/address at a coordinate.
    @objc public func searchByCoordinates(
        latitude: Double,
        longitude: Double,
        maxItems: Int32,
        languageCode: Int,
        completion: @escaping ([HerePlace]?, String?) -> Void
    ) {
        guard let engine = engine else {
            completion(nil, "SearchEngine not initialized")
            return
        }

        let geoCoords = GeoCoordinates(latitude: latitude, longitude: longitude)
        let options = buildSearchOptions(maxItems: maxItems, languageRawValue: languageCode)

        engine.searchByCoordinates(geoCoords, options: options) { searchError, places in
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

    private func buildSearchOptions(maxItems: Int32, languageRawValue: Int) -> SearchOptions {
        var options = SearchOptions()
        if maxItems > 0 {
            options.maxItems = maxItems
        }
        if languageRawValue >= 0 {
            options.languageCode = toLanguageCode(rawValue: languageRawValue)
        }
        return options
    }

    private func toLanguageCode(rawValue: Int) -> LanguageCode? {
        switch rawValue {
        case 0: return .enUs
        case 1: return .deDe
        case 2: return .frFr
        case 3: return .esEs
        case 4: return .itIt
        case 5: return .ptBr
        case 6: return .nlNl
        case 7: return .plPl
        case 8: return .ruRu
        case 9: return .zhCn
        case 10: return .jaJp
        case 11: return .koKr
        default: return nil
        }
    }
}

/// ObjC-visible wrapper for Address.
@objc(HereAddress)
public class HereAddress: NSObject {
    @objc public var street: String
    @objc public var houseNumber: String
    @objc public var city: String
    @objc public var district: String
    @objc public var state: String
    @objc public var countryCode: String
    @objc public var countryName: String
    @objc public var postalCode: String

    @objc public init(street: String,
                      houseNumber: String,
                      city: String,
                      district: String,
                      state: String,
                      countryCode: String,
                      countryName: String,
                      postalCode: String) {
        self.street = street
        self.houseNumber = houseNumber
        self.city = city
        self.district = district
        self.state = state
        self.countryCode = countryCode
        self.countryName = countryName
        self.postalCode = postalCode
        super.init()
    }

    static func from(_ swift: Address) -> HereAddress {
        return HereAddress(
            street: swift.street,
            houseNumber: swift.houseNumOrName,
            city: swift.city,
            district: swift.district,
            state: swift.state,
            countryCode: swift.countryCode,
            countryName: swift.country,
            postalCode: swift.postalCode
        )
    }
}

/// ObjC-visible wrapper for PlaceCategory (search result category).
@objc(HerePlaceCategory)
public class HerePlaceCategory: NSObject {
    @objc public var id: String
    @objc public var name: String

    @objc public init(id: String, name: String) {
        self.id = id
        self.name = name
        super.init()
    }

    static func from(_ swift: PlaceCategory) -> HerePlaceCategory {
        return HerePlaceCategory(id: swift.id, name: swift.name ?? "")
    }
}

/// ObjC-visible wrapper for Place (search result).
@objc(HerePlace)
public class HerePlace: NSObject {
    @objc public var id: String
    @objc public var title: String
    @objc public var latitude: Double
    @objc public var longitude: Double
    @objc public var address: HereAddress?
    @objc public var primaryCategories: [HerePlaceCategory]

    @objc public init(id: String, title: String, latitude: Double, longitude: Double, address: HereAddress?, primaryCategories: [HerePlaceCategory]) {
        self.id = id
        self.title = title
        self.latitude = latitude
        self.longitude = longitude
        self.address = address
        self.primaryCategories = primaryCategories
        super.init()
    }

    static func from(_ swift: Place) -> HerePlace {
        return HerePlace(
            id: swift.id ?? "",
            title: swift.title ?? "",
            latitude: swift.geoCoordinates?.latitude ?? 0,
            longitude: swift.geoCoordinates?.longitude ?? 0,
            address: HereAddress.from(swift.address),
            primaryCategories: swift.details.getPrimaryCategories().map { HerePlaceCategory.from($0) }
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
