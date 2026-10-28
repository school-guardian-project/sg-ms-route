using ms_route.Api.Application.UseCase;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Service;
using ms_route.Tests.Fakes;
using RouteModel = ms_route.Api.Domain.Model.Route;
using NUnit.Framework;

namespace ms_route.Tests.Route;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class AssignBusToRouteServiceTests
{
    private readonly InMemoryRouteScheduleRepository _schedules = new();

    private AssignBusToRouteService Service(
        InMemoryRouteBusAssignmentRepository assignments,
        InMemoryRouteRepository routes)
        => new(assignments, routes, new RouteScheduleProvisioner(_schedules), TestMapper.Create());

    [Test]
    public async Task ExecuteAsync_ConRutaYBusesLibres_AsignaElBus()
    {
        var routeId = Guid.NewGuid();
        var busId = Guid.NewGuid();

        var routes = new InMemoryRouteRepository();
        routes.Routes.Add(new RouteModel { Id = routeId, Name = "R-01", Status = Status.Active });

        var assignments = new InMemoryRouteBusAssignmentRepository();
        var service = Service(assignments, routes);

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
        var service = Service(assignments, new InMemoryRouteRepository());

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

        var service = Service(assignments, routes);

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

        var service = Service(assignments, routes);

        Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ExecuteAsync(routeId, busId));
    }

    [Test]
    public async Task ExecuteAsync_AlAsignarBus_CreaHorariosIdaYVueltaDeLunesAViernesConLaHoraDeLaRuta()
    {
        var routeId = Guid.NewGuid();
        var routes = new InMemoryRouteRepository();
        routes.Routes.Add(new RouteModel
        {
            Id = routeId, Name = "R-01", Status = Status.Active, StartTime = new TimeOnly(6, 30)
        });
        var assignments = new InMemoryRouteBusAssignmentRepository();

        var result = await Service(assignments, routes).ExecuteAsync(routeId, Guid.NewGuid());

        Assert.That(_schedules.Schedules, Has.Exactly(10).Items);
        Assert.That(_schedules.Schedules.All(s =>
            s.RouteBusAssignmentsId == result.Id
            && s.StartTime == new TimeOnly(6, 30)
            && s.Status == Status.Active), Is.True);
        Assert.That(_schedules.Schedules.Select(s => s.DayOfWeek).Distinct(),
            Is.EquivalentTo(RouteScheduleProvisioner.WorkingDays));
        Assert.That(_schedules.Schedules.Count(s => s.Direction == "Ida"), Is.EqualTo(5));
        Assert.That(_schedules.Schedules.Count(s => s.Direction == "Vuelta"), Is.EqualTo(5));
    }

    [Test]
    public async Task ExecuteAsync_AlAsignarBus_UsaLosDiasConfiguradosEnLaRuta()
    {
        var routeId = Guid.NewGuid();
        var routes = new InMemoryRouteRepository();
        routes.Routes.Add(new RouteModel
        {
            Id = routeId,
            Name = "R-01",
            Status = Status.Active,
            DaysOfWeek = ["Monday", "Wednesday"]
        });
        var assignments = new InMemoryRouteBusAssignmentRepository();

        await Service(assignments, routes).ExecuteAsync(routeId, Guid.NewGuid());

        Assert.That(_schedules.Schedules, Has.Exactly(4).Items);
        Assert.That(_schedules.Schedules.Select(schedule => schedule.DayOfWeek).Distinct(),
            Is.EquivalentTo(new[] { DayOfWeek.Monday, DayOfWeek.Wednesday }));
    }

    [Test]
    public void ExecuteAsync_SiLaAsignacionSeRechaza_NoCreaHorarios()
    {
        var routeId = Guid.NewGuid();
        var routes = new InMemoryRouteRepository();
        routes.Routes.Add(new RouteModel { Id = routeId, Name = "R-01", Status = Status.Active });
        var assignments = new InMemoryRouteBusAssignmentRepository();
        assignments.Assignments.Add(new RouteBusAssignment
        {
            Id = Guid.NewGuid(), RouteId = routeId, BusId = Guid.NewGuid(), Status = Status.Active
        });

        Assert.ThrowsAsync<InvalidOperationException>(() =>
            Service(assignments, routes).ExecuteAsync(routeId, Guid.NewGuid()));

        Assert.That(_schedules.Schedules, Is.Empty);
    }
}
