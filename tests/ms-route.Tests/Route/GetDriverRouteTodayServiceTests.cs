using ms_route.Api.Application.Dto;
using ms_route.Api.Application.UseCase;
using ms_route.Api.Domain.Model;
using ms_route.Tests.Fakes;
using NUnit.Framework;
using RouteModel = ms_route.Api.Domain.Model.Route;
using StopModel = ms_route.Api.Domain.Model.Stop;

namespace ms_route.Tests.Route;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class GetDriverRouteTodayServiceTests
{
    private static readonly DateOnly Monday = new(2026, 10, 5);
    private static readonly TimeSpan ColombiaOffset = TimeSpan.FromHours(-5);
    private static readonly TimeOnly RouteStart = new(6, 0);
    private static readonly TimeOnly RouteEnd = new(10, 0);

    private readonly Guid _busId = Guid.NewGuid();
    private readonly Guid _routeId = Guid.NewGuid();
    private readonly Guid _assignmentId = Guid.NewGuid();

    private readonly FakeFleetService _fleet = new();
    private readonly InMemoryRouteBusAssignmentRepository _assignments = new();
    private readonly InMemoryRouteRepository _routes = new();
    private readonly InMemoryRouteScheduleRepository _schedules = new();
    private readonly InMemoryRouteStopRepository _routeStops = new();
    private readonly InMemoryStopRepository _stops = new();

    public GetDriverRouteTodayServiceTests()
    {
        _fleet.AssignedBus = new BusInfo { Id = _busId, Plate = "ABC123", Capacity = 40 };

        _routes.Routes.Add(new RouteModel
        {
            Id = _routeId,
            Name = "Ruta Norte",
            StartTime = new TimeOnly(5, 30),
            EndTime = RouteEnd,
            Status = Status.Active
        });

        _assignments.Assignments.Add(new RouteBusAssignment
        {
            Id = _assignmentId,
            BusId = _busId,
            RouteId = _routeId,
            Status = Status.Active
        });

        _schedules.Schedules.Add(new RouteSchedule
        {
            Id = Guid.NewGuid(),
            RouteBusAssignmentsId = _assignmentId,
            DayOfWeek = Monday.DayOfWeek,
            Direction = "OUTBOUND",
            StartTime = RouteStart,
            Status = Status.Active
        });
    }

    private GetDriverRouteTodayService CreateService(DateTimeOffset now)
        => new(
            _fleet,
            _assignments,
            _routes,
            _schedules,
            _routeStops,
            _stops,
            TestMapper.Create(),
            new FixedTimeProvider(now.ToUniversalTime()));

    private void AddStopToRoute()
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
        _routeStops.RouteStops.Add(new RouteStop
        {
            Id = Guid.NewGuid(),
            RouteId = _routeId,
            StopId = stop.Id,
            OrderSequence = 1,
            Status = Status.Active
        });
    }

    private static DateTimeOffset Local(DateOnly date, TimeOnly time)
        => new(date.ToDateTime(time), ColombiaOffset);

    [Test]
    public async Task ExecuteAsync_DentroDeLaVentana_RetornaActiveConParadas()
    {
        AddStopToRoute();
        var service = CreateService(Local(Monday, new TimeOnly(7, 0)));

        var result = await service.ExecuteAsync(Guid.NewGuid());

        Assert.That(result.Status, Is.EqualTo(DriverRouteStatus.Active));
        Assert.That(result.Route, Is.Not.Null);
        Assert.That(result.Route.Stops, Has.Exactly(1).Items);
        Assert.That(result.NextOccurrence, Is.Null);
    }

    [Test]
    public async Task ExecuteAsync_QuinceMinutosAntesDeLaSalida_RetornaActive()
    {
        var service = CreateService(Local(Monday, new TimeOnly(5, 45)));

        var result = await service.ExecuteAsync(Guid.NewGuid());

        Assert.That(result.Status, Is.EqualTo(DriverRouteStatus.Active));
    }

    [Test]
    public async Task ExecuteAsync_AntesDeLaSalida_RetornaScheduledParaHoyConParadas()
    {
        AddStopToRoute();
        var service = CreateService(Local(Monday, new TimeOnly(5, 0)));

        var result = await service.ExecuteAsync(Guid.NewGuid());

        Assert.That(result.Status, Is.EqualTo(DriverRouteStatus.Scheduled));
        Assert.That(result.Route, Is.Not.Null);
        Assert.That(result.Route.Stops, Has.Exactly(1).Items);
        Assert.That(result.NextOccurrence, Is.EqualTo(Local(Monday, RouteStart)));
    }

    [Test]
    public async Task ExecuteAsync_DiaAntes_RetornaScheduledParaManana()
    {
        var sunday = Monday.AddDays(-1);
        var service = CreateService(Local(sunday, new TimeOnly(21, 0)));

        var result = await service.ExecuteAsync(Guid.NewGuid());

        Assert.That(result.Status, Is.EqualTo(DriverRouteStatus.Scheduled));
        Assert.That(result.NextOccurrence, Is.EqualTo(Local(Monday, RouteStart)));
    }

    [Test]
    public async Task ExecuteAsync_DespuesDeLaVentana_RetornaScheduledParaLaProximaSalida()
    {
        var service = CreateService(Local(Monday, new TimeOnly(11, 0)));

        var result = await service.ExecuteAsync(Guid.NewGuid());

        Assert.That(result.Status, Is.EqualTo(DriverRouteStatus.Scheduled));
        // Solo hay horario los lunes: la proxima salida es el lunes siguiente.
        Assert.That(result.NextOccurrence, Is.EqualTo(Local(Monday.AddDays(7), RouteStart)));
    }

    [Test]
    public async Task ExecuteAsync_FinDeSemana_RetornaScheduledParaElLunes()
    {
        var saturday = Monday.AddDays(-2);
        var service = CreateService(Local(saturday, new TimeOnly(10, 0)));

        var result = await service.ExecuteAsync(Guid.NewGuid());

        Assert.That(result.Status, Is.EqualTo(DriverRouteStatus.Scheduled));
        Assert.That(result.NextOccurrence, Is.EqualTo(Local(Monday, RouteStart)));
    }

    [Test]
    public async Task ExecuteAsync_SinBusAsignado_RetornaNoRoute()
    {
        _fleet.AssignedBus = null;
        var service = CreateService(Local(Monday, new TimeOnly(7, 0)));

        var result = await service.ExecuteAsync(Guid.NewGuid());

        Assert.That(result.Status, Is.EqualTo(DriverRouteStatus.NoRoute));
        Assert.That(result.Route, Is.Null);
        Assert.That(result.NextOccurrence, Is.Null);
    }

    [Test]
    public async Task ExecuteAsync_IncluyeElBusAsignadoAlConductor()
    {
        var service = CreateService(Local(Monday, new TimeOnly(7, 0)));

        var result = await service.ExecuteAsync(Guid.NewGuid());

        Assert.That(result.Status, Is.EqualTo(DriverRouteStatus.Active));
        Assert.That(result.Route, Is.Not.Null);
        Assert.That(result.Route.Bus, Is.Not.Null);
        Assert.That(result.Route.Bus.Id, Is.EqualTo(_busId));
        Assert.That(result.Route.Bus.Plate, Is.EqualTo("ABC123"));
        Assert.That(result.Route.Bus.Capacity, Is.EqualTo(40));
    }

    [Test]
    public async Task ExecuteAsync_SinAsignacionDeRuta_RetornaNoRoute()
    {
        _assignments.Assignments.Clear();
        var service = CreateService(Local(Monday, new TimeOnly(7, 0)));

        var result = await service.ExecuteAsync(Guid.NewGuid());

        Assert.That(result.Status, Is.EqualTo(DriverRouteStatus.NoRoute));
    }

    [Test]
    public async Task ExecuteAsync_RutaInactiva_RetornaNoRoute()
    {
        _routes.Routes[0].Status = Status.Inactive;
        var service = CreateService(Local(Monday, new TimeOnly(7, 0)));

        var result = await service.ExecuteAsync(Guid.NewGuid());

        Assert.That(result.Status, Is.EqualTo(DriverRouteStatus.NoRoute));
    }

    [Test]
    public async Task ExecuteAsync_SinHorarios_RetornaNoSchedule()
    {
        _schedules.Schedules.Clear();
        var service = CreateService(Local(Monday, new TimeOnly(7, 0)));

        var result = await service.ExecuteAsync(Guid.NewGuid());

        Assert.That(result.Status, Is.EqualTo(DriverRouteStatus.NoSchedule));
        Assert.That(result.Route, Is.Not.Null);
        Assert.That(result.Route.Stops, Is.Empty);
        Assert.That(result.NextOccurrence, Is.Null);
    }

    [Test]
    public async Task ExecuteAsync_DentroDeLaVentana_SiempreIncluyeElHorarioDeLaRuta()
    {
        var service = CreateService(Local(Monday, new TimeOnly(7, 0)));

        var result = await service.ExecuteAsync(Guid.NewGuid());

        Assert.That(result.Status, Is.EqualTo(DriverRouteStatus.Active));
        Assert.That(result.Route, Is.Not.Null);
        var scheduleItems = result.Route.Schedules;
        Assert.That(scheduleItems, Has.Exactly(1).Items);
        var schedule = scheduleItems.Single();
        Assert.That(schedule.DayOfWeek, Is.EqualTo(Monday.DayOfWeek.ToString()));
        Assert.That(schedule.StartTime, Is.EqualTo(RouteStart));
    }

    // Ruta sin hora de fin util (EndTime igual a StartTime, p.ej. 00:00:00):
    // la ventana se cierra una hora despues de la salida en lugar de quedar
    // activa para siempre.
    [Test]
    public async Task ExecuteAsync_RutaSinHoraDeFin_UsaDuracionDeRespaldo()
    {
        _routes.Routes[0].EndTime = TimeOnly.MinValue;
        var service = CreateService(Local(Monday, new TimeOnly(6, 30)));

        var result = await service.ExecuteAsync(Guid.NewGuid());

        Assert.That(result.Status, Is.EqualTo(DriverRouteStatus.Active));
    }

    // La ventana de respaldo cierra a la hora de salida + 1 hora.
    [Test]
    public async Task ExecuteAsync_RutaSinHoraDeFin_TrasLaDuracionDeRespaldo_RetornaScheduled()
    {
        _routes.Routes[0].EndTime = TimeOnly.MinValue;
        var service = CreateService(Local(Monday, new TimeOnly(8, 0)));

        var result = await service.ExecuteAsync(Guid.NewGuid());

        Assert.That(result.Status, Is.EqualTo(DriverRouteStatus.Scheduled));
    }

    // Sin salida resoluble (ambas horas en 00:00:00, dato sin configurar) no
    // hay ventana activa: la ruta no debe reportarse como activa.
    [Test]
    public async Task ExecuteAsync_RutaSinHorarioConfigurado_RetornaScheduled()
    {
        _routes.Routes[0].StartTime = TimeOnly.MinValue;
        _routes.Routes[0].EndTime = TimeOnly.MinValue;
        _schedules.Schedules[0].StartTime = TimeOnly.MinValue;
        var service = CreateService(Local(Monday, new TimeOnly(7, 0)));

        var result = await service.ExecuteAsync(Guid.NewGuid());

        Assert.That(result.Status, Is.EqualTo(DriverRouteStatus.Scheduled));
        Assert.That(result.Route.Stops, Is.Empty);
    }
}
