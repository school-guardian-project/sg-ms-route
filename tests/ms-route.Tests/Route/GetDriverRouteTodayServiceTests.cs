using ms_route.Api.Application.Dto;
using ms_route.Api.Application.UseCase;
using ms_route.Api.Domain.Model;
using ms_route.Tests.Fakes;
using Xunit;
using RouteModel = ms_route.Api.Domain.Model.Route;
using StopModel = ms_route.Api.Domain.Model.Stop;

namespace ms_route.Tests.Route;

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
        _fleet.BusId = _busId;

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

    [Fact]
    public async Task ExecuteAsync_DentroDeLaVentana_RetornaActiveConParadas()
    {
        AddStopToRoute();
        var service = CreateService(Local(Monday, new TimeOnly(7, 0)));

        var result = await service.ExecuteAsync(Guid.NewGuid());

        Assert.Equal(DriverRouteStatus.Active, result.Status);
        Assert.NotNull(result.Route);
        Assert.Single(result.Route.Stops);
        Assert.Null(result.NextOccurrence);
    }

    [Fact]
    public async Task ExecuteAsync_QuinceMinutosAntesDeLaSalida_RetornaActive()
    {
        var service = CreateService(Local(Monday, new TimeOnly(5, 45)));

        var result = await service.ExecuteAsync(Guid.NewGuid());

        Assert.Equal(DriverRouteStatus.Active, result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_AntesDeLaSalida_RetornaScheduledParaHoySinParadas()
    {
        AddStopToRoute();
        var service = CreateService(Local(Monday, new TimeOnly(5, 0)));

        var result = await service.ExecuteAsync(Guid.NewGuid());

        Assert.Equal(DriverRouteStatus.Scheduled, result.Status);
        Assert.NotNull(result.Route);
        Assert.Empty(result.Route.Stops);
        Assert.Equal(Local(Monday, RouteStart), result.NextOccurrence);
    }

    [Fact]
    public async Task ExecuteAsync_DiaAntes_RetornaScheduledParaManana()
    {
        var sunday = Monday.AddDays(-1);
        var service = CreateService(Local(sunday, new TimeOnly(21, 0)));

        var result = await service.ExecuteAsync(Guid.NewGuid());

        Assert.Equal(DriverRouteStatus.Scheduled, result.Status);
        Assert.Equal(Local(Monday, RouteStart), result.NextOccurrence);
    }

    [Fact]
    public async Task ExecuteAsync_DespuesDeLaVentana_RetornaScheduledParaLaProximaSalida()
    {
        var service = CreateService(Local(Monday, new TimeOnly(11, 0)));

        var result = await service.ExecuteAsync(Guid.NewGuid());

        Assert.Equal(DriverRouteStatus.Scheduled, result.Status);
        // Solo hay horario los lunes: la proxima salida es el lunes siguiente.
        Assert.Equal(Local(Monday.AddDays(7), RouteStart), result.NextOccurrence);
    }

    [Fact]
    public async Task ExecuteAsync_FinDeSemana_RetornaScheduledParaElLunes()
    {
        var saturday = Monday.AddDays(-2);
        var service = CreateService(Local(saturday, new TimeOnly(10, 0)));

        var result = await service.ExecuteAsync(Guid.NewGuid());

        Assert.Equal(DriverRouteStatus.Scheduled, result.Status);
        Assert.Equal(Local(Monday, RouteStart), result.NextOccurrence);
    }

    [Fact]
    public async Task ExecuteAsync_SinBusAsignado_RetornaNoRoute()
    {
        _fleet.BusId = null;
        var service = CreateService(Local(Monday, new TimeOnly(7, 0)));

        var result = await service.ExecuteAsync(Guid.NewGuid());

        Assert.Equal(DriverRouteStatus.NoRoute, result.Status);
        Assert.Null(result.Route);
        Assert.Null(result.NextOccurrence);
    }

    [Fact]
    public async Task ExecuteAsync_SinAsignacionDeRuta_RetornaNoRoute()
    {
        _assignments.Assignments.Clear();
        var service = CreateService(Local(Monday, new TimeOnly(7, 0)));

        var result = await service.ExecuteAsync(Guid.NewGuid());

        Assert.Equal(DriverRouteStatus.NoRoute, result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_RutaInactiva_RetornaNoRoute()
    {
        _routes.Routes[0].Status = Status.Inactive;
        var service = CreateService(Local(Monday, new TimeOnly(7, 0)));

        var result = await service.ExecuteAsync(Guid.NewGuid());

        Assert.Equal(DriverRouteStatus.NoRoute, result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_SinHorarios_RetornaNoSchedule()
    {
        _schedules.Schedules.Clear();
        var service = CreateService(Local(Monday, new TimeOnly(7, 0)));

        var result = await service.ExecuteAsync(Guid.NewGuid());

        Assert.Equal(DriverRouteStatus.NoSchedule, result.Status);
        Assert.NotNull(result.Route);
        Assert.Empty(result.Route.Stops);
        Assert.Null(result.NextOccurrence);
    }

    [Fact]
    public async Task ExecuteAsync_DentroDeLaVentana_SiempreIncluyeElHorarioDeLaRuta()
    {
        var service = CreateService(Local(Monday, new TimeOnly(7, 0)));

        var result = await service.ExecuteAsync(Guid.NewGuid());

        Assert.Equal(DriverRouteStatus.Active, result.Status);
        Assert.NotNull(result.Route);
        var schedule = Assert.Single(result.Route.Schedules);
        Assert.Equal(Monday.DayOfWeek.ToString(), schedule.DayOfWeek);
        Assert.Equal(RouteStart, schedule.StartTime);
    }
}
