using ms_route.Api.Application.Dto;

namespace ms_route.Api.Domain.Ports.In;

public interface IAttachStopToRouteUseCase
{
    Task<RouteStopResponseDto> ExecuteAsync(Guid routeId, Guid stopId, CancellationToken ct = default);
}
