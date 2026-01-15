using CSharpFunctionalExtensions;
using Fahrplanauskunft.Core.Entities;
using Fahrplanauskunft.Core.ValueObjects;

namespace Fahrplanauskunft.Core.Ports.Repositories;

/// <summary>
/// Port interface for Footpath entity persistence.
/// Implementations (adapters) are in the Infrastructure layer.
/// All methods return Result types for functional error handling.
/// </summary>
public interface IFootpathRepository
{
    /// <summary>
    /// Retrieves all footpaths originating from a specific stop.
    /// </summary>
    /// <param name="stopId">The ID of the origin stop</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing footpaths from the stop, or a failure message if an error occurs</returns>
    Task<Result<IReadOnlyList<Footpath>>> GetFromStopAsync(StopId stopId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all footpaths in the system.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing all footpaths, or a failure message if an error occurs</returns>
    Task<Result<IReadOnlyList<Footpath>>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk inserts a collection of footpaths.
    /// </summary>
    /// <param name="footpaths">The footpaths to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure with error details</returns>
    Task<Result> AddRangeAsync(IEnumerable<Footpath> footpaths, CancellationToken cancellationToken = default);
}
