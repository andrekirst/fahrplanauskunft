using CSharpFunctionalExtensions;
using Fahrplanauskunft.Core.Entities;
using Fahrplanauskunft.Core.Ports.Repositories;
using Fahrplanauskunft.Core.ValueObjects;
using Fahrplanauskunft.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fahrplanauskunft.Infrastructure.Repositories;

/// <summary>
/// Entity Framework Core implementation of <see cref="ITripRepository"/>.
/// Provides data access for Trip entities with optimized read operations using AsNoTracking,
/// bulk insert support, and comprehensive error handling with structured logging.
/// </summary>
public sealed partial class TripRepository : ITripRepository
{
    private readonly FahrplanDbContext _context;
    private readonly ILogger<TripRepository> _logger;

    #region LoggerMessage Definitions

    [LoggerMessage(Level = LogLevel.Debug, Message = "Retrieving trip with ID {TripId}")]
    private partial void LogRetrievingTrip(string tripId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Trip with ID {TripId} not found")]
    private partial void LogTripNotFound(string tripId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Successfully retrieved trip {TripId}: {Headsign}")]
    private partial void LogTripRetrieved(string tripId, string? headsign);

    [LoggerMessage(Level = LogLevel.Information, Message = "Operation cancelled while retrieving trip {TripId}")]
    private partial void LogGetByIdCancelled(string tripId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error retrieving trip with ID {TripId}")]
    private partial void LogGetByIdError(Exception ex, string tripId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Retrieving trips for route {RouteId}")]
    private partial void LogRetrievingTripsByRoute(string routeId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Successfully retrieved {TripCount} trips for route {RouteId}")]
    private partial void LogTripsByRouteRetrieved(int tripCount, string routeId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Operation cancelled while retrieving trips for route {RouteId}")]
    private partial void LogGetByRouteCancelled(string routeId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error retrieving trips for route {RouteId}")]
    private partial void LogGetByRouteError(Exception ex, string routeId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Retrieving trips serving stop {StopId}")]
    private partial void LogRetrievingTripsServingStop(string stopId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Successfully retrieved {TripCount} trips serving stop {StopId}")]
    private partial void LogTripsServingStopRetrieved(int tripCount, string stopId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Operation cancelled while retrieving trips serving stop {StopId}")]
    private partial void LogGetTripsServingStopCancelled(string stopId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error retrieving trips serving stop {StopId}")]
    private partial void LogGetTripsServingStopError(Exception ex, string stopId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Attempted to add null trips collection")]
    private partial void LogAddRangeNullCollection();

    [LoggerMessage(Level = LogLevel.Debug, Message = "Empty trips collection provided, nothing to add")]
    private partial void LogAddRangeEmptyCollection();

    [LoggerMessage(Level = LogLevel.Debug, Message = "Adding {TripCount} trips to database")]
    private partial void LogAddingTrips(int tripCount);

    [LoggerMessage(Level = LogLevel.Information, Message = "Successfully added {TripCount} trips to database")]
    private partial void LogTripsAdded(int tripCount);

    [LoggerMessage(Level = LogLevel.Information, Message = "Operation cancelled while adding trips")]
    private partial void LogAddRangeCancelled();

    [LoggerMessage(Level = LogLevel.Error, Message = "Database update error while adding trips")]
    private partial void LogAddRangeDbError(Exception ex);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error adding trips to database")]
    private partial void LogAddRangeError(Exception ex);

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="TripRepository"/> class.
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="logger">The logger for structured logging</param>
    public TripRepository(FahrplanDbContext context, ILogger<TripRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<Result<Trip>> GetByIdAsync(TripId id, CancellationToken cancellationToken = default)
    {
        try
        {
            LogRetrievingTrip(id.Value);

            var trip = await _context.Trips
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

            if (trip is null)
            {
                LogTripNotFound(id.Value);
                return Result.Failure<Trip>($"Trip with ID '{id.Value}' was not found.");
            }

            LogTripRetrieved(id.Value, trip.Headsign);
            return Result.Success(trip);
        }
        catch (OperationCanceledException)
        {
            LogGetByIdCancelled(id.Value);
            throw;
        }
        catch (Exception ex)
        {
            LogGetByIdError(ex, id.Value);
            return Result.Failure<Trip>($"An error occurred while retrieving trip '{id.Value}': {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<Trip>>> GetByRouteAsync(RouteId routeId, CancellationToken cancellationToken = default)
    {
        try
        {
            LogRetrievingTripsByRoute(routeId.Value);

            // Query trips using the shadow property RouteId
            var trips = await _context.Trips
                .AsNoTracking()
                .Where(t => EF.Property<string?>(t, "RouteId") == routeId.Value)
                .ToListAsync(cancellationToken);

            LogTripsByRouteRetrieved(trips.Count, routeId.Value);
            return Result.Success<IReadOnlyList<Trip>>(trips);
        }
        catch (OperationCanceledException)
        {
            LogGetByRouteCancelled(routeId.Value);
            throw;
        }
        catch (Exception ex)
        {
            LogGetByRouteError(ex, routeId.Value);
            return Result.Failure<IReadOnlyList<Trip>>($"An error occurred while retrieving trips for route '{routeId.Value}': {ex.Message}");
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// Performance: Uses two-query pattern (1. fetch TripIds, 2. fetch Trips) because EF Core's
    /// value converters prevent single-query translation with joins on value objects.
    /// The first query returns only string IDs (minimal data), and the second uses an optimized
    /// IN clause. The IX_StopTimes_StopId index ensures the first query is fast.
    /// For very large result sets (>10K trips), consider using raw SQL or stored procedures.
    /// </remarks>
    public async Task<Result<IReadOnlyList<Trip>>> GetTripsServingStopAsync(StopId stopId, CancellationToken cancellationToken = default)
    {
        try
        {
            LogRetrievingTripsServingStop(stopId.Value);

            // Step 1: Get distinct TripIds from StopTimes (uses IX_StopTimes_StopId index)
            // Returns only string IDs to minimize data transfer
            var tripIdStrings = await _context.StopTimes
                .AsNoTracking()
                .Where(st => EF.Property<string>(st, "StopId") == stopId.Value)
                .Select(st => EF.Property<string>(st, "TripId"))
                .Distinct()
                .ToListAsync(cancellationToken);

            // Early return if no trips serve this stop (avoids unnecessary second query)
            if (tripIdStrings.Count == 0)
            {
                LogTripsServingStopRetrieved(0, stopId.Value);
                return Result.Success<IReadOnlyList<Trip>>(Array.Empty<Trip>());
            }

            // Step 2: Fetch trips using IN clause (EF Core translates via value converter)
            var tripIds = tripIdStrings.Select(TripId.From).ToList();
            var trips = await _context.Trips
                .AsNoTracking()
                .Where(t => tripIds.Contains(t.Id))
                .ToListAsync(cancellationToken);

            LogTripsServingStopRetrieved(trips.Count, stopId.Value);
            return Result.Success<IReadOnlyList<Trip>>(trips);
        }
        catch (OperationCanceledException)
        {
            LogGetTripsServingStopCancelled(stopId.Value);
            throw;
        }
        catch (Exception ex)
        {
            LogGetTripsServingStopError(ex, stopId.Value);
            return Result.Failure<IReadOnlyList<Trip>>($"An error occurred while retrieving trips serving stop '{stopId.Value}': {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result> AddRangeAsync(IEnumerable<Trip> trips, CancellationToken cancellationToken = default)
    {
        try
        {
            if (trips is null)
            {
                LogAddRangeNullCollection();
                return Result.Failure("Trips collection cannot be null.");
            }

            var tripsList = trips.ToList();

            if (tripsList.Count == 0)
            {
                LogAddRangeEmptyCollection();
                return Result.Success();
            }

            LogAddingTrips(tripsList.Count);

            // Bulk insert using AddRange
            await _context.Trips.AddRangeAsync(tripsList, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            LogTripsAdded(tripsList.Count);
            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            LogAddRangeCancelled();
            throw;
        }
        catch (DbUpdateException ex)
        {
            LogAddRangeDbError(ex);
            return Result.Failure($"A database error occurred while adding trips: {ex.InnerException?.Message ?? ex.Message}");
        }
        catch (Exception ex)
        {
            LogAddRangeError(ex);
            return Result.Failure($"An error occurred while adding trips: {ex.Message}");
        }
    }
}
