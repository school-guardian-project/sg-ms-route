using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Tests.Fakes;

public class InMemoryRouteStopRepository : IRouteStopRepository
{
    public readonly List<RouteStop> RouteStops = new();

    public Task<IReadOnlyList<RouteStop>> GetByRouteIdAsync(Guid routeId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<RouteStop>>(RouteStops.Where(rs => rs.RouteId == routeId).ToList());

    public Task<RouteStop?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(RouteStops.FirstOrDefault(rs => rs.Id == id));

    public Task<RouteStop> SaveAsync(RouteStop routeStop, CancellationToken ct = default)
    {
        RouteStops.Add(routeStop);
        return Task.FromResult(routeStop);
    }

    public Task<IReadOnlyList<RouteStop>> GetActiveByStopIdsAsync(IEnumerable<Guid> stopIds, CancellationToken ct = default)
    {
        var ids = stopIds.ToHashSet();
        return Task.FromResult<IReadOnlyList<RouteStop>>(
            RouteStops.Where(rs => ids.Contains(rs.StopId) && rs.Status == Status.Active).ToList());
    }

    public Task MoveStopToRouteAsync(Guid stopId, Guid? fromRouteId, Guid routeId, CancellationToken ct = default)
    {
        var active = RouteStops.Where(rs => rs.StopId == stopId && rs.Status == Status.Active).ToList();
        if (active.Any(rs => rs.RouteId == routeId))
        {
            var from = active.FirstOrDefault(rs => fromRouteId != null && rs.RouteId == fromRouteId && rs.RouteId != routeId);
            if (from is not null) from.Status = Status.Inactive;
            return Task.CompletedTask;
        }
        var source = fromRouteId is not null
            ? active.FirstOrDefault(rs => rs.RouteId == fromRouteId)
            : active.Count == 1 ? active[0] : null;
        if (source is null)
        {
            source = new RouteStop { StopId = stopId };
            RouteStops.Add(source);
        }
        source.OrderSequence = RouteStops.Where(rs => rs.RouteId == routeId).Select(rs => rs.OrderSequence).DefaultIfEmpty(0).Max() + 1;
        source.RouteId = routeId;
        source.Status = Status.Active;
        return Task.CompletedTask;
    }
    public Task<IReadOnlyDictionary<Guid, int>> CountByRouteIdsAsync(IEnumerable<Guid> routeIds, CancellationToken ct = default)
    {
        var ids = routeIds.ToHashSet();
        var counts = RouteStops
            .Where(rs => ids.Contains(rs.RouteId) && rs.Status == Status.Active)
            .GroupBy(rs => rs.RouteId)
            .ToDictionary(g => g.Key, g => g.Count());

        return Task.FromResult<IReadOnlyDictionary<Guid, int>>(counts);
    }
}
