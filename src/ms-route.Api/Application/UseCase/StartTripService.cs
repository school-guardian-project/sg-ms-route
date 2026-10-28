using AutoMapper;
using ms_route.Api.Application.Configuration;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;
using ms_route.Api.Domain.Service;
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
    private readonly ITripLifecycleService _tripLifecycleService;
    private readonly TimeZoneInfo _timeZone;

    // Configuración por defecto
    private static readonly TimeSpan FallbackDuration = TimeSpan.FromHours(1);
    private const int MinutesPerDay = 24 * 60;

    public StartTripService(
        IRouteExecutionRepository executionRepository,
        IRouteScheduleRepository scheduleRepository,
        IRouteBusAssignmentRepository routeBusAssignmentRepository,
        IRouteRepository routeRepository,
        IFleetService fleetService,
        IMapper mapper,
        TimeProvider timeProvider,
        ITripLifecycleService tripLifecycleService,
        TimeZoneInfo timeZone)
    {
        _executionRepository = executionRepository;
        _scheduleRepository = scheduleRepository;
        _routeBusAssignmentRepository = routeBusAssignmentRepository;
        _routeRepository = routeRepository;
        _fleetService = fleetService;
        _mapper = mapper;
        _timeProvider = timeProvider;
        _tripLifecycleService = tripLifecycleService;
        _timeZone = timeZone;
    }

    public async Task<TripResponseDto> ExecuteAsync(Guid routeId, Guid busId, Guid driverId, CancellationToken ct = default)
    {
        // 1) Validar que el conductor NO tenga un viaje activo hoy (corte por día local)
        var dayStartUtc = LocalClock.StartOfLocalDayUtc(_timeProvider);
        var existing = await _executionRepository.GetActiveByDriverAsync(driverId, dayStartUtc, ct);
        if (existing is not null)
        {
            throw new InvalidOperationException(
                $"Driver {driverId} already has an active trip (id={existing.Id}) started today");
        }

        // 2) Validar que la ruta tenga horario activo para el día local
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
        var localNow = GetLocalNow();

        // Buscar el schedule de hoy para obtener el StartTime
        var todaysSchedule = schedules.FirstOrDefault(s => s.DayOfWeek == localNow.DayOfWeek);
        if (todaysSchedule is null)
        {
            throw new InvalidOperationException(
                $"Route has no schedule for today ({localNow.DayOfWeek}); trip cannot start");
        }

        // 3) Usar el servicio de dominio para validar ventana de inicio
        // Ventana: StartTime → StartTime + 20 min (GracePeriod)
        var canStart = await _tripLifecycleService.CanStartTripAsync(
            todaysSchedule.StartTime,
            TimeOnly.FromDateTime(localNow.DateTime),
            ct: ct);

        if (!canStart)
        {
            var windowClosesAt = todaysSchedule.StartTime.Add(TripLifecycleService.DefaultStartGracePeriod);
            throw new InvalidOperationException(
                $"Trip can only be started between {todaysSchedule.StartTime.Add(-TripLifecycleService.EarlyStart):HH:mm} and {windowClosesAt:HH:mm}. " +
                $"Current time: {localNow:HH:mm}");
        }

        // 4) Crear el viaje con la dirección del horario activo (Ida/Vuelta)
        var execution = new RouteExecution
        {
            Id = Guid.NewGuid(),
            RouteId = routeId,
            BusId = busId,
            DriverId = driverId,
            StartDateTime = DateTime.UtcNow,
            Status = Status.Active,
            Direction = todaysSchedule.Direction
        };

        var saved = await _executionRepository.SaveAsync(execution, ct);
        return _mapper.Map<TripResponseDto>(saved);
    }

    private DateTimeOffset GetLocalNow()
    {
        var utcNow = _timeProvider.GetUtcNow();
        return TimeZoneInfo.ConvertTime(utcNow, _timeZone);
    }
}