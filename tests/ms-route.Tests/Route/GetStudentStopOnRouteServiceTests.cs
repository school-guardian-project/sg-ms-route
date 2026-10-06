using ms_route.Api.Application.UseCase;
using ms_route.Api.Domain.Model;
using ms_route.Tests.Fakes;
using Xunit;
using StopModel = ms_route.Api.Domain.Model.Stop;

namespace ms_route.Tests.Route;

public class GetStudentStopOnRouteServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ConEstudianteAsignado_RetornaLaParadaDeLaRuta()
    {
        var routeId = Guid.NewGuid();
        var stopId = Guid.NewGuid();
        var routeStopId = Guid.NewGuid();
        var studentId = Guid.NewGuid();

        var routeStopRepo = new InMemoryRouteStopRepository();
        routeStopRepo.RouteStops.Add(new RouteStop
        {
            Id = routeStopId,
            RouteId = routeId,
            StopId = stopId,
            Status = Status.Active
        });

        var stopRepo = new InMemoryStopRepository();
        stopRepo.Stops.Add(new StopModel { Id = stopId, Name = "Libertad" });

        var assignmentRepo = new InMemoryRouteStudentAssignmentRepository();
        assignmentRepo.Assignments.Add(new RouteStudentAssignment
        {
            Id = Guid.NewGuid(),
            ProfileId = studentId,
            RouteStopId = routeStopId,
            Status = Status.Active
        });

        var service = new GetStudentStopOnRouteService(assignmentRepo, routeStopRepo, stopRepo);

        var result = await service.ExecuteAsync(routeId, studentId);

        Assert.NotNull(result);
        Assert.Equal(routeId, result.RouteId);
        Assert.Equal(studentId, result.StudentProfileId);
        Assert.Equal(routeStopId, result.RouteStopId);
        Assert.Equal(stopId, result.StopId);
        Assert.Equal("Libertad", result.StopName);
    }

    [Fact]
    public async Task ExecuteAsync_ConEstudianteSinAsignacion_RetornaNull()
    {
        var service = new GetStudentStopOnRouteService(
            new InMemoryRouteStudentAssignmentRepository(),
            new InMemoryRouteStopRepository(),
            new InMemoryStopRepository());

        var result = await service.ExecuteAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task ExecuteAsync_ConAsignacionDeOtraRuta_RetornaNull()
    {
        var otherRouteStopId = Guid.NewGuid();

        var routeStopRepo = new InMemoryRouteStopRepository();
        routeStopRepo.RouteStops.Add(new RouteStop
        {
            Id = Guid.NewGuid(),
            RouteId = Guid.NewGuid(),
            StopId = Guid.NewGuid(),
            Status = Status.Active
        });

        var assignmentRepo = new InMemoryRouteStudentAssignmentRepository();
        assignmentRepo.Assignments.Add(new RouteStudentAssignment
        {
            Id = Guid.NewGuid(),
            ProfileId = Guid.NewGuid(),
            RouteStopId = otherRouteStopId,
            Status = Status.Active
        });

        var service = new GetStudentStopOnRouteService(assignmentRepo, routeStopRepo, new InMemoryStopRepository());

        var result = await service.ExecuteAsync(Guid.NewGuid(), assignmentRepo.Assignments[0].ProfileId);

        Assert.Null(result);
    }
}
