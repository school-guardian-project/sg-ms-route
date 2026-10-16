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

    public async Task<IReadOnlyList<RouteSchedule>> GetByRouteIdAsync(
        Guid routeId,
        CancellationToken ct = default)
    {
        // Los horarios cuelgan de la asignacion bus-ruta (RouteBusAssignmentsId),
        // no de la ruta: se resuelven atraves de las asignaciones activas de la ruta.
        var entities = await (
                from s in _context.RouteSchedules.AsNoTracking()
                join a in _context.RouteBusAssignments.AsNoTracking()
                    on s.RouteBusAssignmentsId equals a.Id
                where a.RouteId == routeId
                      && a.Status == Status.Active
                      && s.Status == Status.Active
                orderby s.DayOfWeek, s.StartTime
                select s)
            .ToListAsync(ct);

        return entities.Select(ToDomain).ToList();
    }

    public async Task<bool> ExistsActiveAsync(
        Guid routeBusAssignmentId,
        DayOfWeek dayOfWeek,
        string direction,
        CancellationToken ct = default)
    {
        return await _context.RouteSchedules
            .AsNoTracking()
            .AnyAsync(s => s.RouteBusAssignmentsId == routeBusAssignmentId
                           && s.DayOfWeek == dayOfWeek
                           && s.Direction == direction
                           && s.Status == Status.Active, ct);
    }

    public async Task<RouteSchedule> SaveAsync(RouteSchedule schedule, CancellationToken ct = default)
    {
        var entity = new RouteScheduleEntity
        {
            Id = schedule.Id,
            RouteBusAssignmentsId = schedule.RouteBusAssignmentsId,
            DayOfWeek = schedule.DayOfWeek,
            Direction = schedule.Direction,
            StartTime = schedule.StartTime,
            Status = schedule.Status
        };

        await _context.RouteSchedules.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);

        return ToDomain(entity);
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
