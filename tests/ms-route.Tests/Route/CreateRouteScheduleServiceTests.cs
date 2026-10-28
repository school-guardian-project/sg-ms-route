using ms_route.Api.Application.Dto;
using ms_route.Api.Application.UseCase;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Service;
using ms_route.Tests.Fakes;
using NUnit.Framework;
using RouteModel = ms_route.Api.Domain.Model.Route;

namespace ms_route.Tests.Route;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class CreateRouteScheduleServiceTests
{
    private static InMemoryRouteBusAssignmentRepository AssignmentFor(Guid routeId)
    {
        var assignments = new InMemoryRouteBusAssignmentRepository();
        assignments.Assignments.Add(new RouteBusAssignment
        {
            Id = Guid.NewGuid(),
            RouteId = routeId,
            BusId = Guid.NewGuid(),
            Status = Status.Active
        });
        return assignments;
    }

    private static InMemoryRouteRepository RouteFor(Guid routeId)
    {
        var routes = new InMemoryRouteRepository();
        routes.Routes.Add(new RouteModel
        {
            Id = routeId,
            Name = "Test Route",
            TargetSector = "Norte",
            Status = Status.Active,
            StartTime = new TimeOnly(6, 30)
        });
        return routes;
    }

    [Test]
    public async Task ExecuteAsync_ConBusAsignadoYCamposValidos_CreaElHorario()
    {
        var routeId = Guid.NewGuid();
        var assignments = AssignmentFor(routeId);
        var schedules = new InMemoryRouteScheduleRepository();
        var routes = RouteFor(routeId);
        var service = new CreateRouteScheduleService(new RouteScheduleProvisioner(schedules), assignments, routes);

        var result = await service.ExecuteAsync(routeId, new RouteRequestDto
        {
            DaysOfWeek = new List<string> { "Monday" }
        });

        Assert.That(schedules.Schedules, Has.Exactly(2).Items); // Ida + Vuelta
        Assert.That(schedules.Schedules.All(s => s.RouteBusAssignmentsId == assignments.Assignments[0].Id), Is.True);
        Assert.That(schedules.Schedules.All(s => s.Status == Status.Active), Is.True);
        Assert.That(result.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task ExecuteAsync_SinBusAsignado_Rechaza()
    {
        var routeId = Guid.NewGuid();
        var schedules = new InMemoryRouteScheduleRepository();
        var assignments = new InMemoryRouteBusAssignmentRepository();
        var routes = RouteFor(routeId);
        var service = new CreateRouteScheduleService(new RouteScheduleProvisioner(schedules), assignments, routes);

        Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ExecuteAsync(routeId, new RouteRequestDto
            {
                DaysOfWeek = new List<string> { "Monday" }
            }));

        Assert.That(schedules.Schedules, Is.Empty);
    }

    [Test]
    public void ExecuteAsync_ConDiaNoLaborable_Rechaza()
    {
        var routeId = Guid.NewGuid();
        var schedules = new InMemoryRouteScheduleRepository();
        var assignments = AssignmentFor(routeId);
        var routes = RouteFor(routeId);
        var service = new CreateRouteScheduleService(new RouteScheduleProvisioner(schedules), assignments, routes);

        Assert.ThrowsAsync<ArgumentException>(() =>
            service.ExecuteAsync(routeId, new RouteRequestDto
            {
                DaysOfWeek = new List<string> { "Saturday" }
            }));
    }

    [Test]
    public void ExecuteAsync_SinDaysOfWeek_Rechaza()
    {
        var routeId = Guid.NewGuid();
        var schedules = new InMemoryRouteScheduleRepository();
        var assignments = AssignmentFor(routeId);
        var routes = RouteFor(routeId);
        var service = new CreateRouteScheduleService(new RouteScheduleProvisioner(schedules), assignments, routes);

        Assert.ThrowsAsync<ArgumentException>(() =>
            service.ExecuteAsync(routeId, new RouteRequestDto
            {
                DaysOfWeek = new List<string>()
            }));
    }

    [Test]
    public async Task ExecuteAsync_ConHorarioDuplicado_Rechaza()
    {
        var routeId = Guid.NewGuid();
        var assignments = AssignmentFor(routeId);
        var schedules = new InMemoryRouteScheduleRepository();
        schedules.Schedules.Add(new RouteSchedule
        {
            Id = Guid.NewGuid(),
            RouteBusAssignmentsId = assignments.Assignments[0].Id,
            DayOfWeek = DayOfWeek.Monday,
            Direction = "Ida",
            StartTime = new TimeOnly(6, 30),
            Status = Status.Active
        });
        var routes = RouteFor(routeId);
        var service = new CreateRouteScheduleService(new RouteScheduleProvisioner(schedules), assignments, routes);

        var result = await service.ExecuteAsync(routeId, new RouteRequestDto
        {
            DaysOfWeek = new List<string> { "Monday" }
        });

        // Should only create Vuelta since Ida already exists
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Direction, Is.EqualTo("Vuelta"));
    }

    [Test]
    public void ExecuteAsync_ConUnDiaInvalidoEntreValidos_NoCreaNingunHorario()
    {
        var routeId = Guid.NewGuid();
        var schedules = new InMemoryRouteScheduleRepository();
        var service = new CreateRouteScheduleService(
            new RouteScheduleProvisioner(schedules), AssignmentFor(routeId), RouteFor(routeId));

        Assert.ThrowsAsync<ArgumentException>(() =>
            service.ExecuteAsync(routeId, new RouteRequestDto
            {
                DaysOfWeek = new List<string> { "Monday", "Sunday" }
            }));

        Assert.That(schedules.Schedules, Is.Empty);
    }
}
