using AutoMapper;
using ms_route.Api.Application.Configuration;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;
using RouteModel = ms_route.Api.Domain.Model.Route;

namespace ms_route.Api.Application.UseCase;

public class GetDriverRouteTodayService : IGetDriverRouteTodayUseCase
{
    // El conductor puede ver la ruta desde 15 minutos antes de la hora programada.
    private static readonly TimeSpan EarlyGrace = TimeSpan.FromMinutes(15);

    // Si la hora de fin de la ruta no supera a la salida programada (dato inconsistente),
    // la ventana se cierra 1 hora despues de la salida para no dejar la ruta nunca activa.
    private static readonly TimeSpan FallbackDuration = TimeSpan.FromHours(1);

    private const int MinutesPerDay = 24 * 60;
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
        var bus = await _fleetService.GetBusAssignedToDriverAsync(driverId, ct);
        if (bus is null) return NoRoute();

        var assignment = await _routeBusAssignmentRepository.GetActiveByBusIdAsync(bus.Id, ct);
        if (assignment is null) return NoRoute();

        var route = await _routeRepository.GetByIdAsync(assignment.RouteId, ct);
        if (route is null || route.Status != Status.Active) return NoRoute();

        var schedules = await _scheduleRepository.GetActiveByAssignmentIdAsync(assignment.Id, ct);

        var localNow = LocalClock.UtcNow(_timeProvider);

        var activeSchedule = schedules.FirstOrDefault(s => IsWithinActiveWindow(localNow, s, route));

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

        // FR-9.2: el conductor identifica su bus (placa) junto a la ruta.
        detail.Bus = new RouteBusDto
        {
            Id = bus.Id,
            Plate = bus.Plate,
            Capacity = bus.Capacity,
            DriverProfileId = bus.DriverProfileId,
            DriverName = bus.DriverName
        };

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

    /// <summary>
    /// Decide si la ruta esta activa ahora para el conductor.
    ///
    /// La ventana corre de <c>salida - EarlyGrace</c> hasta la hora de fin de la
    /// ruta. Se compara sobre minutos del dia con aritmetrica modular para que
    /// funcione tambien una ruta cuya hora de fin es menor que la de salida
    /// (recorrido que cruza la medianoche).
    ///
    /// Si la hora de fin no es util (EndTime igual a StartTime, p.ej. el
    /// 00:00:00 que dejan los datos sin configurar) la ventana se cierra
    /// FallbackDuration despues de la salida. Si tampoco hay salida resoluble
    /// no hay ventana activa: sin horario la ruta no debe reportarse como activa.
    /// </summary>
    private static bool IsWithinActiveWindow(
        DateTimeOffset localNow,
        RouteSchedule schedule,
        RouteModel route)
    {
        if (schedule.DayOfWeek != localNow.DayOfWeek) return false;

        var startMinutes = schedule.StartTime.Hour * 60 + schedule.StartTime.Minute;
        var endMinutes = route.EndTime.Hour * 60 + route.EndTime.Minute;

        // Fin inutil: solo existe salida programada. Se acota la ventana para no
        // dejar la ruta activa de forma indefinida.
        if (endMinutes <= startMinutes)
            endMinutes = startMinutes + (int)FallbackDuration.TotalMinutes;

        var opensAt = startMinutes - (int)EarlyGrace.TotalMinutes;
        var closesAt = endMinutes;

        var nowMinutes = localNow.Hour * 60 + localNow.Minute;
        var elapsed = (nowMinutes - opensAt + MinutesPerDay) % MinutesPerDay;
        var span = (closesAt - opensAt + MinutesPerDay) % MinutesPerDay;

        return span > 0 && elapsed <= span;
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

                var occurrence = LocalClock.AtLocal(date, schedule.StartTime);
                if (occurrence <= localNow) continue;

                if (next is null || occurrence < next.Value) next = occurrence;
            }

            // Ya hay una salida hoy mas temprano: no hace falta mirar mas dias.
            if (next is not null && DateOnly.FromDateTime(next.Value.DateTime) == date) break;
        }

        return next;
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
