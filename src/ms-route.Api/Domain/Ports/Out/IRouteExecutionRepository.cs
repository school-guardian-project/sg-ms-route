using ms_route.Api.Domain.Model;

namespace ms_route.Api.Domain.Ports.Out;

public interface IRouteExecutionRepository
{
    Task<RouteExecution?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<RouteExecution?> GetActiveByDriverAsync(Guid driverId, CancellationToken ct = default);
    Task<RouteExecution> SaveAsync(RouteExecution execution, CancellationToken ct = default);
    Task UpdateAsync(RouteExecution execution, CancellationToken ct = default);
}
