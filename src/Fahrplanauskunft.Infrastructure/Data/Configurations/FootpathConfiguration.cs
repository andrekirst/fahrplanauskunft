using Fahrplanauskunft.Core.Entities;
using Fahrplanauskunft.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fahrplanauskunft.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="Footpath"/> entity.
/// Configures the composite primary key (FromStopId + ToStopId), value conversions for
/// Duration value object, relationships to stops, and indexes.
/// </summary>
public class FootpathConfiguration : IEntityTypeConfiguration<Footpath>
{
    /// <summary>
    /// Configures the Footpath entity mapping.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Footpath> builder)
    {
        // Table configuration
        builder.ToTable("Footpaths");

        // Shadow property for FromStopId foreign key
        builder.Property<string>("FromStopId")
            .HasMaxLength(50)
            .IsRequired();

        // Shadow property for ToStopId foreign key
        builder.Property<string>("ToStopId")
            .HasMaxLength(50)
            .IsRequired();

        // Composite primary key: FromStopId + ToStopId
        // This ensures each footpath between two stops is unique
        builder.HasKey("FromStopId", "ToStopId");

        // Duration property with Duration value conversion (stored as minutes)
        builder.Property(f => f.Duration)
            .HasConversion(
                duration => duration.TotalMinutes,
                minutes => Duration.FromMinutes(minutes))
            .HasColumnName("DurationMinutes")
            .IsRequired();

        // WheelchairAccessible property
        builder.Property(f => f.WheelchairAccessible)
            .HasDefaultValue(false);

        // DistanceMeters property (nullable)
        builder.Property(f => f.DistanceMeters)
            .IsRequired(false);

        // Ignore navigation properties
        // Note: EF Core cannot properly map relationships when the principal key (StopId)
        // has a value converter and the foreign key is a shadow property (string).
        // The FromStopId and ToStopId shadow properties store the string values directly.
        builder.Ignore(f => f.From);
        builder.Ignore(f => f.To);

        // Indexes for frequently queried columns
        builder.HasIndex("FromStopId")
            .HasDatabaseName("IX_Footpaths_FromStopId");

        builder.HasIndex("ToStopId")
            .HasDatabaseName("IX_Footpaths_ToStopId");

        // Note: WheelchairAccessible index removed - boolean columns have ~50% selectivity
        // which rarely helps the query optimizer. Use filtered indexes if your database
        // supports them (e.g., PostgreSQL: CREATE INDEX ... WHERE wheelchair_accessible = true)

        // Composite index for reverse lookup queries (finding footpaths to a stop)
        builder.HasIndex("ToStopId", "FromStopId")
            .HasDatabaseName("IX_Footpaths_ToStopId_FromStopId");
    }
}
