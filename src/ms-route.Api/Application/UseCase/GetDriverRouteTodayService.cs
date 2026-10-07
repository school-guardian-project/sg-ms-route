using AutoMapper;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;
using RouteModel = ms_route.Api.Domain.Model.Route;

namespace ms_route.Api.Application.UseCase;

public class GetDriverRouteTodayService : IGetDriverRouteTodayUseCase
{
    // Colombia no aplica horario de verano: el offset es fijo UTC-5.
    private static readonly TimeSpan LocalOffset = TimeSpan.FromHours(-5);

    // El conductor puede ver la ruta desde 15 minutos antes de la hora programada.
    private static readonly TimeSpan EarlyGrace = TimeSpan.FromMinutes(15);

    // Si la hora de fin de la ruta no supera a la salida programada (dato inconsistente),
    // la ventana se cierra 1 hora despues de la salida para no dejar la ruta nunca activa.
    private static readonly TimeSpan FallbackDuration = TimeSpan.FromHours(1);

    private const int DaysAheadToScan = 7;

    private readonly IFleetService _fleetService;
    private readonly IRouteBusAssignmentRepository _routeBusAssignmentRepository;
    private readonly IRouteRepository _routeRepository;
    private readonly IRouteScheduleRepository _scheduleRepository;
    private readonly IRouteStopRepository _routeStopRepository;
    private readonly IStopRepository _stopRepository;
    private readonly IMapper _mapper;
    private readonly TimeProvider _timeProvider;

    public GetDriverRouteTodayService(
        IFleetService fleetService,
        IRouteBusAssignmentRepository routeBusAssignmentRepository,
        IRouteRepository routeRepository,
        IRouteScheduleRepository scheduleRepository,
        IRouteStopRepository routeStopRepository,
        IStopRepository stopRepository,
        IMapper mapper,
        TimeProvider timeProvider)
    {
        _fleetService = fleetService;
        _routeBusAssignmentRepository = routeBusAssignmentRepository;
        _routeRepository = routeRepository;
        _scheduleRepository = scheduleRepository;
        _routeStopRepository = routeStopRepository;
        _stopRepository = stopRepository;
        _mapper = mapper;
        _timeProvider = timeProvider;
    }

    public async Task<DriverRouteTodayDto> ExecuteAsync(Guid driverId, CancellationToken ct = default)
    {
        var busId = await _fleetService.GetBusIdAssignedToDriverAsync(driverId, ct);
        if (busId is null) return NoRoute();

        var assignment = await _routeBusAssignmentRepository.GetActiveByBusIdAsync(busId.Value, ct);
        if (assignment is null) return NoRoute();

        var route = await _routeRepository.GetByIdAsync(assignment.RouteId, ct);
        if (route is null || route.Status != Status.Active) return NoRoute();

        var schedules = await _scheduleRepository.GetActiveByAssignmentIdAsync(assignment.Id, ct);

        var localNow = _timeProvider.GetUtcNow().ToOffset(LocalOffset);
        var today = DateOnly.FromDateTime(localNow.DateTime);

        var activeSchedule = schedules.FirstOrDefault(s => IsWithinActiveWindow(localNow, today, s, route));

        var detail = _mapper.Map<RouteDetailDto>(route);
        detail.Schedules = schedules
            .Select(s => new ScheduleDetailDto
            {
                Id = s.Id,
                DayOfWeek = s.DayOfWeek.ToString(),
                Direction = s.Direction,
                StartTime = s.StartTime
            })
            .ToList();

        if (activeSchedule is not null)
        {
            await PopulateStopsAsync(detail, route.Id, ct);
            return new DriverRouteTodayDto
            {
                Status = DriverRouteStatus.Active,
                Route = detail
            };
        }

        var next = NextOccurrence(schedules, localNow);
        if (schedules.Count == 0 || next is null)
        {
            return new DriverRouteTodayDto
            {
                Status = DriverRouteStatus.NoSchedule,
                Route = detail
            };
        }

        // Sin paradas hasta que la ruta este activa: el conductor no debe
        // ver el recorrido antes de tiempo, solo el recordatorio.
        return new DriverRouteTodayDto
        {
            Status = DriverRouteStatus.Scheduled,
            Route = detail,
            NextOccurrence = next
        };
    }

    private static bool IsWithinActiveWindow(
        DateTimeOffset localNow,
        DateOnly today,
        RouteSchedule schedule,
        RouteModel route)
    {
        if (schedule.DayOfWeek != localNow.DayOfWeek) return false;

        var start = ToLocal(today, schedule.StartTime) - EarlyGrace;
        var end = route.EndTime > schedule.StartTime
            ? ToLocal(today, route.EndTime)
            : ToLocal(today, schedule.StartTime) + FallbackDuration;

        return localNow >= start && localNow <= end;
    }

    private static DateTimeOffset? NextOccurrence(
        IReadOnlyList<RouteSchedule> schedules,
        DateTimeOffset localNow)
    {
        DateTimeOffset? next = null;

        for (var dayOffset = 0; dayOffset <= DaysAheadToScan; dayOffset++)
        {
            var date = DateOnly.FromDateTime(localNow.DateTime).AddDays(dayOffset);

            foreach (var schedule in schedules)
            {
                if (schedule.DayOfWeek != date.DayOfWeek) continue;

                var occurrence = ToLocal(date, schedule.StartTime);
                if (occurrence <= localNow) continue;

                if (next is null || occurrence < next.Value) next = occurrence;
            }

            // Ya hay una salida hoy mas temprano: no hace falta mirar mas dias.
            if (next is not null && DateOnly.FromDateTime(next.Value.DateTime) == date) break;
        }

        return next;
    }

    private static DateTimeOffset ToLocal(DateOnly date, TimeOnly time)
    {
        var local = date.ToDateTime(time);
        return new DateTimeOffset(local, LocalOffset);
    }

    private static DriverRouteTodayDto NoRoute() => new()
    {
        Status = DriverRouteStatus.NoRoute
    };

    private async Task PopulateStopsAsync(RouteDetailDto detail, Guid routeId, CancellationToken ct)
    {
        var routeStops = await _routeStopRepository.GetByRouteIdAsync(routeId, ct);
        var stops = await _stopRepository.GetByIdsAsync(routeStops.Select(rs => rs.StopId), ct);
        var stopById = stops.ToDictionary(s => s.Id);

        detail.Stops = routeStops
            .Where(rs => rs.Status == Status.Active && stopById.ContainsKey(rs.StopId))
            .Select(rs => new StopDetailDto
            {
                Id = rs.StopId,
                Name = stopById[rs.StopId].Name,
                Address = stopById[rs.StopId].Address,
                Latitude = stopById[rs.StopId].Latitude,
                Longitude = stopById[rs.StopId].Longitude,
                OrderSequence = rs.OrderSequence
            })
            .ToList();
    }
}
