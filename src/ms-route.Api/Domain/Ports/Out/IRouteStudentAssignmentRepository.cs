using ms_route.Api.Domain.Model;

namespace ms_route.Api.Domain.Ports.Out;

public interface IRouteStudentAssignmentRepository
{
    Task<RouteStudentAssignment> SaveAsync(RouteStudentAssignment assignment, CancellationToken ct = default);

    Task<RouteStudentAssignment?> GetActiveByProfileIdAsync(Guid profileId, CancellationToken ct = default);
}
