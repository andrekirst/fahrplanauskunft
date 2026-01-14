using Fahrplanauskunft.Core.ValueObjects;
using FluentAssertions;

namespace Fahrplanauskunft.Tests.Unit.ValueObjects;

public class StopSequenceTests
{
    #region Constructor Tests

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(10000)]
    public void Constructor_WithValidValue_CreatesStopSequence(int value)
    {
        // Act
        var sequence = new StopSequence(value);

        // Assert
        sequence.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Constructor_WithValueBelowMinimum_ThrowsArgumentOutOfRangeException(int value)
    {
        // Act
        var act = () => new StopSequence(value);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("value");
    }

    [Fact]
    public void Constructor_WithValueAboveMaximum_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => new StopSequence(10001);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("value");
    }

    #endregion

    #region Factory Method Tests

    [Fact]
    public void From_WithValidValue_CreatesStopSequence()
    {
        // Act
        var sequence = StopSequence.From(5);

        // Assert
        sequence.Value.Should().Be(5);
    }

    [Fact]
    public void TryCreate_WithValidValue_ReturnsTrue()
    {
        // Act
        var result = StopSequence.TryCreate(5, out var sequence);

        // Assert
        result.Should().BeTrue();
        sequence.Value.Should().Be(5);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(10001)]
    public void TryCreate_WithInvalidValue_ReturnsFalse(int value)
    {
        // Act
        var result = StopSequence.TryCreate(value, out var sequence);

        // Assert
        result.Should().BeFalse();
        sequence.Should().Be(default);
    }

    #endregion

    #region Static Properties Tests

    [Fact]
    public void First_ReturnsSequenceWithValueOne()
    {
        // Act
        var first = StopSequence.First;

        // Assert
        first.Value.Should().Be(1);
    }

    #endregion

    #region IsFirst Tests

    [Fact]
    public void IsFirst_WhenValueIsOne_ReturnsTrue()
    {
        // Arrange
        var sequence = new StopSequence(1);

        // Act & Assert
        sequence.IsFirst.Should().BeTrue();
    }

    [Fact]
    public void IsFirst_WhenValueIsNotOne_ReturnsFalse()
    {
        // Arrange
        var sequence = new StopSequence(2);

        // Act & Assert
        sequence.IsFirst.Should().BeFalse();
    }

    #endregion

    #region Next/Previous Tests

    [Fact]
    public void Next_ReturnsNextSequence()
    {
        // Arrange
        var sequence = new StopSequence(5);

        // Act
        var next = sequence.Next();

        // Assert
        next.Value.Should().Be(6);
    }

    [Fact]
    public void Next_AtMaximum_ThrowsInvalidOperationException()
    {
        // Arrange
        var sequence = new StopSequence(10000);

        // Act
        var act = () => sequence.Next();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Previous_ReturnsPreviousSequence()
    {
        // Arrange
        var sequence = new StopSequence(5);

        // Act
        var previous = sequence.Previous();

        // Assert
        previous.Value.Should().Be(4);
    }

    [Fact]
    public void Previous_AtMinimum_ThrowsInvalidOperationException()
    {
        // Arrange
        var sequence = new StopSequence(1);

        // Act
        var act = () => sequence.Previous();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void OperatorIncrement_ReturnsNextSequence()
    {
        // Arrange
        var sequence = new StopSequence(5);

        // Act
        var next = ++sequence;

        // Assert
        next.Value.Should().Be(6);
    }

    [Fact]
    public void OperatorDecrement_ReturnsPreviousSequence()
    {
        // Arrange
        var sequence = new StopSequence(5);

        // Act
        var previous = --sequence;

        // Assert
        previous.Value.Should().Be(4);
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        // Arrange
        var sequence1 = new StopSequence(5);
        var sequence2 = new StopSequence(5);

        // Act & Assert
        sequence1.Equals(sequence2).Should().BeTrue();
        (sequence1 == sequence2).Should().BeTrue();
        (sequence1 != sequence2).Should().BeFalse();
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        // Arrange
        var sequence1 = new StopSequence(5);
        var sequence2 = new StopSequence(6);

        // Act & Assert
        sequence1.Equals(sequence2).Should().BeFalse();
        (sequence1 == sequence2).Should().BeFalse();
        (sequence1 != sequence2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_SameValue_ReturnsSameHashCode()
    {
        // Arrange
        var sequence1 = new StopSequence(5);
        var sequence2 = new StopSequence(5);

        // Act & Assert
        sequence1.GetHashCode().Should().Be(sequence2.GetHashCode());
    }

    #endregion

    #region Comparison Tests

    [Fact]
    public void CompareTo_LowerSequence_ReturnsPositive()
    {
        // Arrange
        var higher = new StopSequence(10);
        var lower = new StopSequence(5);

        // Act & Assert
        higher.CompareTo(lower).Should().BePositive();
        (higher > lower).Should().BeTrue();
        (higher >= lower).Should().BeTrue();
        (higher < lower).Should().BeFalse();
        (higher <= lower).Should().BeFalse();
    }

    [Fact]
    public void CompareTo_HigherSequence_ReturnsNegative()
    {
        // Arrange
        var lower = new StopSequence(5);
        var higher = new StopSequence(10);

        // Act & Assert
        lower.CompareTo(higher).Should().BeNegative();
        (lower < higher).Should().BeTrue();
        (lower <= higher).Should().BeTrue();
        (lower > higher).Should().BeFalse();
        (lower >= higher).Should().BeFalse();
    }

    [Fact]
    public void CompareTo_SameSequence_ReturnsZero()
    {
        // Arrange
        var sequence1 = new StopSequence(5);
        var sequence2 = new StopSequence(5);

        // Act & Assert
        sequence1.CompareTo(sequence2).Should().Be(0);
    }

    #endregion

    #region Conversion Tests

    [Fact]
    public void ExplicitToInt_ReturnsValue()
    {
        // Arrange
        var sequence = new StopSequence(5);

        // Act
        var result = (int)sequence;

        // Assert
        result.Should().Be(5);
    }

    [Fact]
    public void ExplicitFromInt_CreatesStopSequence()
    {
        // Act
        var sequence = (StopSequence)5;

        // Assert
        sequence.Value.Should().Be(5);
    }

    [Fact]
    public void ToString_ReturnsValueAsString()
    {
        // Arrange
        var sequence = new StopSequence(5);

        // Act
        var result = sequence.ToString();

        // Assert
        result.Should().Be("5");
    }

    [Theory]
    [InlineData(1, "1")]
    [InlineData(9, "9")]
    [InlineData(10, "10")]
    [InlineData(99, "99")]
    [InlineData(100, "100")]
    [InlineData(999, "999")]
    [InlineData(1000, "1000")]
    [InlineData(9999, "9999")]
    [InlineData(10000, "10000")]
    public void ToString_VariousValues_ReturnsCorrectString(int value, string expected)
    {
        // Arrange
        var sequence = new StopSequence(value);

        // Act
        var result = sequence.ToString();

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Equality Edge Cases

    [Fact]
    public void Equals_WithNullObject_ReturnsFalse()
    {
        // Arrange
        var sequence = new StopSequence(5);

        // Act & Assert
        sequence.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentType_ReturnsFalse()
    {
        // Arrange
        var sequence = new StopSequence(5);
        object differentType = "5";

        // Act & Assert
        sequence.Equals(differentType).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithBoxedStopSequence_ReturnsTrue()
    {
        // Arrange
        var sequence1 = new StopSequence(5);
        object boxedSequence = new StopSequence(5);

        // Act & Assert
        sequence1.Equals(boxedSequence).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithBoxedDifferentValue_ReturnsFalse()
    {
        // Arrange
        var sequence1 = new StopSequence(5);
        object boxedSequence = new StopSequence(6);

        // Act & Assert
        sequence1.Equals(boxedSequence).Should().BeFalse();
    }

    #endregion

    #region Chained Operations Tests

    [Fact]
    public void Next_MultipleCalls_IncreasesCorrectly()
    {
        // Arrange
        var sequence = new StopSequence(1);

        // Act
        var result = sequence.Next().Next().Next();

        // Assert
        result.Value.Should().Be(4);
    }

    [Fact]
    public void Previous_MultipleCalls_DecreasesCorrectly()
    {
        // Arrange
        var sequence = new StopSequence(10);

        // Act
        var result = sequence.Previous().Previous().Previous();

        // Assert
        result.Value.Should().Be(7);
    }

    [Fact]
    public void Next_Then_Previous_ReturnsOriginal()
    {
        // Arrange
        var original = new StopSequence(5);

        // Act
        var result = original.Next().Previous();

        // Assert
        result.Should().Be(original);
    }

    [Fact]
    public void Previous_Then_Next_ReturnsOriginal()
    {
        // Arrange
        var original = new StopSequence(5);

        // Act
        var result = original.Previous().Next();

        // Assert
        result.Should().Be(original);
    }

    #endregion

    #region Edge Cases and Boundary Tests

    [Fact]
    public void Default_StopSequence_HasZeroValue()
    {
        // Arrange
        var defaultSequence = default(StopSequence);

        // Assert - default struct has Value = 0
        defaultSequence.Value.Should().Be(0);
    }

    [Fact]
    public void MinValue_Constant_IsOne()
    {
        // Assert
        StopSequence.MinValue.Should().Be(1);
    }

    [Fact]
    public void MaxValue_Constant_IsTenThousand()
    {
        // Assert
        StopSequence.MaxValue.Should().Be(10000);
    }

    [Fact]
    public void Constructor_AtMinValue_Succeeds()
    {
        // Act
        var sequence = new StopSequence(StopSequence.MinValue);

        // Assert
        sequence.Value.Should().Be(StopSequence.MinValue);
        sequence.IsFirst.Should().BeTrue();
    }

    [Fact]
    public void Constructor_AtMaxValue_Succeeds()
    {
        // Act
        var sequence = new StopSequence(StopSequence.MaxValue);

        // Assert
        sequence.Value.Should().Be(StopSequence.MaxValue);
    }

    [Fact]
    public void Constructor_JustBelowMinValue_ThrowsException()
    {
        // Act
        var act = () => new StopSequence(StopSequence.MinValue - 1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Constructor_JustAboveMaxValue_ThrowsException()
    {
        // Act
        var act = () => new StopSequence(StopSequence.MaxValue + 1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Next_FromMaxValueMinusOne_ReturnsMaxValue()
    {
        // Arrange
        var sequence = new StopSequence(StopSequence.MaxValue - 1);

        // Act
        var next = sequence.Next();

        // Assert
        next.Value.Should().Be(StopSequence.MaxValue);
    }

    [Fact]
    public void Previous_FromMinValuePlusOne_ReturnsMinValue()
    {
        // Arrange
        var sequence = new StopSequence(StopSequence.MinValue + 1);

        // Act
        var previous = sequence.Previous();

        // Assert
        previous.Value.Should().Be(StopSequence.MinValue);
    }

    [Fact]
    public void TryCreate_AtMinValue_Succeeds()
    {
        // Act
        var result = StopSequence.TryCreate(StopSequence.MinValue, out var sequence);

        // Assert
        result.Should().BeTrue();
        sequence.Value.Should().Be(StopSequence.MinValue);
    }

    [Fact]
    public void TryCreate_AtMaxValue_Succeeds()
    {
        // Act
        var result = StopSequence.TryCreate(StopSequence.MaxValue, out var sequence);

        // Assert
        result.Should().BeTrue();
        sequence.Value.Should().Be(StopSequence.MaxValue);
    }

    [Fact]
    public void ExplicitFromInt_InvalidValue_ThrowsException()
    {
        // Act
        var act = () => (StopSequence)0;

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void From_InvalidValue_ThrowsException()
    {
        // Act
        var act = () => StopSequence.From(0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion

    #region Comparison Edge Cases

    [Fact]
    public void CompareTo_WithSelf_ReturnsZero()
    {
        // Arrange
        var sequence = new StopSequence(5);

        // Act & Assert
        sequence.CompareTo(sequence).Should().Be(0);
    }

    [Fact]
    public void LessThanOrEqual_SameValue_ReturnsTrue()
    {
        // Arrange
        var sequence1 = new StopSequence(5);
        var sequence2 = new StopSequence(5);

        // Act & Assert
        (sequence1 <= sequence2).Should().BeTrue();
    }

    [Fact]
    public void GreaterThanOrEqual_SameValue_ReturnsTrue()
    {
        // Arrange
        var sequence1 = new StopSequence(5);
        var sequence2 = new StopSequence(5);

        // Act & Assert
        (sequence1 >= sequence2).Should().BeTrue();
    }

    [Fact]
    public void Sorting_ReturnsAscendingOrder()
    {
        // Arrange
        var sequences = new[]
        {
            new StopSequence(5),
            new StopSequence(1),
            new StopSequence(10),
            new StopSequence(3),
            new StopSequence(7)
        };

        // Act
        var sorted = sequences.OrderBy(s => s).ToArray();

        // Assert
        sorted.Select(s => s.Value).Should().ContainInOrder(1, 3, 5, 7, 10);
    }

    #endregion

    #region Transit Scenario Tests

    /// <summary>
    /// Tests from the acceptance criteria:
    /// GIVEN a StopSequence, WHEN calling Next(), THEN the sequence increments correctly with validation
    /// </summary>
    [Fact]
    public void AcceptanceCriteria_Next_IncrementsCorrectly()
    {
        // Arrange - representing stops along a route
        var firstStop = StopSequence.First;

        // Act
        var secondStop = firstStop.Next();
        var thirdStop = secondStop.Next();

        // Assert
        firstStop.Value.Should().Be(1);
        secondStop.Value.Should().Be(2);
        thirdStop.Value.Should().Be(3);

        // Also verify validation - Next() is validated by throwing at max
        var maxSequence = new StopSequence(StopSequence.MaxValue);
        var actAtMax = () => maxSequence.Next();
        actAtMax.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void TransitScenario_TypicalBusRoute_SequentialStops()
    {
        // Arrange - a bus route with 10 stops
        var stopSequences = new List<StopSequence>();
        var current = StopSequence.First;

        // Act - simulate building a route
        for (int i = 0; i < 10; i++)
        {
            stopSequences.Add(current);
            if (i < 9) // Don't increment after last stop
                current = current.Next();
        }

        // Assert
        stopSequences.Should().HaveCount(10);
        stopSequences.First().Value.Should().Be(1);
        stopSequences.Last().Value.Should().Be(10);

        // All stops should be in sequence
        for (int i = 0; i < stopSequences.Count; i++)
        {
            stopSequences[i].Value.Should().Be(i + 1);
        }
    }

    [Fact]
    public void TransitScenario_LongDistanceTrain_ManyStops()
    {
        // Arrange - a long-distance train might have many stops
        var numberOfStops = 50;
        var firstStop = StopSequence.First;
        var current = firstStop;

        // Act - traverse to the last stop
        for (int i = 1; i < numberOfStops; i++)
        {
            current = current.Next();
        }

        // Assert
        current.Value.Should().Be(numberOfStops);
    }

    [Fact]
    public void TransitScenario_CheckingStopPosition()
    {
        // Arrange - check if a stop is the first, middle, or approaching end
        var firstStop = StopSequence.First;
        var middleStop = new StopSequence(5);
        var nearEndStop = new StopSequence(9999);

        // Assert
        firstStop.IsFirst.Should().BeTrue();
        middleStop.IsFirst.Should().BeFalse();
        nearEndStop.IsFirst.Should().BeFalse();

        // Can still go next from near end
        nearEndStop.Next().Value.Should().Be(10000);
    }

    [Fact]
    public void TransitScenario_StopOrdering_ComparisonWorksCorrectly()
    {
        // Arrange - stops along a route
        var departureStop = new StopSequence(1);
        var intermediateStop = new StopSequence(5);
        var arrivalStop = new StopSequence(10);

        // Assert - ordering is correct
        departureStop.Should().BeLessThan(intermediateStop);
        intermediateStop.Should().BeLessThan(arrivalStop);
        departureStop.Should().BeLessThan(arrivalStop);

        // Can be used in conditions
        (intermediateStop > departureStop && intermediateStop < arrivalStop).Should().BeTrue();
    }

    [Fact]
    public void TransitScenario_FindingRemainingStops()
    {
        // Arrange
        var currentStop = new StopSequence(3);
        var totalStops = 10;
        var finalStop = new StopSequence(totalStops);

        // Act - calculate remaining stops using comparison
        var remainingStops = (int)finalStop - (int)currentStop;

        // Assert
        remainingStops.Should().Be(7);
    }

    [Fact]
    public void TransitScenario_GroupStopsByRange()
    {
        // Arrange - categorize stops in a route
        var stops = Enumerable.Range(1, 20)
            .Select(i => new StopSequence(i))
            .ToList();

        // Act - group into first third, middle, last third
        var firstThird = stops.Where(s => s.Value <= 7).ToList();
        var middleThird = stops.Where(s => s.Value > 7 && s.Value <= 14).ToList();
        var lastThird = stops.Where(s => s.Value > 14).ToList();

        // Assert
        firstThird.Should().HaveCount(7);
        middleThird.Should().HaveCount(7);
        lastThird.Should().HaveCount(6);
    }

    #endregion

    #region Immutability Tests

    [Fact]
    public void Next_DoesNotModifyOriginal()
    {
        // Arrange
        var original = new StopSequence(5);
        var originalValue = original.Value;

        // Act
        var next = original.Next();

        // Assert - original is unchanged (readonly struct)
        original.Value.Should().Be(originalValue);
        next.Value.Should().Be(6);
    }

    [Fact]
    public void Previous_DoesNotModifyOriginal()
    {
        // Arrange
        var original = new StopSequence(5);
        var originalValue = original.Value;

        // Act
        var previous = original.Previous();

        // Assert - original is unchanged (readonly struct)
        original.Value.Should().Be(originalValue);
        previous.Value.Should().Be(4);
    }

    #endregion
}
