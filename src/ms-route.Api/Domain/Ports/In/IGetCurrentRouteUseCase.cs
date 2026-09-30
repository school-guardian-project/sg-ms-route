using ms_route.Api.Application.Dto;

namespace ms_route.Api.Domain.Ports.In;

public interface IGetCurrentRouteUseCase
{
    Task<RouteDetailDto> ExecuteAsync(Guid driverId, CancellationToken ct = default);
}
