using ms_route.Api.Application.UseCase;
using ms_route.Api.Domain.Model;
using ms_route.Tests.Fakes;
using NUnit.Framework;
using StopModel = ms_route.Api.Domain.Model.Stop;

namespace ms_route.Tests.Route;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class GetStudentStopOnRouteServiceTests
{
    [Test]
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
        stopRepo.Stops.Add(new StopModel
        {
            Id = stopId,
            Name = "Libertad",
            Address = "Calle 12 #34-56",
            Latitude = 3.4516m,
            Longitude = -76.5320m
        });

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

        Assert.That(result, Is.Not.Null);
        Assert.That(result.RouteId, Is.EqualTo(routeId));
        Assert.That(result.StudentProfileId, Is.EqualTo(studentId));
        Assert.That(result.RouteStopId, Is.EqualTo(routeStopId));
        Assert.That(result.StopId, Is.EqualTo(stopId));
        Assert.That(result.StopName, Is.EqualTo("Libertad"));
        // La ubicacion de la parada viaja con el caso de uso para que la
        // notificacion al guardian incluya donde subio o bajo el estudiante
        // (FR-5.2, FR-5.3).
        Assert.That(result.StopAddress, Is.EqualTo("Calle 12 #34-56"));
        Assert.That(result.Latitude, Is.EqualTo(3.4516m));
        Assert.That(result.Longitude, Is.EqualTo(-76.5320m));
    }

    [Test]
    public async Task ExecuteAsync_ConEstudianteSinAsignacion_RetornaNull()
    {
        var service = new GetStudentStopOnRouteService(
            new InMemoryRouteStudentAssignmentRepository(),
            new InMemoryRouteStopRepository(),
            new InMemoryStopRepository());

        var result = await service.ExecuteAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.That(result, Is.Null);
    }

    [Test]
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

        Assert.That(result, Is.Null);
    }
}
