using Microsoft.EntityFrameworkCore;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.Out;
using ms_route.Api.Domain.Service;
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

    public async Task UpdateStartTimeAsync(
        Guid routeBusAssignmentId,
        TimeOnly startTime,
        CancellationToken ct = default)
    {
        await _context.RouteSchedules
            .Where(s => s.RouteBusAssignmentsId == routeBusAssignmentId && s.Status == Status.Active)
            .ExecuteUpdateAsync(set => set.SetProperty(s => s.StartTime, startTime), ct);
    }

    public async Task SynchronizeActiveDaysAsync(
        Guid routeBusAssignmentId,
        TimeOnly startTime,
        IReadOnlyCollection<DayOfWeek> days,
        CancellationToken ct = default)
    {
        var schedules = await _context.RouteSchedules
            .Where(s => s.RouteBusAssignmentsId == routeBusAssignmentId)
            .ToListAsync(ct);
        var activeSchedules = new HashSet<(DayOfWeek Day, string Direction)>();
        var configuredDays = days.ToHashSet();

        foreach (var schedule in schedules)
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
                if (activeSchedules.Add((day, direction)))
                {
                    _context.RouteSchedules.Add(new RouteScheduleEntity
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
        }

        await _context.SaveChangesAsync(ct);
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
