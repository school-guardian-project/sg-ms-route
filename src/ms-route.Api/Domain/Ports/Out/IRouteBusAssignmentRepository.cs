using ms_route.Api.Domain.Model;

namespace ms_route.Api.Domain.Ports.Out;

public interface IRouteBusAssignmentRepository
{
    Task<RouteBusAssignment> SaveAsync(RouteBusAssignment assignment, CancellationToken ct = default);
    Task<bool> RouteHasBusAsync(Guid routeId, CancellationToken ct = default);
}
