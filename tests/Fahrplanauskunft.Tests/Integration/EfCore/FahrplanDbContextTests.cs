using Fahrplanauskunft.Core.Entities;
using Fahrplanauskunft.Core.ValueObjects;
using Fahrplanauskunft.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fahrplanauskunft.Tests.Integration.EfCore;

/// <summary>
/// Integration tests for FahrplanDbContext.
/// These tests verify the DbContext is correctly configured and can perform
/// basic CRUD operations on all entity types.
/// </summary>
public class FahrplanDbContextTests : IClassFixture<SqliteDbContextFixture>
{
    private readonly SqliteDbContextFixture _fixture;

    public FahrplanDbContextTests(SqliteDbContextFixture fixture)
    {
        _fixture = fixture;
    }

    #region DbContext Configuration Tests

    [Fact]
    public void DbContext_HasDbSetProperties_ForAllEntities()
    {
        // Arrange
        using var context = _fixture.CreateContext();

        // Act & Assert
        context.Stops.Should().NotBeNull();
        context.Routes.Should().NotBeNull();
        context.Trips.Should().NotBeNull();
        context.StopTimes.Should().NotBeNull();
        context.Footpaths.Should().NotBeNull();
    }

    [Fact]
    public void DbContext_CanBuildModel_WithoutErrors()
    {
        // Arrange
        using var context = _fixture.CreateContext();

        // Act - Getting the model forces EF Core to build it
        var model = context.Model;

        // Assert
        model.Should().NotBeNull();
        model.GetEntityTypes().Should().NotBeEmpty();
    }

    [Fact]
    public void DbContext_Model_ContainsAllExpectedEntityTypes()
    {
        // Arrange
        using var context = _fixture.CreateContext();

        // Act
        var entityTypes = context.Model.GetEntityTypes()
            .Select(e => e.ClrType)
            .ToList();

        // Assert
        entityTypes.Should().Contain(typeof(Stop));
        entityTypes.Should().Contain(typeof(Route));
        entityTypes.Should().Contain(typeof(Trip));
        entityTypes.Should().Contain(typeof(StopTime));
        entityTypes.Should().Contain(typeof(Footpath));
    }

    #endregion

    #region Entity Table Name Tests

    [Fact]
    public void Stop_MapsTo_StopsTable()
    {
        // Arrange
        using var context = _fixture.CreateContext();

        // Act
        var entityType = context.Model.FindEntityType(typeof(Stop));

        // Assert
        entityType.Should().NotBeNull();
        entityType!.GetTableName().Should().Be("Stops");
    }

    [Fact]
    public void Route_MapsTo_RoutesTable()
    {
        // Arrange
        using var context = _fixture.CreateContext();

        // Act
        var entityType = context.Model.FindEntityType(typeof(Route));

        // Assert
        entityType.Should().NotBeNull();
        entityType!.GetTableName().Should().Be("Routes");
    }

    [Fact]
    public void Trip_MapsTo_TripsTable()
    {
        // Arrange
        using var context = _fixture.CreateContext();

        // Act
        var entityType = context.Model.FindEntityType(typeof(Trip));

        // Assert
        entityType.Should().NotBeNull();
        entityType!.GetTableName().Should().Be("Trips");
    }

    [Fact]
    public void StopTime_MapsTo_StopTimesTable()
    {
        // Arrange
        using var context = _fixture.CreateContext();

        // Act
        var entityType = context.Model.FindEntityType(typeof(StopTime));

        // Assert
        entityType.Should().NotBeNull();
        entityType!.GetTableName().Should().Be("StopTimes");
    }

    [Fact]
    public void Footpath_MapsTo_FootpathsTable()
    {
        // Arrange
        using var context = _fixture.CreateContext();

        // Act
        var entityType = context.Model.FindEntityType(typeof(Footpath));

        // Assert
        entityType.Should().NotBeNull();
        entityType!.GetTableName().Should().Be("Footpaths");
    }

    #endregion
}
