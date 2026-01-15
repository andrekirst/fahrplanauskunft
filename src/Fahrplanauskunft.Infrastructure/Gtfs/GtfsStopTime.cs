using CsvHelper.Configuration.Attributes;

namespace Fahrplanauskunft.Infrastructure.Gtfs;

/// <summary>
/// Represents a stop time from the GTFS stop_times.txt file.
/// This is a plain POCO for CsvHelper parsing - no validation or domain logic.
/// See: https://gtfs.org/schedule/reference/#stop_timestxt
/// </summary>
/// <remarks>
/// Times are stored as strings because GTFS allows times greater than 24:00:00
/// for trips that span past midnight (e.g., "25:30:00" represents 1:30 AM the next day).
/// </remarks>
public sealed class GtfsStopTime
{
    /// <summary>
    /// Required. Identifies a trip.
    /// References trips.txt trip_id.
    /// </summary>
    [Name("trip_id")]
    public string TripId { get; set; } = string.Empty;

    /// <summary>
    /// Conditionally Required. Arrival time at the stop (HH:MM:SS or H:MM:SS format).
    /// Required for the first and last stop in a trip. Optional for intermediate stops.
    /// Times can exceed 24:00:00 for trips that span past midnight.
    /// </summary>
    [Name("arrival_time")]
    public string? ArrivalTime { get; set; }

    /// <summary>
    /// Conditionally Required. Departure time from the stop (HH:MM:SS or H:MM:SS format).
    /// Required for the first and last stop in a trip. Optional for intermediate stops.
    /// Times can exceed 24:00:00 for trips that span past midnight.
    /// </summary>
    [Name("departure_time")]
    public string? DepartureTime { get; set; }

    /// <summary>
    /// Required. Identifies the serviced stop.
    /// References stops.txt stop_id.
    /// </summary>
    [Name("stop_id")]
    public string StopId { get; set; } = string.Empty;

    /// <summary>
    /// Required. Order of stops for a particular trip.
    /// Values must increase along the trip but do not need to be consecutive.
    /// </summary>
    [Name("stop_sequence")]
    public int StopSequence { get; set; }

    /// <summary>
    /// Optional. Text that appears on signage identifying the trip's destination to riders.
    /// Overrides trip_headsign when the headsign changes between stops.
    /// </summary>
    [Name("stop_headsign")]
    public string? StopHeadsign { get; set; }

    /// <summary>
    /// Optional. Indicates pickup method:
    /// 0 (or empty) - Regularly scheduled pickup.
    /// 1 - No pickup available.
    /// 2 - Must phone agency to arrange pickup.
    /// 3 - Must coordinate with driver to arrange pickup.
    /// </summary>
    [Name("pickup_type")]
    public int? PickupType { get; set; }

    /// <summary>
    /// Optional. Indicates drop-off method:
    /// 0 (or empty) - Regularly scheduled drop off.
    /// 1 - No drop off available.
    /// 2 - Must phone agency to arrange drop off.
    /// 3 - Must coordinate with driver to arrange drop off.
    /// </summary>
    [Name("drop_off_type")]
    public int? DropOffType { get; set; }

    /// <summary>
    /// Optional. Indicates continuous stopping pickup behavior:
    /// 0 - Continuous stopping pickup.
    /// 1 (or empty) - No continuous stopping pickup.
    /// 2 - Must phone agency to arrange continuous stopping pickup.
    /// 3 - Must coordinate with driver to arrange continuous stopping pickup.
    /// </summary>
    [Name("continuous_pickup")]
    public int? ContinuousPickup { get; set; }

    /// <summary>
    /// Optional. Indicates continuous stopping drop-off behavior:
    /// 0 - Continuous stopping drop off.
    /// 1 (or empty) - No continuous stopping drop off.
    /// 2 - Must phone agency to arrange continuous stopping drop off.
    /// 3 - Must coordinate with driver to arrange continuous stopping drop off.
    /// </summary>
    [Name("continuous_drop_off")]
    public int? ContinuousDropOff { get; set; }

    /// <summary>
    /// Optional. Actual distance traveled along the associated shape, from the first stop to the stop specified in this record.
    /// This field specifies how much of the shape to draw between any two stops during a trip.
    /// Must be in the same units used in shapes.txt. Values must increase along with stop_sequence.
    /// </summary>
    [Name("shape_dist_traveled")]
    public double? ShapeDistTraveled { get; set; }

    /// <summary>
    /// Optional. Indicates if arrival and departure times for a stop are strictly adhered to by the vehicle or if they are instead approximate and/or interpolated times:
    /// 0 - Times are considered approximate.
    /// 1 (or empty) - Times are considered exact.
    /// </summary>
    [Name("timepoint")]
    public int? Timepoint { get; set; }
}
