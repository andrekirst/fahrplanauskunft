using CsvHelper.Configuration.Attributes;

namespace Fahrplanauskunft.Infrastructure.Gtfs;

/// <summary>
/// Represents a transfer rule from the GTFS transfers.txt file.
/// This is a plain POCO for CsvHelper parsing - no validation or domain logic.
/// See: https://gtfs.org/schedule/reference/#transferstxt
/// </summary>
/// <remarks>
/// The transfers.txt file is optional and defines rules for making connections
/// at transfer points between routes.
/// </remarks>
public sealed class GtfsTransfer
{
    /// <summary>
    /// Required. Identifies a stop or station where a connection between routes begins.
    /// References stops.txt stop_id.
    /// </summary>
    [Name("from_stop_id")]
    public string FromStopId { get; set; } = string.Empty;

    /// <summary>
    /// Required. Identifies a stop or station where a connection between routes ends.
    /// References stops.txt stop_id.
    /// </summary>
    [Name("to_stop_id")]
    public string ToStopId { get; set; } = string.Empty;

    /// <summary>
    /// Required. Indicates the type of connection for the specified pair:
    /// 0 - Recommended transfer point between routes.
    /// 1 - Timed transfer point (departing vehicle expected to wait for arriving vehicle).
    /// 2 - Transfer requires a minimum amount of time (min_transfer_time specifies the time).
    /// 3 - Transfers are not possible between routes at this location.
    /// 4 - Passengers can stay onboard the same vehicle (in-seat transfer).
    /// 5 - In-seat transfers are not allowed between sequential trips (must alight and reboard).
    /// </summary>
    [Name("transfer_type")]
    public int TransferType { get; set; }

    /// <summary>
    /// Optional. Amount of time, in seconds, that must be available to permit a transfer
    /// between routes at the specified stops. The min_transfer_time should be sufficient
    /// to permit a typical rider to move between the two stops, including buffer time to
    /// allow for schedule variance on each route.
    /// </summary>
    [Name("min_transfer_time")]
    public int? MinTransferTime { get; set; }

    /// <summary>
    /// Optional. Identifies a route where a connection begins.
    /// References routes.txt route_id.
    /// If from_route_id is defined, the transfer will apply to the arriving trip on the route.
    /// </summary>
    [Name("from_route_id")]
    public string? FromRouteId { get; set; }

    /// <summary>
    /// Optional. Identifies a route where a connection ends.
    /// References routes.txt route_id.
    /// If to_route_id is defined, the transfer will apply to the departing trip on the route.
    /// </summary>
    [Name("to_route_id")]
    public string? ToRouteId { get; set; }

    /// <summary>
    /// Optional. Identifies a trip where a connection between routes begins.
    /// References trips.txt trip_id.
    /// If from_trip_id is defined, the transfer will apply to the arriving trip.
    /// </summary>
    [Name("from_trip_id")]
    public string? FromTripId { get; set; }

    /// <summary>
    /// Optional. Identifies a trip where a connection between routes ends.
    /// References trips.txt trip_id.
    /// If to_trip_id is defined, the transfer will apply to the departing trip.
    /// </summary>
    [Name("to_trip_id")]
    public string? ToTripId { get; set; }
}
