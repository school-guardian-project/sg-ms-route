using AutoMapper;
using ms_route.Api.Application.Configuration;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;
using RouteModel = ms_route.Api.Domain.Model.Route;

namespace ms_route.Api.Application.UseCase;

public class StartTripService : IStartTripUseCase
{
    private readonly IRouteExecutionRepository _executionRepository;
    private readonly IRouteScheduleRepository _scheduleRepository;
    private readonly IRouteBusAssignmentRepository _routeBusAssignmentRepository;
    private readonly IRouteRepository _routeRepository;
    private readonly IFleetService _fleetService;
    private readonly IMapper _mapper;
    private readonly TimeProvider _timeProvider;

    private static readonly TimeSpan EarlyGrace = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan FallbackDuration = TimeSpan.FromHours(1);
    private const int MinutesPerDay = 24 * 60;

    public StartTripService(
        IRouteExecutionRepository executionRepository,
        IRouteScheduleRepository scheduleRepository,
        IRouteBusAssignmentRepository routeBusAssignmentRepository,
        IRouteRepository routeRepository,
        IFleetService fleetService,
        IMapper mapper,
        TimeProvider timeProvider)
    {
        _executionRepository = executionRepository;
        _scheduleRepository = scheduleRepository;
        _routeBusAssignmentRepository = routeBusAssignmentRepository;
        _routeRepository = routeRepository;
        _fleetService = fleetService;
        _mapper = mapper;
        _timeProvider = timeProvider;
    }

    public async Task<TripResponseDto> ExecuteAsync(Guid routeId, Guid busId, Guid driverId, CancellationToken ct = default)
    {
        // 1) Validar que el conductor NO tenga un viaje activo hoy (corte por dia local)
        var dayStartUtc = LocalClock.StartOfLocalDayUtc(_timeProvider);
        var existing = await _executionRepository.GetActiveByDriverAsync(driverId, dayStartUtc, ct);
        if (existing is not null)
        {
            throw new InvalidOperationException(
                $"Driver {driverId} already has an active trip (id={existing.Id}) started today");
        }

        // 2) Validar que la ruta tenga horario activo para el dia local
        var bus = await _fleetService.GetBusAssignedToDriverAsync(driverId, ct);
        if (bus is null || bus.Id != busId)
            throw new InvalidOperationException("Bus not assigned to this driver");

        var assignment = await _routeBusAssignmentRepository.GetActiveByBusIdAsync(busId, ct);
        if (assignment is null || assignment.RouteId != routeId)
            throw new InvalidOperationException("Route not assigned to this bus");

        var route = await _routeRepository.GetByIdAsync(routeId, ct);
        if (route is null || route.Status != Status.Active)
            throw new InvalidOperationException("Route not found or inactive");

        var schedules = await _scheduleRepository.GetByRouteIdAsync(routeId, ct);
        var localNow = LocalClock.UtcNow(_timeProvider);

        var activeSchedule = schedules.FirstOrDefault(s => IsWithinActiveWindow(localNow, s, route));
        if (activeSchedule is null)
        {
            throw new InvalidOperationException(
                "Route has no active schedule for today; trip cannot start outside the schedule window");
        }

        // 3) Crear el viaje con la dirección del horario activo (Ida/Vuelta)
        var execution = new RouteExecution
        {
            Id = Guid.NewGuid(),
            RouteId = routeId,
            BusId = busId,
            DriverId = driverId,
            StartDateTime = DateTime.UtcNow,
            Status = Status.Active,
            Direction = activeSchedule.Direction
        };

        var saved = await _executionRepository.SaveAsync(execution, ct);
        return _mapper.Map<TripResponseDto>(saved);
    }

    /// <summary>
    /// Mismo criterio de ventana que <see cref="GetDriverRouteTodayService.IsWithinActiveWindow"/>:
    /// el viaje solo puede iniciarse dentro de la ventana del dia (salida - 15min .. fin de ruta).
    /// </summary>
    private static bool IsWithinActiveWindow(
        DateTimeOffset localNow,
        RouteSchedule schedule,
        RouteModel route)
    {
        if (schedule.DayOfWeek != localNow.DayOfWeek) return false;

        var startMinutes = schedule.StartTime.Hour * 60 + schedule.StartTime.Minute;
        var endMinutes = route.EndTime.Hour * 60 + route.EndTime.Minute;

        // Fin inutil (p.ej. 00:00:00 de datos sin configurar): se acota a 1h tras la salida.
        if (endMinutes <= startMinutes)
            endMinutes = startMinutes + (int)FallbackDuration.TotalMinutes;

        var opensAt = startMinutes - (int)EarlyGrace.TotalMinutes;
        var closesAt = endMinutes;

        var nowMinutes = localNow.Hour * 60 + localNow.Minute;
        var elapsed = (nowMinutes - opensAt + MinutesPerDay) % MinutesPerDay;
        var span = (closesAt - opensAt + MinutesPerDay) % MinutesPerDay;

        return span > 0 && elapsed <= span;
    }
}
