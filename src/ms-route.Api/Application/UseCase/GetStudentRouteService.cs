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
    private readonly IMapper _mapper;

    public GetStudentRouteService(
        IRouteRepository routeRepository,
        IRouteStudentAssignmentRepository assignmentRepository,
        IRouteStopRepository routeStopRepository,
        IStopRepository stopRepository,
        IMapper mapper)
    {
        _routeRepository = routeRepository;
        _assignmentRepository = assignmentRepository;
        _routeStopRepository = routeStopRepository;
        _stopRepository = stopRepository;
        _mapper = mapper;
    }

    public async Task<RouteDetailDto> ExecuteAsync(Guid studentId, CancellationToken ct = default)
    {
        var assignment = await _assignmentRepository.GetActiveByProfileIdAsync(studentId, ct);
        var routeStop = assignment is null
            ? null
            : await _routeStopRepository.GetByIdAsync(assignment.RouteStopId, ct);
        var route = routeStop is null
            ? null
            : await _routeRepository.GetByIdAsync(routeStop.RouteId, ct);

        if (route is null)
            throw new InvalidOperationException("No route assigned yet");

        var detail = _mapper.Map<RouteDetailDto>(route);
        await PopulateStopsAsync(detail, route.Id, ct);

        return detail;
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
