using Fahrplanauskunft.Core.Entities;
using Fahrplanauskunft.Core.Exceptions;
using Fahrplanauskunft.Core.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Fahrplanauskunft.Tests.Unit.Domain;

public class StopTimeTests
{
    private static Stop CreateStop(string id = "S1", string name = "Test Stop") =>
        Stop.Create(id, name);

    [Fact]
    public void Constructor_WithArrivalBeforeDeparture_ShouldSucceed()
    {
        // Arrange
        var stop = CreateStop();
        var arrival = new TimeOfDay(8, 0);
        var departure = new TimeOfDay(8, 5);

        // Act
        var stopTime = new StopTime(stop, StopSequence.From(1), arrival, departure);

        // Assert
        stopTime.Arrival.Should().Be(arrival);
        stopTime.Departure.Should().Be(departure);
    }

    [Fact]
    public void Constructor_WithArrivalEqualToDeparture_ShouldSucceed()
    {
        // Arrange
        var stop = CreateStop();
        var time = new TimeOfDay(8, 0);

        // Act
        var stopTime = new StopTime(stop, StopSequence.From(1), time, time);

        // Assert
        stopTime.Arrival.Should().Be(time);
        stopTime.Departure.Should().Be(time);
    }

    [Fact]
    public void Constructor_WithArrivalAfterDeparture_ShouldThrowInvalidStopTimeException()
    {
        // Arrange
        var stop = CreateStop();
        var arrival = new TimeOfDay(8, 30);    // Later time
        var departure = new TimeOfDay(8, 0);   // Earlier time

        // Act
        var act = () => new StopTime(stop, StopSequence.From(1), arrival, departure);

        // Assert
        act.Should().Throw<InvalidStopTimeException>()
            .WithMessage("*Arrival time*cannot be after departure time*");
    }

    [Fact]
    public void Constructor_WithNullStop_ShouldThrowArgumentNullException()
    {
        // Arrange
        var arrival = new TimeOfDay(8, 0);
        var departure = new TimeOfDay(8, 5);

        // Act
        var act = () => new StopTime(null!, StopSequence.From(1), arrival, departure);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_WithSingleTime_ShouldSetArrivalAndDepartureEqual()
    {
        // Arrange
        var stop = CreateStop();
        var time = new TimeOfDay(10, 30);

        // Act
        var stopTime = StopTime.Create(stop, StopSequence.From(1), time);

        // Assert
        stopTime.Arrival.Should().Be(time);
        stopTime.Departure.Should().Be(time);
    }

    [Fact]
    public void CreateArrivalOnly_ShouldHavePickupDisabled()
    {
        // Arrange
        var stop = CreateStop();
        var arrival = new TimeOfDay(12, 0);

        // Act
        var stopTime = StopTime.CreateArrivalOnly(stop, StopSequence.From(1), arrival);

        // Assert
        stopTime.PickupAllowed.Should().BeFalse();
        stopTime.DropOffAllowed.Should().BeTrue();
    }

    [Fact]
    public void CreateDepartureOnly_ShouldHaveDropOffDisabled()
    {
        // Arrange
        var stop = CreateStop();
        var departure = new TimeOfDay(6, 0);

        // Act
        var stopTime = StopTime.CreateDepartureOnly(stop, StopSequence.From(1), departure);

        // Assert
        stopTime.PickupAllowed.Should().BeTrue();
        stopTime.DropOffAllowed.Should().BeFalse();
    }

    [Fact]
    public void DwellTime_ShouldCalculateCorrectly()
    {
        // Arrange
        var stop = CreateStop();
        var arrival = new TimeOfDay(8, 0);
        var departure = new TimeOfDay(8, 5);

        // Act
        var stopTime = new StopTime(stop, StopSequence.From(1), arrival, departure);

        // Assert
        stopTime.DwellTime.TotalMinutes.Should().Be(5);
    }

    [Fact]
    public void DwellTime_WhenArrivalEqualsDeparture_ShouldBeZero()
    {
        // Arrange
        var stop = CreateStop();
        var time = new TimeOfDay(8, 0);

        // Act
        var stopTime = StopTime.Create(stop, StopSequence.From(1), time);

        // Assert
        stopTime.DwellTime.TotalMinutes.Should().Be(0);
    }

    [Fact]
    public void CompareTo_ShouldOrderBySequence()
    {
        // Arrange
        var stop = CreateStop();
        var stopTime1 = StopTime.Create(stop, StopSequence.From(1), new TimeOfDay(8, 0));
        var stopTime2 = StopTime.Create(stop, StopSequence.From(5), new TimeOfDay(8, 30));
        var stopTime3 = StopTime.Create(stop, StopSequence.From(3), new TimeOfDay(8, 15));

        // Act
        var sorted = new[] { stopTime2, stopTime3, stopTime1 }.OrderBy(x => x).ToList();

        // Assert
        sorted[0].Sequence.Value.Should().Be(1);
        sorted[1].Sequence.Value.Should().Be(3);
        sorted[2].Sequence.Value.Should().Be(5);
    }

    [Fact]
    public void Equality_StopTimesWithSameValues_ShouldBeEqual()
    {
        // Arrange
        var stop = CreateStop();
        var sequence = StopSequence.From(1);
        var arrival = new TimeOfDay(8, 0);
        var departure = new TimeOfDay(8, 5);

        var stopTime1 = new StopTime(stop, sequence, arrival, departure);
        var stopTime2 = new StopTime(stop, sequence, arrival, departure);

        // Act & Assert
        stopTime1.Should().Be(stopTime2);
        (stopTime1 == stopTime2).Should().BeTrue();
    }

    [Fact]
    public void Equality_StopTimesWithDifferentTimes_ShouldNotBeEqual()
    {
        // Arrange
        var stop = CreateStop();
        var stopTime1 = StopTime.Create(stop, StopSequence.From(1), new TimeOfDay(8, 0));
        var stopTime2 = StopTime.Create(stop, StopSequence.From(1), new TimeOfDay(9, 0));

        // Act & Assert
        stopTime1.Should().NotBe(stopTime2);
        (stopTime1 != stopTime2).Should().BeTrue();
    }

    [Fact]
    public void ToString_WithSameArrivalAndDeparture_ShouldShowSingleTime()
    {
        // Arrange
        var stop = CreateStop(name: "Central");
        var stopTime = StopTime.Create(stop, StopSequence.From(1), new TimeOfDay(8, 0));

        // Act
        var result = stopTime.ToString();

        // Assert
        result.Should().Contain("Central");
        result.Should().Contain("08:00");
    }

    [Fact]
    public void ToString_WithDifferentArrivalAndDeparture_ShouldShowBothTimes()
    {
        // Arrange
        var stop = CreateStop(name: "Central");
        var stopTime = new StopTime(
            stop,
            StopSequence.From(1),
            new TimeOfDay(8, 0),
            new TimeOfDay(8, 5));

        // Act
        var result = stopTime.ToString();

        // Assert
        result.Should().Contain("Central");
        result.Should().Contain("arr");
        result.Should().Contain("dep");
    }

    [Fact]
    public void ExtendedTime_ShouldBeAllowed()
    {
        // Arrange - Transit schedules often use times > 24:00 for overnight services
        var stop = CreateStop();
        var arrival = new TimeOfDay(25, 0);     // 01:00 next day in transit notation
        var departure = new TimeOfDay(25, 5);

        // Act
        var stopTime = new StopTime(stop, StopSequence.From(1), arrival, departure);

        // Assert
        stopTime.Arrival.Hours.Should().Be(25);
        stopTime.Departure.Hours.Should().Be(25);
    }

    [Fact]
    public void Constructor_WithExtendedArrivalAfterDeparture_ShouldThrow()
    {
        // Arrange
        var stop = CreateStop();
        var arrival = new TimeOfDay(26, 0);    // Later
        var departure = new TimeOfDay(25, 30); // Earlier

        // Act
        var act = () => new StopTime(stop, StopSequence.From(1), arrival, departure);

        // Assert
        act.Should().Throw<InvalidStopTimeException>();
    }
}
