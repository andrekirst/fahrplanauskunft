using Fahrplanauskunft.Core.Entities;
using Fahrplanauskunft.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fahrplanauskunft.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="Route"/> entity.
/// Configures the primary key, value conversions for RouteId and TransportMode,
/// relationships with Trip entities, and indexes.
/// </summary>
public class RouteConfiguration : IEntityTypeConfiguration<Route>
{
    /// <summary>
    /// Configures the Route entity mapping.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Route> builder)
    {
        // Table configuration
        builder.ToTable("Routes");

        // Primary key with RouteId value conversion
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasConversion(
                id => id.Value,
                value => RouteId.From(value))
            .HasMaxLength(50)
            .IsRequired();

        // ShortName property configuration
        builder.Property(r => r.ShortName)
            .HasMaxLength(50)
            .IsRequired();

        // LongName property configuration
        builder.Property(r => r.LongName)
            .HasMaxLength(200);

        // TransportMode enum conversion (stored as integer)
        builder.Property(r => r.Mode)
            .HasConversion<int>()
            .IsRequired();

        // Color property configuration (hex format e.g., "FF0000")
        builder.Property(r => r.Color)
            .HasMaxLength(6);

        // TextColor property configuration (hex format e.g., "FFFFFF")
        builder.Property(r => r.TextColor)
            .HasMaxLength(6);

        // Ignore computed and navigation properties
        // Note: Trips relationship is managed via shadow property on Trip entity
        // We ignore the navigation property here because EF Core cannot properly
        // map the relationship when the principal key has a value converter
        builder.Ignore(r => r.TripCount);
        builder.Ignore(r => r.DisplayName);
        builder.Ignore(r => r.RouteLabel);
        builder.Ignore(r => r.Trips);

        // Indexes for frequently queried columns
        builder.HasIndex(r => r.ShortName)
            .HasDatabaseName("IX_Routes_ShortName");

        builder.HasIndex(r => r.Mode)
            .HasDatabaseName("IX_Routes_Mode");
    }
}
