using ms_route.Api.Domain.Model;

namespace ms_route.Api.Domain.Ports.Out;

public interface IRouteBusAssignmentRepository
{
    Task<RouteBusAssignment> SaveAsync(RouteBusAssignment assignment, CancellationToken ct = default);
    Task<bool> RouteHasBusAsync(Guid routeId, CancellationToken ct = default);
    Task<RouteBusAssignment?> GetActiveByRouteIdAsync(Guid routeId, CancellationToken ct = default);
    Task<RouteBusAssignment?> GetActiveByBusIdAsync(Guid busId, CancellationToken ct = default);

    /// <summary>
    /// Releases the active bus assignment of the route (sets it to Inactive).
    /// Returns false when the route has no active bus assignment.
    /// </summary>
    Task<bool> UnassignAsync(Guid routeId, CancellationToken ct = default);
}
