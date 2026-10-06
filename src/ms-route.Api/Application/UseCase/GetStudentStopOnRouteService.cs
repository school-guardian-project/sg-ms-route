using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Application.UseCase;

public class GetStudentStopOnRouteService : IGetStudentStopOnRouteUseCase
{
    private readonly IRouteStudentAssignmentRepository _assignmentRepository;
    private readonly IRouteStopRepository _routeStopRepository;
    private readonly IStopRepository _stopRepository;

    public GetStudentStopOnRouteService(
        IRouteStudentAssignmentRepository assignmentRepository,
        IRouteStopRepository routeStopRepository,
        IStopRepository stopRepository)
    {
        _assignmentRepository = assignmentRepository;
        _routeStopRepository = routeStopRepository;
        _stopRepository = stopRepository;
    }

    public async Task<StudentRouteStopDto?> ExecuteAsync(Guid routeId, Guid studentProfileId, CancellationToken ct = default)
    {
        var assignment = await _assignmentRepository.GetActiveByProfileIdAsync(studentProfileId, ct);
        if (assignment is null)
            return null;

        var routeStops = await _routeStopRepository.GetByRouteIdAsync(routeId, ct);
        var routeStop = routeStops.FirstOrDefault(rs => rs.Id == assignment.RouteStopId);
        if (routeStop is null)
            return null;

        var stop = await _stopRepository.GetByIdAsync(routeStop.StopId, ct);

        return new StudentRouteStopDto
        {
            RouteId = routeId,
            StudentProfileId = studentProfileId,
            RouteStopId = routeStop.Id,
            StopId = routeStop.StopId,
            StopName = stop?.Name ?? string.Empty
        };
    }
}
