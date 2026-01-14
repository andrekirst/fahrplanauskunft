using Fahrplanauskunft.Core.Entities;
using Fahrplanauskunft.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fahrplanauskunft.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="StopTime"/> entity.
/// Configures the composite primary key (TripId + Sequence), value conversions for
/// TimeOfDay and StopSequence value objects, relationships, and indexes.
/// </summary>
public class StopTimeConfiguration : IEntityTypeConfiguration<StopTime>
{
    /// <summary>
    /// Configures the StopTime entity mapping.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<StopTime> builder)
    {
        // Table configuration
        builder.ToTable("StopTimes");

        // Shadow property for TripId foreign key (referenced by TripConfiguration)
        builder.Property<string>("TripId")
            .HasMaxLength(50)
            .IsRequired();

        // Shadow property for StopId foreign key
        builder.Property<string>("StopId")
            .HasMaxLength(50)
            .IsRequired();

        // Composite primary key: TripId + Sequence
        // This ensures each stop sequence within a trip is unique
        builder.HasKey("TripId", "Sequence");

        // Sequence property with StopSequence value conversion
        builder.Property(st => st.Sequence)
            .HasConversion(
                sequence => sequence.Value,
                value => StopSequence.From(value))
            .IsRequired();

        // Arrival property with TimeOfDay value conversion
        builder.Property(st => st.Arrival)
            .HasConversion(
                time => time.TotalMinutes,
                minutes => TimeOfDay.FromTotalMinutes(minutes))
            .HasColumnName("ArrivalMinutes")
            .IsRequired();

        // Departure property with TimeOfDay value conversion
        builder.Property(st => st.Departure)
            .HasConversion(
                time => time.TotalMinutes,
                minutes => TimeOfDay.FromTotalMinutes(minutes))
            .HasColumnName("DepartureMinutes")
            .IsRequired();

        // Boolean properties
        builder.Property(st => st.PickupAllowed)
            .HasDefaultValue(true);

        builder.Property(st => st.DropOffAllowed)
            .HasDefaultValue(true);

        // Ignore computed property (calculated from Arrival and Departure)
        builder.Ignore(st => st.DwellTime);

        // Ignore navigation property
        // Note: EF Core cannot properly map relationships when the principal key (StopId)
        // has a value converter and the foreign key is a shadow property (string).
        // The StopId shadow property stores the string value directly.
        builder.Ignore(st => st.Stop);

        // Indexes for frequently queried columns
        // Note: TripId index is not needed since it's the leading column of the composite primary key
        builder.HasIndex("StopId")
            .HasDatabaseName("IX_StopTimes_StopId");

        // Index for time-based queries (finding stop times by arrival/departure)
        builder.HasIndex(st => st.Arrival)
            .HasDatabaseName("IX_StopTimes_Arrival");

        builder.HasIndex(st => st.Departure)
            .HasDatabaseName("IX_StopTimes_Departure");

        // Composite index for common query pattern: finding stops at a specific stop within a time range
        builder.HasIndex("StopId", "Arrival")
            .HasDatabaseName("IX_StopTimes_StopId_Arrival");

        // Composite index for departure board queries: finding departures from a stop
        builder.HasIndex("StopId", "Departure")
            .HasDatabaseName("IX_StopTimes_StopId_Departure");
    }
}
