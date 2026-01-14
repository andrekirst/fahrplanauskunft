using Fahrplanauskunft.Core.Entities;
using Fahrplanauskunft.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fahrplanauskunft.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="Trip"/> entity.
/// Configures the primary key, value conversions for TripId,
/// relationships with Route and StopTime entities, and indexes.
/// </summary>
public class TripConfiguration : IEntityTypeConfiguration<Trip>
{
    /// <summary>
    /// Configures the Trip entity mapping.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Trip> builder)
    {
        // Table configuration
        builder.ToTable("Trips");

        // Primary key with TripId value conversion
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasConversion(
                id => id.Value,
                value => TripId.From(value))
            .HasMaxLength(50)
            .IsRequired();

        // Headsign property configuration
        builder.Property(t => t.Headsign)
            .HasMaxLength(200);

        // ShortName property configuration
        builder.Property(t => t.ShortName)
            .HasMaxLength(50);

        // Shadow property for RouteId foreign key (optional - trips can exist without a route)
        builder.Property<string?>("RouteId")
            .HasMaxLength(50)
            .IsRequired(false);

        // Ignore computed and navigation properties that shouldn't be persisted
        // Note: StopTimes relationship cannot be configured via EF Core because TripId
        // uses a value converter and the shadow foreign key on StopTime is a string.
        // The relationship is managed via the TripId shadow property directly.
        builder.Ignore(t => t.StopTimes);
        builder.Ignore(t => t.FirstStopTime);
        builder.Ignore(t => t.LastStopTime);
        builder.Ignore(t => t.StopCount);
        builder.Ignore(t => t.Origin);
        builder.Ignore(t => t.Destination);
        builder.Ignore(t => t.DepartureTime);
        builder.Ignore(t => t.ArrivalTime);
        builder.Ignore(t => t.TotalDuration);

        // Indexes for frequently queried columns
        builder.HasIndex(t => t.Headsign)
            .HasDatabaseName("IX_Trips_Headsign");

        builder.HasIndex("RouteId")
            .HasDatabaseName("IX_Trips_RouteId");
    }
}
