using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.Out;
using RouteModel = ms_route.Api.Domain.Model.Route;

namespace ms_route.Tests.Fakes;

public class InMemoryRouteRepository : IRouteRepository
{
    public readonly List<RouteModel> Routes = new();

    public Task<RouteModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(Routes.FirstOrDefault(r => r.Id == id));

    public Task<IReadOnlyList<RouteModel>> GetAllAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<RouteModel>>(Routes.ToList());

    public Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
        => Task.FromResult(Routes.Any(r => r.Name == name));

    public Task<RouteModel> SaveAsync(RouteModel route, CancellationToken ct = default)
    {
        Routes.Add(route);
        return Task.FromResult(route);
    }

    public Task UpdateAsync(RouteModel route, CancellationToken ct = default)
    {
        var existing = Routes.FirstOrDefault(r => r.Id == route.Id)
            ?? throw new InvalidOperationException($"Route not found: {route.Id}");

        existing.Name = route.Name;
        existing.TargetSector = route.TargetSector;
        existing.CampuseId = route.CampuseId;
        existing.Status = route.Status;
        existing.UpdatedAt = route.UpdatedAt;

        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var existing = Routes.FirstOrDefault(r => r.Id == id)
            ?? throw new InvalidOperationException($"Route not found: {id}");

        Routes.Remove(existing);
        return Task.CompletedTask;
    }
}
