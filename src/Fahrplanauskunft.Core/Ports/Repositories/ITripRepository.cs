using CSharpFunctionalExtensions;
using Fahrplanauskunft.Core.Entities;
using Fahrplanauskunft.Core.ValueObjects;

namespace Fahrplanauskunft.Core.Ports.Repositories;

/// <summary>
/// Port interface for Trip entity persistence.
/// Implementations (adapters) are in the Infrastructure layer.
/// All methods return Result types for functional error handling.
/// </summary>
public interface ITripRepository
{
    /// <summary>
    /// Retrieves a trip by its unique identifier.
    /// </summary>
    /// <param name="id">The trip ID to search for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the trip if found, or a failure message if not found or an error occurs</returns>
    Task<Result<Trip>> GetByIdAsync(TripId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all trips belonging to a specific route.
    /// </summary>
    /// <param name="routeId">The route ID to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing trips for the route, or a failure message if an error occurs</returns>
    Task<Result<IReadOnlyList<Trip>>> GetByRouteAsync(RouteId routeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all trips that serve a specific stop.
    /// </summary>
    /// <param name="stopId">The stop ID to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing trips serving the stop, or a failure message if an error occurs</returns>
    Task<Result<IReadOnlyList<Trip>>> GetTripsServingStopAsync(StopId stopId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk inserts a collection of trips.
    /// </summary>
    /// <param name="trips">The trips to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure with error details</returns>
    Task<Result> AddRangeAsync(IEnumerable<Trip> trips, CancellationToken cancellationToken = default);
}
