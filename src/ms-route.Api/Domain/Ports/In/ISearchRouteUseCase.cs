using ms_route.Api.Application.Dto;

namespace ms_route.Api.Domain.Ports.In;

public interface ISearchRouteUseCase
{
    Task<IEnumerable<RouteListDto>> ExecuteAsync(string search, CancellationToken ct = default);
}
