using ms_route.Api.Application.Dto;

namespace ms_route.Api.Domain.Ports.In;

public interface ICreateRouteUseCase
{
    Task<RouteResponseDto> ExecuteAsync(RouteRequestDto request, CancellationToken ct = default);
}
