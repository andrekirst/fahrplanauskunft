using CSharpFunctionalExtensions;
using Fahrplanauskunft.Core.Entities;
using Fahrplanauskunft.Core.Ports.Repositories;
using Fahrplanauskunft.Core.ValueObjects;
using Fahrplanauskunft.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fahrplanauskunft.Infrastructure.Repositories;

/// <summary>
/// Entity Framework Core implementation of <see cref="IFootpathRepository"/>.
/// Provides data access for Footpath entities with optimized read operations using AsNoTracking,
/// bulk insert support, and comprehensive error handling with structured logging.
/// </summary>
public sealed partial class FootpathRepository : IFootpathRepository
{
    private readonly FahrplanDbContext _context;
    private readonly ILogger<FootpathRepository> _logger;

    #region LoggerMessage Definitions

    [LoggerMessage(Level = LogLevel.Debug, Message = "Retrieving footpaths from stop {StopId}")]
    private partial void LogRetrievingFootpathsFromStop(string stopId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Retrieved {FootpathCount} footpaths from stop {StopId}")]
    private partial void LogFootpathsFromStopRetrieved(int footpathCount, string stopId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Operation cancelled while retrieving footpaths from stop {StopId}")]
    private partial void LogGetFromStopCancelled(string stopId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error retrieving footpaths from stop {StopId}")]
    private partial void LogGetFromStopError(Exception ex, string stopId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Retrieving all footpaths")]
    private partial void LogRetrievingAllFootpaths();

    [LoggerMessage(Level = LogLevel.Information, Message = "Successfully retrieved {FootpathCount} footpaths")]
    private partial void LogAllFootpathsRetrieved(int footpathCount);

    [LoggerMessage(Level = LogLevel.Information, Message = "Operation cancelled while retrieving all footpaths")]
    private partial void LogGetAllCancelled();

    [LoggerMessage(Level = LogLevel.Error, Message = "Error retrieving all footpaths")]
    private partial void LogGetAllError(Exception ex);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Attempted to add null footpaths collection")]
    private partial void LogAddRangeNullCollection();

    [LoggerMessage(Level = LogLevel.Debug, Message = "Empty footpaths collection provided, nothing to add")]
    private partial void LogAddRangeEmptyCollection();

    [LoggerMessage(Level = LogLevel.Debug, Message = "Adding {FootpathCount} footpaths to database")]
    private partial void LogAddingFootpaths(int footpathCount);

    [LoggerMessage(Level = LogLevel.Information, Message = "Successfully added {FootpathCount} footpaths to database")]
    private partial void LogFootpathsAdded(int footpathCount);

    [LoggerMessage(Level = LogLevel.Information, Message = "Operation cancelled while adding footpaths")]
    private partial void LogAddRangeCancelled();

    [LoggerMessage(Level = LogLevel.Error, Message = "Database update error while adding footpaths")]
    private partial void LogAddRangeDbError(Exception ex);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error adding footpaths to database")]
    private partial void LogAddRangeError(Exception ex);

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="FootpathRepository"/> class.
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="logger">The logger for structured logging</param>
    public FootpathRepository(FahrplanDbContext context, ILogger<FootpathRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<Footpath>>> GetFromStopAsync(StopId stopId, CancellationToken cancellationToken = default)
    {
        try
        {
            LogRetrievingFootpathsFromStop(stopId.Value);

            // Query using the shadow property FromStopId since navigation properties From/To are ignored in config
            var footpaths = await _context.Footpaths
                .AsNoTracking()
                .Where(f => EF.Property<string>(f, "FromStopId") == stopId.Value)
                .ToListAsync(cancellationToken);

            LogFootpathsFromStopRetrieved(footpaths.Count, stopId.Value);
            return Result.Success<IReadOnlyList<Footpath>>(footpaths);
        }
        catch (OperationCanceledException)
        {
            LogGetFromStopCancelled(stopId.Value);
            throw;
        }
        catch (Exception ex)
        {
            LogGetFromStopError(ex, stopId.Value);
            return Result.Failure<IReadOnlyList<Footpath>>($"An error occurred while retrieving footpaths from stop '{stopId.Value}': {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<Footpath>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            LogRetrievingAllFootpaths();

            // Note: Navigation properties From/To are ignored in config, so Include() has no effect
            // The footpaths are returned without populated navigation properties
            var footpaths = await _context.Footpaths
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            LogAllFootpathsRetrieved(footpaths.Count);
            return Result.Success<IReadOnlyList<Footpath>>(footpaths);
        }
        catch (OperationCanceledException)
        {
            LogGetAllCancelled();
            throw;
        }
        catch (Exception ex)
        {
            LogGetAllError(ex);
            return Result.Failure<IReadOnlyList<Footpath>>($"An error occurred while retrieving footpaths: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result> AddRangeAsync(IEnumerable<Footpath> footpaths, CancellationToken cancellationToken = default)
    {
        try
        {
            if (footpaths is null)
            {
                LogAddRangeNullCollection();
                return Result.Failure("Footpaths collection cannot be null.");
            }

            var footpathsList = footpaths.ToList();

            if (footpathsList.Count == 0)
            {
                LogAddRangeEmptyCollection();
                return Result.Success();
            }

            LogAddingFootpaths(footpathsList.Count);

            // Bulk insert using AddRange
            await _context.Footpaths.AddRangeAsync(footpathsList, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            LogFootpathsAdded(footpathsList.Count);
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
            return Result.Failure($"A database error occurred while adding footpaths: {ex.InnerException?.Message ?? ex.Message}");
        }
        catch (Exception ex)
        {
            LogAddRangeError(ex);
            return Result.Failure($"An error occurred while adding footpaths: {ex.Message}");
        }
    }
}
