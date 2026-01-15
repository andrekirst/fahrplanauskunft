using Bogus;
using Fahrplanauskunft.Core.Entities;
using Fahrplanauskunft.Core.ValueObjects;
using Fahrplanauskunft.Infrastructure.Data;
using Fahrplanauskunft.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fahrplanauskunft.Tests.Unit.Repositories;

/// <summary>
/// Unit tests for RouteRepository.
/// Uses SQLite in-memory database for testing EF Core operations.
/// </summary>
public class RouteRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<FahrplanDbContext> _options;
    private readonly ILogger<RouteRepository> _logger;
    private readonly Faker _faker;

    public RouteRepositoryTests()
    {
        // SQLite in-memory requires the connection to stay open
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<FahrplanDbContext>()
            .UseSqlite(_connection)
            .Options;

        // Create the schema
        using var context = CreateContext();
        context.Database.EnsureCreated();

        _logger = Substitute.For<ILogger<RouteRepository>>();
        _faker = new Faker("de");
    }

    private FahrplanDbContext CreateContext() => new(_options);

    private RouteRepository CreateRepository(FahrplanDbContext context) => new(context, _logger);

    public void Dispose()
    {
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new RouteRepository(null!, _logger);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("context");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var act = () => new RouteRepository(context, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var repository = new RouteRepository(context, _logger);

        // Assert
        repository.Should().NotBeNull();
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithExistingRoute_ReturnsSuccessWithRoute()
    {
        // Arrange
        using var context = CreateContext();
        var routeId = RouteId.From("route_001");
        var route = new Route(routeId, "U1", TransportMode.Subway);
        context.Routes.Add(route);
        await context.SaveChangesAsync();

        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetByIdAsync(routeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(routeId);
        result.Value.ShortName.Should().Be("U1");
        result.Value.Mode.Should().Be(TransportMode.Subway);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingRoute_ReturnsFailure()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);
        var routeId = RouteId.From("non_existing_route");

        // Act
        var result = await repository.GetByIdAsync(routeId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("non_existing_route");
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task GetByIdAsync_WithCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);
        var routeId = RouteId.From("route_001");
        var cancellationToken = new CancellationToken(canceled: true);

        // Act
        var act = async () => await repository.GetByIdAsync(routeId, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task GetByIdAsync_WithRouteHavingAllProperties_ReturnsCompleteRoute()
    {
        // Arrange
        using var context = CreateContext();
        var routeId = RouteId.From("route_full");
        var route = new Route(
            routeId,
            "S1",
            TransportMode.Rail,
            longName: "S-Bahn Linie 1",
            color: "00FF00",
            textColor: "FFFFFF");
        context.Routes.Add(route);
        await context.SaveChangesAsync();

        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetByIdAsync(routeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ShortName.Should().Be("S1");
        result.Value.Mode.Should().Be(TransportMode.Rail);
        result.Value.LongName.Should().Be("S-Bahn Linie 1");
        result.Value.Color.Should().Be("00FF00");
        result.Value.TextColor.Should().Be("FFFFFF");
    }

    [Fact]
    public async Task GetByIdAsync_WithRouteHavingNoOptionalProperties_ReturnsRoute()
    {
        // Arrange
        using var context = CreateContext();
        var routeId = RouteId.From("route_minimal");
        var route = new Route(routeId, "42", TransportMode.Bus);
        context.Routes.Add(route);
        await context.SaveChangesAsync();

        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetByIdAsync(routeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(routeId);
        result.Value.ShortName.Should().Be("42");
        result.Value.Mode.Should().Be(TransportMode.Bus);
        result.Value.LongName.Should().BeNull();
        result.Value.Color.Should().BeNull();
        result.Value.TextColor.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithDifferentTransportModes_ReturnsCorrectMode()
    {
        // Arrange
        using var context = CreateContext();
        var tramRoute = new Route(RouteId.From("tram_001"), "T1", TransportMode.Tram);
        var busRoute = new Route(RouteId.From("bus_001"), "B42", TransportMode.Bus);
        var subwayRoute = new Route(RouteId.From("subway_001"), "U7", TransportMode.Subway);
        var ferryRoute = new Route(RouteId.From("ferry_001"), "F1", TransportMode.Ferry);

        context.Routes.AddRange(tramRoute, busRoute, subwayRoute, ferryRoute);
        await context.SaveChangesAsync();

        var repository = CreateRepository(context);

        // Act
        var tramResult = await repository.GetByIdAsync(RouteId.From("tram_001"));
        var busResult = await repository.GetByIdAsync(RouteId.From("bus_001"));
        var subwayResult = await repository.GetByIdAsync(RouteId.From("subway_001"));
        var ferryResult = await repository.GetByIdAsync(RouteId.From("ferry_001"));

        // Assert
        tramResult.IsSuccess.Should().BeTrue();
        tramResult.Value.Mode.Should().Be(TransportMode.Tram);

        busResult.IsSuccess.Should().BeTrue();
        busResult.Value.Mode.Should().Be(TransportMode.Bus);

        subwayResult.IsSuccess.Should().BeTrue();
        subwayResult.Value.Mode.Should().Be(TransportMode.Subway);

        ferryResult.IsSuccess.Should().BeTrue();
        ferryResult.Value.Mode.Should().Be(TransportMode.Ferry);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WithNoRoutes_ReturnsEmptyList()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleRoutes_ReturnsAllRoutes()
    {
        // Arrange
        using var context = CreateContext();
        var routes = new[]
        {
            new Route(RouteId.From("route_1"), "U1", TransportMode.Subway),
            new Route(RouteId.From("route_2"), "S1", TransportMode.Rail),
            new Route(RouteId.From("route_3"), "42", TransportMode.Bus),
        };
        context.Routes.AddRange(routes);
        await context.SaveChangesAsync();

        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
        result.Value.Select(r => r.ShortName).Should().Contain("U1");
        result.Value.Select(r => r.ShortName).Should().Contain("S1");
        result.Value.Select(r => r.ShortName).Should().Contain("42");
    }

    [Fact]
    public async Task GetAllAsync_WithCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);
        var cancellationToken = new CancellationToken(canceled: true);

        // Act
        var act = async () => await repository.GetAllAsync(cancellationToken);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsReadOnlyList()
    {
        // Arrange
        using var context = CreateContext();
        var route = new Route(RouteId.From("route_readonly"), "R1", TransportMode.Rail);
        context.Routes.Add(route);
        await context.SaveChangesAsync();

        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeAssignableTo<IReadOnlyList<Route>>();
    }

    [Fact]
    public async Task GetAllAsync_WithManyRoutes_ReturnsAllRoutes()
    {
        // Arrange
        using var context = CreateContext();
        var routes = Enumerable.Range(1, 50)
            .Select(i => new Route(RouteId.From($"route_{i}"), $"R{i}", TransportMode.Bus))
            .ToList();
        context.Routes.AddRange(routes);
        await context.SaveChangesAsync();

        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(50);
    }

    [Fact]
    public async Task GetAllAsync_WithRoutesHavingDifferentModes_ReturnsAllModes()
    {
        // Arrange
        using var context = CreateContext();
        var routes = new[]
        {
            new Route(RouteId.From("tram"), "T1", TransportMode.Tram),
            new Route(RouteId.From("subway"), "U1", TransportMode.Subway),
            new Route(RouteId.From("rail"), "S1", TransportMode.Rail),
            new Route(RouteId.From("bus"), "42", TransportMode.Bus),
            new Route(RouteId.From("ferry"), "F1", TransportMode.Ferry),
        };
        context.Routes.AddRange(routes);
        await context.SaveChangesAsync();

        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(5);
        result.Value.Select(r => r.Mode).Should().Contain(TransportMode.Tram);
        result.Value.Select(r => r.Mode).Should().Contain(TransportMode.Subway);
        result.Value.Select(r => r.Mode).Should().Contain(TransportMode.Rail);
        result.Value.Select(r => r.Mode).Should().Contain(TransportMode.Bus);
        result.Value.Select(r => r.Mode).Should().Contain(TransportMode.Ferry);
    }

    #endregion

    #region AddRangeAsync Tests

    [Fact]
    public async Task AddRangeAsync_WithNullCollection_ReturnsFailure()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);

        // Act
        var result = await repository.AddRangeAsync(null!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Routes collection cannot be null");
    }

    [Fact]
    public async Task AddRangeAsync_WithEmptyCollection_ReturnsSuccess()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);

        // Act
        var result = await repository.AddRangeAsync(Array.Empty<Route>());

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AddRangeAsync_WithValidRoutes_AddsRoutesToDatabase()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);
        var routes = new[]
        {
            new Route(RouteId.From("add_route_1"), "U1", TransportMode.Subway),
            new Route(RouteId.From("add_route_2"), "S2", TransportMode.Rail),
        };

        // Act
        var result = await repository.AddRangeAsync(routes);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify routes were added
        using var verifyContext = CreateContext();
        var count = await verifyContext.Routes.CountAsync();
        count.Should().Be(2);
    }

    [Fact]
    public async Task AddRangeAsync_WithCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);
        var routes = new[] { new Route(RouteId.From("route_cancel"), "X1", TransportMode.Bus) };
        var cancellationToken = new CancellationToken(canceled: true);

        // Act
        var act = async () => await repository.AddRangeAsync(routes, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task AddRangeAsync_WithDuplicateIds_ReturnsFailure()
    {
        // Arrange
        using var context = CreateContext();
        var existingRoute = new Route(RouteId.From("duplicate_route"), "Existing", TransportMode.Bus);
        context.Routes.Add(existingRoute);
        await context.SaveChangesAsync();

        var repository = CreateRepository(context);
        var newRoutes = new[] { new Route(RouteId.From("duplicate_route"), "New", TransportMode.Subway) };

        // Act
        var result = await repository.AddRangeAsync(newRoutes);

        // Assert
        result.IsFailure.Should().BeTrue();
        // Error message should indicate failure - either database error or generic error
        result.Error.Should().ContainAny("error", "Error");
    }

    [Fact]
    public async Task AddRangeAsync_WithLargeCollection_AddsAllRoutes()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);
        var routes = Enumerable.Range(1, 100)
            .Select(i => new Route(RouteId.From($"bulk_route_{i}"), $"R{i}", TransportMode.Bus))
            .ToList();

        // Act
        var result = await repository.AddRangeAsync(routes);

        // Assert
        result.IsSuccess.Should().BeTrue();

        using var verifyContext = CreateContext();
        var count = await verifyContext.Routes.CountAsync();
        count.Should().Be(100);
    }

    [Fact]
    public async Task AddRangeAsync_WithRoutesHavingAllProperties_PersistsAllProperties()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);
        var routeId = RouteId.From("full_props_route");
        var routes = new[]
        {
            new Route(
                routeId,
                "U3",
                TransportMode.Subway,
                longName: "U-Bahn Linie 3 Hamburg",
                color: "FFCC00",
                textColor: "000000")
        };

        // Act
        var result = await repository.AddRangeAsync(routes);

        // Assert
        result.IsSuccess.Should().BeTrue();

        using var verifyContext = CreateContext();
        var loaded = await verifyContext.Routes.FirstAsync(r => r.Id == routeId);
        loaded.ShortName.Should().Be("U3");
        loaded.Mode.Should().Be(TransportMode.Subway);
        loaded.LongName.Should().Be("U-Bahn Linie 3 Hamburg");
        loaded.Color.Should().Be("FFCC00");
        loaded.TextColor.Should().Be("000000");
    }

    [Fact]
    public async Task AddRangeAsync_WithRoutesHavingNullOptionalProperties_PersistsRoutes()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);
        var routeId = RouteId.From("minimal_route");
        var routes = new[]
        {
            new Route(routeId, "M1", TransportMode.Tram)
        };

        // Act
        var result = await repository.AddRangeAsync(routes);

        // Assert
        result.IsSuccess.Should().BeTrue();

        using var verifyContext = CreateContext();
        var loaded = await verifyContext.Routes.FirstAsync(r => r.Id == routeId);
        loaded.ShortName.Should().Be("M1");
        loaded.Mode.Should().Be(TransportMode.Tram);
        loaded.LongName.Should().BeNull();
        loaded.Color.Should().BeNull();
        loaded.TextColor.Should().BeNull();
    }

    [Fact]
    public async Task AddRangeAsync_WithSingleRoute_AddsRoute()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);
        var routes = new[] { new Route(RouteId.From("single_route"), "S99", TransportMode.Rail) };

        // Act
        var result = await repository.AddRangeAsync(routes);

        // Assert
        result.IsSuccess.Should().BeTrue();

        using var verifyContext = CreateContext();
        var count = await verifyContext.Routes.CountAsync();
        count.Should().Be(1);
    }

    #endregion

    #region AsNoTracking Verification Tests

    [Fact]
    public async Task GetByIdAsync_ReturnsUntrackedEntity()
    {
        // Arrange
        using var context = CreateContext();
        var routeId = RouteId.From("untracked_route");
        var route = new Route(routeId, "UT1", TransportMode.Bus);
        context.Routes.Add(route);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetByIdAsync(routeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        context.Entry(result.Value).State.Should().Be(EntityState.Detached);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsUntrackedEntities()
    {
        // Arrange
        using var context = CreateContext();
        var route = new Route(RouteId.From("untracked_all"), "UA1", TransportMode.Rail);
        context.Routes.Add(route);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        foreach (var r in result.Value)
        {
            context.Entry(r).State.Should().Be(EntityState.Detached);
        }
    }

    #endregion

    #region Bogus Data Tests

    [Fact]
    public async Task GetByIdAsync_WithBogusGeneratedRoute_WorksCorrectly()
    {
        // Arrange
        using var context = CreateContext();
        var routeFaker = new Faker<Route>()
            .CustomInstantiator(f => new Route(
                RouteId.From($"bogus_route_{f.Random.AlphaNumeric(5)}"),
                f.PickRandom("U1", "S2", "42", "M5", "RE7"),
                f.PickRandom<TransportMode>()));

        var fakeRoute = routeFaker.Generate();
        context.Routes.Add(fakeRoute);
        await context.SaveChangesAsync();

        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetByIdAsync(fakeRoute.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ShortName.Should().Be(fakeRoute.ShortName);
        result.Value.Mode.Should().Be(fakeRoute.Mode);
    }

    [Fact]
    public async Task AddRangeAsync_WithBogusGeneratedRoutes_AddsAllRoutes()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);
        var counter = 0;
        var routeFaker = new Faker<Route>()
            .CustomInstantiator(f =>
            {
                counter++;
                return new Route(
                    RouteId.From($"bogus_bulk_{counter}"),
                    f.PickRandom("U", "S", "M", "RE") + f.Random.Number(1, 99),
                    f.PickRandom<TransportMode>());
            });

        var fakeRoutes = routeFaker.Generate(10);

        // Act
        var result = await repository.AddRangeAsync(fakeRoutes);

        // Assert
        result.IsSuccess.Should().BeTrue();

        using var verifyContext = CreateContext();
        var count = await verifyContext.Routes.CountAsync();
        count.Should().Be(10);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task GetByIdAsync_WithSpecialCharactersInLongName_ReturnsRoute()
    {
        // Arrange
        using var context = CreateContext();
        var routeId = RouteId.From("route_special");
        var route = new Route(
            routeId,
            "S1",
            TransportMode.Rail,
            longName: "S-Bahn Linie 1 (Hamburg → Wedel)");
        context.Routes.Add(route);
        await context.SaveChangesAsync();

        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetByIdAsync(routeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.LongName.Should().Be("S-Bahn Linie 1 (Hamburg → Wedel)");
    }

    [Fact]
    public async Task GetByIdAsync_WithGermanUmlautsInLongName_ReturnsRoute()
    {
        // Arrange
        using var context = CreateContext();
        var routeId = RouteId.From("route_umlaut");
        var route = new Route(
            routeId,
            "U2",
            TransportMode.Subway,
            longName: "U-Bahn Köln-Düsseldorf Mülheim");
        context.Routes.Add(route);
        await context.SaveChangesAsync();

        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetByIdAsync(routeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.LongName.Should().Be("U-Bahn Köln-Düsseldorf Mülheim");
    }

    [Fact]
    public async Task AddRangeAsync_WithValidHexColors_PersistsColors()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);
        var routes = new[]
        {
            new Route(RouteId.From("color_red"), "R1", TransportMode.Rail, color: "FF0000", textColor: "FFFFFF"),
            new Route(RouteId.From("color_blue"), "B1", TransportMode.Bus, color: "0000FF", textColor: "FFFFFF"),
            new Route(RouteId.From("color_green"), "T1", TransportMode.Tram, color: "00FF00", textColor: "000000"),
        };

        // Act
        var result = await repository.AddRangeAsync(routes);

        // Assert
        result.IsSuccess.Should().BeTrue();

        using var verifyContext = CreateContext();
        var redRoute = await verifyContext.Routes.FirstAsync(r => r.Id == RouteId.From("color_red"));
        redRoute.Color.Should().Be("FF0000");
        redRoute.TextColor.Should().Be("FFFFFF");

        var blueRoute = await verifyContext.Routes.FirstAsync(r => r.Id == RouteId.From("color_blue"));
        blueRoute.Color.Should().Be("0000FF");

        var greenRoute = await verifyContext.Routes.FirstAsync(r => r.Id == RouteId.From("color_green"));
        greenRoute.Color.Should().Be("00FF00");
        greenRoute.TextColor.Should().Be("000000");
    }

    [Fact]
    public async Task GetAllAsync_WithRoutesHavingMixedOptionalProperties_ReturnsAllRoutes()
    {
        // Arrange
        using var context = CreateContext();
        var routes = new[]
        {
            new Route(RouteId.From("mixed_1"), "U1", TransportMode.Subway), // minimal
            new Route(RouteId.From("mixed_2"), "S1", TransportMode.Rail, longName: "With Long Name"),
            new Route(RouteId.From("mixed_3"), "42", TransportMode.Bus, color: "AABBCC"),
            new Route(RouteId.From("mixed_4"), "M1", TransportMode.Tram, longName: "Full Props", color: "112233", textColor: "FFFFFF"),
        };
        context.Routes.AddRange(routes);
        await context.SaveChangesAsync();

        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(4);

        var minimal = result.Value.First(r => r.Id == RouteId.From("mixed_1"));
        minimal.LongName.Should().BeNull();
        minimal.Color.Should().BeNull();
        minimal.TextColor.Should().BeNull();

        var withLongName = result.Value.First(r => r.Id == RouteId.From("mixed_2"));
        withLongName.LongName.Should().Be("With Long Name");
        withLongName.Color.Should().BeNull();

        var withColor = result.Value.First(r => r.Id == RouteId.From("mixed_3"));
        withColor.LongName.Should().BeNull();
        withColor.Color.Should().Be("AABBCC");

        var fullProps = result.Value.First(r => r.Id == RouteId.From("mixed_4"));
        fullProps.LongName.Should().Be("Full Props");
        fullProps.Color.Should().Be("112233");
        fullProps.TextColor.Should().Be("FFFFFF");
    }

    [Fact]
    public async Task GetByIdAsync_WithNumericShortName_ReturnsRoute()
    {
        // Arrange
        using var context = CreateContext();
        var routeId = RouteId.From("bus_numeric");
        var route = new Route(routeId, "142", TransportMode.Bus);
        context.Routes.Add(route);
        await context.SaveChangesAsync();

        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetByIdAsync(routeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ShortName.Should().Be("142");
    }

    [Fact]
    public async Task GetByIdAsync_WithAlphanumericShortName_ReturnsRoute()
    {
        // Arrange
        using var context = CreateContext();
        var routeId = RouteId.From("mixed_name");
        var route = new Route(routeId, "RE42a", TransportMode.Rail);
        context.Routes.Add(route);
        await context.SaveChangesAsync();

        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetByIdAsync(routeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ShortName.Should().Be("RE42a");
    }

    #endregion
}
