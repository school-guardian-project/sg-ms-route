using ms_route.Api.Application.UseCase;
using ms_route.Api.Domain.Model;
using ms_route.Tests.Fakes;
using Xunit;

namespace ms_route.Tests.Route;

public class AssignStudentToRouteServiceTests
{
    [Fact]
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

        Assert.Equal(routeStopId, assignmentRepo.Assignments.Single().RouteStopId);
        Assert.Equal(routeId, result.RouteId);
        Assert.Equal(studentId, result.StudentId);
        Assert.Equal(stopId, result.StopId);
        Assert.Equal("Active", result.Status);
    }

    [Fact]
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

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ExecuteAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));

        Assert.Empty(assignmentRepo.Assignments);
    }
}
