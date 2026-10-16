using ms_route.Api.Domain.Model;
using RouteModel = ms_route.Api.Domain.Model.Route;

namespace ms_route.Api.Domain.Ports.Out;

public interface IRouteRepository
{
    Task<RouteModel?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<RouteModel>> GetAllAsync(IReadOnlyCollection<Guid>? campusIds = null, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
    Task<RouteModel> SaveAsync(RouteModel route, CancellationToken ct = default);
    Task UpdateAsync(RouteModel route, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
