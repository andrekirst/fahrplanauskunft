using Fahrplanauskunft.Core.Domain;

namespace Fahrplanauskunft.Core.Ports;

/// <summary>
/// Port interface for station persistence.
/// Implementations (adapters) are in the Infrastructure layer.
/// </summary>
public interface IStationRepository
{
    Station? GetById(string id);
    IEnumerable<Station> GetAll();
    void Add(Station station);
}
