using ms_route.Api.Application.Dto;

namespace ms_route.Api.Domain.Ports.In;

public interface ICreateRouteScheduleUseCase
{
    Task<ScheduleDetailDto> ExecuteAsync(
        Guid routeId,
        RouteScheduleRequestDto request,
        CancellationToken ct = default);
}
