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
    private readonly IRouteStopFilter _stopFilter;
    private readonly IRouteDetailPopulator _detailPopulator;

    public GetStudentRouteService(
        IRouteRepository routeRepository,
        IRouteStudentAssignmentRepository assignmentRepository,
        IRouteStopRepository routeStopRepository,
        IStopRepository stopRepository,
        IRouteBusAssignmentRepository routeBusAssignmentRepository,
        IRouteScheduleRepository scheduleRepository,
        IFleetService fleetService,
        IMapper mapper,
        IRouteStopFilter stopFilter,
        IRouteDetailPopulator detailPopulator)
    {
        _routeRepository = routeRepository;
        _assignmentRepository = assignmentRepository;
        _routeStopRepository = routeStopRepository;
        _stopRepository = stopRepository;
        _routeBusAssignmentRepository = routeBusAssignmentRepository;
        _scheduleRepository = scheduleRepository;
        _fleetService = fleetService;
        _mapper = mapper;
        _stopFilter = stopFilter;
        _detailPopulator = detailPopulator;
    }

    public async Task<RouteDetailDto?> ExecuteAsync(Guid studentId, CancellationToken ct = default, bool isParent = false, Guid? studentProfileId = null)
    {
        var assignment = await _assignmentRepository.GetActiveByProfileIdAsync(studentId, ct);
        var routeStop = assignment is null
            ? null
            : await _routeStopRepository.GetByIdAsync(assignment.RouteStopId, ct);
        var route = routeStop is null
            ? null
            : await _routeRepository.GetByIdAsync(routeStop.RouteId, ct);

        if (route is null)
            return null;

        var detail = _mapper.Map<RouteDetailDto>(route);
        await _detailPopulator.PopulateAsync(detail, route.Id, ct, isParent, studentProfileId, assignment);

        return detail;
    }
}