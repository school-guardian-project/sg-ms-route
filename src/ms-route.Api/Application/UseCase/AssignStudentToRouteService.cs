using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Application.UseCase;

public class AssignStudentToRouteService : IAssignStudentToRouteUseCase
{
    private readonly IRouteStudentAssignmentRepository _assignmentRepository;
    private readonly IRouteStopRepository _routeStopRepository;

    public AssignStudentToRouteService(
        IRouteStudentAssignmentRepository assignmentRepository,
        IRouteStopRepository routeStopRepository)
    {
        _assignmentRepository = assignmentRepository;
        _routeStopRepository = routeStopRepository;
    }

    public async Task<StudentAssignmentDto> ExecuteAsync(Guid routeId, Guid studentId, Guid stopId, CancellationToken ct = default)
    {
        var routeStops = await _routeStopRepository.GetByRouteIdAsync(routeId, ct);
        var routeStop = routeStops.FirstOrDefault(rs => rs.StopId == stopId);
        if (routeStop is null)
            throw new InvalidOperationException("The stop does not belong to this route");

        var assignment = new RouteStudentAssignment
        {
            Id = Guid.NewGuid(),
            ProfileId = studentId,
            RouteStopId = routeStop.Id,
            Status = Status.Active
        };

        var saved = await _assignmentRepository.SaveAsync(assignment, ct);

        return new StudentAssignmentDto
        {
            Id = saved.Id,
            RouteId = routeId,
            StudentId = saved.ProfileId,
            StopId = stopId,
            Status = saved.Status.ToString()
        };
    }
}
