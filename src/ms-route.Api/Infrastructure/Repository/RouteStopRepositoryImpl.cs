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

    public async Task<IReadOnlyList<RouteStop>> GetActiveByStopIdsAsync(IEnumerable<Guid> stopIds, CancellationToken ct = default)
    {
        var ids = stopIds.Distinct().ToList();
        if (ids.Count == 0)
            return [];

        var entities = await _context.RouteStops
            .AsNoTracking()
            .Where(rs => ids.Contains(rs.StopId) && rs.Status == Status.Active)
            .ToListAsync(ct);

        return entities.Select(ToDomain).ToList();
    }

    public async Task MoveStopToRouteAsync(Guid stopId, Guid? fromRouteId, Guid routeId, CancellationToken ct = default)
    {
        var links = await _context.RouteStops
            .Where(rs => rs.StopId == stopId)
            .ToListAsync(ct);
        var active = links.Where(rs => rs.Status == Status.Active).ToList();

        // Ya esta en la ruta destino: solo se suelta la de origen, si la habia.
        if (active.Any(rs => rs.RouteId == routeId))
        {
            var from = active.FirstOrDefault(rs => fromRouteId != null && rs.RouteId == fromRouteId && rs.RouteId != routeId);
            if (from is null) return;
            from.Status = Status.Inactive;
            await _context.SaveChangesAsync(ct);
            return;
        }

        var nextOrder = (await _context.RouteStops
            .Where(rs => rs.RouteId == routeId && rs.Status == Status.Active)
            .MaxAsync(rs => (int?)rs.OrderSequence, ct) ?? 0) + 1;

        var source = fromRouteId is not null
            ? active.FirstOrDefault(rs => rs.RouteId == fromRouteId)
            : active.Count == 1 ? active[0] : null;

        var target = source
                     ?? links.FirstOrDefault(rs => rs.RouteId == routeId);
        if (target is null)
        {
            target = new RouteStopEntity { Id = Guid.NewGuid(), StopId = stopId };
            await _context.RouteStops.AddAsync(target, ct);
        }

        target.RouteId = routeId;
        target.OrderSequence = nextOrder;
        target.Status = Status.Active;

        await _context.SaveChangesAsync(ct);
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
