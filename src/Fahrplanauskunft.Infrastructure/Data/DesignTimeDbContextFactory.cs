using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Fahrplanauskunft.Infrastructure.Data;

/// <summary>
/// Design-time factory for creating FahrplanDbContext instances.
/// Used by EF Core tools for migrations and other design-time operations.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<FahrplanDbContext>
{
    /// <summary>
    /// Creates a new instance of FahrplanDbContext for design-time operations.
    /// </summary>
    /// <param name="args">Arguments passed by the design-time tools.</param>
    /// <returns>A configured FahrplanDbContext instance.</returns>
    public FahrplanDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FahrplanDbContext>();

        // Use environment variable or default connection string for design-time operations.
        // Default matches docker-compose.yml configuration for local development.
        var connectionString = Environment.GetEnvironmentVariable("FAHRPLAN_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=fahrplan;Username=fahrplan;Password=fahrplan_dev";

        optionsBuilder.UseNpgsql(connectionString);

        return new FahrplanDbContext(optionsBuilder.Options);
    }
}
