using FluentAssertions;
using NetArchTest.Rules;

namespace Fahrplanauskunft.Tests.Architecture;

/// <summary>
/// Tests that verify the hexagonal architecture dependency rules are maintained.
/// These tests ensure that the dependency flow follows: CLI -> Application/Infrastructure -> Core
/// Core must have ZERO dependencies on other layers.
/// </summary>
public class HexagonalArchitectureTests
{
    private const string CoreNamespace = "Fahrplanauskunft.Core";
    private const string ApplicationNamespace = "Fahrplanauskunft.Application";
    private const string InfrastructureNamespace = "Fahrplanauskunft.Infrastructure";
    private const string CliNamespace = "Fahrplanauskunft.CLI";

    [Fact]
    public void Core_ShouldNotDependOn_Application()
    {
        var coreAssembly = typeof(Core.Domain.Station).Assembly;

        var result = Types
            .InAssembly(coreAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApplicationNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Core layer must not depend on Application layer. " +
            "Violating types: {0}",
            string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Core_ShouldNotDependOn_Infrastructure()
    {
        var coreAssembly = typeof(Core.Domain.Station).Assembly;

        var result = Types
            .InAssembly(coreAssembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Core layer must not depend on Infrastructure layer. " +
            "Violating types: {0}",
            string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Core_ShouldNotDependOn_CLI()
    {
        var coreAssembly = typeof(Core.Domain.Station).Assembly;

        var result = Types
            .InAssembly(coreAssembly)
            .ShouldNot()
            .HaveDependencyOn(CliNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Core layer must not depend on CLI layer. " +
            "Violating types: {0}",
            string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Application_ShouldNotDependOn_Infrastructure()
    {
        var applicationAssembly = typeof(Application.UseCases.GetStationByIdUseCase).Assembly;

        var result = Types
            .InAssembly(applicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Application layer must not depend on Infrastructure layer. " +
            "Use dependency injection via ports (interfaces in Core). " +
            "Violating types: {0}",
            string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Application_ShouldNotDependOn_CLI()
    {
        var applicationAssembly = typeof(Application.UseCases.GetStationByIdUseCase).Assembly;

        var result = Types
            .InAssembly(applicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(CliNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Application layer must not depend on CLI layer. " +
            "Violating types: {0}",
            string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Application_ShouldDependOn_Core()
    {
        var applicationAssembly = typeof(Application.UseCases.GetStationByIdUseCase).Assembly;

        var result = Types
            .InAssembly(applicationAssembly)
            .That()
            .ResideInNamespace(ApplicationNamespace)
            .Should()
            .HaveDependencyOn(CoreNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Application layer should depend on Core layer for domain entities and ports.");
    }

    [Fact]
    public void Infrastructure_ShouldNotDependOn_Application()
    {
        var infrastructureAssembly = typeof(Infrastructure.Adapters.InMemoryStationRepository).Assembly;

        var result = Types
            .InAssembly(infrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApplicationNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Infrastructure layer must not depend on Application layer. " +
            "Infrastructure implements ports from Core, not Application. " +
            "Violating types: {0}",
            string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Infrastructure_ShouldNotDependOn_CLI()
    {
        var infrastructureAssembly = typeof(Infrastructure.Adapters.InMemoryStationRepository).Assembly;

        var result = Types
            .InAssembly(infrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn(CliNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Infrastructure layer must not depend on CLI layer. " +
            "Violating types: {0}",
            string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Infrastructure_ShouldDependOn_Core()
    {
        var infrastructureAssembly = typeof(Infrastructure.Adapters.InMemoryStationRepository).Assembly;

        var result = Types
            .InAssembly(infrastructureAssembly)
            .That()
            .ResideInNamespace(InfrastructureNamespace)
            .Should()
            .HaveDependencyOn(CoreNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Infrastructure layer should depend on Core layer to implement ports.");
    }
}
