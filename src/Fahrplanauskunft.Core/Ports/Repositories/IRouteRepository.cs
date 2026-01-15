using CSharpFunctionalExtensions;
using Fahrplanauskunft.Core.Entities;
using Fahrplanauskunft.Core.ValueObjects;

namespace Fahrplanauskunft.Core.Ports.Repositories;

/// <summary>
/// Port interface for Route entity persistence.
/// Implementations (adapters) are in the Infrastructure layer.
/// All methods return Result types for functional error handling.
/// </summary>
public interface IRouteRepository
{
    /// <summary>
    /// Retrieves a route by its unique identifier.
    /// </summary>
    /// <param name="id">The route ID to search for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the route if found, or a failure message if not found or an error occurs</returns>
    Task<Result<Route>> GetByIdAsync(RouteId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all routes in the system.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing all routes, or a failure message if an error occurs</returns>
    Task<Result<IReadOnlyList<Route>>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk inserts a collection of routes.
    /// </summary>
    /// <param name="routes">The routes to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure with error details</returns>
    Task<Result> AddRangeAsync(IEnumerable<Route> routes, CancellationToken cancellationToken = default);
}
