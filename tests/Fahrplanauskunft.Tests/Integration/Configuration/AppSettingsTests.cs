using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace Fahrplanauskunft.Tests.Integration.Configuration;

/// <summary>
/// Shared fixture for configuration tests - loads configuration once and reuses across all tests.
/// This improves test performance by avoiding repeated file I/O and JSON parsing.
/// </summary>
public class AppSettingsFixture
{
    public string CliProjectPath { get; }
    public IConfiguration Configuration { get; }
    public IConfiguration ConfigurationWithDevelopment { get; }

    public AppSettingsFixture()
    {
        // Navigate from test bin to CLI project
        var testDirectory = AppContext.BaseDirectory;
        CliProjectPath = Path.GetFullPath(
            Path.Combine(testDirectory, "..", "..", "..", "..", "..", "src", "Fahrplanauskunft.CLI"));

        // Build configurations once for all tests
        Configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(CliProjectPath, "appsettings.json"), optional: false)
            .Build();

        ConfigurationWithDevelopment = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(CliProjectPath, "appsettings.json"), optional: false)
            .AddJsonFile(Path.Combine(CliProjectPath, "appsettings.Development.json"), optional: true)
            .Build();
    }
}

/// <summary>
/// Tests for verifying the configuration files are valid and contain expected sections.
/// These tests ensure the appsettings.json files created in Issue #5 are properly structured.
/// Uses IClassFixture to share configuration loading across all tests for better performance.
/// </summary>
public class AppSettingsTests : IClassFixture<AppSettingsFixture>
{
    private readonly AppSettingsFixture _fixture;

    public AppSettingsTests(AppSettingsFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void AppSettings_FileExists()
    {
        // Arrange
        var appSettingsPath = Path.Combine(_fixture.CliProjectPath, "appsettings.json");

        // Act & Assert
        File.Exists(appSettingsPath).Should().BeTrue(
            because: "appsettings.json should exist in the CLI project");
    }

    [Fact]
    public void AppSettingsDevelopment_FileExists()
    {
        // Arrange
        var appSettingsPath = Path.Combine(_fixture.CliProjectPath, "appsettings.Development.json");

        // Act & Assert
        File.Exists(appSettingsPath).Should().BeTrue(
            because: "appsettings.Development.json should exist in the CLI project");
    }

    [Fact]
    public void AppSettings_CanBeLoaded()
    {
        // Assert - configuration was loaded in fixture, verify it's not null
        _fixture.Configuration.Should().NotBeNull();
    }

    [Fact]
    public void AppSettings_ContainsConnectionStringsSection()
    {
        // Act
        var connectionString = _fixture.Configuration.GetConnectionString("DefaultConnection");

        // Assert
        connectionString.Should().NotBeNullOrEmpty(
            because: "DefaultConnection should be configured for PostgreSQL");
    }

    [Fact]
    public void AppSettings_ConnectionString_ContainsRequiredComponents()
    {
        // Act
        var connectionString = _fixture.Configuration.GetConnectionString("DefaultConnection");

        // Assert
        connectionString.Should().Contain("Host=", because: "connection string needs a host");
        connectionString.Should().Contain("Port=", because: "connection string needs a port");
        connectionString.Should().Contain("Database=", because: "connection string needs a database name");
        connectionString.Should().Contain("Username=", because: "connection string needs a username");
        connectionString.Should().Contain("Password=", because: "connection string needs a password");
    }

    [Fact]
    public void AppSettings_ContainsLoggingSection()
    {
        // Act
        var loggingSection = _fixture.Configuration.GetSection("Logging");

        // Assert
        loggingSection.Exists().Should().BeTrue(
            because: "Logging section should be configured");
        loggingSection.GetSection("LogLevel").Exists().Should().BeTrue(
            because: "LogLevel should be defined within Logging");
    }

    [Fact]
    public void AppSettings_ContainsRaptorSection()
    {
        // Act
        var raptorSection = _fixture.Configuration.GetSection("Raptor");

        // Assert
        raptorSection.Exists().Should().BeTrue(
            because: "Raptor algorithm configuration should be defined");
    }

    [Theory]
    [InlineData("MaxTransfers")]
    [InlineData("WalkingSpeedKmh")]
    [InlineData("MaxWalkingDistanceMeters")]
    [InlineData("TransferPenaltyMinutes")]
    public void AppSettings_RaptorSection_ContainsExpectedSettings(string settingName)
    {
        // Act
        var value = _fixture.Configuration.GetSection("Raptor")[settingName];

        // Assert
        value.Should().NotBeNullOrEmpty(
            because: $"Raptor:{settingName} should be configured");
    }

    [Fact]
    public void AppSettings_RaptorMaxTransfers_HasReasonableDefault()
    {
        // Act
        var maxTransfers = _fixture.Configuration.GetValue<int>("Raptor:MaxTransfers");

        // Assert
        maxTransfers.Should().BeGreaterThan(0, because: "MaxTransfers must be positive")
            .And.BeLessThanOrEqualTo(10, because: "MaxTransfers should have a reasonable limit");
    }

    [Fact]
    public void AppSettingsDevelopment_OverridesLoggingLevel()
    {
        // Act
        var defaultLogLevel = _fixture.ConfigurationWithDevelopment["Logging:LogLevel:Default"];

        // Assert
        defaultLogLevel.Should().Be("Debug",
            because: "Development environment should use Debug logging level");
    }
}
