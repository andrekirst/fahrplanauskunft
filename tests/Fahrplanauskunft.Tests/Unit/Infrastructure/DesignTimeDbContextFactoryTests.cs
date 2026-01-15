using Fahrplanauskunft.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fahrplanauskunft.Tests.Unit.Infrastructure;

/// <summary>
/// Unit tests for DesignTimeDbContextFactory.
/// Tests our factory implementation - not EF Core's behavior.
/// </summary>
public class DesignTimeDbContextFactoryTests : IDisposable
{
    private readonly string? _originalEnvValue;

    public DesignTimeDbContextFactoryTests()
    {
        _originalEnvValue = Environment.GetEnvironmentVariable("FAHRPLAN_CONNECTION_STRING");
    }

    public void Dispose()
    {
        if (_originalEnvValue != null)
        {
            Environment.SetEnvironmentVariable("FAHRPLAN_CONNECTION_STRING", _originalEnvValue);
        }
        else
        {
            Environment.SetEnvironmentVariable("FAHRPLAN_CONNECTION_STRING", null);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public void CreateDbContext_ReturnsConfiguredContext()
    {
        // Arrange
        var factory = new DesignTimeDbContextFactory();

        // Act
        var context = factory.CreateDbContext(Array.Empty<string>());

        // Assert - Our factory should create a valid context with PostgreSQL
        context.Should().NotBeNull();
        context.Should().BeOfType<FahrplanDbContext>();
        context.Database.ProviderName.Should().Be("Npgsql.EntityFrameworkCore.PostgreSQL");
    }

    [Fact]
    public void CreateDbContext_UsesEnvironmentVariable_WhenSet()
    {
        // Arrange - Set custom connection string via environment variable
        Environment.SetEnvironmentVariable("FAHRPLAN_CONNECTION_STRING",
            "Host=custom-host;Database=custom_db;Username=user;Password=pass");
        var factory = new DesignTimeDbContextFactory();

        // Act
        var context = factory.CreateDbContext(Array.Empty<string>());

        // Assert - Factory should not throw when using custom connection string
        context.Should().NotBeNull();
    }

    [Fact]
    public void CreateDbContext_UsesDefaultConnectionString_WhenEnvironmentVariableNotSet()
    {
        // Arrange - Ensure no environment variable is set
        Environment.SetEnvironmentVariable("FAHRPLAN_CONNECTION_STRING", null);
        var factory = new DesignTimeDbContextFactory();

        // Act
        var context = factory.CreateDbContext(Array.Empty<string>());

        // Assert - Factory should use default and not throw
        context.Should().NotBeNull();
    }
}
