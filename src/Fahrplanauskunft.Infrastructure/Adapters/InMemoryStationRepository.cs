using System.Collections.Concurrent;
using Fahrplanauskunft.Core.Domain;
using Fahrplanauskunft.Core.Ports;

namespace Fahrplanauskunft.Infrastructure.Adapters;

/// <summary>
/// Thread-safe in-memory implementation of IStationRepository.
/// This adapter implements the port defined in Core.
/// Uses ConcurrentDictionary for thread-safe operations.
/// </summary>
public sealed class InMemoryStationRepository : IStationRepository
{
    private readonly ConcurrentDictionary<string, Station> _stations = new(StringComparer.Ordinal);

    public Station? GetById(string id)
    {
        return _stations.GetValueOrDefault(id);
    }

    public IEnumerable<Station> GetAll()
    {
        return _stations.Values;
    }

    public void Add(Station station)
    {
        ArgumentNullException.ThrowIfNull(station);
        _stations[station.Id] = station;
    }
}
