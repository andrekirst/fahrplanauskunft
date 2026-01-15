using CSharpFunctionalExtensions;
using Fahrplanauskunft.Core.Entities;
using Fahrplanauskunft.Core.Ports.Repositories;
using Fahrplanauskunft.Core.ValueObjects;
using Fahrplanauskunft.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fahrplanauskunft.Infrastructure.Repositories;

/// <summary>
/// Entity Framework Core implementation of <see cref="IStopRepository"/>.
/// Provides data access for Stop entities with optimized read operations using AsNoTracking,
/// bulk insert support, and comprehensive error handling with structured logging.
/// </summary>
public sealed partial class StopRepository : IStopRepository
{
    private const int BatchSize = 1000; // Optimal batch size for EF Core bulk inserts
    private readonly FahrplanDbContext _context;
    private readonly ILogger<StopRepository> _logger;

    #region LoggerMessage Definitions

    [LoggerMessage(Level = LogLevel.Debug, Message = "Retrieving stop with ID {StopId}")]
    private partial void LogRetrievingStop(string stopId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Stop with ID {StopId} not found")]
    private partial void LogStopNotFound(string stopId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Successfully retrieved stop {StopId}: {StopName}")]
    private partial void LogStopRetrieved(string stopId, string stopName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Operation cancelled while retrieving stop {StopId}")]
    private partial void LogGetByIdCancelled(string stopId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error retrieving stop with ID {StopId}")]
    private partial void LogGetByIdError(Exception ex, string stopId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Retrieving all stops")]
    private partial void LogRetrievingAllStops();

    [LoggerMessage(Level = LogLevel.Information, Message = "Successfully retrieved {StopCount} stops")]
    private partial void LogAllStopsRetrieved(int stopCount);

    [LoggerMessage(Level = LogLevel.Information, Message = "Operation cancelled while retrieving all stops")]
    private partial void LogGetAllCancelled();

    [LoggerMessage(Level = LogLevel.Error, Message = "Error retrieving all stops")]
    private partial void LogGetAllError(Exception ex);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Search term is null or empty")]
    private partial void LogSearchTermEmpty();

    [LoggerMessage(Level = LogLevel.Debug, Message = "Searching stops with term: {SearchTerm}")]
    private partial void LogSearchingStops(string searchTerm);

    [LoggerMessage(Level = LogLevel.Information, Message = "Search for '{SearchTerm}' returned {ResultCount} stops")]
    private partial void LogSearchCompleted(string searchTerm, int resultCount);

    [LoggerMessage(Level = LogLevel.Information, Message = "Operation cancelled while searching stops with term: {SearchTerm}")]
    private partial void LogSearchCancelled(string searchTerm);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error searching stops with term: {SearchTerm}")]
    private partial void LogSearchError(Exception ex, string searchTerm);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Attempted to add null stops collection")]
    private partial void LogAddRangeNullCollection();

    [LoggerMessage(Level = LogLevel.Debug, Message = "Empty stops collection provided, nothing to add")]
    private partial void LogAddRangeEmptyCollection();

    [LoggerMessage(Level = LogLevel.Debug, Message = "Adding {StopCount} stops to database")]
    private partial void LogAddingStops(int stopCount);

    [LoggerMessage(Level = LogLevel.Information, Message = "Successfully added {StopCount} stops to database")]
    private partial void LogStopsAdded(int stopCount);

    [LoggerMessage(Level = LogLevel.Information, Message = "Operation cancelled while adding stops")]
    private partial void LogAddRangeCancelled();

    [LoggerMessage(Level = LogLevel.Error, Message = "Database update error while adding stops")]
    private partial void LogAddRangeDbError(Exception ex);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error adding stops to database")]
    private partial void LogAddRangeError(Exception ex);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Processing batch {BatchNumber} of {TotalBatches} ({BatchCount} stops)")]
    private partial void LogProcessingBatch(int batchNumber, int totalBatches, int batchCount);

    [LoggerMessage(Level = LogLevel.Information, Message = "Successfully added {StopCount} stops in {BatchCount} batches")]
    private partial void LogStopsAddedInBatches(int stopCount, int batchCount);

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="StopRepository"/> class.
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="logger">The logger for structured logging</param>
    public StopRepository(FahrplanDbContext context, ILogger<StopRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<Result<Stop>> GetByIdAsync(StopId id, CancellationToken cancellationToken = default)
    {
        try
        {
            LogRetrievingStop(id.Value);

            var stop = await _context.Stops
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

            if (stop is null)
            {
                LogStopNotFound(id.Value);
                return Result.Failure<Stop>($"Stop with ID '{id.Value}' was not found.");
            }

            LogStopRetrieved(id.Value, stop.Name);
            return Result.Success(stop);
        }
        catch (OperationCanceledException)
        {
            LogGetByIdCancelled(id.Value);
            throw;
        }
        catch (Exception ex)
        {
            LogGetByIdError(ex, id.Value);
            return Result.Failure<Stop>($"An error occurred while retrieving stop '{id.Value}': {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<Stop>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            LogRetrievingAllStops();

            var stops = await _context.Stops
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            LogAllStopsRetrieved(stops.Count);
            return Result.Success<IReadOnlyList<Stop>>(stops);
        }
        catch (OperationCanceledException)
        {
            LogGetAllCancelled();
            throw;
        }
        catch (Exception ex)
        {
            LogGetAllError(ex);
            return Result.Failure<IReadOnlyList<Stop>>($"An error occurred while retrieving stops: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<Stop>>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                LogSearchTermEmpty();
                return Result.Failure<IReadOnlyList<Stop>>("Search term cannot be null or empty.");
            }

            LogSearchingStops(searchTerm);

            // Use EF.Functions.Like for case-insensitive pattern matching
            // The pattern includes wildcards for partial matching
            var pattern = $"%{EscapeLikePattern(searchTerm)}%";

            var stops = await _context.Stops
                .AsNoTracking()
                .Where(s => EF.Functions.Like(s.Name, pattern))
                .ToListAsync(cancellationToken);

            LogSearchCompleted(searchTerm, stops.Count);
            return Result.Success<IReadOnlyList<Stop>>(stops);
        }
        catch (OperationCanceledException)
        {
            LogSearchCancelled(searchTerm);
            throw;
        }
        catch (Exception ex)
        {
            LogSearchError(ex, searchTerm);
            return Result.Failure<IReadOnlyList<Stop>>($"An error occurred while searching stops: {ex.Message}");
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// Performance: Uses batch processing for large collections (>1000 items) to reduce memory pressure
    /// and improve throughput. Each batch is committed separately to avoid holding large transactions.
    /// </remarks>
    public async Task<Result> AddRangeAsync(IEnumerable<Stop> stops, CancellationToken cancellationToken = default)
    {
        try
        {
            if (stops is null)
            {
                LogAddRangeNullCollection();
                return Result.Failure("Stops collection cannot be null.");
            }

            var stopsList = stops.ToList();

            if (stopsList.Count == 0)
            {
                LogAddRangeEmptyCollection();
                return Result.Success();
            }

            LogAddingStops(stopsList.Count);

            // For small collections, use simple bulk insert
            if (stopsList.Count <= BatchSize)
            {
                await _context.Stops.AddRangeAsync(stopsList, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                LogStopsAdded(stopsList.Count);
                return Result.Success();
            }

            // OPTIMIZED: Batch processing for large collections
            // Reduces memory pressure and allows progress tracking
            var totalBatches = (int)Math.Ceiling((double)stopsList.Count / BatchSize);
            var processedCount = 0;

            for (var batchNumber = 1; batchNumber <= totalBatches; batchNumber++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var batch = stopsList
                    .Skip((batchNumber - 1) * BatchSize)
                    .Take(BatchSize)
                    .ToList();

                LogProcessingBatch(batchNumber, totalBatches, batch.Count);

                await _context.Stops.AddRangeAsync(batch, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                // Clear change tracker to reduce memory usage
                _context.ChangeTracker.Clear();

                processedCount += batch.Count;
            }

            LogStopsAddedInBatches(processedCount, totalBatches);
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
            return Result.Failure($"A database error occurred while adding stops: {ex.InnerException?.Message ?? ex.Message}");
        }
        catch (Exception ex)
        {
            LogAddRangeError(ex);
            return Result.Failure($"An error occurred while adding stops: {ex.Message}");
        }
    }

    /// <summary>
    /// Escapes special characters in a LIKE pattern to ensure they are treated as literals.
    /// </summary>
    /// <param name="input">The input string to escape</param>
    /// <returns>The escaped string safe for use in LIKE patterns</returns>
    private static string EscapeLikePattern(string input)
    {
        // Escape LIKE special characters: %, _, [, ]
        return input
            .Replace("[", "[[]")
            .Replace("%", "[%]")
            .Replace("_", "[_]");
    }
}
