using Fahrplanauskunft.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fahrplanauskunft.Infrastructure.Data;

/// <summary>
/// The main database context for the Fahrplanauskunft application.
/// Provides access to transit schedule data including stops, routes, trips, stop times, and footpaths.
/// </summary>
public class FahrplanDbContext : DbContext
{
    /// <summary>
    /// Gets or sets the stops in the transit network.
    /// </summary>
    public DbSet<Stop> Stops => Set<Stop>();

    /// <summary>
    /// Gets or sets the routes in the transit network.
    /// </summary>
    public DbSet<Route> Routes => Set<Route>();

    /// <summary>
    /// Gets or sets the trips (specific journeys) in the transit network.
    /// </summary>
    public DbSet<Trip> Trips => Set<Trip>();

    /// <summary>
    /// Gets or sets the stop times (scheduled arrivals/departures) in the transit network.
    /// </summary>
    public DbSet<StopTime> StopTimes => Set<StopTime>();

    /// <summary>
    /// Gets or sets the footpaths (walking connections) between stops.
    /// </summary>
    public DbSet<Footpath> Footpaths => Set<Footpath>();

    /// <summary>
    /// Initializes a new instance of the <see cref="FahrplanDbContext"/> class.
    /// </summary>
    /// <param name="options">The options for configuring the context.</param>
    public FahrplanDbContext(DbContextOptions<FahrplanDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Configures the entity mappings and relationships for the model.
    /// </summary>
    /// <param name="modelBuilder">The builder used to construct the model.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from the current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FahrplanDbContext).Assembly);
    }
}
