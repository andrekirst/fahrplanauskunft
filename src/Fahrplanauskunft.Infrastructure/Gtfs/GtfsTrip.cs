using CsvHelper.Configuration.Attributes;

namespace Fahrplanauskunft.Infrastructure.Gtfs;

/// <summary>
/// Represents a trip from the GTFS trips.txt file.
/// This is a plain POCO for CsvHelper parsing - no validation or domain logic.
/// See: https://gtfs.org/schedule/reference/#tripstxt
/// </summary>
public sealed class GtfsTrip
{
    /// <summary>
    /// Required. Identifies a route.
    /// References routes.txt route_id.
    /// </summary>
    [Name("route_id")]
    public string RouteId { get; set; } = string.Empty;

    /// <summary>
    /// Required. Identifies a set of dates when service is available for one or more routes.
    /// References calendar.txt or calendar_dates.txt service_id.
    /// </summary>
    [Name("service_id")]
    public string ServiceId { get; set; } = string.Empty;

    /// <summary>
    /// Required. Identifies a trip.
    /// </summary>
    [Name("trip_id")]
    public string TripId { get; set; } = string.Empty;

    /// <summary>
    /// Optional. Text that appears on signage identifying the trip's destination to riders.
    /// Should be used to distinguish between different patterns of service on the same route.
    /// </summary>
    [Name("trip_headsign")]
    public string? TripHeadsign { get; set; }

    /// <summary>
    /// Optional. Public facing text used to identify the trip to riders, for instance,
    /// to identify train numbers for commuter rail trips.
    /// </summary>
    [Name("trip_short_name")]
    public string? TripShortName { get; set; }

    /// <summary>
    /// Optional. Indicates the direction of travel for a trip.
    /// 0 - Travel in one direction (e.g., outbound travel).
    /// 1 - Travel in the opposite direction (e.g., inbound travel).
    /// </summary>
    [Name("direction_id")]
    public int? DirectionId { get; set; }

    /// <summary>
    /// Optional. Identifies the block to which the trip belongs.
    /// A block consists of a single trip or many sequential trips made using the same vehicle,
    /// defined by shared service days and block_id.
    /// </summary>
    [Name("block_id")]
    public string? BlockId { get; set; }

    /// <summary>
    /// Conditionally Required. Identifies a geospatial shape that describes the vehicle travel path for a trip.
    /// References shapes.txt shape_id.
    /// Required if the trip has continuous pickup or drop-off behavior defined.
    /// </summary>
    [Name("shape_id")]
    public string? ShapeId { get; set; }

    /// <summary>
    /// Optional. Indicates wheelchair accessibility:
    /// 0 (or empty) - No accessibility information for the trip.
    /// 1 - Vehicle can accommodate at least one rider in a wheelchair.
    /// 2 - No riders in wheelchairs can be accommodated on this trip.
    /// </summary>
    [Name("wheelchair_accessible")]
    public int? WheelchairAccessible { get; set; }

    /// <summary>
    /// Optional. Indicates whether bikes are allowed:
    /// 0 (or empty) - No bike information for the trip.
    /// 1 - Vehicle can accommodate at least one bicycle.
    /// 2 - No bicycles are allowed on this trip.
    /// </summary>
    [Name("bikes_allowed")]
    public int? BikesAllowed { get; set; }
}
