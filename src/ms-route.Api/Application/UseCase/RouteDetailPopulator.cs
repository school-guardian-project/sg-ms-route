using AutoMapper;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Application.UseCase;

public class RouteDetailPopulator : IRouteDetailPopulator
{
    private readonly IRouteBusAssignmentRepository _routeBusAssignmentRepository;
    private readonly IRouteScheduleRepository _scheduleRepository;
    private readonly IFleetService _fleetService;
    private readonly IRouteStopRepository _routeStopRepository;
    private readonly IStopRepository _stopRepository;
    private readonly IMapper _mapper;
    private readonly IRouteStopFilter _stopFilter;

    public RouteDetailPopulator(
        IRouteBusAssignmentRepository routeBusAssignmentRepository,
        IRouteScheduleRepository scheduleRepository,
        IFleetService fleetService,
        IRouteStopRepository routeStopRepository,
        IStopRepository stopRepository,
        IMapper mapper,
        IRouteStopFilter stopFilter)
    {
        _routeBusAssignmentRepository = routeBusAssignmentRepository;
        _scheduleRepository = scheduleRepository;
        _fleetService = fleetService;
        _routeStopRepository = routeStopRepository;
        _stopRepository = stopRepository;
        _mapper = mapper;
        _stopFilter = stopFilter;
    }

    public async Task PopulateAsync(RouteDetailDto detail, Guid routeId, CancellationToken ct, bool isParent = false, Guid? studentProfileId = null, RouteStudentAssignment? assignment = null)
    {
        await PopulateStopsAsync(detail, routeId, ct, isParent, studentProfileId, assignment);
        await PopulateBusAndSchedulesAsync(detail, routeId, ct);
    }

    private async Task PopulateBusAndSchedulesAsync(RouteDetailDto detail, Guid routeId, CancellationToken ct)
    {
        var assignment = await _routeBusAssignmentRepository.GetActiveByRouteIdAsync(routeId, ct);
        if (assignment is null) return;

        var schedules = await _scheduleRepository.GetActiveByAssignmentIdAsync(assignment.Id, ct);
        detail.Schedules = _mapper.Map<List<ScheduleDetailDto>>(schedules);

        var bus = await _fleetService.GetBusDetailsAsync(assignment.BusId, ct);
        if (bus is null) return;

        detail.Bus = _mapper.Map<RouteBusDto>(bus);
    }

    private async Task PopulateStopsAsync(RouteDetailDto detail, Guid routeId, CancellationToken ct, bool isParent = false, Guid? studentProfileId = null, RouteStudentAssignment? assignment = null)
    {
        var routeStops = await _routeStopRepository.GetByRouteIdAsync(routeId, ct);
        var stops = await _stopRepository.GetByIdsAsync(routeStops.Select(rs => rs.StopId), ct);
        var stopById = stops.ToDictionary(s => s.Id);

        var filteredStops = _stopFilter.FilterStops(
            routeStops.Where(rs => rs.Status == Status.Active && stopById.ContainsKey(rs.StopId)).ToList(),
            stops.ToDictionary(s => s.Id),
            assignment,
            isParent,
            studentProfileId);

        detail.Stops = filteredStops
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