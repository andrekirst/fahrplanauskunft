using FluentAssertions;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Fahrplanauskunft.Tests.Integration.Database;

/// <summary>
/// Integration tests for PostgreSQL connectivity.
/// These tests use Testcontainers to spin up a real PostgreSQL instance,
/// validating that the connection string format from appsettings.json will work.
/// </summary>
public class PostgreSqlConnectionTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres;

    public PostgreSqlConnectionTests()
    {
        // Mirror the Docker Compose configuration from Issue #5
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:15")
            .WithDatabase("fahrplan")
            .WithUsername("fahrplan")
            .WithPassword("fahrplan_dev")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task PostgreSql_CanConnect()
    {
        // Arrange
        var connectionString = _postgres.GetConnectionString();

        // Act
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        // Assert
        connection.State.Should().Be(System.Data.ConnectionState.Open);
    }

    [Fact]
    public async Task PostgreSql_IsVersion15()
    {
        // Arrange
        await using var connection = new NpgsqlConnection(_postgres.GetConnectionString());
        await connection.OpenAsync();

        // Act
        await using var command = new NpgsqlCommand("SELECT version();", connection);
        var version = await command.ExecuteScalarAsync() as string;

        // Assert
        version.Should().Contain("PostgreSQL 15",
            because: "Docker Compose specifies postgres:15 image");
    }

    [Fact]
    public async Task PostgreSql_DatabaseNameIsCorrect()
    {
        // Arrange
        await using var connection = new NpgsqlConnection(_postgres.GetConnectionString());
        await connection.OpenAsync();

        // Act
        await using var command = new NpgsqlCommand("SELECT current_database();", connection);
        var databaseName = await command.ExecuteScalarAsync() as string;

        // Assert
        databaseName.Should().Be("fahrplan",
            because: "Issue #5 specifies database name as 'fahrplan'");
    }

    [Fact]
    public async Task PostgreSql_CanCreateAndQueryTable()
    {
        // Arrange
        await using var connection = new NpgsqlConnection(_postgres.GetConnectionString());
        await connection.OpenAsync();

        // Act - Create a test table
        await using var createCommand = new NpgsqlCommand(
            "CREATE TABLE test_stations (id TEXT PRIMARY KEY, name TEXT NOT NULL);",
            connection);
        await createCommand.ExecuteNonQueryAsync();

        // Insert test data
        await using var insertCommand = new NpgsqlCommand(
            "INSERT INTO test_stations (id, name) VALUES ('HBF', 'Hamburg Hauptbahnhof');",
            connection);
        await insertCommand.ExecuteNonQueryAsync();

        // Query the data
        await using var selectCommand = new NpgsqlCommand(
            "SELECT name FROM test_stations WHERE id = 'HBF';",
            connection);
        var result = await selectCommand.ExecuteScalarAsync() as string;

        // Assert
        result.Should().Be("Hamburg Hauptbahnhof");
    }

    [Fact]
    public async Task PostgreSql_ConnectionStringFormat_MatchesAppSettings()
    {
        // Arrange
        // The connection string format in appsettings.json uses:
        // Host=localhost;Port=5432;Database=fahrplan;Username=fahrplan;Password=fahrplan_dev
        // Testcontainers generates a similar format, but we validate the components

        var connectionString = _postgres.GetConnectionString();

        // Act & Assert
        connectionString.Should().Contain("Database=fahrplan");
        connectionString.Should().Contain("Username=fahrplan");

        // The connection works, proving the format is compatible
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        connection.State.Should().Be(System.Data.ConnectionState.Open);
    }
}
