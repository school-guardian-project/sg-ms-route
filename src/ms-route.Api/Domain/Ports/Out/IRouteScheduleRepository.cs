using ms_route.Api.Domain.Model;

namespace ms_route.Api.Domain.Ports.Out;

public interface IRouteScheduleRepository
{
    Task<IReadOnlyList<RouteSchedule>> GetActiveByAssignmentIdAsync(
        Guid routeBusAssignmentId,
        CancellationToken ct = default);

    Task<IReadOnlyList<RouteSchedule>> GetByRouteIdAsync(
        Guid routeId,
        CancellationToken ct = default);

    Task<bool> ExistsActiveAsync(
        Guid routeBusAssignmentId,
        DayOfWeek dayOfWeek,
        string direction,
        CancellationToken ct = default);

    Task<RouteSchedule> SaveAsync(RouteSchedule schedule, CancellationToken ct = default);

    /// <summary>Cambia la hora de salida de los horarios activos de la asignacion.</summary>
    Task UpdateStartTimeAsync(
        Guid routeBusAssignmentId,
        TimeOnly startTime,
        CancellationToken ct = default);

    Task SynchronizeActiveDaysAsync(
        Guid routeBusAssignmentId,
        TimeOnly startTime,
        IReadOnlyCollection<DayOfWeek> days,
        CancellationToken ct = default);
}
