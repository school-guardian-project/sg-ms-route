using ms_route.Api.Application.Dto;
using ms_route.Api.Application.UseCase;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Service;
using ms_route.Tests.Fakes;
using RouteModel = ms_route.Api.Domain.Model.Route;
using NUnit.Framework;

namespace ms_route.Tests.Route;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class UpdateRouteServiceTests
{
    private readonly InMemoryRouteBusAssignmentRepository _assignments = new();
    private readonly InMemoryRouteScheduleRepository _schedules = new();

    private UpdateRouteService Service(InMemoryRouteRepository repo)
        => new(repo, _assignments, new RouteScheduleProvisioner(_schedules), TestMapper.Create());

    [Test]
    public async Task ExecuteAsync_ConRutaExistente_ActualizaRuta()
    {
        var repo = new InMemoryRouteRepository();
        var routeId = Guid.NewGuid();
        repo.Routes.Add(new RouteModel { Id = routeId, Name = "R-01", TargetSector = "Norte", Status = Status.Active });
        var service = Service(repo);

        await service.ExecuteAsync(routeId, new RouteRequestDto
        {
            CampuseId = Guid.NewGuid(),
            Name = "R-01-Updated",
            TargetSector = "Sur"
        });

        var updated = repo.Routes.First(r => r.Id == routeId);
        Assert.That(updated.Name, Is.EqualTo("R-01-Updated"));
    }

    [Test]
    public async Task ExecuteAsync_ConRutaInexistente_LanzaExcepcion()
    {
        var repo = new InMemoryRouteRepository();
        var service = Service(repo);

        Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ExecuteAsync(Guid.NewGuid(), new RouteRequestDto
            {
                CampuseId = Guid.NewGuid(),
                Name = "R-01",
                TargetSector = "Norte"
            }));
    }

    [Test]
    public async Task ExecuteAsync_AlCambiarLaHoraDeSalida_ActualizaLosHorariosActivosDelBus()
    {
        var repo = new InMemoryRouteRepository();
        var routeId = Guid.NewGuid();
        repo.Routes.Add(new RouteModel
        {
            Id = routeId, Name = "R-01", Status = Status.Active, StartTime = new TimeOnly(6, 30)
        });
        var assignmentId = Guid.NewGuid();
        _assignments.Assignments.Add(new RouteBusAssignment
        {
            Id = assignmentId, RouteId = routeId, BusId = Guid.NewGuid(), Status = Status.Active
        });
        _schedules.Schedules.Add(new RouteSchedule
        {
            RouteBusAssignmentsId = assignmentId, DayOfWeek = DayOfWeek.Monday,
            Direction = "Ida", StartTime = new TimeOnly(6, 30), Status = Status.Active
        });
        _schedules.Schedules.Add(new RouteSchedule
        {
            RouteBusAssignmentsId = assignmentId, DayOfWeek = DayOfWeek.Monday,
            Direction = "Vuelta", StartTime = new TimeOnly(6, 30), Status = Status.Inactive
        });

        await Service(repo).ExecuteAsync(routeId, new RouteRequestDto
        {
            CampuseId = Guid.NewGuid(),
            Name = "R-01",
            TargetSector = "Norte",
            StartTime = new TimeOnly(14, 0),
            EndTime = new TimeOnly(15, 0)
        });

        var monday = _schedules.Schedules.Where(s => s.DayOfWeek == DayOfWeek.Monday).ToList();
        Assert.That(monday.Single(s => s.Status == Status.Inactive).StartTime,
            Is.EqualTo(new TimeOnly(6, 30)), "un horario inactivo no se toca");
        Assert.That(_schedules.Schedules.Where(s => s.Status == Status.Active).All(s => s.StartTime == new TimeOnly(14, 0)),
            Is.True);
    }

    [Test]
    public async Task ExecuteAsync_SinBusAsignado_ActualizaLaRutaSinTocarHorarios()
    {
        var repo = new InMemoryRouteRepository();
        var routeId = Guid.NewGuid();
        repo.Routes.Add(new RouteModel { Id = routeId, Name = "R-01", Status = Status.Active });

        await Service(repo).ExecuteAsync(routeId, new RouteRequestDto
        {
            CampuseId = Guid.NewGuid(),
            Name = "R-01",
            TargetSector = "Norte",
            StartTime = new TimeOnly(14, 0)
        });

        Assert.That(repo.Routes[0].StartTime, Is.EqualTo(new TimeOnly(14, 0)));
        Assert.That(_schedules.Schedules, Is.Empty);
    }

    [Test]
    public async Task ExecuteAsync_RutaConBusSinHorarios_CreaLosHorariosQueFaltan()
    {
        var repo = new InMemoryRouteRepository();
        var routeId = Guid.NewGuid();
        repo.Routes.Add(new RouteModel { Id = routeId, Name = "R-01", Status = Status.Active });
        var assignmentId = Guid.NewGuid();
        _assignments.Assignments.Add(new RouteBusAssignment
        {
            Id = assignmentId, RouteId = routeId, BusId = Guid.NewGuid(), Status = Status.Active
        });

        await Service(repo).ExecuteAsync(routeId, new RouteRequestDto
        {
            CampuseId = Guid.NewGuid(),
            Name = "R-01",
            TargetSector = "Norte",
            StartTime = new TimeOnly(6, 0)
        });
        // Guardar de nuevo no duplica.
        await Service(repo).ExecuteAsync(routeId, new RouteRequestDto
        {
            CampuseId = Guid.NewGuid(),
            Name = "R-01",
            TargetSector = "Norte",
            StartTime = new TimeOnly(6, 0)
        });

        Assert.That(_schedules.Schedules, Has.Exactly(10).Items);
        Assert.That(_schedules.Schedules.All(s =>
            s.RouteBusAssignmentsId == assignmentId && s.StartTime == new TimeOnly(6, 0)), Is.True);
    }

    [Test]
    public async Task ExecuteAsync_AlCambiarLosDias_DesactivaLosDiasRetiradosYActivaLosNuevos()
    {
        var repo = new InMemoryRouteRepository();
        var routeId = Guid.NewGuid();
        repo.Routes.Add(new RouteModel
        {
            Id = routeId,
            Name = "R-01",
            Status = Status.Active,
            DaysOfWeek = ["Monday", "Wednesday"]
        });
        var assignmentId = Guid.NewGuid();
        _assignments.Assignments.Add(new RouteBusAssignment
        {
            Id = assignmentId,
            RouteId = routeId,
            BusId = Guid.NewGuid(),
            Status = Status.Active
        });
        foreach (var day in new[] { DayOfWeek.Monday, DayOfWeek.Wednesday })
        {
            foreach (var direction in RouteScheduleProvisioner.Directions)
            {
                _schedules.Schedules.Add(new RouteSchedule
                {
                    RouteBusAssignmentsId = assignmentId,
                    DayOfWeek = day,
                    Direction = direction,
                    StartTime = new TimeOnly(6, 30),
                    Status = Status.Active
                });
            }
        }

        await Service(repo).ExecuteAsync(routeId, new RouteRequestDto
        {
            CampuseId = Guid.NewGuid(),
            Name = "R-01",
            TargetSector = "Norte",
            StartTime = new TimeOnly(7, 0),
            DaysOfWeek = ["Tuesday", "Thursday"]
        });

        Assert.That(_schedules.Schedules
            .Where(schedule => schedule.Status == Status.Active)
            .Select(schedule => schedule.DayOfWeek).Distinct(),
            Is.EquivalentTo(new[] { DayOfWeek.Tuesday, DayOfWeek.Thursday }));
        Assert.That(_schedules.Schedules
            .Where(schedule => schedule.Status == Status.Active)
            .All(schedule => schedule.StartTime == new TimeOnly(7, 0)), Is.True);
    }
}
