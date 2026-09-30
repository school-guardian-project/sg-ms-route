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

    public async Task<IReadOnlyList<RouteStop>> GetByRouteIdAsync(Guid routeId, CancellationToken ct = default)
    {
        var entities = await _context.RouteStops
            .AsNoTracking()
            .Where(rs => rs.RouteId == routeId)
            .OrderBy(rs => rs.OrderSequence)
            .ToListAsync(ct);

        return entities.Select(ToDomain).ToList();
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
