using Microsoft.EntityFrameworkCore;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.Out;
using ms_route.Api.Infrastructure.Persistence.Entity;
using RouteContext = ms_route.Api.Infrastructure.Persistence.Context.RouteContext;

namespace ms_route.Api.Infrastructure.Repository;

public class RouteScheduleRepositoryImpl : IRouteScheduleRepository
{
    private readonly RouteContext _context;

    public RouteScheduleRepositoryImpl(RouteContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<RouteSchedule>> GetActiveByAssignmentIdAsync(
        Guid routeBusAssignmentId,
        CancellationToken ct = default)
    {
        var entities = await _context.RouteSchedules
            .AsNoTracking()
            .Where(s => s.RouteBusAssignmentsId == routeBusAssignmentId && s.Status == Status.Active)
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .ToListAsync(ct);

        return entities.Select(ToDomain).ToList();
    }

    private static RouteSchedule ToDomain(RouteScheduleEntity entity) => new()
    {
        Id = entity.Id,
        RouteBusAssignmentsId = entity.RouteBusAssignmentsId,
        DayOfWeek = entity.DayOfWeek,
        Direction = entity.Direction,
        StartTime = entity.StartTime,
        Status = entity.Status
    };
}
