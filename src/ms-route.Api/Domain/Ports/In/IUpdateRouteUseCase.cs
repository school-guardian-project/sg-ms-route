using ms_route.Api.Application.Dto;

namespace ms_route.Api.Domain.Ports.In;

public interface IUpdateRouteUseCase
{
    Task ExecuteAsync(Guid id, RouteRequestDto request, CancellationToken ct = default);
}
