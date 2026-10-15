using ms_route.Api.Application.UseCase;
using ms_route.Api.Domain.Model;
using ms_route.Tests.Fakes;
using RouteModel = ms_route.Api.Domain.Model.Route;
using NUnit.Framework;

namespace ms_route.Tests.Route;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class AssignBusToRouteServiceTests
{
    [Test]
    public async Task ExecuteAsync_ConRutaYBusesLibres_AsignaElBus()
    {
        var routeId = Guid.NewGuid();
        var busId = Guid.NewGuid();

        var routes = new InMemoryRouteRepository();
        routes.Routes.Add(new RouteModel { Id = routeId, Name = "R-01", Status = Status.Active });

        var assignments = new InMemoryRouteBusAssignmentRepository();
        var service = new AssignBusToRouteService(assignments, routes, TestMapper.Create());

        var result = await service.ExecuteAsync(routeId, busId);

        Assert.That(assignments.Assignments, Has.Exactly(1).Items);
        Assert.That(result.RouteId, Is.EqualTo(routeId));
        Assert.That(result.BusId, Is.EqualTo(busId));
        Assert.That(result.Status, Is.EqualTo("Active"));
    }

    [Test]
    public async Task ExecuteAsync_ConRutaInexistente_LanzaExcepcion()
    {
        var assignments = new InMemoryRouteBusAssignmentRepository();
        var service = new AssignBusToRouteService(
            assignments, new InMemoryRouteRepository(), TestMapper.Create());

        Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ExecuteAsync(Guid.NewGuid(), Guid.NewGuid()));

        Assert.That(assignments.Assignments, Is.Empty);
    }

    [Test]
    public async Task ExecuteAsync_ConRutaQueYaTieneBus_Rechaza()
    {
        var routeId = Guid.NewGuid();
        var routes = new InMemoryRouteRepository();
        routes.Routes.Add(new RouteModel { Id = routeId, Name = "R-01", Status = Status.Active });

        var assignments = new InMemoryRouteBusAssignmentRepository();
        assignments.Assignments.Add(new RouteBusAssignment
        {
            Id = Guid.NewGuid(),
            RouteId = routeId,
            BusId = Guid.NewGuid(),
            Status = Status.Active
        });

        var service = new AssignBusToRouteService(assignments, routes, TestMapper.Create());

        Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ExecuteAsync(routeId, Guid.NewGuid()));

        Assert.That(assignments.Assignments, Has.Exactly(1).Items);
    }

    [Test]
    public async Task ExecuteAsync_ConBusAsignadoAOtraRuta_Rechaza()
    {
        var routeId = Guid.NewGuid();
        var otherRouteId = Guid.NewGuid();
        var busId = Guid.NewGuid();

        var routes = new InMemoryRouteRepository();
        routes.Routes.Add(new RouteModel { Id = routeId, Name = "R-01", Status = Status.Active });

        var assignments = new InMemoryRouteBusAssignmentRepository();
        assignments.Assignments.Add(new RouteBusAssignment
        {
            Id = Guid.NewGuid(),
            RouteId = otherRouteId,
            BusId = busId,
            Status = Status.Active
        });

        var service = new AssignBusToRouteService(assignments, routes, TestMapper.Create());

        Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ExecuteAsync(routeId, busId));
    }
}
