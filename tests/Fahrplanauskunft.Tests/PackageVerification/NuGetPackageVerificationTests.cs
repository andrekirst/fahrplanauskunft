using System.Globalization;
using Ardalis.GuardClauses;
using Bogus;
using CSharpFunctionalExtensions;
using FluentValidation;
using NSubstitute;
using Fahrplanauskunft.Core.Domain;
using Fahrplanauskunft.Core.Ports;

namespace Fahrplanauskunft.Tests.PackageVerification;

/// <summary>
/// Tests that verify all required NuGet packages from Issue #2 are properly installed
/// and accessible. These tests ensure the packages can be used in the test project.
/// </summary>
public class NuGetPackageVerificationTests
{
    #region Core Layer Packages

    [Fact]
    public void CSharpFunctionalExtensions_Result_CanCreateSuccessResult()
    {
        // Arrange & Act
        var result = Result.Success("Test value");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Test value");
    }

    [Fact]
    public void CSharpFunctionalExtensions_Result_CanCreateFailureResult()
    {
        // Arrange & Act
        var result = Result.Failure<string>("Error occurred");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Error occurred");
    }

    [Fact]
    public void CSharpFunctionalExtensions_Maybe_CanCreateWithValue()
    {
        // Arrange & Act
        Maybe<string> maybe = "Test value";

        // Assert
        maybe.HasValue.Should().BeTrue();
        maybe.Value.Should().Be("Test value");
    }

    [Fact]
    public void CSharpFunctionalExtensions_Maybe_CanCreateNone()
    {
        // Arrange & Act
        Maybe<string> maybe = Maybe<string>.None;

        // Assert
        maybe.HasNoValue.Should().BeTrue();
    }

    [Fact]
    public void ArdalisGuardClauses_Guard_CanValidateNonNull()
    {
        // Arrange
        var validValue = "Test";

        // Act
        var guarded = Guard.Against.Null(validValue);

        // Assert
        guarded.Should().Be(validValue);
    }

    [Fact]
    public void ArdalisGuardClauses_Guard_ThrowsOnNull()
    {
        // Arrange
        string? nullValue = null;

        // Act
        var act = () => Guard.Against.Null(nullValue);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ArdalisGuardClauses_Guard_CanValidateNonEmptyString()
    {
        // Arrange
        var validString = "Test";

        // Act
        var guarded = Guard.Against.NullOrWhiteSpace(validString);

        // Assert
        guarded.Should().Be(validString);
    }

    [Fact]
    public void ArdalisGuardClauses_Guard_ThrowsOnEmptyString()
    {
        // Act
        var act = () => Guard.Against.NullOrWhiteSpace("");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region Application Layer Packages

    [Fact]
    public void FluentValidation_Validator_CanValidateObject()
    {
        // Arrange
        var validator = new StationDtoValidator();
        var validDto = new StationDto("HBF", "Hamburg Hauptbahnhof");

        // Act
        var result = validator.Validate(validDto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void FluentValidation_Validator_ReturnsErrorsForInvalidObject()
    {
        // Arrange
        var validator = new StationDtoValidator();
        var invalidDto = new StationDto("", "");

        // Act
        var result = validator.Validate(invalidDto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    #endregion

    #region Test Layer Packages

    [Fact]
    public void NSubstitute_CanCreateMock()
    {
        // Arrange
        var mockRepository = Substitute.For<IStationRepository>();
        var expectedStation = new Station("HBF", "Hamburg Hauptbahnhof");
        mockRepository.GetById("HBF").Returns(expectedStation);

        // Act
        var result = mockRepository.GetById("HBF");

        // Assert
        result.Should().Be(expectedStation);
        mockRepository.Received(1).GetById("HBF");
    }

    [Fact]
    public void NSubstitute_CanVerifyMethodWasNotCalled()
    {
        // Arrange
        var mockRepository = Substitute.For<IStationRepository>();

        // Act - don't call anything

        // Assert
        mockRepository.DidNotReceive().GetById(Arg.Any<string>());
    }

    [Fact]
    public void Bogus_CanGenerateFakeData()
    {
        // Arrange - Use CustomInstantiator for records (which have no parameterless constructor)
        var faker = new Faker<StationDto>()
            .CustomInstantiator(f => new StationDto(
                f.Random.AlphaNumeric(5).ToUpper(CultureInfo.InvariantCulture),
                f.Address.City() + " " + f.Random.Word()));

        // Act
        var fakeStations = faker.Generate(10);

        // Assert
        fakeStations.Should().HaveCount(10);
        fakeStations.Should().AllSatisfy(s =>
        {
            s.Id.Should().NotBeNullOrWhiteSpace();
            s.Name.Should().NotBeNullOrWhiteSpace();
        });
    }

    [Fact]
    public void Bogus_CanGenerateGermanLocaleData()
    {
        // Arrange
        var faker = new Faker("de");

        // Act
        var city = faker.Address.City();
        var name = faker.Name.FullName();

        // Assert
        city.Should().NotBeNullOrWhiteSpace();
        name.Should().NotBeNullOrWhiteSpace();
    }

    #endregion

    #region Helper Classes for Testing

    private sealed record StationDto(string Id, string Name);

    private sealed class StationDtoValidator : AbstractValidator<StationDto>
    {
        public StationDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Station ID is required")
                .MaximumLength(10).WithMessage("Station ID must be 10 characters or less");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Station name is required")
                .MaximumLength(100).WithMessage("Station name must be 100 characters or less");
        }
    }

    #endregion
}
