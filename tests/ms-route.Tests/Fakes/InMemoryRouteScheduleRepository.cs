using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.Out;
using ms_route.Api.Domain.Service;

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

    public Task UpdateStartTimeAsync(
        Guid routeBusAssignmentId,
        TimeOnly startTime,
        CancellationToken ct = default)
    {
        foreach (var schedule in Schedules.Where(s =>
                     s.RouteBusAssignmentsId == routeBusAssignmentId && s.Status == Status.Active))
            schedule.StartTime = startTime;
        return Task.CompletedTask;
    }

    public Task SynchronizeActiveDaysAsync(
        Guid routeBusAssignmentId,
        TimeOnly startTime,
        IReadOnlyCollection<DayOfWeek> days,
        CancellationToken ct = default)
    {
        var configuredDays = days.ToHashSet();
        var activeSchedules = new HashSet<(DayOfWeek Day, string Direction)>();

        foreach (var schedule in Schedules.Where(s => s.RouteBusAssignmentsId == routeBusAssignmentId))
        {
            var isConfigured = configuredDays.Contains(schedule.DayOfWeek)
                && RouteScheduleProvisioner.Directions.Contains(schedule.Direction);
            var key = (schedule.DayOfWeek, schedule.Direction);
            var shouldBeActive = schedule.Status == Status.Active
                && isConfigured
                && activeSchedules.Add(key);

            if (schedule.Status == Status.Active)
                schedule.Status = shouldBeActive ? Status.Active : Status.Inactive;
            if (shouldBeActive)
                schedule.StartTime = startTime;
        }

        foreach (var day in configuredDays)
        {
            foreach (var direction in RouteScheduleProvisioner.Directions)
            {
                if (!activeSchedules.Add((day, direction)))
                    continue;

                Schedules.Add(new RouteSchedule
                {
                    Id = Guid.NewGuid(),
                    RouteBusAssignmentsId = routeBusAssignmentId,
                    DayOfWeek = day,
                    Direction = direction,
                    StartTime = startTime,
                    Status = Status.Active
                });
            }
        }

        return Task.CompletedTask;
    }
}
