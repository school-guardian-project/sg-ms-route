using ms_route.Api.Application.Dto;

namespace ms_route.Api.Domain.Ports.In;

public interface IListRouteUseCase
{
    Task<IEnumerable<RouteListDto>> ExecuteAsync(CancellationToken ct = default);
}
