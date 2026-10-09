using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Tests.Fakes;

public class InMemoryRouteScheduleRepository : IRouteScheduleRepository
{
    public readonly List<RouteSchedule> Schedules = new();

    public Task<IReadOnlyList<RouteSchedule>> GetActiveByAssignmentIdAsync(
        Guid routeBusAssignmentId,
        CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<RouteSchedule>>(Schedules
            .Where(s => s.RouteBusAssignmentsId == routeBusAssignmentId && s.Status == Status.Active)
            .ToList());
}
