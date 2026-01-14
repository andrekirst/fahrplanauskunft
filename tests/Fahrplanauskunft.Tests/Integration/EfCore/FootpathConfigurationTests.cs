using Fahrplanauskunft.Core.Entities;
using Fahrplanauskunft.Core.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fahrplanauskunft.Tests.Integration.EfCore;

/// <summary>
/// Tests for Footpath entity EF Core configuration.
/// Validates composite key, Duration conversion, shadow properties, and indexes.
/// </summary>
public class FootpathConfigurationTests : IClassFixture<SqliteDbContextFixture>
{
    private readonly SqliteDbContextFixture _fixture;

    public FootpathConfigurationTests(SqliteDbContextFixture fixture)
    {
        _fixture = fixture;
    }

    #region Primary Key Configuration Tests

    [Fact]
    public void Footpath_HasCompositeKey_FromStopIdAndToStopId()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(Footpath));

        // Act
        var primaryKey = entityType!.FindPrimaryKey();
        var keyPropertyNames = primaryKey!.Properties.Select(p => p.Name).ToList();

        // Assert
        keyPropertyNames.Should().Contain("FromStopId");
        keyPropertyNames.Should().Contain("ToStopId");
        keyPropertyNames.Should().HaveCount(2);
    }

    #endregion

    #region Value Conversion Tests

    [Fact]
    public async Task Footpath_Duration_IsPersisted_AsTotalMinutes()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var fromStop = new Stop(StopId.From("fp_from_stop_1"), "From Station");
        var toStop = new Stop(StopId.From("fp_to_stop_1"), "To Station");
        var duration = Duration.FromMinutes(5);

        context.Stops.Add(fromStop);
        context.Stops.Add(toStop);
        await context.SaveChangesAsync();

        var footpath = new Footpath(fromStop, toStop, duration);

        // Set shadow properties via Entry BEFORE marking as Added
        // This is required because FromStopId/ToStopId form the composite primary key
        var entry = context.Entry(footpath);
        entry.Property("FromStopId").CurrentValue = fromStop.Id.Value;
        entry.Property("ToStopId").CurrentValue = toStop.Id.Value;
        entry.State = EntityState.Added;
        await context.SaveChangesAsync();

        // Assert - query using shadow property values
        using var verifyContext = _fixture.CreateContext();
        var loaded = await verifyContext.Footpaths
            .FirstOrDefaultAsync(f =>
                EF.Property<string>(f, "FromStopId") == fromStop.Id.Value &&
                EF.Property<string>(f, "ToStopId") == toStop.Id.Value);

        loaded.Should().NotBeNull();
        loaded!.Duration.TotalMinutes.Should().Be(5);
    }

    #endregion

    #region Property Persistence Tests

    [Fact]
    public async Task Footpath_AllProperties_ArePersisted_Correctly()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var fromStop = new Stop(StopId.From("fp_full_from"), "Origin Station");
        var toStop = new Stop(StopId.From("fp_full_to"), "Destination Station");
        var duration = Duration.FromMinutes(10);

        context.Stops.Add(fromStop);
        context.Stops.Add(toStop);
        await context.SaveChangesAsync();

        var footpath = new Footpath(fromStop, toStop, duration, true, 750);

        // Set shadow properties via Entry BEFORE marking as Added
        var entry = context.Entry(footpath);
        entry.Property("FromStopId").CurrentValue = fromStop.Id.Value;
        entry.Property("ToStopId").CurrentValue = toStop.Id.Value;
        entry.State = EntityState.Added;
        await context.SaveChangesAsync();

        // Assert
        using var verifyContext = _fixture.CreateContext();
        var loaded = await verifyContext.Footpaths
            .FirstOrDefaultAsync(f =>
                EF.Property<string>(f, "FromStopId") == fromStop.Id.Value &&
                EF.Property<string>(f, "ToStopId") == toStop.Id.Value);

        loaded.Should().NotBeNull();
        loaded!.WheelchairAccessible.Should().BeTrue();
        loaded.DistanceMeters.Should().Be(750);
    }

    [Fact]
    public async Task Footpath_WheelchairAccessible_DefaultsToFalse()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var fromStop = new Stop(StopId.From("fp_default_from"), "Default From");
        var toStop = new Stop(StopId.From("fp_default_to"), "Default To");
        var duration = Duration.FromMinutes(3);

        context.Stops.Add(fromStop);
        context.Stops.Add(toStop);
        await context.SaveChangesAsync();

        var footpath = new Footpath(fromStop, toStop, duration);

        // Set shadow properties via Entry BEFORE marking as Added
        var entry = context.Entry(footpath);
        entry.Property("FromStopId").CurrentValue = fromStop.Id.Value;
        entry.Property("ToStopId").CurrentValue = toStop.Id.Value;
        entry.State = EntityState.Added;
        await context.SaveChangesAsync();

        // Assert
        using var verifyContext = _fixture.CreateContext();
        var loaded = await verifyContext.Footpaths
            .FirstOrDefaultAsync(f =>
                EF.Property<string>(f, "FromStopId") == fromStop.Id.Value &&
                EF.Property<string>(f, "ToStopId") == toStop.Id.Value);

        loaded.Should().NotBeNull();
        loaded!.WheelchairAccessible.Should().BeFalse();
    }

    [Fact]
    public async Task Footpath_WithNullDistance_PersistsCorrectly()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var fromStop = new Stop(StopId.From("fp_null_dist_from"), "Null Dist From");
        var toStop = new Stop(StopId.From("fp_null_dist_to"), "Null Dist To");
        var duration = Duration.FromMinutes(7);

        context.Stops.Add(fromStop);
        context.Stops.Add(toStop);
        await context.SaveChangesAsync();

        var footpath = new Footpath(fromStop, toStop, duration);

        // Set shadow properties via Entry BEFORE marking as Added
        var entry = context.Entry(footpath);
        entry.Property("FromStopId").CurrentValue = fromStop.Id.Value;
        entry.Property("ToStopId").CurrentValue = toStop.Id.Value;
        entry.State = EntityState.Added;
        await context.SaveChangesAsync();

        // Assert
        using var verifyContext = _fixture.CreateContext();
        var loaded = await verifyContext.Footpaths
            .FirstOrDefaultAsync(f =>
                EF.Property<string>(f, "FromStopId") == fromStop.Id.Value &&
                EF.Property<string>(f, "ToStopId") == toStop.Id.Value);

        loaded.Should().NotBeNull();
        loaded!.DistanceMeters.Should().BeNull();
    }

    #endregion

    #region Index Configuration Tests

    [Fact]
    public void Footpath_HasIndex_OnFromStopId()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(Footpath));

        // Act
        var indexes = entityType!.GetIndexes().ToList();
        var fromStopIdIndex = indexes.FirstOrDefault(i =>
            i.Properties.Count == 1 &&
            i.Properties.Any(p => p.Name == "FromStopId"));

        // Assert
        fromStopIdIndex.Should().NotBeNull();
    }

    [Fact]
    public void Footpath_HasIndex_OnToStopId()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(Footpath));

        // Act
        var indexes = entityType!.GetIndexes().ToList();
        var toStopIdIndex = indexes.FirstOrDefault(i =>
            i.Properties.Count == 1 &&
            i.Properties.Any(p => p.Name == "ToStopId"));

        // Assert
        toStopIdIndex.Should().NotBeNull();
    }

    [Fact]
    public void Footpath_NoIndex_OnWheelchairAccessible_LowSelectivityOptimization()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(Footpath));

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

    #region Shadow Property Tests

    [Fact]
    public void Footpath_HasShadowProperty_FromStopId()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(Footpath));

        // Act
        var fromStopIdProperty = entityType!.FindProperty("FromStopId");

        // Assert
        fromStopIdProperty.Should().NotBeNull();
    }

    [Fact]
    public void Footpath_HasShadowProperty_ToStopId()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(Footpath));

        // Act
        var toStopIdProperty = entityType!.FindProperty("ToStopId");

        // Assert
        toStopIdProperty.Should().NotBeNull();
    }

    [Fact]
    public async Task Footpath_ShadowProperties_AreSetCorrectly()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var fromStop = new Stop(StopId.From("fp_shadow_from"), "Shadow From");
        var toStop = new Stop(StopId.From("fp_shadow_to"), "Shadow To");
        var duration = Duration.FromMinutes(5);

        context.Stops.Add(fromStop);
        context.Stops.Add(toStop);
        await context.SaveChangesAsync();

        var footpath = new Footpath(fromStop, toStop, duration);

        // Set shadow properties via Entry BEFORE marking as Added
        var entry = context.Entry(footpath);
        entry.Property("FromStopId").CurrentValue = fromStop.Id.Value;
        entry.Property("ToStopId").CurrentValue = toStop.Id.Value;
        entry.State = EntityState.Added;
        await context.SaveChangesAsync();

        // Assert - verify shadow properties can be queried
        using var verifyContext = _fixture.CreateContext();
        var loaded = await verifyContext.Footpaths
            .FirstOrDefaultAsync(f =>
                EF.Property<string>(f, "FromStopId") == fromStop.Id.Value &&
                EF.Property<string>(f, "ToStopId") == toStop.Id.Value);

        loaded.Should().NotBeNull();

        // Verify shadow property values via Entry
        var loadedEntry = verifyContext.Entry(loaded!);
        loadedEntry.Property("FromStopId").CurrentValue.Should().Be(fromStop.Id.Value);
        loadedEntry.Property("ToStopId").CurrentValue.Should().Be(toStop.Id.Value);
    }

    #endregion

    #region CRUD Operations Tests

    [Fact]
    public async Task Footpath_CanBeCreated_AndRetrieved()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var fromStop = new Stop(StopId.From("fp_crud_from"), "CRUD From");
        var toStop = new Stop(StopId.From("fp_crud_to"), "CRUD To");
        var duration = Duration.FromMinutes(6);

        context.Stops.Add(fromStop);
        context.Stops.Add(toStop);
        await context.SaveChangesAsync();

        var footpath = new Footpath(fromStop, toStop, duration);

        // Set shadow properties via Entry BEFORE marking as Added
        var entry = context.Entry(footpath);
        entry.Property("FromStopId").CurrentValue = fromStop.Id.Value;
        entry.Property("ToStopId").CurrentValue = toStop.Id.Value;
        entry.State = EntityState.Added;
        await context.SaveChangesAsync();

        // Assert
        using var verifyContext = _fixture.CreateContext();
        var exists = await verifyContext.Footpaths
            .AnyAsync(f =>
                EF.Property<string>(f, "FromStopId") == fromStop.Id.Value &&
                EF.Property<string>(f, "ToStopId") == toStop.Id.Value);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task Footpath_CanBeDeleted()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var fromStop = new Stop(StopId.From("fp_del_from"), "Delete From");
        var toStop = new Stop(StopId.From("fp_del_to"), "Delete To");
        var duration = Duration.FromMinutes(8);

        context.Stops.Add(fromStop);
        context.Stops.Add(toStop);
        await context.SaveChangesAsync();

        var footpath = new Footpath(fromStop, toStop, duration);

        // Set shadow properties via Entry BEFORE marking as Added
        var entry = context.Entry(footpath);
        entry.Property("FromStopId").CurrentValue = fromStop.Id.Value;
        entry.Property("ToStopId").CurrentValue = toStop.Id.Value;
        entry.State = EntityState.Added;
        await context.SaveChangesAsync();

        // Act
        using var deleteContext = _fixture.CreateContext();
        var toDelete = await deleteContext.Footpaths
            .FirstAsync(f =>
                EF.Property<string>(f, "FromStopId") == fromStop.Id.Value &&
                EF.Property<string>(f, "ToStopId") == toStop.Id.Value);
        deleteContext.Footpaths.Remove(toDelete);
        await deleteContext.SaveChangesAsync();

        // Assert
        using var verifyContext = _fixture.CreateContext();
        var exists = await verifyContext.Footpaths
            .AnyAsync(f =>
                EF.Property<string>(f, "FromStopId") == fromStop.Id.Value &&
                EF.Property<string>(f, "ToStopId") == toStop.Id.Value);

        exists.Should().BeFalse();
    }

    #endregion
}
