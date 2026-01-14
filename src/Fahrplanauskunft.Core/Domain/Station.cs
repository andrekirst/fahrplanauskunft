namespace Fahrplanauskunft.Core.Domain;

/// <summary>
/// Represents a transit station in the network.
/// </summary>
public sealed class Station : IEquatable<Station>
{
    public string Id { get; }
    public string Name { get; }

    public Station(string id, string name)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Station ID cannot be empty.", nameof(id));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Station name cannot be empty.", nameof(name));

        Id = id;
        Name = name;
    }

    public bool Equals(Station? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return string.Equals(Id, other.Id, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj)
    {
        return obj is Station other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode(StringComparison.Ordinal);
    }

    public static bool operator ==(Station? left, Station? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(Station? left, Station? right)
    {
        return !(left == right);
    }
}
