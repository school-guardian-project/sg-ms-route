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
