using ms_route.Api.Domain.Model;

namespace ms_route.Api.Domain.Ports.Out;

public interface IRouteStopRepository
{
    Task<RouteStop?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<RouteStop>> GetByRouteIdAsync(Guid routeId, CancellationToken ct = default);

    Task<IReadOnlyDictionary<Guid, int>> CountByRouteIdsAsync(IEnumerable<Guid> routeIds, CancellationToken ct = default);
    Task<RouteStop> SaveAsync(RouteStop routeStop, CancellationToken ct = default);

    Task<IReadOnlyList<RouteStop>> GetActiveByStopIdsAsync(IEnumerable<Guid> stopIds, CancellationToken ct = default);

    /// <summary>
    /// Pasa la parada de <paramref name="fromRouteId"/> a <paramref name="routeId"/>
    /// reutilizando su RouteStop (los estudiantes asignados apuntan a ese id).
    /// Sin ruta de origen: mueve el unico enlace activo o crea uno nuevo.
    /// </summary>
    Task MoveStopToRouteAsync(Guid stopId, Guid? fromRouteId, Guid routeId, CancellationToken ct = default);
}
