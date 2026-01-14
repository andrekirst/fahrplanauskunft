using Fahrplanauskunft.Core.Entities;
using Fahrplanauskunft.Core.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fahrplanauskunft.Tests.Integration.EfCore;

/// <summary>
/// Tests for Stop entity EF Core configuration.
/// Validates value conversions, indexes, and CRUD operations.
/// </summary>
public class StopConfigurationTests : IClassFixture<SqliteDbContextFixture>
{
    private readonly SqliteDbContextFixture _fixture;

    public StopConfigurationTests(SqliteDbContextFixture fixture)
    {
        _fixture = fixture;
    }

    #region Value Conversion Tests

    [Fact]
    public async Task Stop_StopId_IsPersisted_AsString()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var stopId = StopId.From("stop_test_001");
        var stop = new Stop(stopId, "Test Station");

        // Act
        context.Stops.Add(stop);
        await context.SaveChangesAsync();

        // Assert - Reload from database
        using var verifyContext = _fixture.CreateContext();
        var loaded = await verifyContext.Stops.FirstOrDefaultAsync(s => s.Id == stopId);

        loaded.Should().NotBeNull();
        loaded!.Id.Should().Be(stopId);
        loaded.Id.Value.Should().Be("stop_test_001");
    }

    [Fact]
    public async Task Stop_WithCoordinates_PersistsAndLoads_Correctly()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var stopId = StopId.From("stop_coord_test");
        var coordinates = new Coordinates(52.520008, 13.404954); // Berlin
        var stop = new Stop(stopId, "Berlin Hbf", coordinates);

        // Act
        context.Stops.Add(stop);
        await context.SaveChangesAsync();

        // Assert
        using var verifyContext = _fixture.CreateContext();
        var loaded = await verifyContext.Stops.FirstOrDefaultAsync(s => s.Id == stopId);

        loaded.Should().NotBeNull();
        loaded!.Location.Should().NotBeNull();
        loaded.Location!.Value.Latitude.Should().BeApproximately(52.520008, 0.000001);
        loaded.Location.Value.Longitude.Should().BeApproximately(13.404954, 0.000001);
    }

    [Fact]
    public async Task Stop_WithoutCoordinates_PersistsNull_Correctly()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var stopId = StopId.From("stop_no_coord");
        var stop = new Stop(stopId, "Unknown Location Stop");

        // Act
        context.Stops.Add(stop);
        await context.SaveChangesAsync();

        // Assert
        using var verifyContext = _fixture.CreateContext();
        var loaded = await verifyContext.Stops.FirstOrDefaultAsync(s => s.Id == stopId);

        loaded.Should().NotBeNull();
        loaded!.Location.Should().BeNull();
    }

    #endregion

    #region Property Persistence Tests

    [Fact]
    public async Task Stop_AllProperties_ArePersisted_Correctly()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var stopId = StopId.From("stop_full_props");
        var coordinates = new Coordinates(48.137154, 11.576124); // Munich
        var stop = new Stop(stopId, "Munich Hbf", coordinates, "Platform 12", true);

        // Act
        context.Stops.Add(stop);
        await context.SaveChangesAsync();

        // Assert
        using var verifyContext = _fixture.CreateContext();
        var loaded = await verifyContext.Stops.FirstOrDefaultAsync(s => s.Id == stopId);

        loaded.Should().NotBeNull();
        loaded!.Name.Should().Be("Munich Hbf");
        loaded.PlatformCode.Should().Be("Platform 12");
        loaded.WheelchairAccessible.Should().BeTrue();
    }

    [Fact]
    public async Task Stop_WheelchairAccessible_DefaultsToFalse()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var stopId = StopId.From("stop_default_wheelchair");
        var stop = new Stop(stopId, "Default Accessibility");

        // Act
        context.Stops.Add(stop);
        await context.SaveChangesAsync();

        // Assert
        using var verifyContext = _fixture.CreateContext();
        var loaded = await verifyContext.Stops.FirstOrDefaultAsync(s => s.Id == stopId);

        loaded.Should().NotBeNull();
        loaded!.WheelchairAccessible.Should().BeFalse();
    }

    #endregion

    #region Index Configuration Tests

    [Fact]
    public void Stop_HasIndex_OnName()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(Stop));

        // Act
        var indexes = entityType!.GetIndexes().ToList();
        var nameIndex = indexes.FirstOrDefault(i =>
            i.Properties.Any(p => p.Name == "Name"));

        // Assert
        nameIndex.Should().NotBeNull();
    }

    [Fact]
    public void Stop_NoIndex_OnWheelchairAccessible_LowSelectivityOptimization()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(Stop));

        // Act
        var indexes = entityType!.GetIndexes().ToList();
        var accessibilityIndex = indexes.FirstOrDefault(i =>
            i.Properties.Any(p => p.Name == "WheelchairAccessible"));

        // Assert - No index on boolean columns (low selectivity ~50%)
        // This is intentional - boolean indexes rarely help the query optimizer
        // and add write overhead. Use filtered indexes if needed.
        accessibilityIndex.Should().BeNull();
    }

    #endregion

    #region CRUD Operations Tests

    [Fact]
    public async Task Stop_CanBeCreated_AndRetrieved()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var stopId = StopId.From("stop_crud_create");
        var stop = new Stop(stopId, "CRUD Test Station");

        // Act
        context.Stops.Add(stop);
        await context.SaveChangesAsync();

        // Assert
        using var verifyContext = _fixture.CreateContext();
        var exists = await verifyContext.Stops.AnyAsync(s => s.Id == stopId);
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task Stop_CanBeDeleted()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var stopId = StopId.From("stop_crud_delete");
        var stop = new Stop(stopId, "Delete Test Station");
        context.Stops.Add(stop);
        await context.SaveChangesAsync();

        // Act
        using var deleteContext = _fixture.CreateContext();
        var toDelete = await deleteContext.Stops.FirstAsync(s => s.Id == stopId);
        deleteContext.Stops.Remove(toDelete);
        await deleteContext.SaveChangesAsync();

        // Assert
        using var verifyContext = _fixture.CreateContext();
        var exists = await verifyContext.Stops.AnyAsync(s => s.Id == stopId);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task Stop_CanBeQueried_ById()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var stopId = StopId.From("stop_query_test");
        var stop = new Stop(stopId, "Query Test Station");
        context.Stops.Add(stop);
        await context.SaveChangesAsync();

        // Act
        using var queryContext = _fixture.CreateContext();
        var queried = await queryContext.Stops
            .Where(s => s.Id == stopId)
            .FirstOrDefaultAsync();

        // Assert
        queried.Should().NotBeNull();
        queried!.Name.Should().Be("Query Test Station");
    }

    #endregion
}
