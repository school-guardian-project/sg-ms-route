using ms_route.Api.Application.UseCase;
using ms_route.Api.Domain.Model;
using ms_route.Tests.Fakes;
using RouteModel = ms_route.Api.Domain.Model.Route;
using NUnit.Framework;

namespace ms_route.Tests.Route;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class DeleteRouteServiceTests
{
    [Test]
    public async Task ExecuteAsync_ConRutaExistente_EliminaRuta()
    {
        var repo = new InMemoryRouteRepository();
        var routeId = Guid.NewGuid();
        repo.Routes.Add(new RouteModel { Id = routeId, Name = "R-01", TargetSector = "Norte", Status = Status.Active });
        var service = new DeleteRouteService(repo, new InMemoryRouteExecutionRepository());

        await service.ExecuteAsync(routeId);

        Assert.That(repo.Routes, Is.Empty);
    }

    [Test]
    public async Task ExecuteAsync_ConRutaInexistente_LanzaExcepcion()
    {
        var repo = new InMemoryRouteRepository();
        var service = new DeleteRouteService(repo, new InMemoryRouteExecutionRepository());

        Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ExecuteAsync(Guid.NewGuid()));
    }

    [Test]
    public async Task ExecuteAsync_ConViajeActivo_RechazaYNoElimina()
    {
        var repo = new InMemoryRouteRepository();
        var routeId = Guid.NewGuid();
        repo.Routes.Add(new RouteModel { Id = routeId, Name = "R-01", TargetSector = "Norte", Status = Status.Active });

        var executions = new InMemoryRouteExecutionRepository();
        executions.Executions.Add(new RouteExecution
        {
            Id = Guid.NewGuid(),
            RouteId = routeId,
            BusId = Guid.NewGuid(),
            DriverId = Guid.NewGuid(),
            StartDateTime = DateTime.UtcNow,
            Status = Status.Active
        });

        var service = new DeleteRouteService(repo, executions);

        Assert.ThrowsAsync<InvalidOperationException>(() => service.ExecuteAsync(routeId));
        Assert.That(repo.Routes, Has.Exactly(1).Items);
    }

    [Test]
    public async Task ExecuteAsync_ConViajeFinalizado_EliminaRuta()
    {
        var repo = new InMemoryRouteRepository();
        var routeId = Guid.NewGuid();
        repo.Routes.Add(new RouteModel { Id = routeId, Name = "R-01", TargetSector = "Norte", Status = Status.Active });

        var executions = new InMemoryRouteExecutionRepository();
        executions.Executions.Add(new RouteExecution
        {
            Id = Guid.NewGuid(),
            RouteId = routeId,
            BusId = Guid.NewGuid(),
            DriverId = Guid.NewGuid(),
            StartDateTime = DateTime.UtcNow.AddHours(-1),
            EndDateTime = DateTime.UtcNow,
            Status = Status.Inactive
        });

        var service = new DeleteRouteService(repo, executions);

        await service.ExecuteAsync(routeId);

        Assert.That(repo.Routes, Is.Empty);
    }
}
