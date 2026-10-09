using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Tests.Fakes;

public class InMemoryRouteBusAssignmentRepository : IRouteBusAssignmentRepository
{
    public readonly List<RouteBusAssignment> Assignments = new();

    public Task<RouteBusAssignment> SaveAsync(RouteBusAssignment assignment, CancellationToken ct = default)
    {
        Assignments.Add(assignment);
        return Task.FromResult(assignment);
    }

    public Task<bool> RouteHasBusAsync(Guid routeId, CancellationToken ct = default)
        => Task.FromResult(Assignments.Any(a => a.RouteId == routeId && a.Status == Status.Active));

    public Task<RouteBusAssignment?> GetActiveByRouteIdAsync(Guid routeId, CancellationToken ct = default)
        => Task.FromResult(Assignments.FirstOrDefault(a => a.RouteId == routeId && a.Status == Status.Active));

    public Task<RouteBusAssignment?> GetActiveByBusIdAsync(Guid busId, CancellationToken ct = default)
        => Task.FromResult(Assignments.FirstOrDefault(a => a.BusId == busId && a.Status == Status.Active));
}
