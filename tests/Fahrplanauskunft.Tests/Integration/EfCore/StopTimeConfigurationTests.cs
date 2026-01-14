using Fahrplanauskunft.Core.Entities;
using Fahrplanauskunft.Core.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fahrplanauskunft.Tests.Integration.EfCore;

/// <summary>
/// Tests for StopTime entity EF Core configuration.
/// Validates composite key, TimeOfDay/StopSequence conversions, and indexes.
/// </summary>
public class StopTimeConfigurationTests : IClassFixture<SqliteDbContextFixture>
{
    private readonly SqliteDbContextFixture _fixture;

    public StopTimeConfigurationTests(SqliteDbContextFixture fixture)
    {
        _fixture = fixture;
    }

    #region Primary Key Configuration Tests

    [Fact]
    public void StopTime_HasCompositeKey_TripIdAndSequence()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(StopTime));

        // Act
        var primaryKey = entityType!.FindPrimaryKey();
        var keyPropertyNames = primaryKey!.Properties.Select(p => p.Name).ToList();

        // Assert
        keyPropertyNames.Should().Contain("TripId");
        keyPropertyNames.Should().Contain("Sequence");
        keyPropertyNames.Should().HaveCount(2);
    }

    #endregion

    #region Value Conversion Tests

    [Fact]
    public async Task StopTime_StopSequence_IsPersisted_AsInt()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var stop = new Stop(StopId.From("stop_time_seq_test"), "Test Stop");
        var trip = new Trip(TripId.From("trip_seq_test"));
        var sequence = StopSequence.From(5);
        var arrival = TimeOfDay.FromTotalMinutes(480); // 8:00
        var departure = TimeOfDay.FromTotalMinutes(482); // 8:02

        context.Stops.Add(stop);
        context.Trips.Add(trip);
        await context.SaveChangesAsync();

        var stopTime = new StopTime(stop, sequence, arrival, departure);

        // Set shadow properties via Entry BEFORE marking as Added
        // This is required because TripId is part of composite primary key
        var entry = context.Entry(stopTime);
        entry.Property("TripId").CurrentValue = trip.Id.Value;
        entry.Property("StopId").CurrentValue = stop.Id.Value;
        entry.State = EntityState.Added;
        await context.SaveChangesAsync();

        // Assert
        using var verifyContext = _fixture.CreateContext();
        var loaded = await verifyContext.StopTimes
            .FirstOrDefaultAsync(st => st.Sequence == sequence);

        loaded.Should().NotBeNull();
        loaded!.Sequence.Value.Should().Be(5);
    }

    [Fact]
    public async Task StopTime_TimeOfDay_IsPersisted_AsTotalMinutes()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var stop = new Stop(StopId.From("stop_time_time_test"), "Test Stop");
        var trip = new Trip(TripId.From("trip_time_test"));
        var sequence = StopSequence.From(1);
        var arrival = TimeOfDay.FromTotalMinutes(600); // 10:00
        var departure = TimeOfDay.FromTotalMinutes(605); // 10:05

        context.Stops.Add(stop);
        context.Trips.Add(trip);
        await context.SaveChangesAsync();

        var stopTime = new StopTime(stop, sequence, arrival, departure);

        // Set shadow properties via Entry BEFORE marking as Added
        var entry = context.Entry(stopTime);
        entry.Property("TripId").CurrentValue = trip.Id.Value;
        entry.Property("StopId").CurrentValue = stop.Id.Value;
        entry.State = EntityState.Added;
        await context.SaveChangesAsync();

        // Assert
        using var verifyContext = _fixture.CreateContext();
        var loaded = await verifyContext.StopTimes
            .FirstOrDefaultAsync(st =>
                EF.Property<string>(st, "TripId") == trip.Id.Value &&
                st.Sequence == sequence);

        loaded.Should().NotBeNull();
        loaded!.Arrival.TotalMinutes.Should().Be(600);
        loaded.Departure.TotalMinutes.Should().Be(605);
    }

    #endregion

    #region Property Persistence Tests

    [Fact]
    public async Task StopTime_AllProperties_ArePersisted_Correctly()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var stop = new Stop(StopId.From("stop_time_all_props"), "Full Properties Stop");
        var trip = new Trip(TripId.From("trip_all_props"));
        var sequence = StopSequence.From(3);
        var arrival = TimeOfDay.FromTotalMinutes(720); // 12:00
        var departure = TimeOfDay.FromTotalMinutes(725); // 12:05

        context.Stops.Add(stop);
        context.Trips.Add(trip);
        await context.SaveChangesAsync();

        var stopTime = new StopTime(stop, sequence, arrival, departure, false, true);

        // Set shadow properties via Entry BEFORE marking as Added
        var entry = context.Entry(stopTime);
        entry.Property("TripId").CurrentValue = trip.Id.Value;
        entry.Property("StopId").CurrentValue = stop.Id.Value;
        entry.State = EntityState.Added;
        await context.SaveChangesAsync();

        // Assert
        using var verifyContext = _fixture.CreateContext();
        var allStopTimes = await verifyContext.StopTimes
            .Where(st => EF.Property<string>(st, "TripId") == trip.Id.Value)
            .ToListAsync();

        // Filter client-side since Sequence.Value can't be translated
        var loaded = allStopTimes.FirstOrDefault(st => st.Sequence.Value == 3);

        loaded.Should().NotBeNull();
        loaded!.PickupAllowed.Should().BeFalse();
        loaded.DropOffAllowed.Should().BeTrue();
    }

    #endregion

    #region Index Configuration Tests

    [Fact]
    public void StopTime_HasIndex_OnStopId()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(StopTime));

        // Act
        var indexes = entityType!.GetIndexes().ToList();
        var stopIdIndex = indexes.FirstOrDefault(i =>
            i.Properties.Any(p => p.Name == "StopId"));

        // Assert
        stopIdIndex.Should().NotBeNull();
    }

    [Fact]
    public void StopTime_HasIndex_OnArrival()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(StopTime));

        // Act
        var indexes = entityType!.GetIndexes().ToList();
        var arrivalIndex = indexes.FirstOrDefault(i =>
            i.Properties.Any(p => p.Name == "Arrival"));

        // Assert
        arrivalIndex.Should().NotBeNull();
    }

    [Fact]
    public void StopTime_HasIndex_OnDeparture()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(StopTime));

        // Act
        var indexes = entityType!.GetIndexes().ToList();
        var departureIndex = indexes.FirstOrDefault(i =>
            i.Properties.Any(p => p.Name == "Departure"));

        // Assert
        departureIndex.Should().NotBeNull();
    }

    [Fact]
    public void StopTime_HasCompositeIndex_OnStopIdAndArrival()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(StopTime));

        // Act
        var indexes = entityType!.GetIndexes().ToList();
        var compositeIndex = indexes.FirstOrDefault(i =>
            i.Properties.Count == 2 &&
            i.Properties.Any(p => p.Name == "StopId") &&
            i.Properties.Any(p => p.Name == "Arrival"));

        // Assert
        compositeIndex.Should().NotBeNull();
    }

    #endregion

    #region Computed Properties Are Ignored Tests

    [Fact]
    public void StopTime_DwellTime_IsIgnored_InModel()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(StopTime));

        // Act
        var property = entityType!.FindProperty("DwellTime");

        // Assert
        property.Should().BeNull();
    }

    #endregion

    #region Shadow Property Tests

    [Fact]
    public void StopTime_HasShadowProperty_TripId()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(StopTime));

        // Act
        var tripIdProperty = entityType!.FindProperty("TripId");

        // Assert
        tripIdProperty.Should().NotBeNull();
        tripIdProperty!.IsShadowProperty().Should().BeTrue();
    }

    [Fact]
    public void StopTime_HasShadowProperty_StopId()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(StopTime));

        // Act
        var stopIdProperty = entityType!.FindProperty("StopId");

        // Assert
        stopIdProperty.Should().NotBeNull();
        stopIdProperty!.IsShadowProperty().Should().BeTrue();
    }

    #endregion
}
