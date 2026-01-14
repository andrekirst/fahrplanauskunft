using Fahrplanauskunft.Core.Domain;
using Fahrplanauskunft.Core.Ports;

namespace Fahrplanauskunft.Application.UseCases;

/// <summary>
/// Use case for retrieving a station by its ID.
/// </summary>
public sealed class GetStationByIdUseCase
{
    private readonly IStationRepository _stationRepository;

    public GetStationByIdUseCase(IStationRepository stationRepository)
    {
        _stationRepository = stationRepository ?? throw new ArgumentNullException(nameof(stationRepository));
    }

    public Station? Execute(string stationId)
    {
        if (string.IsNullOrWhiteSpace(stationId))
            throw new ArgumentException("Station ID cannot be empty.", nameof(stationId));

        return _stationRepository.GetById(stationId);
    }
}
