using Fahrplanauskunft.Core.Entities;
using Fahrplanauskunft.Core.Exceptions;
using Fahrplanauskunft.Core.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Fahrplanauskunft.Tests.Unit.Domain;

public class TripTests
{
    private static Stop CreateStop(string id, string name) =>
        Stop.Create(id, name);

    private static StopTime CreateStopTime(Stop stop, int sequence, int hour, int minute) =>
        StopTime.Create(stop, StopSequence.From(sequence), new TimeOfDay(hour, minute));

    [Fact]
    public void Constructor_WithUniqueSequences_ShouldSucceed()
    {
        // Arrange
        var stop1 = CreateStop("S1", "Stop 1");
        var stop2 = CreateStop("S2", "Stop 2");
        var stop3 = CreateStop("S3", "Stop 3");

        var stopTimes = new[]
        {
            CreateStopTime(stop1, 1, 8, 0),
            CreateStopTime(stop2, 2, 8, 10),
            CreateStopTime(stop3, 3, 8, 20)
        };

        // Act
        var trip = new Trip(TripId.From("T1"), stopTimes);

        // Assert
        trip.StopTimes.Should().HaveCount(3);
        trip.StopTimes[0].Sequence.Value.Should().Be(1);
        trip.StopTimes[1].Sequence.Value.Should().Be(2);
        trip.StopTimes[2].Sequence.Value.Should().Be(3);
    }

    [Fact]
    public void Constructor_WithDuplicateSequence_ShouldThrowDuplicateSequenceException()
    {
        // Arrange
        var stop1 = CreateStop("S1", "Stop 1");
        var stop2 = CreateStop("S2", "Stop 2");

        var stopTimes = new[]
        {
            CreateStopTime(stop1, 1, 8, 0),
            CreateStopTime(stop2, 1, 8, 10)  // Duplicate sequence 1
        };

        // Act
        var act = () => new Trip(TripId.From("T1"), stopTimes);

        // Assert
        act.Should().Throw<DuplicateSequenceException>()
            .Which.DuplicateSequenceNumber.Should().Be(1);
    }

    [Fact]
    public void AddStopTime_WithDuplicateSequence_ShouldThrowDuplicateSequenceException()
    {
        // Arrange
        var stop1 = CreateStop("S1", "Stop 1");
        var stop2 = CreateStop("S2", "Stop 2");
        var trip = new Trip(TripId.From("T1"));
        trip.AddStopTime(CreateStopTime(stop1, 1, 8, 0));

        // Act
        var act = () => trip.AddStopTime(CreateStopTime(stop2, 1, 8, 10));

        // Assert
        act.Should().Throw<DuplicateSequenceException>()
            .Which.DuplicateSequenceNumber.Should().Be(1);
    }

    [Fact]
    public void AddStopTimes_WithDuplicateSequenceWithinBatch_ShouldThrowDuplicateSequenceException()
    {
        // Arrange
        var stop1 = CreateStop("S1", "Stop 1");
        var stop2 = CreateStop("S2", "Stop 2");
        var trip = new Trip(TripId.From("T1"));

        var stopTimes = new[]
        {
            CreateStopTime(stop1, 2, 8, 0),
            CreateStopTime(stop2, 2, 8, 10)  // Duplicate within batch
        };

        // Act
        var act = () => trip.AddStopTimes(stopTimes);

        // Assert
        act.Should().Throw<DuplicateSequenceException>()
            .Which.DuplicateSequenceNumber.Should().Be(2);
    }

    [Fact]
    public void StopTimes_ShouldBeSortedBySequence()
    {
        // Arrange
        var stop1 = CreateStop("S1", "Stop 1");
        var stop2 = CreateStop("S2", "Stop 2");
        var stop3 = CreateStop("S3", "Stop 3");

        // Add in reverse order
        var stopTimes = new[]
        {
            CreateStopTime(stop3, 3, 8, 20),
            CreateStopTime(stop1, 1, 8, 0),
            CreateStopTime(stop2, 2, 8, 10)
        };

        // Act
        var trip = new Trip(TripId.From("T1"), stopTimes);

        // Assert
        trip.StopTimes[0].Stop.Name.Should().Be("Stop 1");
        trip.StopTimes[1].Stop.Name.Should().Be("Stop 2");
        trip.StopTimes[2].Stop.Name.Should().Be("Stop 3");
    }

    [Fact]
    public void FirstStopTime_ShouldReturnLowestSequence()
    {
        // Arrange
        var stop1 = CreateStop("S1", "First Stop");
        var stop2 = CreateStop("S2", "Last Stop");

        var trip = new Trip(TripId.From("T1"), new[]
        {
            CreateStopTime(stop2, 5, 8, 30),
            CreateStopTime(stop1, 1, 8, 0)
        });

        // Act & Assert
        trip.FirstStopTime.Should().NotBeNull();
        trip.FirstStopTime!.Stop.Name.Should().Be("First Stop");
    }

    [Fact]
    public void LastStopTime_ShouldReturnHighestSequence()
    {
        // Arrange
        var stop1 = CreateStop("S1", "First Stop");
        var stop2 = CreateStop("S2", "Last Stop");

        var trip = new Trip(TripId.From("T1"), new[]
        {
            CreateStopTime(stop1, 1, 8, 0),
            CreateStopTime(stop2, 5, 8, 30)
        });

        // Act & Assert
        trip.LastStopTime.Should().NotBeNull();
        trip.LastStopTime!.Stop.Name.Should().Be("Last Stop");
    }

    [Fact]
    public void StopTimes_ShouldBeReadOnly()
    {
        // Arrange
        var trip = new Trip(TripId.From("T1"));

        // Act
        var stopTimes = trip.StopTimes;

        // Assert
        stopTimes.Should().BeAssignableTo<IReadOnlyList<StopTime>>();
    }

    [Fact]
    public void HasSequence_WithExistingSequence_ShouldReturnTrue()
    {
        // Arrange
        var stop = CreateStop("S1", "Stop 1");
        var trip = new Trip(TripId.From("T1"));
        trip.AddStopTime(CreateStopTime(stop, 5, 8, 0));

        // Act & Assert
        trip.HasSequence(StopSequence.From(5)).Should().BeTrue();
    }

    [Fact]
    public void HasSequence_WithNonExistingSequence_ShouldReturnFalse()
    {
        // Arrange
        var stop = CreateStop("S1", "Stop 1");
        var trip = new Trip(TripId.From("T1"));
        trip.AddStopTime(CreateStopTime(stop, 5, 8, 0));

        // Act & Assert
        trip.HasSequence(StopSequence.From(10)).Should().BeFalse();
    }

    [Fact]
    public void ServesStop_ShouldReturnTrueForIncludedStop()
    {
        // Arrange
        var stop1 = CreateStop("S1", "Stop 1");
        var stop2 = CreateStop("S2", "Stop 2");
        var trip = new Trip(TripId.From("T1"));
        trip.AddStopTime(CreateStopTime(stop1, 1, 8, 0));

        // Act & Assert
        trip.ServesStop(stop1).Should().BeTrue();
        trip.ServesStop(stop2).Should().BeFalse();
    }

    [Fact]
    public void Origin_ShouldReturnFirstStop()
    {
        // Arrange
        var stop1 = CreateStop("S1", "Origin");
        var stop2 = CreateStop("S2", "Destination");
        var trip = new Trip(TripId.From("T1"), new[]
        {
            CreateStopTime(stop1, 1, 8, 0),
            CreateStopTime(stop2, 2, 8, 30)
        });

        // Act & Assert
        trip.Origin.Should().Be(stop1);
    }

    [Fact]
    public void Destination_ShouldReturnLastStop()
    {
        // Arrange
        var stop1 = CreateStop("S1", "Origin");
        var stop2 = CreateStop("S2", "Destination");
        var trip = new Trip(TripId.From("T1"), new[]
        {
            CreateStopTime(stop1, 1, 8, 0),
            CreateStopTime(stop2, 2, 8, 30)
        });

        // Act & Assert
        trip.Destination.Should().Be(stop2);
    }

    [Fact]
    public void EmptyTrip_ShouldHaveNullFirstAndLastStopTimes()
    {
        // Arrange
        var trip = new Trip(TripId.From("T1"));

        // Act & Assert
        trip.FirstStopTime.Should().BeNull();
        trip.LastStopTime.Should().BeNull();
        trip.Origin.Should().BeNull();
        trip.Destination.Should().BeNull();
        trip.DepartureTime.Should().BeNull();
        trip.ArrivalTime.Should().BeNull();
        trip.TotalDuration.Should().BeNull();
    }

    [Fact]
    public void Equality_TripsWithSameId_ShouldBeEqual()
    {
        // Arrange
        var trip1 = new Trip(TripId.From("T1"), headsign: "A");
        var trip2 = new Trip(TripId.From("T1"), headsign: "B");

        // Act & Assert
        trip1.Should().Be(trip2);
        (trip1 == trip2).Should().BeTrue();
    }

    [Fact]
    public void Equality_TripsWithDifferentIds_ShouldNotBeEqual()
    {
        // Arrange
        var trip1 = new Trip(TripId.From("T1"));
        var trip2 = new Trip(TripId.From("T2"));

        // Act & Assert
        trip1.Should().NotBe(trip2);
        (trip1 != trip2).Should().BeTrue();
    }

    [Fact]
    public void GetStopTimeAt_WithExistingSequence_ShouldReturnStopTime()
    {
        // Arrange
        var stop = CreateStop("S1", "Stop 1");
        var trip = new Trip(TripId.From("T1"));
        trip.AddStopTime(CreateStopTime(stop, 5, 8, 0));

        // Act
        var result = trip.GetStopTimeAt(StopSequence.From(5));

        // Assert
        result.Should().NotBeNull();
        result!.Stop.Should().Be(stop);
    }

    [Fact]
    public void GetStopTimeAt_WithNonExistingSequence_ShouldReturnNull()
    {
        // Arrange
        var stop = CreateStop("S1", "Stop 1");
        var trip = new Trip(TripId.From("T1"));
        trip.AddStopTime(CreateStopTime(stop, 5, 8, 0));

        // Act
        var result = trip.GetStopTimeAt(StopSequence.From(10));

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetStopTimeFor_WithExistingStop_ShouldReturnStopTime()
    {
        // Arrange
        var stop1 = CreateStop("S1", "Stop 1");
        var stop2 = CreateStop("S2", "Stop 2");
        var trip = new Trip(TripId.From("T1"));
        trip.AddStopTime(CreateStopTime(stop1, 1, 8, 0));
        trip.AddStopTime(CreateStopTime(stop2, 2, 8, 15));

        // Act
        var result = trip.GetStopTimeFor(stop2);

        // Assert
        result.Should().NotBeNull();
        result!.Sequence.Value.Should().Be(2);
    }

    [Fact]
    public void GetStopTimeFor_WithNonExistingStop_ShouldReturnNull()
    {
        // Arrange
        var stop1 = CreateStop("S1", "Stop 1");
        var stop2 = CreateStop("S2", "Stop 2");
        var trip = new Trip(TripId.From("T1"));
        trip.AddStopTime(CreateStopTime(stop1, 1, 8, 0));

        // Act
        var result = trip.GetStopTimeFor(stop2);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetStopTimesBetween_ShouldReturnStopsInRange()
    {
        // Arrange
        var stop1 = CreateStop("S1", "Stop 1");
        var stop2 = CreateStop("S2", "Stop 2");
        var stop3 = CreateStop("S3", "Stop 3");
        var stop4 = CreateStop("S4", "Stop 4");

        var trip = new Trip(TripId.From("T1"), new[]
        {
            CreateStopTime(stop1, 1, 8, 0),
            CreateStopTime(stop2, 2, 8, 10),
            CreateStopTime(stop3, 3, 8, 20),
            CreateStopTime(stop4, 4, 8, 30)
        });

        // Act
        var result = trip.GetStopTimesBetween(StopSequence.From(2), StopSequence.From(3)).ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].Stop.Name.Should().Be("Stop 2");
        result[1].Stop.Name.Should().Be("Stop 3");
    }

    [Fact]
    public void AddStopTime_WithNullStopTime_ShouldThrowArgumentNullException()
    {
        // Arrange
        var trip = new Trip(TripId.From("T1"));

        // Act
        var act = () => trip.AddStopTime(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddStopTimes_WithNullCollection_ShouldThrowArgumentNullException()
    {
        // Arrange
        var trip = new Trip(TripId.From("T1"));

        // Act
        var act = () => trip.AddStopTimes(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullStopTimes_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => new Trip(TripId.From("T1"), (IEnumerable<StopTime>)null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TotalDuration_WithValidTrip_ShouldReturnCorrectDuration()
    {
        // Arrange
        var stop1 = CreateStop("S1", "Origin");
        var stop2 = CreateStop("S2", "Destination");
        var trip = new Trip(TripId.From("T1"), new[]
        {
            CreateStopTime(stop1, 1, 8, 0),
            CreateStopTime(stop2, 2, 8, 45)
        });

        // Act
        var duration = trip.TotalDuration;

        // Assert
        duration.Should().NotBeNull();
        duration!.Value.TotalMinutes.Should().Be(45);
    }

    [Fact]
    public void ToString_WithHeadsign_ShouldIncludeHeadsign()
    {
        // Arrange
        var stop = CreateStop("S1", "Stop");
        var trip = new Trip(TripId.From("T1"), headsign: "Central Station");
        trip.AddStopTime(CreateStopTime(stop, 1, 8, 0));

        // Act
        var result = trip.ToString();

        // Assert
        result.Should().Contain("Central Station");
        result.Should().Contain("1 stops");
    }

    [Fact]
    public void Equality_WithNull_ShouldNotBeEqual()
    {
        // Arrange
        var trip = new Trip(TripId.From("T1"));

        // Act & Assert
        trip.Equals(null).Should().BeFalse();
        (trip == null).Should().BeFalse();
        (null == trip).Should().BeFalse();
    }

    [Fact]
    public void Equality_WithSameReference_ShouldBeEqual()
    {
        // Arrange
        var trip = new Trip(TripId.From("T1"));

        // Act & Assert
        trip.Equals(trip).Should().BeTrue();
#pragma warning disable CS1718 // Comparison made to same variable
        (trip == trip).Should().BeTrue();
#pragma warning restore CS1718
    }

    [Fact]
    public void GetHashCode_ForEqualTrips_ShouldBeSame()
    {
        // Arrange
        var trip1 = new Trip(TripId.From("T1"));
        var trip2 = new Trip(TripId.From("T1"));

        // Act & Assert
        trip1.GetHashCode().Should().Be(trip2.GetHashCode());
    }
}
