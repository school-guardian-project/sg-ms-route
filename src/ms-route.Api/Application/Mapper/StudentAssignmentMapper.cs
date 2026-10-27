using ms_route.Api.Application.Dto.Response;
using ms_route.Api.Domain.Model;

namespace ms_route.Api.Application.Mapper;

/// <summary>
/// Maps RouteStudentAssignment domain entities to Response DTOs.
/// This mapper is stateless and can be registered as a singleton.
/// </summary>
public interface IStudentAssignmentMapper
{
    StudentAssignmentResponse ToResponse(RouteStudentAssignment assignment, RouteStop routeStop);
}

public sealed class StudentAssignmentMapper : IStudentAssignmentMapper
{
    public StudentAssignmentResponse ToResponse(RouteStudentAssignment assignment, RouteStop routeStop)
    {
        return new StudentAssignmentResponse(
            assignment.Id,
            routeStop.RouteId,
            assignment.ProfileId,
            assignment.RouteStopId,
            assignment.Status.ToString());
    }
}