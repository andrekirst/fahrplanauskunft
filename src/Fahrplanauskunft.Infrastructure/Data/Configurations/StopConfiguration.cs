using System.Text.Json;
using Fahrplanauskunft.Core.Entities;
using Fahrplanauskunft.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fahrplanauskunft.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="Stop"/> entity.
/// Configures the primary key, value conversions, owned types, and indexes.
/// </summary>
public class StopConfiguration : IEntityTypeConfiguration<Stop>
{
    // Cached serializer options for performance - avoids repeated reflection
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNamingPolicy = null, // Use exact property names
        WriteIndented = false        // Minimize storage size
    };

    /// <summary>
    /// Configures the Stop entity mapping.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Stop> builder)
    {
        // Table configuration
        builder.ToTable("Stops");

        // Primary key with StopId value conversion
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasConversion(
                id => id.Value,
                value => StopId.From(value))
            .HasMaxLength(50)
            .IsRequired();

        // Name property configuration
        builder.Property(s => s.Name)
            .HasMaxLength(200)
            .IsRequired();

        // Platform code property configuration
        builder.Property(s => s.PlatformCode)
            .HasMaxLength(20);

        // Wheelchair accessible property
        builder.Property(s => s.WheelchairAccessible)
            .HasDefaultValue(false);

        // Coordinates configuration using JSON serialization for the nullable struct
        // This stores the Location as a JSON string column, which naturally handles null values
        // Note: For PostgreSQL, consider adding .HasColumnType("jsonb") for better JSON support
        builder.Property(s => s.Location)
            .HasColumnName("Location")
            .HasConversion(
                coords => SerializeCoordinates(coords),
                json => ParseCoordinates(json));

        // Indexes for frequently queried columns
        builder.HasIndex(s => s.Name)
            .HasDatabaseName("IX_Stops_Name");

        // Note: WheelchairAccessible index removed - boolean columns have ~50% selectivity
        // which rarely helps the query optimizer. Consider filtered indexes for specific
        // queries that need accessible stops only.
    }

    /// <summary>
    /// Serializes Coordinates to a JSON string using cached options.
    /// </summary>
    private static string? SerializeCoordinates(Coordinates? coords)
    {
        if (!coords.HasValue)
            return null;

        // Use anonymous type with cached options for optimal performance
        return JsonSerializer.Serialize(
            new { lat = coords.Value.Latitude, lon = coords.Value.Longitude },
            s_jsonOptions);
    }

    /// <summary>
    /// Parses a JSON string to Coordinates.
    /// </summary>
    private static Coordinates? ParseCoordinates(string? json)
    {
        if (string.IsNullOrEmpty(json))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("lat", out var latElement) &&
                root.TryGetProperty("lon", out var lonElement))
            {
                var lat = latElement.GetDouble();
                var lon = lonElement.GetDouble();
                return Coordinates.From(lat, lon);
            }
        }
        catch (JsonException)
        {
            // Ignore JSON parsing errors
        }

        return null;
    }
}
