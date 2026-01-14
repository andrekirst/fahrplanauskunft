using Fahrplanauskunft.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Fahrplanauskunft.Tests.Integration.EfCore;

/// <summary>
/// Provides a SQLite in-memory database for EF Core integration tests.
/// This fixture manages the database connection lifecycle and provides
/// fresh DbContext instances for each test.
/// </summary>
public class SqliteDbContextFixture : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<FahrplanDbContext> _options;

    public SqliteDbContextFixture()
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
    }

    /// <summary>
    /// Creates a new DbContext instance for testing.
    /// </summary>
    public FahrplanDbContext CreateContext()
    {
        return new FahrplanDbContext(_options);
    }

    public void Dispose()
    {
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}
