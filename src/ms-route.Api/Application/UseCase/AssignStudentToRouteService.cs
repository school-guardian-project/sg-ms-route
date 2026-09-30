using AutoMapper;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Application.UseCase;

public class AssignStudentToRouteService : IAssignStudentToRouteUseCase
{
    private readonly IRouteStudentAssignmentRepository _assignmentRepository;
    private readonly IRouteStopRepository _routeStopRepository;
    private readonly IMapper _mapper;

    public AssignStudentToRouteService(
        IRouteStudentAssignmentRepository assignmentRepository,
        IRouteStopRepository routeStopRepository,
        IMapper mapper)
    {
        _assignmentRepository = assignmentRepository;
        _routeStopRepository = routeStopRepository;
        _mapper = mapper;
    }

    public async Task<StudentAssignmentDto> ExecuteAsync(Guid routeId, Guid studentId, Guid stopId, CancellationToken ct = default)
    {
        var routeStops = await _routeStopRepository.GetByRouteIdAsync(routeId, ct);
        if (!routeStops.Any(rs => rs.StopId == stopId))
            throw new InvalidOperationException("The stop does not belong to this route");

        var assignment = new RouteStudentAssignment
        {
            Id = Guid.NewGuid(),
            ProfileId = studentId,
            RouteStopId = stopId,
            Status = Status.Active
        };

        var saved = await _assignmentRepository.SaveAsync(assignment, ct);

        return _mapper.Map<StudentAssignmentDto>(saved);
    }
}
