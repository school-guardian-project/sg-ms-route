using ms_route.Api.Domain.Model;

namespace ms_route.Api.Domain.Ports.Out;

public interface IRouteExecutionRepository
{
    Task<RouteExecution?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Viaje activo mas reciente del conductor cuyo inicio es igual o posterior
    /// a <paramref name="startedFromUtc"/> (expresado en UTC, igual que la
    /// columna <c>RouteExecution.StartDateTime</c>).
    ///
    /// El corte por fecha evita que un viaje que nunca se cerro quede reportado
    /// como activo para siempre; lo calcula el caso de uso, que conoce la zona
    /// horaria de la operacion.
    /// </summary>
    Task<RouteExecution?> GetActiveByDriverAsync(
        Guid driverId, DateTime startedFromUtc, CancellationToken ct = default);

    Task<bool> HasActiveByRouteIdAsync(Guid routeId, CancellationToken ct = default);
    Task<RouteExecution> SaveAsync(RouteExecution execution, CancellationToken ct = default);
    Task UpdateAsync(RouteExecution execution, CancellationToken ct = default);
}
