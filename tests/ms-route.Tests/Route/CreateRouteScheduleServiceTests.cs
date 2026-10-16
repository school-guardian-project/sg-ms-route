using ms_route.Api.Application.Dto;
using ms_route.Api.Application.UseCase;
using ms_route.Api.Domain.Model;
using ms_route.Tests.Fakes;
using NUnit.Framework;

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

    [Test]
    public async Task ExecuteAsync_ConBusAsignadoYCamposValidos_CreaElHorario()
    {
        var routeId = Guid.NewGuid();
        var assignments = AssignmentFor(routeId);
        var schedules = new InMemoryRouteScheduleRepository();
        var service = new CreateRouteScheduleService(schedules, assignments);

        var result = await service.ExecuteAsync(routeId, new RouteScheduleRequestDto
        {
            DayOfWeek = "Monday",
            Direction = "Ida",
            StartTime = new TimeOnly(6, 30)
        });

        Assert.That(schedules.Schedules, Has.Exactly(1).Items);
        Assert.That(schedules.Schedules[0].RouteBusAssignmentsId, Is.EqualTo(assignments.Assignments[0].Id));
        Assert.That(schedules.Schedules[0].Status, Is.EqualTo(Status.Active));
        Assert.That(result.DayOfWeek, Is.EqualTo("Monday"));
        Assert.That(result.Direction, Is.EqualTo("Ida"));
        Assert.That(result.StartTime, Is.EqualTo(new TimeOnly(6, 30)));
    }

    [Test]
    public async Task ExecuteAsync_SinBusAsignado_Rechaza()
    {
        var schedules = new InMemoryRouteScheduleRepository();
        var service = new CreateRouteScheduleService(schedules, new InMemoryRouteBusAssignmentRepository());

        Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ExecuteAsync(Guid.NewGuid(), new RouteScheduleRequestDto
            {
                DayOfWeek = "Monday",
                Direction = "Ida",
                StartTime = new TimeOnly(6, 30)
            }));

        Assert.That(schedules.Schedules, Is.Empty);
    }

    [Test]
    public void ExecuteAsync_ConDiaNoLaborable_Rechaza()
    {
        var routeId = Guid.NewGuid();
        var service = new CreateRouteScheduleService(
            new InMemoryRouteScheduleRepository(), AssignmentFor(routeId));

        Assert.ThrowsAsync<ArgumentException>(() =>
            service.ExecuteAsync(routeId, new RouteScheduleRequestDto
            {
                DayOfWeek = "Saturday",
                Direction = "Ida",
                StartTime = new TimeOnly(6, 30)
            }));
    }

    [Test]
    public void ExecuteAsync_SinDireccion_Rechaza()
    {
        var routeId = Guid.NewGuid();
        var service = new CreateRouteScheduleService(
            new InMemoryRouteScheduleRepository(), AssignmentFor(routeId));

        Assert.ThrowsAsync<ArgumentException>(() =>
            service.ExecuteAsync(routeId, new RouteScheduleRequestDto
            {
                DayOfWeek = "Monday",
                Direction = "  ",
                StartTime = new TimeOnly(6, 30)
            }));
    }

    [Test]
    public void ExecuteAsync_ConHorarioDuplicado_Rechaza()
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

        var service = new CreateRouteScheduleService(schedules, assignments);

        Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ExecuteAsync(routeId, new RouteScheduleRequestDto
            {
                DayOfWeek = "Monday",
                Direction = "Ida",
                StartTime = new TimeOnly(6, 30)
            }));
    }
}
