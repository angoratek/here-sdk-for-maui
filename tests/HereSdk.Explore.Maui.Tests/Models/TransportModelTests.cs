using Xunit;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Routing;

namespace Here.Explore.Maui.Tests.Models;

public class TransportModelTests
{
    // --- TruckSpecifications ---

    [Fact]
    public void TruckSpecifications_Defaults()
    {
        var truck = new TruckSpecifications();

        Assert.Equal(4000, truck.GrossWeightInKilograms);
        Assert.Equal(2.6, truck.WidthInMeters);
        Assert.Null(truck.HeightInMeters);
        Assert.Null(truck.LengthInMeters);
        Assert.Equal(4, truck.AxleCount);
        Assert.Null(truck.WeightPerAxleInKilograms);
        Assert.Equal(TruckType.Straight, truck.TruckType);
    }

    [Fact]
    public void TruckSpecifications_WithAllFields()
    {
        var truck = new TruckSpecifications(
            GrossWeightInKilograms: 18000,
            HeightInMeters: 4.0,
            WidthInMeters: 3.0,
            LengthInMeters: 16.5,
            AxleCount: 5,
            WeightPerAxleInKilograms: 4000,
            TruckType: TruckType.Tractor
        );

        Assert.Equal(18000, truck.GrossWeightInKilograms);
        Assert.Equal(4.0, truck.HeightInMeters);
        Assert.Equal(3.0, truck.WidthInMeters);
        Assert.Equal(16.5, truck.LengthInMeters);
        Assert.Equal(5, truck.AxleCount);
        Assert.Equal(4000, truck.WeightPerAxleInKilograms);
        Assert.Equal(TruckType.Tractor, truck.TruckType);
    }

    // --- CarSpecifications ---

    [Fact]
    public void CarSpecifications_Defaults()
    {
        var car = new CarSpecifications();

        Assert.Null(car.MaxSpeedInKilometersPerHour);
        Assert.Null(car.WeightInKilograms);
        Assert.Null(car.HeightInMeters);
        Assert.Null(car.LengthInMeters);
    }

    [Fact]
    public void CarSpecifications_WithOverrides()
    {
        var car = new CarSpecifications(
            MaxSpeedInKilometersPerHour: 180,
            WeightInKilograms: 1500,
            HeightInMeters: 1.5,
            LengthInMeters: 4.5
        );

        Assert.Equal(180, car.MaxSpeedInKilometersPerHour);
        Assert.Equal(1500, car.WeightInKilograms);
    }

    // --- TransportSpecification ---

    [Fact]
    public void TransportSpecification_ForCar()
    {
        var spec = TransportSpecification.ForCar();

        Assert.Equal(SectionTransportMode.Car, spec.TransportMode);
        Assert.Null(spec.CarSpecifications);
    }

    [Fact]
    public void TransportSpecification_ForCarWithSpecs()
    {
        var carSpecs = new CarSpecifications(MaxSpeedInKilometersPerHour: 180);
        var spec = TransportSpecification.ForCar(carSpecs);

        Assert.Equal(SectionTransportMode.Car, spec.TransportMode);
        Assert.NotNull(spec.CarSpecifications);
        Assert.Equal(180, spec.CarSpecifications!.MaxSpeedInKilometersPerHour);
    }

    [Fact]
    public void TransportSpecification_ForTruck()
    {
        var truckSpecs = new TruckSpecifications(GrossWeightInKilograms: 18000, HeightInMeters: 4.0);
        var spec = TransportSpecification.ForTruck(truckSpecs);

        Assert.Equal(SectionTransportMode.Truck, spec.TransportMode);
        Assert.NotNull(spec.TruckSpecifications);
        Assert.Equal(18000, spec.TruckSpecifications!.GrossWeightInKilograms);
        Assert.Equal(4.0, truckSpecs.HeightInMeters);
    }

    [Fact]
    public void TransportSpecification_ForPedestrian()
    {
        var spec = TransportSpecification.ForPedestrian();

        Assert.Equal(SectionTransportMode.Pedestrian, spec.TransportMode);
    }

    // --- AvoidanceOptions ---

    [Fact]
    public void AvoidanceOptions_Defaults()
    {
        var opts = new AvoidanceOptions();

        Assert.Empty(opts.AvoidTypes);
        Assert.Empty(opts.AvoidAreas);
    }

    [Fact]
    public void AvoidanceOptions_WithAvoidTypes()
    {
        var opts = new AvoidanceOptions(
            AvoidTypes: new List<AvoidType> { AvoidType.TollRoads, AvoidType.Ferries },
            AvoidAreas: new List<GeoBox> { new(new GeoCoordinates(52, 13), new GeoCoordinates(53, 14)) }
        );

        Assert.Equal(2, opts.AvoidTypes.Count);
        Assert.Single(opts.AvoidAreas);
    }

    // --- Enums ---

    [Fact]
    public void TruckType_Values()
    {
        Assert.Equal(0, (int)TruckType.Straight);
        Assert.Equal(1, (int)TruckType.Tractor);
    }

    [Fact]
    public void AvoidType_Values()
    {
        Assert.Equal(0, (int)AvoidType.TollRoads);
        Assert.Equal(1, (int)AvoidType.Ferries);
        Assert.Equal(2, (int)AvoidType.Tunnels);
        Assert.Equal(3, (int)AvoidType.DirtRoads);
        Assert.Equal(4, (int)AvoidType.CarShuttleTrains);
    }
}