using ms_route.Api.Application.UseCase;
using ms_route.Api.Domain.Model;
using ms_route.Tests.Fakes;
using NUnit.Framework;

namespace ms_route.Tests.Route;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class AssignStudentToRouteServiceTests
{
    [Test]
    public async Task ExecuteAsync_ConParadaDeLaRuta_GuardaElIdDeLaRouteStop()
    {
        var routeId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var stopId = Guid.NewGuid();
        var routeStopId = Guid.NewGuid();

        var routeStopRepo = new InMemoryRouteStopRepository();
        routeStopRepo.RouteStops.Add(new RouteStop
        {
            Id = routeStopId,
            RouteId = routeId,
            StopId = stopId,
            Status = Status.Active
        });

        var assignmentRepo = new InMemoryRouteStudentAssignmentRepository();
        var service = new AssignStudentToRouteService(assignmentRepo, routeStopRepo);

        var result = await service.ExecuteAsync(routeId, studentId, stopId);

        Assert.That(assignmentRepo.Assignments.Single().RouteStopId, Is.EqualTo(routeStopId));
        Assert.That(result.RouteId, Is.EqualTo(routeId));
        Assert.That(result.StudentId, Is.EqualTo(studentId));
        Assert.That(result.StopId, Is.EqualTo(stopId));
        Assert.That(result.Status, Is.EqualTo("Active"));
    }

    [Test]
    public async Task ExecuteAsync_ConParadaQueNoPerteneceALaRuta_LanzaExcepcion()
    {
        var assignmentRepo = new InMemoryRouteStudentAssignmentRepository();
        var routeStopRepo = new InMemoryRouteStopRepository();
        routeStopRepo.RouteStops.Add(new RouteStop
        {
            Id = Guid.NewGuid(),
            RouteId = Guid.NewGuid(),
            StopId = Guid.NewGuid(),
            Status = Status.Active
        });

        var service = new AssignStudentToRouteService(assignmentRepo, routeStopRepo);

        Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ExecuteAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));

        Assert.That(assignmentRepo.Assignments, Is.Empty);
    }

    [Test]
    public async Task ExecuteAsync_ConAsignacionPrevia_MueveLaParada()
    {
        var routeId = Guid.NewGuid();
        var studentId = Guid.NewGuid();

        var firstStop = new RouteStop
        {
            Id = Guid.NewGuid(),
            RouteId = routeId,
            StopId = Guid.NewGuid(),
            OrderSequence = 1,
            Status = Status.Active
        };
        var secondStop = new RouteStop
        {
            Id = Guid.NewGuid(),
            RouteId = routeId,
            StopId = Guid.NewGuid(),
            OrderSequence = 2,
            Status = Status.Active
        };

        var routeStopRepo = new InMemoryRouteStopRepository();
        routeStopRepo.RouteStops.Add(firstStop);
        routeStopRepo.RouteStops.Add(secondStop);

        var assignmentRepo = new InMemoryRouteStudentAssignmentRepository();
        var service = new AssignStudentToRouteService(assignmentRepo, routeStopRepo);

        await service.ExecuteAsync(routeId, studentId, firstStop.StopId);
        await service.ExecuteAsync(routeId, studentId, secondStop.StopId);

        var assignmentItems = assignmentRepo.Assignments;
        Assert.That(assignmentItems, Has.Exactly(1).Items);
        var assignment = assignmentItems.Single();
        Assert.That(assignment.RouteStopId, Is.EqualTo(secondStop.Id));
    }
}
