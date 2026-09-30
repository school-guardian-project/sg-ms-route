using ms_route.Api.Application.Dto;

namespace ms_route.Api.Domain.Ports.In;

public interface IGetRouteUseCase
{
    Task<RouteResponseDto> ExecuteAsync(Guid id, CancellationToken ct = default);
}
