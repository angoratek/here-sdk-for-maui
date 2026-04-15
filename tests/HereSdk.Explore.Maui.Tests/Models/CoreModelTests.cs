using Xunit;
using Here.Explore.Maui.Models;

namespace Here.Explore.Maui.Tests.Models;

public class CoreModelTests
{
    [Fact]
    public void Location_CreatedWithDefaults()
    {
        var loc = new Location(new GeoCoordinates(52.5, 13.4));

        Assert.Equal(52.5, loc.Coordinates.Latitude);
        Assert.Equal(13.4, loc.Coordinates.Longitude);
        Assert.Null(loc.Altitude);
        Assert.Null(loc.SpeedInMetersPerSecond);
        Assert.Null(loc.BearingInDegrees);
        Assert.Equal(LocationSource.Unknown, loc.Source);
    }

    [Fact]
    public void Location_CreatedWithAllFields()
    {
        var loc = new Location(
            new GeoCoordinates(52.5, 13.4),
            Altitude: 34.0,
            SpeedInMetersPerSecond: 15.5,
            BearingInDegrees: 180.0,
            Source: LocationSource.Gps
        );

        Assert.Equal(34.0, loc.Altitude);
        Assert.Equal(15.5, loc.SpeedInMetersPerSecond);
        Assert.Equal(180.0, loc.BearingInDegrees);
        Assert.Equal(LocationSource.Gps, loc.Source);
    }

    [Fact]
    public void LocationSource_Values()
    {
        Assert.Equal(0, (int)LocationSource.Unknown);
        Assert.Equal(1, (int)LocationSource.Gps);
        Assert.Equal(2, (int)LocationSource.Network);
        Assert.Equal(3, (int)LocationSource.Passive);
    }

    [Fact]
    public void GeoCoordinatesUpdate_Created()
    {
        var update = new GeoCoordinatesUpdate(52.5, 13.4);
        Assert.Equal(52.5, update.Latitude);
        Assert.Equal(13.4, update.Longitude);
    }

    [Fact]
    public void GeoOrientation_Created()
    {
        var orientation = new GeoOrientation(180.0, 45.0);
        Assert.Equal(180.0, orientation.Bearing);
        Assert.Equal(45.0, orientation.Tilt);
    }

    [Fact]
    public void GeoOrientation_CreatedWithZeroValues()
    {
        var orientation = new GeoOrientation(0.0, 0.0);
        Assert.Equal(0.0, orientation.Bearing);
        Assert.Equal(0.0, orientation.Tilt);
    }

    [Fact]
    public void GeoOrientationUpdate_ApplyTo_OverridesSpecifiedFields()
    {
        var original = new GeoOrientation(90.0, 30.0);
        var update = new GeoOrientationUpdate(Bearing: 180.0);
        var result = update.ApplyTo(original);

        Assert.Equal(180.0, result.Bearing);
        Assert.Equal(30.0, result.Tilt); // Unchanged
    }

    [Fact]
    public void GeoOrientationUpdate_ApplyTo_AllNull_ReturnsOriginal()
    {
        var original = new GeoOrientation(90.0, 30.0);
        var update = new GeoOrientationUpdate();
        var result = update.ApplyTo(original);

        Assert.Equal(90.0, result.Bearing);
        Assert.Equal(30.0, result.Tilt);
    }

    [Fact]
    public void InstantiationErrorCode_Values()
    {
        Assert.Equal(0, (int)InstantiationErrorCode.None);
        Assert.Equal(1, (int)InstantiationErrorCode.NetworkError);
        Assert.Equal(2, (int)InstantiationErrorCode.InvalidCredentials);
        Assert.Equal(3, (int)InstantiationErrorCode.InvalidOptions);
        Assert.Equal(4, (int)InstantiationErrorCode.AlreadyInitialized);
        Assert.Equal(5, (int)InstantiationErrorCode.EngineDisposed);
        Assert.Equal(6, (int)InstantiationErrorCode.InternalError);
    }

    [Fact]
    public void LogLevel_Ordered()
    {
        Assert.True((int)LogLevel.Emergency < (int)LogLevel.Alert);
        Assert.True((int)LogLevel.Alert < (int)LogLevel.Critical);
        Assert.True((int)LogLevel.Critical < (int)LogLevel.Error);
        Assert.True((int)LogLevel.Error < (int)LogLevel.Warning);
        Assert.True((int)LogLevel.Warning < (int)LogLevel.Notice);
        Assert.True((int)LogLevel.Notice < (int)LogLevel.Info);
        Assert.True((int)LogLevel.Info < (int)LogLevel.Debug);
    }

    [Fact]
    public void UnitSystem_Values()
    {
        Assert.Equal(0, (int)UnitSystem.Metric);
        Assert.Equal(1, (int)UnitSystem.ImperialUk);
        Assert.Equal(2, (int)UnitSystem.ImperialUs);
    }

    [Fact]
    public void EngineBaseUrl_Values()
    {
        Assert.Equal(0, (int)EngineBaseUrl.DefaultUrl);
        Assert.Equal(1, (int)EngineBaseUrl.ChinaUrl);
    }
}