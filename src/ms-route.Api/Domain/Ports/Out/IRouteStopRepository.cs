using ms_route.Api.Domain.Model;

namespace ms_route.Api.Domain.Ports.Out;

public interface IRouteStopRepository
{
    Task<RouteStop?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<RouteStop>> GetByRouteIdAsync(Guid routeId, CancellationToken ct = default);

    Task<IReadOnlyDictionary<Guid, int>> CountByRouteIdsAsync(IEnumerable<Guid> routeIds, CancellationToken ct = default);
    Task<RouteStop> SaveAsync(RouteStop routeStop, CancellationToken ct = default);
}
