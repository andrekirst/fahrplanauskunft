using CSharpFunctionalExtensions;
using Fahrplanauskunft.Core.Entities;
using Fahrplanauskunft.Core.Ports.Repositories;
using Fahrplanauskunft.Core.ValueObjects;
using Fahrplanauskunft.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fahrplanauskunft.Infrastructure.Repositories;

/// <summary>
/// Entity Framework Core implementation of <see cref="IRouteRepository"/>.
/// Provides data access for Route entities with optimized read operations using AsNoTracking,
/// bulk insert support, and comprehensive error handling with structured logging.
/// </summary>
public sealed partial class RouteRepository : IRouteRepository
{
    private readonly FahrplanDbContext _context;
    private readonly ILogger<RouteRepository> _logger;

    #region LoggerMessage Definitions

    [LoggerMessage(Level = LogLevel.Debug, Message = "Retrieving route with ID {RouteId}")]
    private partial void LogRetrievingRoute(string routeId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Route with ID {RouteId} not found")]
    private partial void LogRouteNotFound(string routeId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Successfully retrieved route {RouteId}: {ShortName}")]
    private partial void LogRouteRetrieved(string routeId, string shortName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Operation cancelled while retrieving route {RouteId}")]
    private partial void LogGetByIdCancelled(string routeId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error retrieving route with ID {RouteId}")]
    private partial void LogGetByIdError(Exception ex, string routeId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Retrieving all routes")]
    private partial void LogRetrievingAllRoutes();

    [LoggerMessage(Level = LogLevel.Information, Message = "Successfully retrieved {RouteCount} routes")]
    private partial void LogAllRoutesRetrieved(int routeCount);

    [LoggerMessage(Level = LogLevel.Information, Message = "Operation cancelled while retrieving all routes")]
    private partial void LogGetAllCancelled();

    [LoggerMessage(Level = LogLevel.Error, Message = "Error retrieving all routes")]
    private partial void LogGetAllError(Exception ex);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Attempted to add null routes collection")]
    private partial void LogAddRangeNullCollection();

    [LoggerMessage(Level = LogLevel.Debug, Message = "Empty routes collection provided, nothing to add")]
    private partial void LogAddRangeEmptyCollection();

    [LoggerMessage(Level = LogLevel.Debug, Message = "Adding {RouteCount} routes to database")]
    private partial void LogAddingRoutes(int routeCount);

    [LoggerMessage(Level = LogLevel.Information, Message = "Successfully added {RouteCount} routes to database")]
    private partial void LogRoutesAdded(int routeCount);

    [LoggerMessage(Level = LogLevel.Information, Message = "Operation cancelled while adding routes")]
    private partial void LogAddRangeCancelled();

    [LoggerMessage(Level = LogLevel.Error, Message = "Database update error while adding routes")]
    private partial void LogAddRangeDbError(Exception ex);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error adding routes to database")]
    private partial void LogAddRangeError(Exception ex);

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="RouteRepository"/> class.
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="logger">The logger for structured logging</param>
    public RouteRepository(FahrplanDbContext context, ILogger<RouteRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<Result<Route>> GetByIdAsync(RouteId id, CancellationToken cancellationToken = default)
    {
        try
        {
            LogRetrievingRoute(id.Value);

            var route = await _context.Routes
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

            if (route is null)
            {
                LogRouteNotFound(id.Value);
                return Result.Failure<Route>($"Route with ID '{id.Value}' was not found.");
            }

            LogRouteRetrieved(id.Value, route.ShortName);
            return Result.Success(route);
        }
        catch (OperationCanceledException)
        {
            LogGetByIdCancelled(id.Value);
            throw;
        }
        catch (Exception ex)
        {
            LogGetByIdError(ex, id.Value);
            return Result.Failure<Route>($"An error occurred while retrieving route '{id.Value}': {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<Route>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            LogRetrievingAllRoutes();

            var routes = await _context.Routes
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            LogAllRoutesRetrieved(routes.Count);
            return Result.Success<IReadOnlyList<Route>>(routes);
        }
        catch (OperationCanceledException)
        {
            LogGetAllCancelled();
            throw;
        }
        catch (Exception ex)
        {
            LogGetAllError(ex);
            return Result.Failure<IReadOnlyList<Route>>($"An error occurred while retrieving routes: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result> AddRangeAsync(IEnumerable<Route> routes, CancellationToken cancellationToken = default)
    {
        try
        {
            if (routes is null)
            {
                LogAddRangeNullCollection();
                return Result.Failure("Routes collection cannot be null.");
            }

            var routesList = routes.ToList();

            if (routesList.Count == 0)
            {
                LogAddRangeEmptyCollection();
                return Result.Success();
            }

            LogAddingRoutes(routesList.Count);

            // Bulk insert using AddRange
            await _context.Routes.AddRangeAsync(routesList, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            LogRoutesAdded(routesList.Count);
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
            return Result.Failure($"A database error occurred while adding routes: {ex.InnerException?.Message ?? ex.Message}");
        }
        catch (Exception ex)
        {
            LogAddRangeError(ex);
            return Result.Failure($"An error occurred while adding routes: {ex.Message}");
        }
    }
}
