using ms_route.Api.Application.UseCase;
using ms_route.Api.Domain.Model;
using ms_route.Tests.Fakes;
using RouteModel = ms_route.Api.Domain.Model.Route;
using NUnit.Framework;

namespace ms_route.Tests.Route;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class ListRouteSchedulesServiceTests
{
    [Test]
    public async Task ExecuteAsync_ConHorariosDeLaRuta_LosDevuelve()
    {
        var routeId = Guid.NewGuid();
        var assignmentId = Guid.NewGuid();

        var routes = new InMemoryRouteRepository();
        routes.Routes.Add(new RouteModel { Id = routeId, Name = "R-01", Status = Status.Active });

        var schedules = new InMemoryRouteScheduleRepository();
        schedules.AssignmentRouteByAssignment[assignmentId] = routeId;
        schedules.Schedules.Add(new RouteSchedule
        {
            Id = Guid.NewGuid(),
            RouteBusAssignmentsId = assignmentId,
            DayOfWeek = DayOfWeek.Monday,
            Direction = "Ida",
            StartTime = new TimeOnly(6, 30),
            Status = Status.Active
        });
        // Horario inactivo: no debe aparecer.
        schedules.Schedules.Add(new RouteSchedule
        {
            Id = Guid.NewGuid(),
            RouteBusAssignmentsId = assignmentId,
            DayOfWeek = DayOfWeek.Monday,
            Direction = "Vuelta",
            StartTime = new TimeOnly(14, 30),
            Status = Status.Inactive
        });

        var service = new ListRouteSchedulesService(routes, schedules);

        var result = await service.ExecuteAsync(routeId);

        Assert.That(result, Has.Exactly(1).Items);
        Assert.That(result[0].Direction, Is.EqualTo("Ida"));
        Assert.That(result[0].DayOfWeek, Is.EqualTo("Monday"));
    }

    [Test]
    public async Task ExecuteAsync_ConRutaInexistente_LanzaExcepcion()
    {
        var service = new ListRouteSchedulesService(
            new InMemoryRouteRepository(), new InMemoryRouteScheduleRepository());

        Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ExecuteAsync(Guid.NewGuid()));
    }
}
