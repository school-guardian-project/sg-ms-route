using ms_route.Api.Application.Dto.Response;
using ms_route.Api.Domain.Model;

namespace ms_route.Api.Application.Mapper;

/// <summary>
/// Maps RouteStudentAssignment domain entities to Response DTOs.
/// This mapper is stateless and can be registered as a singleton.
/// </summary>
public interface IStudentAssignmentMapper
{
    StudentAssignmentResponse ToResponse(RouteStudentAssignment assignment);
}

public sealed class StudentAssignmentMapper : IStudentAssignmentMapper
{
    public StudentAssignmentResponse ToResponse(RouteStudentAssignment assignment)
    {
        return new StudentAssignmentResponse(
            assignment.Id,
            assignment.RouteId,
            assignment.ProfileId,
            assignment.RouteStopId,
            assignment.Status.ToString());
    }
}