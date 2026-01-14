using FluentAssertions;

namespace Fahrplanauskunft.Tests.Architecture;

/// <summary>
/// Tests that verify the solution structure follows the expected conventions.
/// </summary>
public class SolutionStructureTests
{
    [Fact]
    public void Core_Assembly_ShouldExist()
    {
        // Act
        var coreAssembly = typeof(Core.Domain.Station).Assembly;

        // Assert
        coreAssembly.Should().NotBeNull();
        coreAssembly.GetName().Name.Should().Be("Fahrplanauskunft.Core");
    }

    [Fact]
    public void Application_Assembly_ShouldExist()
    {
        // Act
        var applicationAssembly = typeof(Application.UseCases.GetStationByIdUseCase).Assembly;

        // Assert
        applicationAssembly.Should().NotBeNull();
        applicationAssembly.GetName().Name.Should().Be("Fahrplanauskunft.Application");
    }

    [Fact]
    public void Infrastructure_Assembly_ShouldExist()
    {
        // Act
        var infrastructureAssembly = typeof(Infrastructure.Adapters.InMemoryStationRepository).Assembly;

        // Assert
        infrastructureAssembly.Should().NotBeNull();
        infrastructureAssembly.GetName().Name.Should().Be("Fahrplanauskunft.Infrastructure");
    }

    [Fact]
    public void Core_ShouldContain_DomainNamespace()
    {
        // Act
        var domainTypes = typeof(Core.Domain.Station).Assembly
            .GetTypes()
            .Where(t => t.Namespace?.StartsWith("Fahrplanauskunft.Core.Domain", StringComparison.Ordinal) == true)
            .ToList();

        // Assert
        domainTypes.Should().NotBeEmpty("Core should contain domain entities");
    }

    [Fact]
    public void Core_ShouldContain_PortsNamespace()
    {
        // Act
        var portTypes = typeof(Core.Domain.Station).Assembly
            .GetTypes()
            .Where(t => t.Namespace?.StartsWith("Fahrplanauskunft.Core.Ports", StringComparison.Ordinal) == true)
            .ToList();

        // Assert
        portTypes.Should().NotBeEmpty("Core should contain port interfaces");
    }

    [Fact]
    public void Core_ShouldContain_ValueObjectsNamespace()
    {
        // Act
        var valueObjectTypes = typeof(Core.Domain.Station).Assembly
            .GetTypes()
            .Where(t => t.Namespace?.StartsWith("Fahrplanauskunft.Core.ValueObjects", StringComparison.Ordinal) == true)
            .ToList();

        // Assert
        valueObjectTypes.Should().NotBeEmpty("Core should contain value objects");
    }

    [Fact]
    public void Application_ShouldContain_UseCasesNamespace()
    {
        // Act
        var useCaseTypes = typeof(Application.UseCases.GetStationByIdUseCase).Assembly
            .GetTypes()
            .Where(t => t.Namespace?.StartsWith("Fahrplanauskunft.Application.UseCases", StringComparison.Ordinal) == true)
            .ToList();

        // Assert
        useCaseTypes.Should().NotBeEmpty("Application should contain use cases");
    }

    [Fact]
    public void Infrastructure_ShouldContain_AdaptersNamespace()
    {
        // Act
        var adapterTypes = typeof(Infrastructure.Adapters.InMemoryStationRepository).Assembly
            .GetTypes()
            .Where(t => t.Namespace?.StartsWith("Fahrplanauskunft.Infrastructure.Adapters", StringComparison.Ordinal) == true)
            .ToList();

        // Assert
        adapterTypes.Should().NotBeEmpty("Infrastructure should contain adapters");
    }

    [Fact]
    public void Ports_ShouldOnlyContain_Interfaces()
    {
        // Act
        var portTypes = typeof(Core.Domain.Station).Assembly
            .GetTypes()
            .Where(t => t.Namespace?.StartsWith("Fahrplanauskunft.Core.Ports", StringComparison.Ordinal) == true)
            .ToList();

        // Assert
        foreach (var portType in portTypes)
        {
            portType.IsInterface.Should().BeTrue($"{portType.Name} in Ports namespace should be an interface");
        }
    }

    [Fact]
    public void Adapters_ShouldImplement_PortInterfaces()
    {
        // Arrange
        var portInterfaces = typeof(Core.Domain.Station).Assembly
            .GetTypes()
            .Where(t => t.Namespace?.StartsWith("Fahrplanauskunft.Core.Ports", StringComparison.Ordinal) == true && t.IsInterface)
            .ToList();

        var adapterTypes = typeof(Infrastructure.Adapters.InMemoryStationRepository).Assembly
            .GetTypes()
            .Where(t => t.Namespace?.StartsWith("Fahrplanauskunft.Infrastructure.Adapters", StringComparison.Ordinal) == true && t.IsClass)
            .ToList();

        // Act & Assert
        foreach (var adapter in adapterTypes)
        {
            var implementedInterfaces = adapter.GetInterfaces()
                .Where(i => portInterfaces.Contains(i))
                .ToList();

            implementedInterfaces.Should().NotBeEmpty(
                $"{adapter.Name} should implement at least one port interface from Core");
        }
    }
}
