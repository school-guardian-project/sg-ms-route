using Microsoft.EntityFrameworkCore;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.Out;
using ms_route.Api.Infrastructure.Persistence.Context;
using ms_route.Api.Infrastructure.Persistence.Entity;
using RouteContext = ms_route.Api.Infrastructure.Persistence.Context.RouteContext;

namespace ms_route.Api.Infrastructure.Repository;

public class RouteStopRepositoryImpl : IRouteStopRepository
{
    private readonly RouteContext _context;

    public RouteStopRepositoryImpl(RouteContext context)
    {
        _context = context;
    }

    public async Task<RouteStop?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.RouteStops
            .AsNoTracking()
            .FirstOrDefaultAsync(rs => rs.Id == id, ct);

        return entity is null ? null : ToDomain(entity);
    }

    public async Task<IReadOnlyList<RouteStop>> GetByRouteIdAsync(Guid routeId, CancellationToken ct = default)
    {
        var entities = await _context.RouteStops
            .AsNoTracking()
            .Where(rs => rs.RouteId == routeId)
            .OrderBy(rs => rs.OrderSequence)
            .ToListAsync(ct);

        return entities.Select(ToDomain).ToList();
    }

    public async Task<IReadOnlyDictionary<Guid, int>> CountByRouteIdsAsync(IEnumerable<Guid> routeIds, CancellationToken ct = default)
    {
        var ids = routeIds.Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<Guid, int>();

        var counts = await _context.RouteStops
            .AsNoTracking()
            .Where(rs => ids.Contains(rs.RouteId) && rs.Status == Status.Active)
            .GroupBy(rs => rs.RouteId)
            .Select(g => new { RouteId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        return counts.ToDictionary(x => x.RouteId, x => x.Count);
    }

    public async Task<RouteStop> SaveAsync(RouteStop routeStop, CancellationToken ct = default)
    {
        var entity = new RouteStopEntity
        {
            Id = routeStop.Id,
            RouteId = routeStop.RouteId,
            StopId = routeStop.StopId,
            OrderSequence = routeStop.OrderSequence,
            Status = routeStop.Status
        };

        await _context.RouteStops.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);

        return ToDomain(entity);
    }

    private static RouteStop ToDomain(RouteStopEntity entity) => new()
    {
        Id = entity.Id,
        RouteId = entity.RouteId,
        StopId = entity.StopId,
        OrderSequence = entity.OrderSequence,
        Status = entity.Status
    };
}
