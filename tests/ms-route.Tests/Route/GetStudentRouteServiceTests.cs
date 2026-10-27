using ms_route.Api.Application.UseCase;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.Out;
using ms_route.Tests.Fakes;
using NUnit.Framework;
using RouteModel = ms_route.Api.Domain.Model.Route;
using StopModel = ms_route.Api.Domain.Model.Stop;

namespace ms_route.Tests.Route;

/// <summary>FR-9.1: el acudiente ve bus, conductor, paradas y horas de su ruta.</summary>
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class GetStudentRouteServiceTests
{
    private static readonly DateOnly Monday = new(2026, 10, 5);
    private static readonly TimeOnly RouteStart = new(6, 0);

    private readonly Guid _studentId = Guid.NewGuid();
    private readonly Guid _routeId = Guid.NewGuid();
    private readonly Guid _routeStopId = Guid.NewGuid();
    private readonly Guid _busId = Guid.NewGuid();
    private readonly Guid _driverId = Guid.NewGuid();
    private readonly Guid _busAssignmentId = Guid.NewGuid();

    private readonly InMemoryRouteRepository _routes = new();
    private readonly InMemoryRouteStudentAssignmentRepository _studentAssignments = new();
    private readonly InMemoryRouteStopRepository _routeStops = new();
    private readonly InMemoryStopRepository _stops = new();
    private readonly InMemoryRouteBusAssignmentRepository _busAssignments = new();
    private readonly InMemoryRouteScheduleRepository _schedules = new();
    private readonly FakeFleetService _fleet = new();

    public GetStudentRouteServiceTests()
    {
        _routes.Routes.Add(new RouteModel
        {
            Id = _routeId,
            Name = "Ruta Norte",
            StartTime = new TimeOnly(5, 30),
            EndTime = new TimeOnly(10, 0),
            Status = Status.Active
        });

        _studentAssignments.Assignments.Add(new RouteStudentAssignment
        {
            Id = Guid.NewGuid(),
            ProfileId = _studentId,
            RouteStopId = _routeStopId,
            Status = Status.Active
        });

        _routeStops.RouteStops.Add(new RouteStop
        {
            Id = _routeStopId,
            RouteId = _routeId,
            StopId = AddStop(),
            OrderSequence = 1,
            Status = Status.Active
        });

        _busAssignments.Assignments.Add(new RouteBusAssignment
        {
            Id = _busAssignmentId,
            BusId = _busId,
            RouteId = _routeId,
            Status = Status.Active
        });

        _schedules.Schedules.Add(new RouteSchedule
        {
            Id = Guid.NewGuid(),
            RouteBusAssignmentsId = _busAssignmentId,
            DayOfWeek = Monday.DayOfWeek,
            Direction = "Ida",
            StartTime = RouteStart,
            Status = Status.Active
        });

        _fleet.BusDetails = new BusInfo
        {
            Id = _busId,
            Plate = "ABC123",
            Capacity = 40,
            DriverProfileId = _driverId,
            DriverName = "Juan Perez"
        };
    }

    private Guid AddStop()
    {
        var stop = new StopModel
        {
            Id = Guid.NewGuid(),
            Name = "Parada 1",
            Address = "Calle 1",
            Latitude = 2.9m,
            Longitude = -75.2m,
            Status = Status.Active
        };
        _stops.Stops.Add(stop);
        return stop.Id;
    }

    private GetStudentRouteService CreateService()
    {
        var mapper = TestMapper.Create();
        var stopFilter = new RouteStopFilter();
        return new(
            _routes,
            _studentAssignments,
            _routeStops,
            _stops,
            _busAssignments,
            _schedules,
            _fleet,
            mapper,
            stopFilter,
            new RouteDetailPopulator(_busAssignments, _schedules, _fleet, _routeStops, _stops, mapper, stopFilter));
    }

    [Test]
    public async Task ExecuteAsync_SinAsignacionDeEstudiante_RetornaNull()
    {
        var result = await CreateService().ExecuteAsync(Guid.NewGuid());

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task ExecuteAsync_IncluyeLasParadasDeLaRuta()
    {
        var result = await CreateService().ExecuteAsync(_studentId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Stops, Has.Exactly(1).Items);
        Assert.That(result.Stops[0].Name, Is.EqualTo("Parada 1"));
    }

    [Test]
    public async Task ExecuteAsync_IncluyeBusConductorYHorarios()
    {
        var result = await CreateService().ExecuteAsync(_studentId);

        Assert.That(result, Is.Not.Null);

        // Bus y conductor (desde ms-fleet).
        Assert.That(result.Bus, Is.Not.Null);
        Assert.That(result.Bus.Id, Is.EqualTo(_busId));
        Assert.That(result.Bus.Plate, Is.EqualTo("ABC123"));
        Assert.That(result.Bus.Capacity, Is.EqualTo(40));
        Assert.That(result.Bus.DriverProfileId, Is.EqualTo(_driverId));
        Assert.That(result.Bus.DriverName, Is.EqualTo("Juan Perez"));

        // Horas programadas.
        var schedule = result.Schedules.Single();
        Assert.That(schedule.DayOfWeek, Is.EqualTo(Monday.DayOfWeek.ToString()));
        Assert.That(schedule.StartTime, Is.EqualTo(RouteStart));
        Assert.That(schedule.Direction, Is.EqualTo("Ida"));
    }

    [Test]
    public async Task ExecuteAsync_SinBusAsignadoASuRuta_NoMuestraBusNiHorarios()
    {
        _busAssignments.Assignments.Clear();

        var result = await CreateService().ExecuteAsync(_studentId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Stops, Has.Exactly(1).Items);
        Assert.That(result.Bus, Is.Null);
        Assert.That(result.Schedules, Is.Empty);
    }

    [Test]
    public async Task ExecuteAsync_SinBusConocidoPorFleet_MantieneLaRutaSinBus()
    {
        _fleet.BusDetails = null;

        var result = await CreateService().ExecuteAsync(_studentId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Bus, Is.Null);
        // Los horarios son locales, no dependen de ms-fleet.
        Assert.That(result.Schedules, Has.Exactly(1).Items);
    }
}
