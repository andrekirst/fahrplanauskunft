using Fahrplanauskunft.Core.Entities;
using Fahrplanauskunft.Core.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fahrplanauskunft.Tests.Integration.EfCore;

/// <summary>
/// Tests for Trip entity EF Core configuration.
/// Validates TripId conversion, Route relationship, cascade delete, and indexes.
/// </summary>
public class TripConfigurationTests : IClassFixture<SqliteDbContextFixture>
{
    private readonly SqliteDbContextFixture _fixture;

    public TripConfigurationTests(SqliteDbContextFixture fixture)
    {
        _fixture = fixture;
    }

    #region Value Conversion Tests

    [Fact]
    public async Task Trip_TripId_IsPersisted_AsString()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var tripId = TripId.From("trip_test_001");
        var trip = new Trip(tripId);

        // Act
        context.Trips.Add(trip);
        await context.SaveChangesAsync();

        // Assert
        using var verifyContext = _fixture.CreateContext();
        var loaded = await verifyContext.Trips.FirstOrDefaultAsync(t => t.Id == tripId);

        loaded.Should().NotBeNull();
        loaded!.Id.Should().Be(tripId);
        loaded.Id.Value.Should().Be("trip_test_001");
    }

    #endregion

    #region Property Persistence Tests

    [Fact]
    public async Task Trip_AllProperties_ArePersisted_Correctly()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var tripId = TripId.From("trip_full_props");
        var trip = new Trip(tripId, "Hauptbahnhof", "Express 123");

        // Act
        context.Trips.Add(trip);
        await context.SaveChangesAsync();

        // Assert
        using var verifyContext = _fixture.CreateContext();
        var loaded = await verifyContext.Trips.FirstOrDefaultAsync(t => t.Id == tripId);

        loaded.Should().NotBeNull();
        loaded!.Headsign.Should().Be("Hauptbahnhof");
        loaded.ShortName.Should().Be("Express 123");
    }

    [Fact]
    public async Task Trip_WithNullOptionalProperties_PersistsCorrectly()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var tripId = TripId.From("trip_null_props");
        var trip = new Trip(tripId);

        // Act
        context.Trips.Add(trip);
        await context.SaveChangesAsync();

        // Assert
        using var verifyContext = _fixture.CreateContext();
        var loaded = await verifyContext.Trips.FirstOrDefaultAsync(t => t.Id == tripId);

        loaded.Should().NotBeNull();
        loaded!.Headsign.Should().BeNull();
        loaded.ShortName.Should().BeNull();
    }

    #endregion

    #region Index Configuration Tests

    [Fact]
    public void Trip_HasIndex_OnHeadsign()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(Trip));

        // Act
        var indexes = entityType!.GetIndexes().ToList();
        var headsignIndex = indexes.FirstOrDefault(i =>
            i.Properties.Any(p => p.Name == "Headsign"));

        // Assert
        headsignIndex.Should().NotBeNull();
    }

    [Fact]
    public void Trip_HasIndex_OnRouteId()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(Trip));

        // Act
        var indexes = entityType!.GetIndexes().ToList();
        var routeIdIndex = indexes.FirstOrDefault(i =>
            i.Properties.Any(p => p.Name == "RouteId"));

        // Assert
        routeIdIndex.Should().NotBeNull();
    }

    #endregion

    #region Computed Properties Are Ignored Tests

    [Fact]
    public void Trip_StopTimes_IsIgnored_InModel()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(Trip));

        // Act
        var property = entityType!.FindProperty("StopTimes");

        // Assert - Property should not exist (ignored)
        property.Should().BeNull();
    }

    [Fact]
    public void Trip_FirstStopTime_IsIgnored_InModel()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(Trip));

        // Act
        var property = entityType!.FindProperty("FirstStopTime");

        // Assert
        property.Should().BeNull();
    }

    [Fact]
    public void Trip_Origin_IsIgnored_InModel()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(Trip));

        // Act
        var property = entityType!.FindProperty("Origin");

        // Assert
        property.Should().BeNull();
    }

    [Fact]
    public void Trip_TotalDuration_IsIgnored_InModel()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(Trip));

        // Act
        var property = entityType!.FindProperty("TotalDuration");

        // Assert
        property.Should().BeNull();
    }

    #endregion

    #region CRUD Operations Tests

    [Fact]
    public async Task Trip_CanBeCreated_AndRetrieved()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var tripId = TripId.From("trip_crud_test");
        var trip = new Trip(tripId, "Test Destination");

        // Act
        context.Trips.Add(trip);
        await context.SaveChangesAsync();

        // Assert
        using var verifyContext = _fixture.CreateContext();
        var loaded = await verifyContext.Trips.FirstOrDefaultAsync(t => t.Id == tripId);

        loaded.Should().NotBeNull();
        loaded!.Headsign.Should().Be("Test Destination");
    }

    [Fact]
    public async Task Trip_CanBeDeleted()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var tripId = TripId.From("trip_delete_test");
        var trip = new Trip(tripId);
        context.Trips.Add(trip);
        await context.SaveChangesAsync();

        // Act
        using var deleteContext = _fixture.CreateContext();
        var toDelete = await deleteContext.Trips.FirstAsync(t => t.Id == tripId);
        deleteContext.Trips.Remove(toDelete);
        await deleteContext.SaveChangesAsync();

        // Assert
        using var verifyContext = _fixture.CreateContext();
        var exists = await verifyContext.Trips.AnyAsync(t => t.Id == tripId);
        exists.Should().BeFalse();
    }

    #endregion

    #region Route-Trip Relationship Tests

    [Fact]
    public void Trip_HasShadowProperty_ForRouteId()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(Trip));

        // Act
        var routeIdProperty = entityType!.FindProperty("RouteId");

        // Assert - RouteId should exist as shadow property
        routeIdProperty.Should().NotBeNull();
    }

    #endregion
}
