using ms_route.Api.Domain.Model;

namespace ms_route.Api.Domain.Ports.Out;

public interface ICityRepository
{
    Task<IReadOnlyList<City>> GetAllAsync(CancellationToken ct = default);
}
