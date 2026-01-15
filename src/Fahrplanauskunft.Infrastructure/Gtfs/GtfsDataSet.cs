namespace Fahrplanauskunft.Infrastructure.Gtfs;

/// <summary>
/// Represents a complete GTFS data set containing all parsed GTFS entities.
/// This is an aggregate container that holds collections of all GTFS model types
/// after parsing from CSV files.
/// See: https://gtfs.org/schedule/reference/
/// </summary>
/// <remarks>
/// This class serves as a unified container for GTFS data. It does not contain
/// any parsing logic - that responsibility belongs to a separate parser/reader class.
/// All collections are initialized to empty lists to avoid null reference issues.
/// </remarks>
public sealed class GtfsDataSet
{
    /// <summary>
    /// Collection of all stops/stations from stops.txt.
    /// </summary>
    public IList<GtfsStop> Stops { get; set; } = new List<GtfsStop>();

    /// <summary>
    /// Collection of all routes from routes.txt.
    /// </summary>
    public IList<GtfsRoute> Routes { get; set; } = new List<GtfsRoute>();

    /// <summary>
    /// Collection of all trips from trips.txt.
    /// </summary>
    public IList<GtfsTrip> Trips { get; set; } = new List<GtfsTrip>();

    /// <summary>
    /// Collection of all stop times from stop_times.txt.
    /// </summary>
    public IList<GtfsStopTime> StopTimes { get; set; } = new List<GtfsStopTime>();

    /// <summary>
    /// Collection of all calendar entries from calendar.txt.
    /// </summary>
    public IList<GtfsCalendar> Calendars { get; set; } = new List<GtfsCalendar>();

    /// <summary>
    /// Collection of all transfer rules from transfers.txt.
    /// This collection may be empty as transfers.txt is an optional GTFS file.
    /// </summary>
    public IList<GtfsTransfer> Transfers { get; set; } = new List<GtfsTransfer>();

    /// <summary>
    /// Gets the total number of entities in this data set.
    /// </summary>
    public int TotalEntityCount =>
        Stops.Count +
        Routes.Count +
        Trips.Count +
        StopTimes.Count +
        Calendars.Count +
        Transfers.Count;

    /// <summary>
    /// Indicates whether this data set contains any data.
    /// Short-circuits on first non-empty collection for efficiency.
    /// </summary>
    public bool IsEmpty =>
        Stops.Count == 0 &&
        Routes.Count == 0 &&
        Trips.Count == 0 &&
        StopTimes.Count == 0 &&
        Calendars.Count == 0 &&
        Transfers.Count == 0;

    /// <summary>
    /// Indicates whether this data set has the minimum required GTFS files.
    /// A valid GTFS feed requires at least stops, routes, trips, stop_times, and calendar.
    /// </summary>
    public bool HasRequiredData =>
        Stops.Count > 0 &&
        Routes.Count > 0 &&
        Trips.Count > 0 &&
        StopTimes.Count > 0 &&
        Calendars.Count > 0;
}
