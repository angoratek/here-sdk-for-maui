using ObjCRuntime;

namespace Here.Explore.iOS
{
    // MapScheme values — mirrors the HereMapScheme NSInteger enum from NativeBridge
    public enum HereMapScheme : long
    {
        NormalDay = 0,
        NormalNight = 1,
        Satellite = 2,
        HybridDay = 3,
        HybridNight = 4,
        LiteDay = 5,
        LiteNight = 6,
        LiteHybridDay = 7,
        LiteHybridNight = 8,
        LogisticsDay = 9,
        LogisticsNight = 10,
        LogisticsHybridDay = 11,
        LogisticsHybridNight = 12,
        RoadNetworkDay = 13,
        RoadNetworkNight = 14
    }

    // InstantiationErrorCode values — mirrors the HereInstantiationErrorCode NSInteger enum
    public enum HereInstantiationErrorCode : long
    {
        None = 0,
        NetworkError = 1,
        InvalidCredentials = 2,
        InvalidOptions = 3,
        AlreadyInitialized = 4,
        EngineDisposed = 5,
        InternalError = 6
    }
}