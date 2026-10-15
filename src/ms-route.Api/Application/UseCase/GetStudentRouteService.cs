using AutoMapper;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Application.UseCase;

public class GetStudentRouteService : IGetStudentRouteUseCase
{
    private readonly IRouteRepository _routeRepository;
    private readonly IRouteStudentAssignmentRepository _assignmentRepository;
    private readonly IRouteStopRepository _routeStopRepository;
    private readonly IStopRepository _stopRepository;
    private readonly IRouteBusAssignmentRepository _routeBusAssignmentRepository;
    private readonly IRouteScheduleRepository _scheduleRepository;
    private readonly IFleetService _fleetService;
    private readonly IMapper _mapper;

    public GetStudentRouteService(
        IRouteRepository routeRepository,
        IRouteStudentAssignmentRepository assignmentRepository,
        IRouteStopRepository routeStopRepository,
        IStopRepository stopRepository,
        IRouteBusAssignmentRepository routeBusAssignmentRepository,
        IRouteScheduleRepository scheduleRepository,
        IFleetService fleetService,
        IMapper mapper)
    {
        _routeRepository = routeRepository;
        _assignmentRepository = assignmentRepository;
        _routeStopRepository = routeStopRepository;
        _stopRepository = stopRepository;
        _routeBusAssignmentRepository = routeBusAssignmentRepository;
        _scheduleRepository = scheduleRepository;
        _fleetService = fleetService;
        _mapper = mapper;
    }

    public async Task<RouteDetailDto?> ExecuteAsync(Guid studentId, CancellationToken ct = default)
    {
        var assignment = await _assignmentRepository.GetActiveByProfileIdAsync(studentId, ct);
        var routeStop = assignment is null
            ? null
            : await _routeStopRepository.GetByIdAsync(assignment.RouteStopId, ct);
        var route = routeStop is null
            ? null
            : await _routeRepository.GetByIdAsync(routeStop.RouteId, ct);

        // Sin asignacion activa todavia: es el estado esperado de un estudiante
        // recien registrado, no una falla. El controlador responde 404.
        if (route is null)
            return null;

        var detail = _mapper.Map<RouteDetailDto>(route);
        await PopulateStopsAsync(detail, route.Id, ct);
        await PopulateBusAndSchedulesAsync(detail, route.Id, ct);

        return detail;
    }

    /// <summary>
    /// FR-9.1: el acudiente ve el bus, el conductor y las horas programadas de
    /// la ruta. Los horarios cuelgan de la asignacion bus-ruta activa; si la
    /// ruta aun no tiene bus asignado no hay horarios ni bus que mostrar.
    /// </summary>
    private async Task PopulateBusAndSchedulesAsync(RouteDetailDto detail, Guid routeId, CancellationToken ct)
    {
        var assignment = await _routeBusAssignmentRepository.GetActiveByRouteIdAsync(routeId, ct);
        if (assignment is null) return;

        var schedules = await _scheduleRepository.GetActiveByAssignmentIdAsync(assignment.Id, ct);
        detail.Schedules = schedules
            .Select(s => new ScheduleDetailDto
            {
                Id = s.Id,
                DayOfWeek = s.DayOfWeek.ToString(),
                Direction = s.Direction,
                StartTime = s.StartTime
            })
            .ToList();

        var bus = await _fleetService.GetBusDetailsAsync(assignment.BusId, ct);
        if (bus is null) return;

        detail.Bus = new RouteBusDto
        {
            Id = bus.Id,
            Plate = bus.Plate,
            Capacity = bus.Capacity,
            DriverProfileId = bus.DriverProfileId,
            DriverName = bus.DriverName
        };
    }

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
