using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Tests.Fakes;

public class InMemoryRouteScheduleRepository : IRouteScheduleRepository
{
    public readonly List<RouteSchedule> Schedules = new();

    /// <summary>Asignaciones bus-ruta conocidas (RouteBusAssignmentsId -> RouteId).</summary>
    public readonly Dictionary<Guid, Guid> AssignmentRouteByAssignment = new();

    public Task<IReadOnlyList<RouteSchedule>> GetActiveByAssignmentIdAsync(
        Guid routeBusAssignmentId,
        CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<RouteSchedule>>(Schedules
            .Where(s => s.RouteBusAssignmentsId == routeBusAssignmentId && s.Status == Status.Active)
            .ToList());

    public Task<IReadOnlyList<RouteSchedule>> GetByRouteIdAsync(
        Guid routeId,
        CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<RouteSchedule>>(Schedules
            .Where(s => s.Status == Status.Active
                        && AssignmentRouteByAssignment.TryGetValue(s.RouteBusAssignmentsId, out var route)
                        && route == routeId)
            .ToList());

    public Task<bool> ExistsActiveAsync(
        Guid routeBusAssignmentId,
        DayOfWeek dayOfWeek,
        string direction,
        CancellationToken ct = default)
        => Task.FromResult(Schedules.Any(s => s.RouteBusAssignmentsId == routeBusAssignmentId
                                              && s.DayOfWeek == dayOfWeek
                                              && s.Direction == direction
                                              && s.Status == Status.Active));

    public Task<RouteSchedule> SaveAsync(RouteSchedule schedule, CancellationToken ct = default)
    {
        Schedules.Add(schedule);
        return Task.FromResult(schedule);
    }
}
