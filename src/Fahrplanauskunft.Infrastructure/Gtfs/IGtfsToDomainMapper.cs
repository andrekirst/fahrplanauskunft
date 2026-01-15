using CSharpFunctionalExtensions;
using Fahrplanauskunft.Core.Entities;
using Fahrplanauskunft.Core.ValueObjects;

namespace Fahrplanauskunft.Infrastructure.Gtfs;

/// <summary>
/// Interface for mapping GTFS data transfer objects to domain entities.
/// Uses the Result pattern for graceful error handling during batch processing.
/// </summary>
/// <remarks>
/// This interface defines the contract for converting raw GTFS data (parsed from CSV files)
/// into strongly-typed domain entities with proper validation. Each mapping method returns
/// a Result&lt;T&gt; to allow graceful handling of invalid data without throwing exceptions,
/// enabling batch processing of thousands of records.
/// </remarks>
public interface IGtfsToDomainMapper
{
    /// <summary>
    /// Maps a GTFS stop to a domain Stop entity.
    /// </summary>
    /// <param name="gtfsStop">The GTFS stop data</param>
    /// <returns>Success with Stop entity, or failure with error message for invalid data</returns>
    /// <remarks>
    /// Validates required fields (stop_id, stop_name) and creates proper StopId and Coordinates value objects.
    /// </remarks>
    Result<Stop> MapStop(GtfsStop gtfsStop);

    /// <summary>
    /// Maps a GTFS route to a domain Route entity.
    /// </summary>
    /// <param name="gtfsRoute">The GTFS route data</param>
    /// <returns>Success with Route entity, or failure with error message for invalid data</returns>
    /// <remarks>
    /// Converts GTFS route_type to TransportMode enum. Validates that at least one of
    /// route_short_name or route_long_name is provided.
    /// </remarks>
    Result<Route> MapRoute(GtfsRoute gtfsRoute);

    /// <summary>
    /// Maps a GTFS trip to a domain Trip entity.
    /// </summary>
    /// <param name="gtfsTrip">The GTFS trip data</param>
    /// <returns>Success with Trip entity, or failure with error message for invalid data</returns>
    /// <remarks>
    /// Creates a Trip entity with optional headsign and short name.
    /// Note: Stop times are added separately via <see cref="MapStopTime"/>.
    /// </remarks>
    Result<Trip> MapTrip(GtfsTrip gtfsTrip);

    /// <summary>
    /// Maps a GTFS stop time to a domain StopTime entity.
    /// </summary>
    /// <param name="gtfsStopTime">The GTFS stop time data</param>
    /// <param name="stopEntity">The already-mapped Stop entity for this stop time</param>
    /// <returns>Success with StopTime entity, or failure with error message for invalid data</returns>
    /// <remarks>
    /// Parses GTFS time strings (HH:MM:SS format) which can exceed 24:00:00 for trips
    /// spanning past midnight (e.g., "25:30:00" represents 1:30 AM the next day = 1530 total minutes).
    /// Converts pickup_type and drop_off_type to boolean flags.
    /// </remarks>
    Result<StopTime> MapStopTime(GtfsStopTime gtfsStopTime, Stop stopEntity);

    /// <summary>
    /// Maps a GTFS transfer to a domain Footpath entity.
    /// </summary>
    /// <param name="gtfsTransfer">The GTFS transfer data</param>
    /// <param name="fromStop">The already-mapped origin Stop entity</param>
    /// <param name="toStop">The already-mapped destination Stop entity</param>
    /// <returns>Success with Footpath entity, or failure with error message for invalid data</returns>
    /// <remarks>
    /// Converts min_transfer_time from seconds to a Duration value object.
    /// Only transfers of type 2 (requires minimum time) produce meaningful Footpath entities.
    /// Returns failure for transfer_type 3 (transfers not possible).
    /// </remarks>
    Result<Footpath> MapTransfer(GtfsTransfer gtfsTransfer, Stop fromStop, Stop toStop);

    /// <summary>
    /// Maps a GTFS route_type integer to a TransportMode enum value.
    /// </summary>
    /// <param name="routeType">The GTFS route_type value (0-12 for standard types)</param>
    /// <returns>Success with TransportMode, or failure for unknown route types</returns>
    /// <remarks>
    /// Standard GTFS route types:
    /// 0 - Tram, 1 - Subway, 2 - Rail, 3 - Bus, 4 - Ferry,
    /// 5 - Cable tram, 6 - Aerial lift, 7 - Funicular, 11 - Trolleybus, 12 - Monorail.
    /// Extended route types (100-1700) are mapped to the closest standard type or Unknown.
    /// </remarks>
    Result<TransportMode> MapTransportMode(int routeType);

    /// <summary>
    /// Parses a GTFS time string to a TimeOfDay value object.
    /// </summary>
    /// <param name="timeString">Time in HH:MM:SS or H:MM:SS format (can exceed 24:00:00)</param>
    /// <returns>Success with TimeOfDay, or failure for invalid format</returns>
    /// <remarks>
    /// GTFS times can exceed 24 hours for trips spanning past midnight.
    /// For example, "25:30:00" represents 1:30 AM the next day with TotalMinutes = 1530.
    /// </remarks>
    Result<TimeOfDay> ParseGtfsTime(string timeString);
}
