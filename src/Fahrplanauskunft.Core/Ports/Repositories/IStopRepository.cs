using CSharpFunctionalExtensions;
using Fahrplanauskunft.Core.Entities;
using Fahrplanauskunft.Core.ValueObjects;

namespace Fahrplanauskunft.Core.Ports.Repositories;

/// <summary>
/// Port interface for Stop entity persistence.
/// Implementations (adapters) are in the Infrastructure layer.
/// All methods return Result types for functional error handling.
/// </summary>
public interface IStopRepository
{
    /// <summary>
    /// Retrieves a stop by its unique identifier.
    /// </summary>
    /// <param name="id">The stop ID to search for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the stop if found, or a failure message if not found or an error occurs</returns>
    Task<Result<Stop>> GetByIdAsync(StopId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all stops in the system.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing all stops, or a failure message if an error occurs</returns>
    Task<Result<IReadOnlyList<Stop>>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for stops by name using case-insensitive matching.
    /// </summary>
    /// <param name="searchTerm">The search term to match against stop names</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing matching stops, or a failure message if an error occurs</returns>
    Task<Result<IReadOnlyList<Stop>>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk inserts a collection of stops.
    /// </summary>
    /// <param name="stops">The stops to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure with error details</returns>
    Task<Result> AddRangeAsync(IEnumerable<Stop> stops, CancellationToken cancellationToken = default);
}
