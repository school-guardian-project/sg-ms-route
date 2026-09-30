using ms_route.Api.Domain.Model;

namespace ms_route.Api.Domain.Ports.Out;

public interface IStopRepository
{
    Task<Stop?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Stop>> GetAllAsync(CancellationToken ct = default);
    Task<Stop> SaveAsync(Stop stop, CancellationToken ct = default);
    Task UpdateAsync(Stop stop, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
