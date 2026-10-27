using ms_route.Api.Application.Configuration;
using ms_route.Api.Application.Dto;
using ms_route.Api.Application.Mapper;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;
using ms_route.Api.Domain.Service;
using RouteModel = ms_route.Api.Domain.Model.Route;

namespace ms_route.Api.Application.UseCase;

public class GetTripStatusService : IGetTripStatusUseCase
{
    private readonly IRouteExecutionRepository _executionRepository;
    private readonly IRouteScheduleRepository _scheduleRepository;
    private readonly IRouteBusAssignmentRepository _routeBusAssignmentRepository;
    private readonly IRouteRepository _routeRepository;
    private readonly IFleetService _fleetService;
    private readonly IGetTripStatusMapper _mapper;
    private readonly TimeProvider _timeProvider;
    private readonly ITripLifecycleService _tripLifecycleService;
    private readonly TimeZoneInfo _timeZone;

    public GetTripStatusService(
        IRouteExecutionRepository executionRepository,
        IRouteScheduleRepository scheduleRepository,
        IRouteBusAssignmentRepository routeBusAssignmentRepository,
        IRouteRepository routeRepository,
        IFleetService fleetService,
        IGetTripStatusMapper mapper,
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

    public async Task<GetTripStatusResponseDto?> ExecuteAsync(Guid driverId, CancellationToken ct = default)
    {
        var bus = await _fleetService.GetBusAssignedToDriverAsync(driverId, ct);
        if (bus is null) return null;

        var assignment = await _routeBusAssignmentRepository.GetActiveByBusIdAsync(bus.Id, ct);
        if (assignment is null) return null;

        var route = await _routeRepository.GetByIdAsync(assignment.RouteId, ct);
        if (route is null || route.Status != Status.Active) return null;

        var schedules = await _scheduleRepository.GetByRouteIdAsync(assignment.RouteId, ct);
        var localNow = GetLocalNow();
        var todaysSchedule = schedules.FirstOrDefault(s => s.DayOfWeek == localNow.DayOfWeek);

        var dayStartUtc = LocalClock.StartOfLocalDayUtc(_timeProvider);
        var activeExecution = await _executionRepository.GetActiveByDriverAsync(driverId, dayStartUtc, ct);

        var scheduleStartTime = todaysSchedule?.StartTime ?? route.StartTime;

        GetTripStatusResponseDto response;

        if (activeExecution is not null)
        {
            var canEnd = await _tripLifecycleService.CanEndTripAsync(
                activeExecution.Id,
                route.Id,
                activeExecution.Direction,
                ct);

            var endDetail = await _tripLifecycleService.GetEndConditionDetailAsync(
                activeExecution.Id,
                route.Id,
                activeExecution.Direction,
                ct);

            var statusDto = new TripStatusDto
            {
                TripId = activeExecution.Id,
                RouteId = route.Id,
                RouteName = route.Name,
                Direction = activeExecution.Direction,
                Status = TripLifecycleStatus.InProgress,
                CanStart = false,
                CanEnd = canEnd,
                EndConditionDetail = endDetail,
                ScheduleStartTime = scheduleStartTime,
                NextScheduleTime = todaysSchedule?.StartTime,
                Message = canEnd ? "Todos los estudiantes han descendido. Puede terminar el viaje." : endDetail.Message
            };

            response = _mapper.ToResponse(statusDto);
        }
        else
        {
            var canStart = todaysSchedule is not null 
                && await _tripLifecycleService.CanStartTripAsync(scheduleStartTime, TimeOnly.FromDateTime(localNow.DateTime), ct: ct);

            var shouldNotify = todaysSchedule is not null
                && await _tripLifecycleService.ShouldNotifyBeforeStartAsync(scheduleStartTime, TimeOnly.FromDateTime(localNow.DateTime), ct: ct);

            var status = canStart ? TripLifecycleStatus.ReadyToStart : TripLifecycleStatus.NotStarted;

            var statusDto = new TripStatusDto
            {
                TripId = Guid.Empty,
                RouteId = route.Id,
                RouteName = route.Name,
                Direction = todaysSchedule?.Direction ?? "Ida",
                Status = status,
                CanStart = canStart,
                CanEnd = false,
                ScheduleStartTime = scheduleStartTime,
                NextScheduleTime = todaysSchedule?.StartTime,
                Message = GetStatusMessage(status, canStart, shouldNotify, scheduleStartTime, localNow)
            };

            response = _mapper.ToResponse(statusDto);
        }

        return response;
    }

    private string GetStatusMessage(TripLifecycleStatus status, bool canStart, bool shouldNotify, TimeOnly scheduleStartTime, DateTimeOffset localNow)
    {
        var nowTimeOnly = TimeOnly.FromDateTime(localNow.DateTime);
        var minutesUntilStart = (int)(scheduleStartTime - nowTimeOnly).TotalMinutes;

        return status switch
        {
            TripLifecycleStatus.ReadyToStart => $"Puede iniciar la ruta (ventana: {scheduleStartTime:HH:mm} - {scheduleStartTime.Add(TripLifecycleService.DefaultStartGracePeriod):HH:mm})",
            TripLifecycleStatus.NotStarted when shouldNotify => $"Ruta programada para {scheduleStartTime:HH:mm}. Inicio disponible en {minutesUntilStart} min.",
            TripLifecycleStatus.NotStarted => $"Ruta programada para {scheduleStartTime:HH:mm}. El botón de inicio estará disponible a partir de esa hora.",
            _ => "Estado desconocido"
        };
    }

    private DateTimeOffset GetLocalNow()
    {
        var utcNow = _timeProvider.GetUtcNow();
        return TimeZoneInfo.ConvertTime(utcNow, _timeZone);
    }
}