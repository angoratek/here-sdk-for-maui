using Here.Explore.Maui.Models;

namespace Here.Explore.Maui.Models.Routing;

/// <summary>
/// Transport mode and vehicle specification for route calculation.
/// Use factory methods (<see cref="ForCar"/>, <see cref="ForTruck"/>, etc.) to construct.
/// </summary>
public record TransportSpecification
{
    /// <summary>The transport mode for route calculation.</summary>
    public SectionTransportMode TransportMode { get; init; }

    /// <summary>Car specifications (when TransportMode is Car).</summary>
    public CarSpecifications? CarSpecifications { get; init; }

    /// <summary>Truck specifications (when TransportMode is Truck).</summary>
    public TruckSpecifications? TruckSpecifications { get; init; }

    /// <summary>Avoidance options for the route.</summary>
    public AvoidanceOptions? AvoidanceOptions { get; init; }

    /// <summary>Creates a transport specification for car routing.</summary>
    public static TransportSpecification ForCar(CarSpecifications? specs = null) =>
        new() { TransportMode = SectionTransportMode.Car, CarSpecifications = specs };

    /// <summary>Creates a transport specification for truck routing.</summary>
    public static TransportSpecification ForTruck(TruckSpecifications specs) =>
        new() { TransportMode = SectionTransportMode.Truck, TruckSpecifications = specs };

    /// <summary>Creates a transport specification for pedestrian routing.</summary>
    public static TransportSpecification ForPedestrian() =>
        new() { TransportMode = SectionTransportMode.Pedestrian };

    /// <summary>Creates a transport specification for bicycle routing.</summary>
    public static TransportSpecification ForBicycle() =>
        new() { TransportMode = SectionTransportMode.Bicycle };

    /// <summary>Creates a transport specification for scooter routing.</summary>
    public static TransportSpecification ForScooter() =>
        new() { TransportMode = SectionTransportMode.Scooter };
}

/// <summary>
/// Car vehicle specifications for route calculation.
/// </summary>
public record CarSpecifications(
    int? MaxSpeedInKilometersPerHour = null,
    double? WeightInKilograms = null,
    double? HeightInMeters = null,
    double? LengthInMeters = null
);

/// <summary>
/// Truck vehicle specifications for route calculation.
/// Defaults follow typical medium truck dimensions.
/// </summary>
public record TruckSpecifications(
    int GrossWeightInKilograms = 4000,
    double WidthInMeters = 2.6,
    double? HeightInMeters = null,
    double? LengthInMeters = null,
    int AxleCount = 4,
    int? WeightPerAxleInKilograms = null,
    TruckType TruckType = TruckType.Straight
);

/// <summary>
/// Route avoidance options (roads, areas, or features to avoid).
/// </summary>
public record AvoidanceOptions(
    IReadOnlyList<AvoidType> AvoidTypes,
    IReadOnlyList<GeoBox> AvoidAreas
)
{
    /// <summary>Creates empty avoidance options (no restrictions).</summary>
    public AvoidanceOptions() : this([], []) { }
}

/// <summary>
/// Type of truck (affects routing restrictions).
/// </summary>
public enum TruckType
{
    /// <summary>Straight truck (rigid body).</summary>
    Straight,
    /// <summary>Tractor-trailer (articulated).</summary>
    Tractor
}

/// <summary>
/// Road or feature types to avoid during route calculation.
/// </summary>
public enum AvoidType
{
    /// <summary>Avoid toll roads.</summary>
    TollRoads,
    /// <summary>Avoid ferries.</summary>
    Ferries,
    /// <summary>Avoid tunnels.</summary>
    Tunnels,
    /// <summary>Avoid dirt/unpaved roads.</summary>
    DirtRoads,
    /// <summary>Avoid car shuttle trains.</summary>
    CarShuttleTrains
}