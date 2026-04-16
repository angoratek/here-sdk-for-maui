using System;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Here.Explore.iOS
{
    // --- Core types ---

    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface HereGeoCoordinates
    {
        [Export("initWithLatitude:longitude:")]
        IntPtr Constructor(double latitude, double longitude);

        [Export("latitude")]
        double Latitude { get; set; }

        [Export("longitude")]
        double Longitude { get; set; }
    }

    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface HereGeoBox
    {
        [Export("initWithSouthWest:northEast:")]
        IntPtr Constructor(HereGeoCoordinates southWest, HereGeoCoordinates northEast);

        [Export("southWest")]
        HereGeoCoordinates SouthWest { get; set; }

        [Export("northEast")]
        HereGeoCoordinates NorthEast { get; set; }
    }

    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface HereGeoPolyline
    {
        [Export("initWithVertices:")]
        IntPtr Constructor(HereGeoCoordinates[] vertices);

        [Export("vertices")]
        HereGeoCoordinates[] Vertices { get; set; }
    }

    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface HereGeoPolygon
    {
        [Export("initWithVertices:")]
        IntPtr Constructor(HereGeoCoordinates[] vertices);

        [Export("vertices")]
        HereGeoCoordinates[] Vertices { get; set; }
    }

    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface HerePoint2D
    {
        [Export("initWithX:y:")]
        IntPtr Constructor(double x, double y);

        [Export("x")]
        double X { get; set; }

        [Export("y")]
        double Y { get; set; }
    }

    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface HereSize2D
    {
        [Export("initWithWidth:height:")]
        IntPtr Constructor(double width, double height);

        [Export("width")]
        double Width { get; set; }

        [Export("height")]
        double Height { get; set; }
    }

    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface HereLocation
    {
        [Export("initWithLatitude:longitude:speedInMetersPerSecond:bearingInDegrees:timestampInMilliseconds:")]
        IntPtr Constructor(double latitude, double longitude, double speedInMetersPerSecond, double bearingInDegrees, long timestampInMilliseconds);

        [Export("latitude")]
        double Latitude { get; set; }

        [Export("longitude")]
        double Longitude { get; set; }

        [Export("speedInMetersPerSecond")]
        double SpeedInMetersPerSecond { get; set; }

        [Export("bearingInDegrees")]
        double BearingInDegrees { get; set; }

        [Export("timestampInMilliseconds")]
        long TimestampInMilliseconds { get; set; }
    }

    // --- SDK initialization ---

    [BaseType(typeof(NSObject))]
    interface HereSdkOptions
    {
        [Export("initWithAccessKeyId:accessKeySecret:cachePath:")]
        IntPtr Constructor(string accessKeyId, string accessKeySecret, string? cachePath);

        [Export("accessKeyId")]
        string AccessKeyId { get; set; }

        [Export("accessKeySecret")]
        string AccessKeySecret { get; set; }

        [Export("cachePath")]
        string? CachePath { get; set; }
    }

    [BaseType(typeof(NSObject))]
    interface HereSdkEngine
    {
        [Static]
        [Export("initialize:")]
        void Initialize(HereSdkOptions options);

        [Static]
        [Export("shutdown")]
        void Shutdown();
    }

    // --- Maps types ---

    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface HereCameraState
    {
        [Export("initWithTargetLatitude:targetLongitude:bearing:tilt:distanceToTargetInMeters:zoomLevel:")]
        IntPtr Constructor(double targetLatitude, double targetLongitude, double bearing, double tilt, double distanceToTargetInMeters, double zoomLevel);

        [Export("targetLatitude")]
        double TargetLatitude { get; set; }

        [Export("targetLongitude")]
        double TargetLongitude { get; set; }

        [Export("bearing")]
        double Bearing { get; set; }

        [Export("tilt")]
        double Tilt { get; set; }

        [Export("distanceToTargetInMeters")]
        double DistanceToTargetInMeters { get; set; }

        [Export("zoomLevel")]
        double ZoomLevel { get; set; }
    }

    [BaseType(typeof(NSObject))]
    [Protocol]
    interface HereMapCameraDelegate
    {
        [Export("onMapCameraUpdated:")]
        void OnMapCameraUpdated(HereCameraState cameraState);
    }

    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface HereMapCamera
    {
        [Export("initWithBridgeView:")]
        IntPtr Constructor(HereMapBridgeView bridgeView);

        [Export("state")]
        HereCameraState State { get; }

        [Export("setTarget:")]
        void SetTarget(HereGeoCoordinates coordinates);

        [Export("setTarget:zoomLevel:")]
        void SetTarget(HereGeoCoordinates coordinates, double zoomLevel);

        [Export("addDelegate:")]
        void AddDelegate(HereMapCameraDelegate delegate_);

        [Export("removeDelegate")]
        void RemoveDelegate();
    }

    [BaseType(typeof(NSObject))]
    interface HereMapBridgeView
    {
        [Export("create")]
        [Static]
        HereMapBridgeView Create();

        [Export("platformView")]
        UIKit.UIView? PlatformView { get; }
    }

    [BaseType(typeof(NSObject))]
    [Protocol]
    interface HereTapDelegate
    {
        [Export("onTapWithOriginX:originY:")]
        void OnTap(double originX, double originY);
    }

    [BaseType(typeof(NSObject))]
    [Protocol]
    interface HereLongPressDelegate
    {
        [Export("onLongPressWithState:originX:originY:")]
        void OnLongPress(nint state, double originX, double originY);
    }

    [BaseType(typeof(NSObject))]
    [Protocol]
    interface HereDoubleTapDelegate
    {
        [Export("onDoubleTapWithOriginX:originY:")]
        void OnDoubleTap(double originX, double originY);
    }

    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface HereGestures
    {
        [Export("initWithBridgeView:")]
        IntPtr Constructor(HereMapBridgeView bridgeView);

        [Export("setTapDelegate:")]
        void SetTapDelegate(HereTapDelegate? delegate_);

        [Export("setLongPressDelegate:")]
        void SetLongPressDelegate(HereLongPressDelegate? delegate_);

        [Export("setDoubleTapDelegate:")]
        void SetDoubleTapDelegate(HereDoubleTapDelegate? delegate_);
    }

    [BaseType(typeof(NSObject))]
    [Protocol]
    interface HereSceneLoadCallback
    {
        [Export("onSceneLoaded:")]
        void OnSceneLoaded(HereMapScheme scheme);

        [Export("onSceneLoadFailed:")]
        void OnSceneLoadFailed(string error);
    }

    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface HereMapScene
    {
        [Export("initWithBridgeView:")]
        IntPtr Constructor(HereMapBridgeView bridgeView);

        [Export("loadScene:")]
        void LoadScene(HereMapScheme scheme);

        [Export("setCallback:")]
        void SetCallback(HereSceneLoadCallback callback);

        [Export("addMapMarker:")]
        void AddMapMarker(HereMapMarker marker);

        [Export("removeMapMarker:")]
        void RemoveMapMarker(HereMapMarker marker);

        [Export("addMapMarker3D:")]
        void AddMapMarker3D(HereMapMarker3D marker);

        [Export("removeMapMarker3D:")]
        void RemoveMapMarker3D(HereMapMarker3D marker);
    }

    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface HereMapMarker
    {
        [Export("initWithLatitude:longitude:")]
        IntPtr Constructor(double latitude, double longitude);

        [Export("initWithLatitude:longitude:imageName:")]
        IntPtr Constructor(double latitude, double longitude, string imageName);

        [Export("latitude")]
        double Latitude { get; set; }

        [Export("longitude")]
        double Longitude { get; set; }

        [Export("imageName")]
        string? ImageName { get; set; }
    }

    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface HereMapMarker3D
    {
        [Export("initWithLatitude:longitude:imageName:width:height:scale:")]
        IntPtr Constructor(double latitude, double longitude, string imageName, int width, int height, double scale);

        [Export("bearing")]
        double Bearing { get; set; }

        [Export("scale")]
        double Scale { get; set; }

        [Export("addToMapView:")]
        void AddToMapView(HereMapBridgeView mapView);

        [Export("removeFromMapView:")]
        void RemoveFromMapView(HereMapBridgeView mapView);
    }

    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface HereMapPolyline
    {
        [Export("drawOrder")]
        int DrawOrder { get; set; }

        [Export("initWithVertices:color:widthInPixels:")]
        IntPtr Constructor(HereGeoCoordinates[] vertices, UIColor color, double widthInPixels);

        [Export("addToMapView:")]
        void AddToMapView(HereMapBridgeView mapView);

        [Export("removeFromMapView:")]
        void RemoveFromMapView(HereMapBridgeView mapView);
    }

    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface HereMapPolygon
    {
        [Export("drawOrder")]
        int DrawOrder { get; set; }

        [Export("initWithVertices:fillColor:")]
        IntPtr Constructor(HereGeoCoordinates[] vertices, UIColor fillColor);

        [Export("initWithVertices:fillColor:outlineColor:outlineWidthInPixels:")]
        IntPtr Constructor(HereGeoCoordinates[] vertices, UIColor fillColor, UIColor outlineColor, double outlineWidthInPixels);

        [Export("addToMapView:")]
        void AddToMapView(HereMapBridgeView mapView);

        [Export("removeFromMapView:")]
        void RemoveFromMapView(HereMapBridgeView mapView);
    }

    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface HereMapArrow
    {
        [Export("initWithVertices:widthInPixels:color:")]
        IntPtr Constructor(HereGeoCoordinates[] vertices, double widthInPixels, UIColor color);

        [Export("addToMapView:")]
        void AddToMapView(HereMapBridgeView mapView);

        [Export("removeFromMapView:")]
        void RemoveFromMapView(HereMapBridgeView mapView);
    }

    // --- Search types ---

    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface HerePlace
    {
        [Export("initWithId:title:latitude:longitude:")]
        IntPtr Constructor(string id, string title, double latitude, double longitude);

        [Export("id")]
        string Id { get; set; }

        [Export("title")]
        string Title { get; set; }

        [Export("latitude")]
        double Latitude { get; set; }

        [Export("longitude")]
        double Longitude { get; set; }
    }

    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface HereSuggestion
    {
        [Export("initWithTitle:id:isPlace:")]
        IntPtr Constructor(string title, string id, bool isPlace);

        [Export("title")]
        string Title { get; set; }

        [Export("id")]
        string Id { get; set; }

        [Export("isPlace")]
        bool IsPlace { get; set; }
    }

    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface HereSearchEngine
    {
        [Export("initWithSdkEnginePointer:")]
        IntPtr Constructor(long sdkEnginePointer);

        [Export("searchByTextWithQuery:latitude:longitude:completion:")]
        void SearchByText(string query, double latitude, double longitude, Action<HerePlace[]?, string?> completion);

        [Export("searchByCategoryWithCategoryId:latitude:longitude:completion:")]
        void SearchByCategory(string categoryId, double latitude, double longitude, Action<HerePlace[]?, string?> completion);

        [Export("suggestWithQuery:latitude:longitude:completion:")]
        void Suggest(string query, double latitude, double longitude, Action<HereSuggestion[]?, string?> completion);

        [Export("searchByPlaceIdWithPlaceId:completion:")]
        void SearchByPlaceId(string placeId, Action<HerePlace?, string?> completion);
    }

    // --- Routing types ---

    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface HereWaypoint
    {
        [Export("initWithLatitude:longitude:type:")]
        IntPtr Constructor(double latitude, double longitude, nint type);

        [Export("latitude")]
        double Latitude { get; set; }

        [Export("longitude")]
        double Longitude { get; set; }

        [Export("type")]
        nint Type { get; set; }
    }

    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface HereRoutingOptions
    {
        [Export("initWithTransportMode:")]
        IntPtr Constructor(nint transportMode);

        [Export("transportMode")]
        nint TransportMode { get; set; }
    }

    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface HereRoute
    {
        [Export("initWithLengthInMeters:durationInSeconds:")]
        IntPtr Constructor(int lengthInMeters, double durationInSeconds);

        [Export("lengthInMeters")]
        int LengthInMeters { get; set; }

        [Export("durationInSeconds")]
        double DurationInSeconds { get; set; }
    }

    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface HereRouteResult
    {
        [Export("initWithError:routes:")]
        IntPtr Constructor(string? error, HereRoute[]? routes);

        [Export("error")]
        string? Error { get; set; }

        [Export("routes")]
        HereRoute[]? Routes { get; set; }
    }

    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface HereRoutingEngine
    {
        [Export("initWithSdkEnginePointer:")]
        IntPtr Constructor(long sdkEnginePointer);

        [Export("calculateRouteWithWaypoints:options:completion:")]
        void CalculateRoute(HereWaypoint[] waypoints, HereRoutingOptions options, Action<HereRouteResult> completion);
    }

    // --- Traffic types ---

    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface HereTrafficEngine
    {
        [Export("initWithSdkEnginePointer:")]
        IntPtr Constructor(long sdkEnginePointer);

        [Export("queryFlowWithLatitude:longitude:radiusInMeters:completion:")]
        void QueryFlow(double latitude, double longitude, double radiusInMeters, Action<HereTrafficFlow[]?, string?> completion);

        [Export("queryIncidentsWithLatitude:longitude:radiusInMeters:completion:")]
        void QueryIncidents(double latitude, double longitude, double radiusInMeters, Action<HereTrafficIncident[]?, string?> completion);
    }

    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface HereTrafficFlow
    {
        [Export("initWithJamFactor:speedInMetersPerSecond:freeFlowSpeedInMetersPerSecond:")]
        IntPtr Constructor(double jamFactor, double speedInMetersPerSecond, double freeFlowSpeedInMetersPerSecond);

        [Export("jamFactor")]
        double JamFactor { get; set; }

        [Export("speedInMetersPerSecond")]
        double SpeedInMetersPerSecond { get; set; }

        [Export("freeFlowSpeedInMetersPerSecond")]
        double FreeFlowSpeedInMetersPerSecond { get; set; }
    }

    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface HereTrafficIncident
    {
        [Export("initWithId:descriptionText:typeRawValue:impactRawValue:isRoadClosed:")]
        IntPtr Constructor(string id, string descriptionText, nint typeRawValue, nint impactRawValue, bool isRoadClosed);

        [Export("id")]
        string Id { get; set; }

        [Export("descriptionText")]
        string DescriptionText { get; set; }

        [Export("typeRawValue")]
        nint TypeRawValue { get; set; }

        [Export("impactRawValue")]
        nint ImpactRawValue { get; set; }

        [Export("isRoadClosed")]
        bool IsRoadClosed { get; set; }
    }
}