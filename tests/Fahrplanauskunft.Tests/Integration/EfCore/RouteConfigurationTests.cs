using Fahrplanauskunft.Core.Entities;
using Fahrplanauskunft.Core.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fahrplanauskunft.Tests.Integration.EfCore;

/// <summary>
/// Tests for Route entity EF Core configuration.
/// Validates RouteId conversion, TransportMode enum conversion, relationships, and indexes.
/// </summary>
public class RouteConfigurationTests : IClassFixture<SqliteDbContextFixture>
{
    private readonly SqliteDbContextFixture _fixture;

    public RouteConfigurationTests(SqliteDbContextFixture fixture)
    {
        _fixture = fixture;
    }

    #region Value Conversion Tests

    [Fact]
    public async Task Route_RouteId_IsPersisted_AsString()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var routeId = RouteId.From("route_test_001");
        var route = new Route(routeId, "S1", TransportMode.Rail);

        // Act
        context.Routes.Add(route);
        await context.SaveChangesAsync();

        // Assert
        using var verifyContext = _fixture.CreateContext();
        var loaded = await verifyContext.Routes.FirstOrDefaultAsync(r => r.Id == routeId);

        loaded.Should().NotBeNull();
        loaded!.Id.Should().Be(routeId);
        loaded.Id.Value.Should().Be("route_test_001");
    }

    [Theory]
    [InlineData(TransportMode.Rail)]
    [InlineData(TransportMode.Subway)]
    [InlineData(TransportMode.Tram)]
    [InlineData(TransportMode.Bus)]
    [InlineData(TransportMode.Ferry)]
    [InlineData(TransportMode.Funicular)]
    [InlineData(TransportMode.Monorail)]
    public async Task Route_TransportMode_IsPersisted_Correctly(TransportMode mode)
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var routeId = RouteId.From($"route_mode_{(int)mode}");
        var route = new Route(routeId, $"Test {mode}", mode);

        // Act
        context.Routes.Add(route);
        await context.SaveChangesAsync();

        // Assert
        using var verifyContext = _fixture.CreateContext();
        var loaded = await verifyContext.Routes.FirstOrDefaultAsync(r => r.Id == routeId);

        loaded.Should().NotBeNull();
        loaded!.Mode.Should().Be(mode);
    }

    #endregion

    #region Property Persistence Tests

    [Fact]
    public async Task Route_AllProperties_ArePersisted_Correctly()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var routeId = RouteId.From("route_full_props");
        var route = new Route(
            routeId,
            "U6",
            TransportMode.Subway,
            "U-Bahn Linie 6",
            "0066CC",
            "FFFFFF"
        );

        // Act
        context.Routes.Add(route);
        await context.SaveChangesAsync();

        // Assert
        using var verifyContext = _fixture.CreateContext();
        var loaded = await verifyContext.Routes.FirstOrDefaultAsync(r => r.Id == routeId);

        loaded.Should().NotBeNull();
        loaded!.ShortName.Should().Be("U6");
        loaded.LongName.Should().Be("U-Bahn Linie 6");
        loaded.Color.Should().Be("0066CC");
        loaded.TextColor.Should().Be("FFFFFF");
        loaded.Mode.Should().Be(TransportMode.Subway);
    }

    [Fact]
    public async Task Route_WithNullOptionalProperties_PersistsCorrectly()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var routeId = RouteId.From("route_null_props");
        var route = new Route(routeId, "B100", TransportMode.Bus);

        // Act
        context.Routes.Add(route);
        await context.SaveChangesAsync();

        // Assert
        using var verifyContext = _fixture.CreateContext();
        var loaded = await verifyContext.Routes.FirstOrDefaultAsync(r => r.Id == routeId);

        loaded.Should().NotBeNull();
        loaded!.LongName.Should().BeNull();
        loaded.Color.Should().BeNull();
        loaded.TextColor.Should().BeNull();
    }

    #endregion

    #region Index Configuration Tests

    [Fact]
    public void Route_HasIndex_OnShortName()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(Route));

        // Act
        var indexes = entityType!.GetIndexes().ToList();
        var shortNameIndex = indexes.FirstOrDefault(i =>
            i.Properties.Any(p => p.Name == "ShortName"));

        // Assert
        shortNameIndex.Should().NotBeNull();
    }

    [Fact]
    public void Route_HasIndex_OnMode()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(Route));

        // Act
        var indexes = entityType!.GetIndexes().ToList();
        var modeIndex = indexes.FirstOrDefault(i =>
            i.Properties.Any(p => p.Name == "Mode"));

        // Assert
        modeIndex.Should().NotBeNull();
    }

    #endregion

    #region Computed Properties Are Ignored Tests

    [Fact]
    public void Route_TripCount_IsIgnored_InModel()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(Route));

        // Act
        var property = entityType!.FindProperty("TripCount");

        // Assert - Property should not exist in the model
        property.Should().BeNull();
    }

    [Fact]
    public void Route_DisplayName_IsIgnored_InModel()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(Route));

        // Act
        var property = entityType!.FindProperty("DisplayName");

        // Assert - Property should not exist in the model
        property.Should().BeNull();
    }

    #endregion

    #region CRUD Operations Tests

    [Fact]
    public async Task Route_CanBeCreated_AndRetrieved()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var routeId = RouteId.From("route_crud_test");
        var route = new Route(routeId, "T1", TransportMode.Tram);

        // Act
        context.Routes.Add(route);
        await context.SaveChangesAsync();

        // Assert
        using var verifyContext = _fixture.CreateContext();
        var loaded = await verifyContext.Routes.FirstOrDefaultAsync(r => r.Id == routeId);

        loaded.Should().NotBeNull();
        loaded!.ShortName.Should().Be("T1");
    }

    [Fact]
    public async Task Route_CanBeQueried_ByMode()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var routeId = RouteId.From("route_query_mode");
        var route = new Route(routeId, "F1", TransportMode.Ferry);
        context.Routes.Add(route);
        await context.SaveChangesAsync();

        // Act
        using var queryContext = _fixture.CreateContext();
        var ferryRoutes = await queryContext.Routes
            .Where(r => r.Mode == TransportMode.Ferry)
            .ToListAsync();

        // Assert
        ferryRoutes.Should().NotBeEmpty();
        ferryRoutes.Should().Contain(r => r.Id == routeId);
    }

    #endregion
}
